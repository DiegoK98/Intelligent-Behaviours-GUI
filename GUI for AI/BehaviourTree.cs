using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : ClickableElement
{
    public readonly long identificator;

    public List<BehaviourNode> nodes = new List<BehaviourNode>();

    public List<Transition> connections;

    static int uniqueNameID = 0;

    /// <summary>
    /// The BehaviourTree
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public BehaviourTree(ClickableElement parent, float posx, float posy)
    {
        this.parent = parent;

        type = elementType.BT;
        elementName = "New BT " + uniqueNameID++;
        identificator = UniqueID();
        connections = new List<Transition>();

        windowRect = new Rect(posx, posy, width, height);
    }

    /// <summary>
    /// Gets the type of the element properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        return "Behaviour Tree";
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
        if (this.elementName != ((BehaviourTree)other).elementName)
            return false;
        if (this.identificator != ((BehaviourTree)other).identificator)
            return false;

        return true;
    }

    /// <summary>
    /// Draws all transitions curves for the Behaviour Tree
    /// </summary>
    public void DrawCurves()
    {
        foreach (Transition elem in connections)
        {
            Rect fromNodeRect = new Rect(elem.fromNode.windowRect);
            Rect toNodeRect = new Rect(elem.toNode.windowRect);

            NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect, elem.isFocused);
        }
    }

    /// <summary>
    /// Deletes the node and its children, including the transitions connected to all of them
    /// </summary>
    /// <param name="node"></param>
    public void DeleteNode(BehaviourNode node)
    {
        nodes.Remove(node);

        foreach (Transition transition in connections.FindAll(t => node.Equals(t.fromNode)))
        {
            connections.Remove(transition);
            DeleteNode((BehaviourNode)transition.toNode);
        }
        foreach (Transition transition in connections.FindAll(t => node.Equals(t.toNode)))
        {
            connections.Remove(transition);
        }
    }

    /// <summary>
    /// Deletes the connection
    /// </summary>
    /// <param name="deletedTrans"></param>
    public void DeleteConnection(Transition deletedTrans)
    {
        connections.Remove(deletedTrans);
    }

    /// <summary>
    /// Returns how many sons a node has
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public int ChildrenCount(GUIElement elem)
    {
        int res = 0;

        foreach (Transition transition in connections.FindAll(t => elem.Equals(t.fromNode)))
        {
            res += ChildrenCount((BehaviourNode)transition.toNode) + 1;
        }

        return res;
    }

    public bool ConnectedCheck(GUIElement start, GUIElement end)
    {
        bool connected = false;

        foreach (Transition transition in connections.FindAll(t => start.Equals(t.fromNode)))
        {
            if (end.Equals((BehaviourNode)transition.toNode))
            {
                connected = true;
                break;
            }
            connected = ConnectedCheck((BehaviourNode)transition.toNode, end);
        }

        return connected;
    }
}
