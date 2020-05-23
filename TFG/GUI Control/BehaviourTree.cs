using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : ClickableElement
{
    public List<BehaviourNode> nodes = new List<BehaviourNode>();

    public List<TransitionGUI> connections = new List<TransitionGUI>();

    /// <summary>
    /// The BehaviourTree
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public void InitBehaviourTree(NodeEditor editor, ClickableElement parent, float posx, float posy)
    {
        InitClickableElement();

        this.parent = parent;
        type = elementType.BT;
        if (parent != null)
            elementName = parent.elementNamer.GenerateUniqueName(identificator, "New BT ");
        else
            elementName = editor.editorNamer.GenerateUniqueName(identificator, "New BT ");

        windowRect = new Rect(posx, posy, width, height);
    }

    public override XMLElement ToXMLElement(params object[] args)
    {
        XMLElement result = new XMLElement
        {
            name = CleanName(this.elementName),
            elemType = this.GetType().ToString(),
            windowPosX = this.windowRect.x,
            windowPosY = this.windowRect.y,
            nodes = nodes.FindAll(o => o.isRootNode).ConvertAll((rootNode) =>
            {
                return rootNode.ToXMLElement(this);
            }),
            Id = this.identificator
        };

        return result;
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
        foreach (TransitionGUI elem in connections)
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

        foreach (TransitionGUI transition in connections.FindAll(t => node.Equals(t.fromNode)))
        {
            connections.Remove(transition);
            DeleteNode((BehaviourNode)transition.toNode);
        }
        foreach (TransitionGUI transition in connections.FindAll(t => node.Equals(t.toNode)))
        {
            connections.Remove(transition);
        }

        if (node.subElem == null)
            elementNamer.RemoveName(node.identificator);
        else
            elementNamer.RemoveName(node.subElem.identificator);
    }

    /// <summary>
    /// Deletes the connection
    /// </summary>
    /// <param name="deletedTrans"></param>
    public void DeleteConnection(TransitionGUI deletedTrans)
    {
        connections.Remove(deletedTrans);

        elementNamer.RemoveName(deletedTrans.identificator);
    }

    /// <summary>
    /// Returns how many sons a node has
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public int ChildrenCount(GUIElement elem)
    {
        int res = 0;

        foreach (TransitionGUI transition in connections.FindAll(t => elem.Equals(t.fromNode)))
        {
            res += ChildrenCount((BehaviourNode)transition.toNode) + 1;
        }

        return res;
    }

    public bool ConnectedCheck(GUIElement start, GUIElement end)
    {
        foreach (TransitionGUI transition in connections.FindAll(t => start.Equals(t.fromNode)))
        {
            if (end.Equals((BehaviourNode)transition.toNode))
            {
                return true;
            }
            if (ConnectedCheck((BehaviourNode)transition.toNode, end))
                return true;
        }

        return false;
    }

    public override List<ClickableElement> GetSubElems()
    {
        List<ClickableElement> result = new List<ClickableElement>();

        foreach (BehaviourNode node in nodes)
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
