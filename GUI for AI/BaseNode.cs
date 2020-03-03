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

    public void DrawWindow()
    {
        nodeName = EditorGUILayout.TextField("State Name", nodeName);
    }
}
