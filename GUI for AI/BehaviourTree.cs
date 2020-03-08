using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : ClickableElement
{
    public readonly long identificator;

    public List<BehaviourNode> states = new List<BehaviourNode>();
    public List<Transition> transitions;

    static int uniqueNameID = 0;

    public BehaviourTree()
    {
        elementName = "New BT " + uniqueNameID++;
        identificator = UniqueID();

        type = elementType.BT;

        transitions = new List<Transition>();
    }

    public override bool Equals(object other)
    {
        if (!base.Equals(other))
            return false;
        if (this.elementName != ((BehaviourTree)other).elementName)
            return false;
        if (this.identificator != ((BehaviourTree)other).identificator)
            return false;

        return true;
    }

    public void DrawCurves()
    {
        foreach (Transition elem in transitions)
        {
            Rect fromNodeRect = new Rect(elem.fromNode.windowRect);
            fromNodeRect.y = elem.fromNode.windowRect.y + fromNodeRect.height / 2;

            Rect toNodeRect = new Rect(elem.toNode.windowRect);
            toNodeRect.y = elem.toNode.windowRect.y - toNodeRect.height / 2;

            NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect, elem.isFocused, true);
        }
    }

    public void DeleteNode(BehaviourNode node)
    {
        states.Remove(node);

        foreach (Transition transition in transitions.FindAll(t => node.Equals(t.fromNode)))
        {
            transitions.Remove(transition);
            DeleteNode((BehaviourNode)transition.toNode);
        }
        foreach (Transition transition in transitions.FindAll(t => node.Equals(t.toNode)))
        {
            transitions.Remove(transition);
        }
    }

    public int SonsCount(int i)
    {
        int res = 0;

        foreach (Transition transition in transitions.FindAll(t => states[i].Equals(t.fromNode)))
        {
            res += SonsCount(states.IndexOf((BehaviourNode)transition.toNode)) + 1;
        }

        return res;
    }
}
