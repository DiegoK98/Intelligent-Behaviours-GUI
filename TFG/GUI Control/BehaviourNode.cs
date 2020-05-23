using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class BehaviourNode : BaseNode
{
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

    public ClickableElement subElem;

    public behaviourType type;

    public bool isRootNode { get; set; } = false;

    /// <summary>
    /// Gets the type of the element properly written
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
    /// The InitBehaviourNode
    /// </summary>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public void InitBehaviourNode(ClickableElement parent, int typeNumber, float posx, float posy, ClickableElement subElem = null)
    {
        InitBaseNode();

        type = (behaviourType)typeNumber;

        if (subElem != null)
        {
            this.subElem = subElem;
            nodeName = this.subElem.elementName;
            windowRect = new Rect(posx, posy, ClickableElement.width, ClickableElement.height);
        }
        else
        {
            nodeName = parent.elementNamer.GenerateUniqueName(identificator, "New " + type + " Node ");
            windowRect = new Rect(posx, posy, width, height);
        }
    }

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
                NProperty = this.NProperty,
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
