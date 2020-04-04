using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ClickableElement : GUIElement
{
    public enum elementType
    {
        FSM,
        BT
    }

    public Rect windowRect;
    public static int width = 150;
    public static int height = 70;

    public string elementName = "";
    public elementType type;

    public ClickableElement parent;

    public void DrawWindow(NodeEditor parent)
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperCenter;
        style.fontSize = 15;
        elementName = EditorGUILayout.TextArea(elementName, style, GUILayout.ExpandWidth(true));

        if (!parent.popupShown)
        {
            //En vez de is null deberia checkeear si ha cambiado
            if (parent.focusedObj is null && CheckNameExisting(parent, elementName))
            {
                parent.popupShown = true;
                PopupWindow.InitNameRepeated(parent, elementName, windowRect);
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
