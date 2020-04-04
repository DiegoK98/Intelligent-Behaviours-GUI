using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class BaseNode : GUIElement
{
    public string nodeName = "";
    public readonly long identificator;

    public Rect windowRect;
    public static int width = 150;
    public static int height = 70;

    public BaseNode()
    {
        identificator = UniqueID();
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((BaseNode)other))
            return false;
        if (this.nodeName != ((BaseNode)other).nodeName)
            return false;
        if (this.identificator != ((BaseNode)other).identificator)
            return false;

        return true;
    }

    public void DrawWindow(NodeEditor parent)
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperCenter;
        style.fontSize = 15;
        nodeName = EditorGUILayout.TextArea(nodeName, style, GUILayout.ExpandWidth(true));

        if (!parent.popupShown)
        {
            //En vez de is null deberia checkeear si ha cambiado
            if (parent.focusedObj is null && CheckNameExisting(parent, nodeName))
            {
                parent.popupShown = true;
                PopupWindow.InitNameRepeated(parent, nodeName, windowRect);
            }
        }
        else
        {
            if (parent.focusedObj is null)
            {
                parent.popupShown = false;
            }
        }
    }
}
