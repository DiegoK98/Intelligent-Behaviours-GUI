using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class StateNode : BaseNode
{
    public enum stateType
    {
        Default,
        Entry,
        Unconnected
    }

    public stateType type;

    public List<Transition> nodeTransitions;

    static int uniqueNameID = 0;

    public StateNode(int typeNumber) : base()
    {
        nodeTransitions = new List<Transition>();

        nodeName = "New State " + uniqueNameID++;

        type = (stateType)typeNumber;
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
            }
        }
    }

    public void NodeDeleted(StateNode node)
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
