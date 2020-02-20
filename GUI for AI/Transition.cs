using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition : ScriptableObject
{

    private string transitionName = "";
    public Node fromNode;
    public Node toNode;

    public Transition(string name, Node from, Node to)
    {
        transitionName = name;

        fromNode = from;
        toNode = to;
    }
}
