using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BehaviourTree : ClickableElement
{
    /// <summary>
    /// List of <see cref="BehaviourNode"/> that belong to this <see cref="BehaviourTree"/>
    /// </summary>
    public List<BehaviourNode> nodes = new List<BehaviourNode>();

    /// <summary>
    /// List of <see cref="TransitionGUI"/> that connect the <see cref="nodes"/>
    /// </summary>
    public List<TransitionGUI> connections = new List<TransitionGUI>();

    /// <summary>
    /// The Initializer for the <seealso cref="BehaviourNode"/>
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="id"></param>
    public void InitBehaviourTree(ClickableElement parent, float posx, float posy)
    {
        InitClickableElement();

        this.editor = EditorWindow.GetWindow<NodeEditor>();
        this.parent = parent;

        if (parent != null)
            elementName = parent.elementNamer.AddName(identificator, "New BT ");
        else
            elementName = editor.editorNamer.AddName(identificator, "New BT ");

        windowRect = new Rect(posx, posy, width, height);
    }

    /// <summary>
    /// The Initializer for the <seealso cref="BehaviourNode"/> when it is being loaded from an XML
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="id"></param>
    public void InitBehaviourTreeFromXML(ClickableElement parent, float posx, float posy, string id, string name)
    {
        InitClickableElement(id);

        this.editor = EditorWindow.GetWindow<NodeEditor>();
        this.parent = parent;

        if (parent != null)
            elementName = parent.elementNamer.AddName(identificator, name);
        else
            elementName = editor.editorNamer.AddName(identificator, name);

        windowRect = new Rect(posx, posy, width, height);
    }

    /// <summary>
    /// Creates and returns an <see cref="XMLElement"/> that corresponds to this <see cref="BehaviourTree"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override XMLElement ToXMLElement(params object[] args)
    {
        XMLElement result = new XMLElement
        {
            name = CleanName(this.elementName),
            elemType = this.GetType().ToString(),
            windowPosX = this.windowRect.x,
            windowPosY = this.windowRect.y,
            nodes = nodes.FindAll(o => o.isRoot).ConvertAll((rootNode) =>
            {
                return rootNode.ToXMLElement(this);
            }),
            Id = this.identificator
        };

        return result;
    }

    /// <summary>
    /// Creates a copy of this <see cref="BehaviourTree"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override GUIElement CopyElement(params object[] args)
    {
        ClickableElement parent = (ClickableElement)args[0];

        GUIElement result = new BehaviourTree
        {
            identificator = this.identificator,
            elementNamer = CreateInstance<UniqueNamer>(),
            elementName = this.elementName,
            parent = parent,
            editor = this.editor,
            windowRect = new Rect(this.windowRect)
        };

        ((BehaviourTree)result).nodes = this.nodes.Select(o => (BehaviourNode)o.CopyElement(result)).ToList();
        ((BehaviourTree)result).connections = this.connections.Select(o => 
        (TransitionGUI)o.CopyElement(((BehaviourTree)result).nodes.Find(n => n.identificator == o.fromNode.identificator),
                                     ((BehaviourTree)result).nodes.Find(n => n.identificator == o.toNode.identificator))).ToList();

        return result;
    }

    /// <summary>
    /// Returns the type properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        return "Behaviour Tree";
    }

    /// <summary>
    /// Compares this <see cref="BehaviourTree"/> with <paramref name="other"/>
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
    /// Draws all <see cref="TransitionGUI"/> curves for the <see cref="BehaviourTree"/>
    /// </summary>
    public void DrawCurves()
    {
        foreach (TransitionGUI elem in connections)
        {
            Rect fromNodeRect = new Rect(elem.fromNode.windowRect);
            Rect toNodeRect = new Rect(elem.toNode.windowRect);

            NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect, editor.focusedObjects.Contains(elem));
        }
    }

    /// <summary>
    /// Deletes the <paramref name="node"/> and its children
    /// </summary>
    /// <param name="node"></param>
    public void DeleteNode(BehaviourNode node)
    {
        if (nodes.Remove(node))
        {
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
    }

    /// <summary>
    /// Deletes the <paramref name="connection"/>
    /// </summary>
    /// <param name="connection"></param>
    public void DeleteConnection(TransitionGUI connection)
    {
        if (connections.Remove(connection))
            elementNamer.RemoveName(connection.identificator);
    }

    /// <summary>
    /// Returns how many children <paramref name="node"/> has
    /// </summary>
    /// <param name="i"></param>
    /// <returns>The number of children <paramref name="node"/> has</returns>
    public int ChildrenCount(BehaviourNode node)
    {
        int res = 0;

        foreach (TransitionGUI transition in connections.FindAll(t => node.Equals(t.fromNode)))
        {
            res += ChildrenCount((BehaviourNode)transition.toNode) + 1;
        }

        return res;
    }

    /// <summary>
    /// Checks wether <paramref name="start"/> could ever reach <paramref name="end"/> in the <see cref="BehaviourTree"/> execution
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public bool ConnectedCheck(BehaviourNode start, BehaviourNode end)
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

    /// <summary>
    /// Returns the list of <see cref="ClickableElement"/> that exist inside each <see cref="BehaviourNode"/> of this <see cref="BehaviourTree"/> 
    /// </summary>
    /// <returns>The list of <see cref="ClickableElement"/> that exist inside each <see cref="BehaviourNode"/> of this <see cref="BehaviourTree"/></returns>
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
