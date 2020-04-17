﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ClickableElement : GUIElement
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

    /// <summary>
    /// Draws all the elements inside the Element window
    /// </summary>
    /// <param name="parent"></param>
    public void DrawWindow(NodeEditor parent)
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.LowerCenter;
        style.fontSize = 15;
        elementName = EditorGUILayout.TextArea(elementName, style, GUILayout.ExpandWidth(true), GUILayout.Height(25));

        //En vez de is null deberia checkeear si ha cambiado
        if (parent.focusedObj is null)
        {
            CheckNameExisting(parent, elementName);
        }
    }
}
