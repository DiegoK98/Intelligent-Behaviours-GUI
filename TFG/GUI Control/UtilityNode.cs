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
    Curve
}

public enum fusionType
{
    Weighted,
    GetMax,
    GetMin
}

public enum curveType
{
    Linear,
    Exponential
}

public class UtilityNode : BaseNode
{
    /// <summary>
    /// The type of <see cref="UtilityNode"/>
    /// </summary>
    public utilityType type;

    /// <summary>
    /// The type of fusion if this <see cref="UtilityNode"/> is of type Fusion
    /// </summary>
    public fusionType fusionType;

    /// <summary>
    /// The type of curve if this <see cref="UtilityNode"/> is of type Curve
    /// </summary>
    public curveType curveType;

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
    public void InitUtilityNode(ClickableElement parent, utilityType type, float posx, float posy, ClickableElement subElem = null)
    {
        InitBaseNode(parent);

        this.type = type;

        if (subElem != null)
        {
            this.subElem = subElem;
            nodeName = this.subElem.elementName;
            windowRect = new Rect(posx, posy, ClickableElement.width, ClickableElement.height);
        }
        else
        {
            string nameToAdd = "New " + type;
            if (type != utilityType.Variable)
                nameToAdd += " Node ";
            nodeName = parent.elementNamer.AddName(identificator, nameToAdd);

            if (type == utilityType.Fusion)
            {
                windowRect = new Rect(posx, posy, width, height * 1.7f);
            }
            else
            {
                windowRect = new Rect(posx, posy, width, height);
            }
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
                GUILayout.Space(10);
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
                nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                GUILayout.BeginHorizontal();
                GUILayout.Space(windowRect.width * 0.2f);
                GUILayout.BeginVertical();
                if (GUILayout.Toggle(fusionType == fusionType.Weighted, fusionType.Weighted.ToString(), EditorStyles.radioButton))
                    fusionType = fusionType.Weighted;
                if (GUILayout.Toggle(fusionType == fusionType.GetMax, fusionType.GetMax.ToString(), EditorStyles.radioButton))
                    fusionType = fusionType.GetMax;
                if (GUILayout.Toggle(fusionType == fusionType.GetMin, fusionType.GetMin.ToString(), EditorStyles.radioButton))
                    fusionType = fusionType.GetMin;
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                break;
            case utilityType.Action:
                nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                break;
            case utilityType.Curve:
                nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                GUILayout.BeginHorizontal();
                GUILayout.Space(windowRect.width * 0.2f);
                if (GUILayout.Button(curveType.ToString(), EditorStyles.toolbarDropDown))
                {
                    GenericMenu toolsMenu = new GenericMenu();

                    foreach (string name in Enum.GetNames(typeof(curveType)).ToArray())
                    {
                        toolsMenu.AddItem(new GUIContent(name), false, () =>
                        {
                            curveType = (curveType)Enum.Parse(typeof(curveType), name);
                        });
                    }

                    toolsMenu.DropDown(new Rect(0, Event.current.mousePosition.y, 0, 0));
                    EditorGUIUtility.ExitGUI();
                }
                GUILayout.Space(windowRect.width * 0.2f);
                GUILayout.EndHorizontal();
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

    /// <summary>
    /// Creates a copy of this <see cref="UtilityNode"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override GUIElement CopyElement(params object[] args)
    {
        UtilitySystem parent = (UtilitySystem)args[0];

        GUIElement result;
        if (this.subElem)
        {
            result = this.subElem.CopyElement(parent);
        }
        else
        {
            result = new UtilityNode
            {
                identificator = this.identificator,
                nodeName = this.nodeName,
                parent = parent,
                windowRect = new Rect(this.windowRect),
                type = this.type,
                fusionType = this.fusionType,
                curveType = this.curveType,
                variableMax = this.variableMax,
                variableMin = this.variableMin
            };
        }

        return result;
    }

    /// <summary>
    /// Updates the value of the weights accordingly
    /// </summary>
    public void WeightsUpdate(string id)
    {
        List<TransitionGUI> weightedTransitions = ((UtilitySystem)parent).connections.Where(t => t.toNode.Equals(this)).ToList();
        float sumOfWeights = weightedTransitions.Sum(t => t.weight);

        if (sumOfWeights != 1)
        {
            foreach (TransitionGUI transition in weightedTransitions.Where(t => t.identificator != id))
            {
                transition.weight += (1 - sumOfWeights) * transition.weight / weightedTransitions.Where(t => t.identificator != id).Sum(t => t.weight);

                transition.weight = (float)decimal.Round((decimal)transition.weight, 2);
            }
        }
    }

    // TODO Ordenar la lista para que cuadre con la lista de factors asociada
    /// <summary>
    /// Returns a list of all the weights associated with this Fusion node
    /// </summary>
    /// <returns></returns>
    public List<float> GetWeightsAndFactors()
    {
        List<float> weights = ((UtilitySystem)parent).connections.Where(t => t.toNode.Equals(this)).Select(t => t.weight).ToList();

        return weights;
    }
}
