using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Styles
{
    public static GUIStyle TitleText = new GUIStyle()
    {
        alignment = TextAnchor.LowerCenter,
        fontSize = 13
    };

    public static GUIStyle SubTitleText = new GUIStyle()
    {
        alignment = TextAnchor.MiddleCenter,
        fontSize = 10
    };

    public static GUIStyle ErrorPrompt = new GUIStyle()
    {
        normal = new GUIStyleState()
        {
            textColor = Color.red
        }
    };

    public static GUIStyle OptionsButton = new GUIStyle()
    {
        hover = new GUIStyleState()
        {
            textColor = Color.grey
        },
        alignment = TextAnchor.UpperRight,
        fontSize = 15,
        fontStyle = FontStyle.Bold
    };

    public static GUIStyle TopBarButton = new GUIStyle(GUI.skin.button)
    {
        hover = new GUIStyleState()
        {
            textColor = Color.grey
        },
        alignment = TextAnchor.MiddleLeft
    };

    public static GUIStyle DeleteStyle = new GUIStyle(GUI.skin.button)
    {
        active = new GUIStyleState()
        {
            textColor = Color.red
        },
        focused = new GUIStyleState()
        {
            textColor = Color.red
        },
        hover = new GUIStyleState()
        {
            textColor = Color.red
        },
        normal = new GUIStyleState()
        {
            textColor = Color.red
        },
    };

    public static GUIStyle WarningLabel = new GUIStyle(GUI.skin.label)
    {
        normal = new GUIStyleState()
        {
            textColor = Color.red
        }
    };
}
