using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : ClickableElement
{
    private StateNode EntryState;

    public List<StateNode> states = new List<StateNode>();

    public List<TransitionGUI> transitions = new List<TransitionGUI>();

    public static int uniqueNameID = 0;

    public bool hasEntryState
    {
        get
        {
            return EntryState != null && states.Contains(EntryState);
        }
    }

    /// <summary>
    /// The FSM
    /// </summary>
    /// <param name="node"></param>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public void InitFSM(StateNode node, ClickableElement parent, float posx, float posy)
    {
        this.parent = parent;

        elementName = "New FSM " + uniqueNameID++;
        identificator = UniqueID();
        type = elementType.FSM;
        windowRect = new Rect(posx, posy, width, height);

        if (node != null)
            AddEntryState(node);
    }

    /// <summary>
    /// Gets the type of the element properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        return "FSM";
    }

    /// <summary>
    /// The Equals
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (!base.Equals(other))
            return false;
        if (this.elementName != ((FSM)other).elementName)
            return false;
        if (this.identificator != ((FSM)other).identificator)
            return false;

        return true;
    }

    /// <summary>
    /// Add a Node as an EntryState
    /// </summary>
    /// <param name="node"></param>
    public void AddEntryState(StateNode node)
    {
        node.type = StateNode.stateType.Entry;
        states.Add(node);
        EntryState = node;
    }

    /// <summary>
    /// Convert a Node to Entry State
    /// </summary>
    /// <param name="node"></param>
    public void SetAsEntry(StateNode node)
    {
        //Previous Entry Node set to Unconnected
        EntryState.type = StateNode.stateType.Unconnected;

        node.type = StateNode.stateType.Entry;
        EntryState = node;

        CheckConnected();
    }

    /// <summary>
    /// Check if a given Node is the EntryState
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool isEntryState(StateNode node)
    {
        return node.Equals(EntryState);
    }

    /// <summary>
    /// Draws all transitions curves for the FSM
    /// </summary>
    public void DrawCurves()
    {
        foreach (TransitionGUI elem in transitions)
        {
            if (elem.fromNode is null || elem.toNode is null)
                break;

            bool hasCouple = false;

            Rect fromNodeRect = new Rect(elem.fromNode.windowRect);
            Rect toNodeRect = new Rect(elem.toNode.windowRect);

            if(transitions.Exists(t => t.fromNode.Equals(elem.toNode) && t.toNode.Equals(elem.fromNode)))
            {
                hasCouple = true;
            }

            NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect, elem.isFocused, hasCouple);
        }
    }

    /// <summary>
    /// Delete a given Node and its transitions
    /// </summary>
    /// <param name="node"></param>
    public void DeleteNode(StateNode node)
    {
        states.Remove(node);

        for (int i = 0; i < node.nodeTransitions.Count; i++)
        {
            DeleteTransition(node.nodeTransitions[i]);
        }

        // Notify all nodes that this node has been deleted, so they can delete the necessary transition references
        foreach (StateNode n in states)
        {
            n.NodeDeleted(node);
        }
    }

    /// <summary>
    /// Delete a given Transition
    /// </summary>
    /// <param name="deletedTrans"></param>
    public void DeleteTransition(TransitionGUI deletedTrans)
    {
        transitions.Remove(deletedTrans);

        foreach (StateNode n in states)
        {
            n.nodeTransitions.Remove(deletedTrans);
        }

        CheckConnected();
    }

    /// <summary>
    /// Recalculate every node's state of connection to the Entry State
    /// </summary>
    /// <param name="baseNode"></param>
    public void CheckConnected(StateNode baseNode = null)
    {
        if (baseNode == null)
        {
            baseNode = EntryState;

            foreach (StateNode elem in states.FindAll(o => o.type != StateNode.stateType.Entry))
            {
                elem.type = StateNode.stateType.Unconnected;
            }

            if (!states.Contains(baseNode))
                return;
        }
        else if (baseNode.type == StateNode.stateType.Unconnected)
        {
            baseNode.type = StateNode.stateType.Default;
        }
        else
        {
            return;
        }

        foreach (TransitionGUI elem in baseNode.nodeTransitions.FindAll(o => o.fromNode.Equals(baseNode)))
        {
            CheckConnected((StateNode)elem.toNode);
        }
    }

    /// <summary>
    /// Add a transition to the FSM
    /// </summary>
    /// <param name="newTransition"></param>
    public void AddTransition(TransitionGUI newTransition)
    {
        transitions.Add(newTransition);

        ((StateNode)newTransition.fromNode).nodeTransitions.Add(newTransition);
        ((StateNode)newTransition.toNode).nodeTransitions.Add(newTransition);

        CheckConnected();
    }

    public override List<ClickableElement> GetSubElems()
    {
        List<ClickableElement> result = new List<ClickableElement>();

        foreach (StateNode node in states)
        {
            if (node.subElem != null)
            {
                result.AddRange(node.subElem.GetSubElems());
                result.Add(node.subElem);
            }
        }

        return result;
    }
}
