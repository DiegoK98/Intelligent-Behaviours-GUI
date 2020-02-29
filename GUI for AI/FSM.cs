using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : ScriptableObject
{
    public string FSMName = "";
    private Node EntryState;

    public List<Node> states = new List<Node>();

    public List<Transition> transitions;

    public FSM(string name, Node node)
    {
        FSMName = name;

        AddEntryState(node);

        transitions = new List<Transition>();
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((FSM)other))
            return false;
        if (this.FSMName != ((FSM)other).FSMName)
            return false;
        if (this.EntryState.Equals(((FSM)other).EntryState))
            return false;

        return true;
    }

    public void AddEntryState(Node node)
    {
        states.Add(node);
        EntryState = node;
    }

    public void AddState(Node node)
    {
        states.Add(node);
    }

    public Node[] GetState(Node node)
    {
        return states.FindAll(o => o.Equals(node)).ToArray();
    }

    public bool RemoveState(Node node)
    {
        return states.Remove(node);
    }

    public bool isEntryState(Node node)
    {
        return node.Equals(EntryState);
    }

    public void DeleteNode(Node node)
    {
        states.Remove(node);

        for (int i = 0; i < transitions.Count; i++)
        {
            DeleteTransition(transitions[i]);
        }

        foreach (Node n in states)
        {
            n.NodeDeleted(node);
        }
    }

    public void DeleteTransition(Transition deletedTrans)
    {
        transitions.Remove(deletedTrans);

        if (deletedTrans.fromNode.type != Node.stateType.Entry && !CheckConnected(deletedTrans.fromNode))
            deletedTrans.fromNode.type = Node.stateType.Unconnected;
        if (deletedTrans.toNode.type != Node.stateType.Entry && !CheckConnected(deletedTrans.toNode))
            deletedTrans.toNode.type = Node.stateType.Unconnected;

        foreach (Node n in states)
        {
            n.nodeTransitions.Remove(deletedTrans);
        }
    }

    public bool CheckConnected(Node baseNode)
    {
        if (isEntryState(baseNode))
            return true;

        foreach (Transition elem in baseNode.nodeTransitions.FindAll(o => o.toNode.Equals(baseNode)))
        {
            if (CheckConnected(elem.fromNode))
                return true;
        }

        return false;
    }

    public void AddTransition(Transition newTransition)
    {
        transitions.Add(newTransition);

        newTransition.fromNode.nodeTransitions.Add(newTransition);
        newTransition.toNode.nodeTransitions.Add(newTransition);
    }
}
