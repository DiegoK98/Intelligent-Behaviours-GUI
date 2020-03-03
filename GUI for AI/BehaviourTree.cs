using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : ClickableElement
{
    public string BTName = "";
    public readonly long identificator;

    public List<BehaviourNode> states = new List<BehaviourNode>();
    public List<Transition> transitions;

    public BehaviourTree(string name)
    {
        BTName = name;
        identificator = UniqueID();

        type = elementType.BT;

        transitions = new List<Transition>();
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((BehaviourTree)other))
            return false;
        if (this.BTName != ((BehaviourTree)other).BTName)
            return false;
        if (this.identificator != ((BehaviourTree)other).identificator)
            return false;

        return true;
    }

    public void DeleteNode(BehaviourNode node)
    {
        states.Remove(node);

        foreach (Transition transition in transitions.FindAll(t => node.Equals(t.fromNode) || node.Equals(t.toNode)))
        {
            transitions.Remove(transition);
        }
    }
}
