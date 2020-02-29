using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Transition : ScriptableObject
{

    public string transitionName;
    public Node fromNode;
    public Node toNode;

    public bool isFocused = false;

    public Rect textBox;

    public Transition(string name, Node from, Node to)
    {
        transitionName = name;

        fromNode = from;
        toNode = to;
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
