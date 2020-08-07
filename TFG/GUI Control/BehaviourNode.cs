using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Globalization;

public enum behaviourType
{
    Sequence,
    Selector,
    Leaf,
    LoopN,
    LoopUntilFail,
    Inverter,
    DelayT,
    Succeeder,
    Conditional
}

public class BehaviourNode : BaseNode
{
    /// <summary>
    /// The type of <see cref="BehaviourNode"/>
    /// </summary>
    public behaviourType type;

    /// <summary>
    /// Parameter used for Decorator Nodes that are <see cref="behaviourType.DelayT"/>
    /// </summary>
    public float delayTime = 0.0f;

    /// <summary>
    /// Parameter used for Decorator Nodes that are <see cref="behaviourType.LoopN"/>
    /// </summary>
    public int Nloops = 0;

    /// <summary>
    /// True if this <see cref="BehaviourNode"/> is the root of the <see cref="BehaviourTree"/>
    /// </summary>
    public bool isRoot = false;

    /// <summary>
    /// Parameter for Sequence Nodes
    /// </summary>
    public bool isRandom = false;

    /// <summary>
    /// Returns the <see cref="behaviourType"/> properly written
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
    /// The Initializer for the <seealso cref="BehaviourNode"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="subElem"></param>
    public void InitBehaviourNode(ClickableElement parent, behaviourType type, float posx, float posy, ClickableElement subElem = null)
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
            nodeName = parent.elementNamer.AddName(identificator, "New " + type + " Node ");
            windowRect = new Rect(posx, posy, width, height);
        }
    }

    /// <summary>
    /// The Initializer for the <seealso cref="BehaviourNode"/> when it is being loaded from an XML
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    /// <param name="subElem"></param>
    public void InitBehaviourNodeFromXML(ClickableElement parent, behaviourType type, float posx, float posy, string id, string name, float delayTime, int Nloops, bool isRandom, ClickableElement subElem = null)
    {
        InitBaseNode(parent, id);

        this.type = type;

        if (subElem != null)
        {
            this.subElem = subElem;
            nodeName = this.subElem.elementName;
            windowRect = new Rect(posx, posy, ClickableElement.width, ClickableElement.height);
        }
        else
        {
            nodeName = parent.elementNamer.AddName(id, name);
            windowRect = new Rect(posx, posy, width, height);
        }

        this.delayTime = delayTime;
        this.Nloops = Nloops;
        this.isRandom = isRandom;
    }

    /// <summary>
    /// Draws all the elements inside the <see cref="BehaviourNode"/>
    /// </summary>
    public override void DrawWindow()
    {
        switch (type)
        {
            case behaviourType.Sequence:
                nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));

                GUILayout.BeginArea(new Rect(windowRect.width * 0.25f, windowRect.height - 20, windowRect.width * 0.5f, height * 0.3f));
                isRandom = GUILayout.Toggle(isRandom, "Random", new GUIStyle(GUI.skin.toggle) { alignment = TextAnchor.MiddleCenter });
                GUILayout.EndArea();
                break;
            case behaviourType.Selector:
            case behaviourType.Leaf:
                nodeName = CleanName(EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                break;
            case behaviourType.LoopN:
                int.TryParse(EditorGUILayout.TextField(Nloops.ToString(), Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)), out Nloops);
                break;
            case behaviourType.DelayT:
                float.TryParse(EditorGUILayout.TextField(delayTime.ToString(CultureInfo.CreateSpecificCulture("en-US")), Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)), NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out delayTime);
                break;
        }
    }

    /// <summary>
    /// Creates and returns an <see cref="XMLElement"/> that corresponds to this <see cref="BehaviourNode"/>
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
                isRandom = this.isRandom,
                delayTime = this.delayTime,
                Nloops = this.Nloops,

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
    /// Creates a copy of this <see cref="BehaviourNode"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override GUIElement CopyElement(params object[] args)
    {
        BehaviourTree parent = (BehaviourTree)args[0];

        GUIElement result = new BehaviourNode
        {
            identificator = this.identificator,
            nodeName = this.nodeName,
            parent = parent,
            type = this.type,
            windowRect = new Rect(this.windowRect),
            isRoot = this.isRoot,
            isRandom = this.isRandom,
            delayTime = this.delayTime,
            Nloops = this.Nloops
        };

        if (this.subElem)
        {
            ((BehaviourNode)result).subElem = (ClickableElement)this.subElem.CopyElement(parent);
        }

        return result;
    }
}
