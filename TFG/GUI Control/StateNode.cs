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

    public ClickableElement subElem;

    public stateType type;

    public List<TransitionGUI> nodeTransitions = new List<TransitionGUI>();

    static int uniqueNameID = 0;

    /// <summary>
    /// The InitStateNode
    /// </summary>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public void InitStateNode(int typeNumber, float posx, float posy, ClickableElement subElem = null)
    {
        InitBaseNode();

        if (subElem != null)
        {
            this.subElem = subElem;
            nodeName = this.subElem.elementName;
        }
        else
        {
            nodeName = "New State " + uniqueNameID++;
        }
        
        type = (stateType)typeNumber;

        windowRect = new Rect(posx, posy, width, height);
    }

    /// <summary>
    /// Gets the type of the element properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        if (subElem is null)
            return "Node";
        else
            return subElem.GetTypeString();
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
