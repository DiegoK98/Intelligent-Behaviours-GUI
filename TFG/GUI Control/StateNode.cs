using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class StateNode : BaseNode
{
    /// <summary>
    /// Current status of this <see cref="State"/>
    /// </summary>
    public stateType type;

    /// <summary>
    /// List of <see cref="TransitionGUI"/> that are connected to this <see cref="StateNode"/>
    /// </summary>
    public List<TransitionGUI> nodeTransitions = new List<TransitionGUI>();

    /// <summary>
    /// The Initializer for the <seealso cref="StateNode"/>
    /// </summary>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public void InitStateNode(ClickableElement parent, int typeNumber, float posx, float posy, ClickableElement subElem = null)
    {
        InitBaseNode(parent);

        if (subElem != null)
        {
            this.subElem = subElem;
            nodeName = this.subElem.elementName;
            windowRect = new Rect(posx, posy, ClickableElement.width, ClickableElement.height);
        }
        else
        {
            nodeName = parent.elementNamer.AddName(identificator, "New State ");
            windowRect = new Rect(posx, posy, width, height);
        }

        type = (stateType)typeNumber;
    }

    /// <summary>
    /// Draws all the elements inside the <see cref="StateNode"/>
    /// </summary>
    public override void DrawWindow()
    {
        nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
    }

    /// <summary>
    /// Creates and returns an <see cref="XMLElement"/> that corresponds to this <see cref="StateNode"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override XMLElement ToXMLElement(params object[] args)
    {
        XMLElement result;

        if (this.subElem)
        {
            result = this.subElem.ToXMLElement();
        }
        else
        {
            result = new XMLElement
            {
                name = CleanName(this.nodeName),
                elemType = this.GetType().ToString(),
                windowPosX = this.windowRect.x,
                windowPosY = this.windowRect.y,
            };
        }

        result.Id = this.identificator;
        result.secondType = this.type.ToString();

        return result;
    }

    /// <summary>
    /// Returns the <see cref="stateType"/> properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        if (subElem is null)
            return "Node";
        else
            return subElem.GetTypeString();
    }

    /// <summary>
    /// Removes the references to all <see cref="TransitionGUI"/> that were connected to the <paramref name="deletedNode"/>
    /// </summary>
    /// <param name="deletedNode"></param>
    public void NodeDeleted(StateNode deletedNode)
    {
        for (int i = 0; i < nodeTransitions.Count; i++)
        {
            if (deletedNode.Equals(nodeTransitions[i].toNode) || deletedNode.Equals(nodeTransitions[i].fromNode))
            {
                nodeTransitions.Remove(nodeTransitions[i]);
            }
        }
    }
}
