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

    public void DrawWindow()
    {
        elementName = EditorGUILayout.TextField(type + " name", elementName);
    }
}
