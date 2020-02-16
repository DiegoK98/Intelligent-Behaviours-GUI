using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class Node : BaseNode
{

    private BaseNode connectedNode;
    private Rect connectedNodeRect;

    public Node()
    {
        windowTitle = "Input Node";
        hasInputs = true;
    }

    public override void DrawWindow()
    {
        base.DrawWindow();

        Event e = Event.current;

        GUILayout.Label("O");

        if (e.type == EventType.Repaint)
        {
            connectedNodeRect = GUILayoutUtility.GetLastRect();
        }
    }

    public override void DrawCurves()
    {
        if (connectedNode)
        {
            Rect rect = windowRect;
            rect.x += connectedNodeRect.x;
            rect.y += connectedNodeRect.y + connectedNodeRect.height / 2;
            rect.width = 1;
            rect.height = 1;

            NodeEditor.DrawNodeCurve(connectedNode.windowRect, rect);
        }
    }

    public override void NodeDeleted(BaseNode node)
    {
        if (node.Equals(connectedNode))
        {
            connectedNode = null;
        }
    }

    public override BaseNode ClickedOnNode(Vector2 pos)
    {
        BaseNode retVal = null;

        pos.x -= windowRect.x;
        pos.y -= windowRect.y;

        if (connectedNodeRect.Contains(pos))
        {
            retVal = connectedNode;
            connectedNode = null;
        }

        return retVal;
    }

    public override void SetInput(BaseNode input, Vector2 clickPos)
    {
        clickPos.x -= windowRect.x;
        clickPos.y -= windowRect.y;

        if (connectedNodeRect.Contains(clickPos))
        {
            connectedNode = input;
        }
    }
}
