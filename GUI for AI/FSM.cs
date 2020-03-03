using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : ClickableElement
{
    private StateNode EntryState;
    public readonly long identificator;

    public List<StateNode> states = new List<StateNode>();
    public List<Transition> transitions;

    public FSM(string name, StateNode node)
    {
        elementName = name;
        identificator = UniqueID();

        type = elementType.FSM;

        AddEntryState(node);

        transitions = new List<Transition>();
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((FSM)other))
            return false;
        if (this.elementName != ((FSM)other).elementName)
            return false;
        if (this.identificator != ((FSM)other).identificator)
            return false;

        return true;
    }

    public void AddEntryState(StateNode node)
    {
        states.Add(node);
        EntryState = node;
    }

    public void SetAsEntry(StateNode node)
    {
        EntryState.type = StateNode.stateType.Unconnected;
        node.type = StateNode.stateType.Entry;
        EntryState = node;

        CheckConnected();
    }

    public bool isEntryState(StateNode node)
    {
        return node.Equals(EntryState);
    }

    public void DeleteNode(StateNode node)
    {
        states.Remove(node);

        for (int i = 0; i < node.nodeTransitions.Count; i++)
        {
            DeleteTransition(node.nodeTransitions[i]);
        }

        foreach (StateNode n in states)
        {
            n.NodeDeleted(node);
        }
    }

    public void DeleteTransition(Transition deletedTrans)
    {
        transitions.Remove(deletedTrans);

        foreach (StateNode n in states)
        {
            n.nodeTransitions.Remove(deletedTrans);
        }

        CheckConnected();
    }

    public void CheckConnected(StateNode baseNode = null)
    {
        if (baseNode == null)
        {
            baseNode = EntryState;

            foreach (StateNode elem in states.FindAll(o => o.type != StateNode.stateType.Entry))
            {
                elem.type = StateNode.stateType.Unconnected;
            }
        }
        else if (baseNode.type == StateNode.stateType.Unconnected)
        {
            baseNode.type = StateNode.stateType.Default;
        }
        else
        {
            return;
        }

        foreach (Transition elem in baseNode.nodeTransitions.FindAll(o => o.fromNode.Equals(baseNode)))
        {
            CheckConnected((StateNode)elem.toNode);
        }
    }

    public void AddTransition(Transition newTransition)
    {
        transitions.Add(newTransition);

        ((StateNode)newTransition.fromNode).nodeTransitions.Add(newTransition);
        ((StateNode)newTransition.toNode).nodeTransitions.Add(newTransition);

        CheckConnected();
    }
}
