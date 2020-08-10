using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FSM : ClickableElement
{
    /// <summary>
    /// The Entry State
    /// </summary>
    private StateNode EntryState;

    /// <summary>
    /// List of <see cref="StateNode"/> that form this <see cref="FSM"/>
    /// </summary>
    public List<StateNode> states = new List<StateNode>();

    /// <summary>
    /// List of <see cref="TransitionGUI"/> that connect the <see cref="states"/>
    /// </summary>
    public List<TransitionGUI> transitions = new List<TransitionGUI>();

    /// <summary>
    /// Returns true if this <see cref="FSM"/> has an <see cref="EntryState"/>
    /// </summary>
    public bool HasEntryState
    {
        get
        {
            return EntryState != null && states.Contains(EntryState);
        }
    }

    /// <summary>
    /// The Initializer for the <seealso cref="FSM"/>
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public void InitFSM(ClickableElement parent, float posx, float posy)
    {
        InitClickableElement();

        this.editor = EditorWindow.GetWindow<NodeEditor>();
        this.parent = parent;

        if (parent != null)
            elementName = parent.elementNamer.AddName(identificator, "New FSM ");
        else
            elementName = editor.editorNamer.AddName(identificator, "New FSM ");

        windowRect = new Rect(posx, posy, width, height);

        // Create the entry state
        StateNode entryNode = CreateInstance<StateNode>();
        entryNode.InitStateNode(this, stateType.Entry, 50, 50);

        if (entryNode != null)
            AddEntryState(entryNode);
    }

    /// <summary>
    /// The Initializer for the <seealso cref="FSM"/> when it is being loaded from XML
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="parent"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public void InitFSMFromXML(ClickableElement parent, float posx, float posy, string id, string name)
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
    /// Creates and returns an <see cref="XMLElement"/> that corresponds to this <see cref="FSM"/>
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
            nodes = states.ConvertAll((node) =>
            {
                return node.ToXMLElement();
            }),
            transitions = transitions.ConvertAll((trans) =>
            {
                return trans.ToXMLElement();
            }),
            Id = this.identificator
        };

        return result;
    }

    /// <summary>
    /// Creates a copy of this <see cref="FSM"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override GUIElement CopyElement(params object[] args)
    {
        ClickableElement parent = (ClickableElement)args[0];

        GUIElement result = new FSM
        {
            identificator = this.identificator,
            elementNamer = CreateInstance<UniqueNamer>(),
            elementName = this.elementName,
            parent = parent,
            editor = this.editor,
            windowRect = new Rect(this.windowRect)
        };

        ((FSM)result).states = this.states.Select(o => (StateNode)o.CopyElement(result)).ToList();
        ((FSM)result).transitions = this.transitions.Select(o =>
        (TransitionGUI)o.CopyElement(((FSM)result).states.Find(n => n.identificator == o.fromNode.identificator),
                                     ((FSM)result).states.Find(n => n.identificator == o.toNode.identificator))).ToList();

        foreach (StateNode elem in ((FSM)result).states)
        {
            if (elem.type == stateType.Entry)
            {
                ((FSM)result).SetAsEntry(elem);
            }
        }

        return result;
    }

    /// <summary>
    /// Returns the type properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        return "FSM";
    }

    /// <summary>
    /// Compares this <see cref="FSM"/> with <paramref name="other"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (!base.Equals(other))
            return false;
        if (this.elementName != ((FSM)other).elementName)
            return false;
        if (this.identificator != ((FSM)other).identificator)
            return false;

        return true;
    }

    /// <summary>
    /// Add <paramref name="node"/> as an <see cref="EntryState"/>
    /// </summary>
    /// <param name="node"></param>
    public void AddEntryState(StateNode node)
    {
        node.type = stateType.Entry;
        states.Add(node);
        EntryState = node;
    }

    /// <summary>
    /// Convert <paramref name="node"/> to <see cref="EntryState"/>
    /// </summary>
    /// <param name="node"></param>
    public void SetAsEntry(StateNode node)
    {
        //Previous Entry Node set to Unconnected
        if (EntryState)
            EntryState.type = stateType.Unconnected;

        node.type = stateType.Entry;
        EntryState = node;

        CheckConnected();
    }

    /// <summary>
    /// Check if <paramref name="node"/> is the <see cref="EntryState"/>
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool isEntryState(StateNode node)
    {
        return node.Equals(EntryState);
    }

    /// <summary>
    /// Draws all <see cref="transitions"/> curves for the <see cref="FSM"/>
    /// </summary>
    public void DrawCurves()
    {
        foreach (TransitionGUI elem in transitions)
        {
            if (elem.fromNode is null || elem.toNode is null)
                break;

            bool isDouble = false;

            Rect fromNodeRect = new Rect(elem.fromNode.windowRect);
            Rect toNodeRect = new Rect(elem.toNode.windowRect);

            if (transitions.Exists(t => t.fromNode.Equals(elem.toNode) && t.toNode.Equals(elem.fromNode)))
            {
                isDouble = true;
            }

            NodeEditor.DrawNodeCurve(fromNodeRect, toNodeRect, editor.focusedObjects.Contains(elem), isDouble);
        }
    }

    /// <summary>
    /// Delete <paramref name="node"/> and all <see cref="TransitionGUI"/> connected to it
    /// </summary>
    /// <param name="node"></param>
    public void DeleteNode(StateNode node)
    {
        if (states.Remove(node))
        {
            for (int i = 0; i < node.nodeTransitions.Count; i++)
            {
                DeleteTransition(node.nodeTransitions[i]);
            }

            // Notify all nodes that this node has been deleted, so they can delete the necessary transition references
            foreach (StateNode n in states)
            {
                n.NodeDeleted(node);
            }

            if (node.subElem == null)
                elementNamer.RemoveName(node.identificator);
            else
                elementNamer.RemoveName(node.subElem.identificator);
        }
    }

    /// <summary>
    /// Delete <paramref name="transition"/>
    /// </summary>
    /// <param name="transition"></param>
    public void DeleteTransition(TransitionGUI transition)
    {
        if (transitions.Remove(transition))
        {
            foreach (StateNode n in states)
            {
                n.nodeTransitions.Remove(transition);
            }

            CheckConnected();

            elementNamer.RemoveName(transition.identificator);
        }
    }

    /// <summary>
    /// Recalculate every <see cref="StateNode"/>'s state of connection to the <see cref="EntryState"/>
    /// </summary>
    /// <param name="baseNode"></param>
    public void CheckConnected(StateNode baseNode = null)
    {
        if (baseNode == null)
        {
            baseNode = EntryState;

            foreach (StateNode elem in states.FindAll(o => o.type != stateType.Entry))
            {
                elem.type = stateType.Unconnected;
            }

            if (!states.Contains(baseNode))
                return;
        }
        else if (baseNode.type == stateType.Unconnected)
        {
            baseNode.type = stateType.Default;
        }
        else
        {
            return;
        }

        foreach (TransitionGUI elem in baseNode.nodeTransitions.FindAll(o => o.fromNode.Equals(baseNode)))
        {
            CheckConnected((StateNode)elem.toNode);
        }
    }

    /// <summary>
    /// Add <paramref name="newTransition"/> to the <see cref="FSM"/>
    /// </summary>
    /// <param name="newTransition"></param>
    public void AddTransition(TransitionGUI newTransition)
    {
        transitions.Add(newTransition);

        ((StateNode)newTransition.fromNode).nodeTransitions.Add(newTransition);
        ((StateNode)newTransition.toNode).nodeTransitions.Add(newTransition);

        CheckConnected();
    }

    /// <summary>
    /// Returns the list of <see cref="ClickableElement"/> that exist inside each <see cref="StateNode"/> of this <see cref="FSM"/> 
    /// </summary>
    /// <returns>The list of <see cref="ClickableElement"/> that exist inside each <see cref="StateNode"/> of this <see cref="FSM"/></returns>
    public override List<ClickableElement> GetSubElems()
    {
        List<ClickableElement> result = new List<ClickableElement>();

        foreach (StateNode node in states)
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
