using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public enum utilityType
{
    Variable,
    Fusion,
    Action,

    // Curves
    LinearCurve,
    ExpCurve,
}

public class UtilityNode : BaseNode
{
    /// <summary>
    /// The type of <see cref="UtilityNode"/>
    /// </summary>
    public utilityType type;

    /// <summary>
    /// Min value for the Variable nodes
    /// </summary>
    public float variableMin;

    /// <summary>
    /// Max value for the Variable nodes
    /// </summary>
    public float variableMax;

    /// <summary>
    /// Returns the <see cref="utilityType"/> properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        if (subElem is null)
            return type.ToString();
        else
            return subElem.GetTypeString();
    }

    /// <summary>
    /// The Initializer for the <seealso cref="UtilityNode"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="subElem"></param>
    public void InitUtilityNode(ClickableElement parent, int typeNumber, float posx, float posy, ClickableElement subElem = null)
    {
        InitBaseNode();

        type = (utilityType)typeNumber;

        if (subElem != null)
        {
            this.subElem = subElem;
            nodeName = this.subElem.elementName;
            windowRect = new Rect(posx, posy, ClickableElement.width, ClickableElement.height);
        }
        else
        {
            nodeName = parent.elementNamer.AddName(identificator, "New " + type + " Node ");
            windowRect = new Rect(posx, posy, width, height);
        }
    }

    /// <summary>
    /// Draws all the elements inside the <see cref="UtilityNode"/>
    /// </summary>
    public override void DrawWindow()
    {
        switch (type)
        {
            case utilityType.Variable:
                nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                GUILayout.Space(15);
                GUILayout.BeginHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Min:", Styles.SubTitleText, GUILayout.Width(40), GUILayout.Height(25));
                float.TryParse(EditorGUILayout.TextArea(variableMin.ToString(), Styles.TitleText, GUILayout.ExpandWidth(false), GUILayout.Height(20)), out variableMin);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max:", Styles.SubTitleText, GUILayout.Width(40), GUILayout.Height(25));
                float.TryParse(EditorGUILayout.TextArea(variableMax.ToString(), Styles.TitleText, GUILayout.ExpandWidth(false), GUILayout.Height(20)), out variableMax);
                GUILayout.EndHorizontal();

                GUILayout.EndHorizontal();
                break;
            case utilityType.Fusion:
                // TODO
                break;
            case utilityType.Action:
                nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                break;
        }
    }

    // TODO
    /// <summary>
    /// Creates and returns an <see cref="XMLElement"/> that corresponds to this <see cref="UtilityNode"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The <see cref="XMLElement"/> corresponding to this <see cref="BehaviourNode"/></returns>
    public override XMLElement ToXMLElement(params object[] args)
    {
        BehaviourTree parentTree = (BehaviourTree)args[0];

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
                //isRandom = this.isRandom,
                //NProperty = this.NProperty,

                nodes = parentTree.connections.FindAll(o => this.Equals(o.fromNode)).Select(o => o.toNode).Cast<BehaviourNode>().ToList().ConvertAll((node) =>
                {
                    return node.ToXMLElement(parentTree);
                }),
            };
        }

        result.Id = this.identificator;
        result.secondType = this.type.ToString();

        return result;
    }
}
