using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Node : ScriptableObject
{
    public enum stateType
    {
        Default,
        Entry,
        Unconnected
    }

    public Rect windowRect;

    public bool hasInputs = false;

    public string stateName = "";

    public stateType type;

    public List<Transition> transitions;

    public List<Node> connectedNodes;

    public readonly long identificator;

    public Node(int typeNumber)
    {
        transitions = new List<Transition>();
        connectedNodes = new List<Node>();

        stateName = "New State";
        hasInputs = true;

        type = (stateType)typeNumber;

        identificator = UniqueID();
    }

    public long UniqueID()
    {
        long i = 1;

        foreach (byte b in Guid.NewGuid().ToByteArray())
        {
            i *= ((int)b + 1);
        }

        long number = (DateTime.Now.Ticks / 10) % 1000000000;

        return number;
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((Node)other))
            return false;
        if (this.stateName != ((Node)other).stateName)
            return false;
        if (this.identificator != ((Node)other).identificator)
            return false;

        return true;

    }

    public void DrawWindow()
    {
        stateName = EditorGUILayout.TextField("State Name", stateName);
    }

    public void DrawCurves()
    {
        foreach (Transition elem in transitions)
        {
            if (Equals(elem.fromNode))
            {
                Rect fromNodeRect = new Rect(windowRect);
                fromNodeRect.x = windowRect.x + fromNodeRect.width / 2;

                Rect toNodeRect = new Rect(elem.toNode.windowRect);
                toNodeRect.x = elem.toNode.windowRect.x - toNodeRect.width / 2;

                NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect);

                elem.textBox = NodeEditor.DrawTextBox(elem);
            }
        }
    }

    public void NodeDeleted(Node node)
    {
        foreach (Transition t in transitions)
        {
            if (node.Equals(t.toNode) || node.Equals(t.fromNode))
            {
                TransitionDeleted(t);
                break;
            }
        }
    }

    public void TransitionDeleted(Transition t)
    {
        //REDO Entry deberia guardar todos los nodos por los que puede pasar (excluyendo asi los que estan conectados pero nunca llegarían porque es una transición de vuelta, sin ninguna ida)
        transitions.Remove(t);
        if (t.fromNode.type != stateType.Entry && !t.fromNode.connectedNodes.Find(o => o.type == stateType.Entry))
            t.fromNode.type = stateType.Unconnected;
        if (t.toNode.type != stateType.Entry && !t.toNode.connectedNodes.Find(o => o.type == stateType.Entry))
            t.toNode.type = stateType.Unconnected;

    }

    public void SetTransitionTo(Node input)
    {
        this.transitions.Add(new Transition("New Transition", this, input));
        input.transitions.Add(new Transition("New Transition", this, input));
    }
}
