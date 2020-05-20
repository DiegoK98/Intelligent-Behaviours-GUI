using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

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

    static int uniqueNameID = 0;

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
    public void InitBehaviourNode(int typeNumber, float posx, float posy, ClickableElement subElem = null)
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
            nodeName = "New " + type + " Node " + uniqueNameID++;
            windowRect = new Rect(posx, posy, width, height);
        }
    }

    // Unused
    public override XMLElement ToXMLElement()
    {
        XMLElement result = new XMLElement
        {
            name = this.subElem ? CleanName(this.subElem.elementName) : CleanName(this.nodeName),
            elemType = this.subElem ? this.subElem.GetType().ToString() : this.GetType().ToString(),
            windowPosX = this.subElem ? this.subElem.windowRect.x : this.windowRect.x,
            windowPosY = this.subElem ? this.subElem.windowRect.y : this.windowRect.y,
            nodes = new List<XMLElement>(),
            transitions = new List<XMLElement>(),
            Id = this.identificator,
            secondType = this.type.ToString()
        };

        return result;
    }
}
