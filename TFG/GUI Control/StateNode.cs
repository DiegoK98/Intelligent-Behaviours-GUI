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

    public List<TransitionsGUI> nodeTransitions;

    static int uniqueNameID = 0;

    /// <summary>
    /// The StateNode
    /// </summary>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public StateNode(int typeNumber, float posx, float posy) : base()
    {
        nodeTransitions = new List<TransitionsGUI>();
        nodeName = "New State " + uniqueNameID++;
        type = (stateType)typeNumber;

        windowRect = new Rect(posx, posy, width, height);
    }

    /// <summary>
    /// The StateNode with a clickable element inside of it
    /// </summary>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="subElem"></param>
    public StateNode(int typeNumber, float posx, float posy, ClickableElement subElem) : base()
    {
        nodeTransitions = new List<TransitionsGUI>();
        elem = subElem;
        nodeName = elem.elementName;
        type = (stateType)typeNumber;

        windowRect = new Rect(posx, posy, width, height);
    }

    /// <summary>
    /// Gets the type of the element properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        if (elem is null)
            return "Node";
        else
            return elem.GetTypeString();
    }

    /// <summary>
    /// Removes the references to the transitions that were connected to the deleted node
    /// </summary>
    /// <param name="deletedNode"></param>
    public void NodeDeleted(StateNode deletedNode)
    {
        for (int i = 0; i < nodeTransitions.Count; i++)
        {
            if (deletedNode.Equals(nodeTransitions[i].toNode) || deletedNode.Equals(nodeTransitions[i].fromNode))
            {
                nodeTransitions.Remove(nodeTransitions[i]);
            }
        }
    }
}
