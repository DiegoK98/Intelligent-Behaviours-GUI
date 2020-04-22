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
        normal = new GUIStyleState()
        {
            background = MakeBackground(Color.red)
        }
    };

    public static GUIStyle CancelStyle = new GUIStyle(GUI.skin.button)
    {
        normal = new GUIStyleState()
        {
            background = MakeBackground(Color.grey)
        }
    };

    public static GUIStyle WarningLabel = new GUIStyle(GUI.skin.label)
    {
        normal = new GUIStyleState()
        {
            textColor = Color.red
        }
    };

    //Habrá que cambiarlo
    private static Texture2D MakeBackground(Color col)
    {
        Color[] pix = new Color[2 * 2];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(2, 2);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}
