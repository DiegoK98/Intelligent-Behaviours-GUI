using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Node : ScriptableObject
{

    public Rect windowRect;

    public bool hasInputs = false;

    public string stateName = "";

    public List<Transition> possibleTransitions;

    public Node()
    {
        possibleTransitions = new List<Transition>();

        stateName = "New State";
        hasInputs = true;
    }

    public void DrawWindow()
    {
        stateName = EditorGUILayout.TextField("State Name", stateName);
    }

    public void DrawCurves()
    {
        foreach (Transition elem in possibleTransitions)
        {
            Rect fromNodeRect = new Rect(windowRect);
            fromNodeRect.x = windowRect.x + fromNodeRect.width / 2;

            Rect toNodeRect = new Rect(elem.toNode.windowRect);
            toNodeRect.x = elem.toNode.windowRect.x - toNodeRect.width / 2;

            NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect);

            elem.textBox = NodeEditor.DrawTextBox(elem);
        }
    }

    public void NodeDeleted(Node node)
    {
        foreach (Transition t in possibleTransitions)
        {
            if (t.toNode == node)
            {
                possibleTransitions.Remove(t);
                break;
            }
        }
    }

    public void TransitionDeleted(Transition trans)
    {
        possibleTransitions.Remove(trans);
    }

    public void SetInput(Node input, Vector2 clickPos)
    {
        if (windowRect.Contains(clickPos))
        {
            //Le ponemos la transicion al node que ha llamado a setinput, que es del que viene la transición
            input.possibleTransitions.Add(new Transition("New Transition", input, this));
        }
    }
}
