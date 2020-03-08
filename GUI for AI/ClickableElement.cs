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

    public string elementName = "";
    public elementType type;

    public void DrawWindow(NodeEditor parent)
    {
        elementName = EditorGUILayout.TextField(type + " name", elementName);

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
