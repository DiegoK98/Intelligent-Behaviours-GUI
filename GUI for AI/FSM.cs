using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : ScriptableObject
{
    public string FSMName = "";
    private Node EntryState;

    public List<Node> states = new List<Node>();

    public List<Transition> transitions = new List<Transition>();

    public FSM(Node node = null)
    {
        FSMName = "New FSM";

        AddEntryState(node);
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
}
