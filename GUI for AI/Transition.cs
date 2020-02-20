using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Transition : ScriptableObject
{

    public string transitionName = "";
    public Node fromNode;
    public Node toNode;

    public Rect textBox;

    public Transition(string name, Node from, Node to)
    {
        transitionName = name;

        fromNode = from;
        toNode = to;
    }
}
