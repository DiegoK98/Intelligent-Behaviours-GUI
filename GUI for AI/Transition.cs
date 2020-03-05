using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Transition : GUIElement
{
    public string transitionName = "";
    public BaseNode fromNode;
    public BaseNode toNode;

    public Rect textBox;

    public Transition(string name, BaseNode from, BaseNode to)
    {
        transitionName = name;

        fromNode = from;
        toNode = to;
    }

    public void DrawBox()
    {
        transitionName = EditorGUILayout.TextField("Name: ", transitionName);
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((Transition)other))
            return false;
        if (this.transitionName != ((Transition)other).transitionName)
            return false;
        if (!fromNode.Equals(((Transition)other).fromNode) || !toNode.Equals(((Transition)other).toNode))
            return false;

        return true;
    }
}
