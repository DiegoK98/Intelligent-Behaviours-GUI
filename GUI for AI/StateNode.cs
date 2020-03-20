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

    public ClickableElement elem;

    public stateType type;

    public List<Transition> nodeTransitions;

    static int uniqueNameID = 0;

    public StateNode(int typeNumber) : base()
    {
        nodeTransitions = new List<Transition>();

        nodeName = "New State " + uniqueNameID++;

        type = (stateType)typeNumber;
    }

    public StateNode(int typeNumber, ClickableElement subElem) : base()
    {
        nodeTransitions = new List<Transition>();

        elem = subElem;

        nodeName = elem.elementName;

        type = (stateType)typeNumber;
    }

    public void DrawCurves()
    {
        foreach (Transition elem in nodeTransitions)
        {
            if (Equals(elem.fromNode))
            {
                Rect fromNodeRect = new Rect(windowRect);
                Rect toNodeRect = new Rect(elem.toNode.windowRect);

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
