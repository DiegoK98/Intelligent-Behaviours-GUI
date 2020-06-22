using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilitySystem : ClickableElement
{
    /// <summary>
    /// List of <see cref="BehaviourNode"/> that belong to this <see cref="BehaviourTree"/>
    /// </summary>
    public List<UtilityNode> nodes = new List<UtilityNode>();

    /// <summary>
    /// List of <see cref="TransitionGUI"/> that connect the <see cref="nodes"/>
    /// </summary>
    public List<TransitionGUI> connections = new List<TransitionGUI>();

    /// <summary>
    /// The Initializer for the <seealso cref="UtilitySystem"/>
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="id"></param>
    public void InitUtilitySystem(NodeEditor editor, ClickableElement parent, float posx, float posy, string id = null)
    {
        InitClickableElement(id);

        this.parent = parent;
        if (parent != null)
            elementName = parent.elementNamer.AddName(identificator, "New US ");
        else
            elementName = editor.editorNamer.AddName(identificator, "New US ");

        windowRect = new Rect(posx, posy, width, height);
    }

    // TODO
    /// <summary>
    /// Creates and returns an <see cref="XMLElement"/> that corresponds to this <see cref="BehaviourTree"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override XMLElement ToXMLElement(params object[] args)
    {
        XMLElement result = new XMLElement
        {
            //name = CleanName(this.elementName),
            //elemType = this.GetType().ToString(),
            //windowPosX = this.windowRect.x,
            //windowPosY = this.windowRect.y,
            //nodes = nodes.FindAll(o => o.isRoot).ConvertAll((rootNode) =>
            //{
            //    return rootNode.ToXMLElement(this);
            //}),
            //Id = this.identificator
        };

        return result;
    }

    /// <summary>
    /// Returns the type properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        return "Utility System";
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
        if (this.elementName != ((UtilitySystem)other).elementName)
            return false;
        if (this.identificator != ((UtilitySystem)other).identificator)
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

            NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect, elem.isFocused);
        }
    }

    /// <summary>
    /// Deletes the <paramref name="node"/>
    /// </summary>
    /// <param name="node"></param>
    public void DeleteNode(UtilityNode node)
    {
        nodes.Remove(node);

        foreach (TransitionGUI transition in connections.FindAll(t => node.Equals(t.fromNode)))
        {
            connections.Remove(transition);
            //DeleteNode((UtilityNode)transition.toNode);
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

    public override List<ClickableElement> GetSubElems()
    {
        throw new NotImplementedException();
    }
}
