using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Node : ScriptableObject
{
    public enum stateType
    {
        Default,
        Entry,
        Unconnected
    }

    public Rect windowRect;

    public bool hasInputs = false;
    public bool isFocused = false;

    public string stateName = "";

    public stateType type;

    public List<Transition> nodeTransitions;

    public readonly long identificator;

    public Node(int typeNumber)
    {
        nodeTransitions = new List<Transition>();

        stateName = "New State";
        hasInputs = true;

        type = (stateType)typeNumber;

        identificator = UniqueID();
    }

    public long UniqueID()
    {
        long i = 1;

        foreach (byte b in Guid.NewGuid().ToByteArray())
        {
            i *= ((int)b + 1);
        }

        long number = (DateTime.Now.Ticks / 10) % 1000000000;

        return number;
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((Node)other))
            return false;
        if (this.stateName != ((Node)other).stateName)
            return false;
        if (this.identificator != ((Node)other).identificator)
            return false;

        return true;
    }

    public void DrawWindow()
    {
        stateName = EditorGUILayout.TextField("State Name", stateName);
    }

    public void DrawCurves()
    {
        foreach (Transition elem in nodeTransitions)
        {
            if (Equals(elem.fromNode))
            {
                Rect fromNodeRect = new Rect(windowRect);
                fromNodeRect.x = windowRect.x + fromNodeRect.width / 2;

                Rect toNodeRect = new Rect(elem.toNode.windowRect);
                toNodeRect.x = elem.toNode.windowRect.x - toNodeRect.width / 2;

                NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect, elem.isFocused);

                elem.textBox = NodeEditor.DrawTextBox(elem);
            }
        }
    }

    public void NodeDeleted(Node node)
    {
        for (int i = 0; i < nodeTransitions.Count; i++)
        {
            if (node.Equals(nodeTransitions[i].toNode) || node.Equals(nodeTransitions[i].fromNode))
            {
                nodeTransitions.Remove(nodeTransitions[i]);
            }
        }
    }
}
