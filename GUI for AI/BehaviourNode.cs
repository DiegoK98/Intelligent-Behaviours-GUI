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

    public ClickableElement elem;

    public behaviourType type;

    static int uniqueNameID = 0;

    /// <summary>
    /// Gets the type of the element properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        if (elem is null)
            return type.ToString();
        else
            return elem.GetTypeString();
    }

    /// <summary>
    /// The BehaviourNode
    /// </summary>
    /// <param name="typeNumber"></param>
    /// <param name="posx"></param>
    /// <param name="posy"></param>
    public BehaviourNode(int typeNumber, float posx, float posy) : base()
    {
        type = (behaviourType)typeNumber;
        nodeName = "New " + type + " Node " + uniqueNameID++;

        windowRect = new Rect(posx, posy, width, height);
    }

    /// <summary>
    /// The BehaviourNode with a clickable element in it
    /// </summary>
    /// <param name="typeNumber"></param>
    /// <param name="subElem"></param>
    public BehaviourNode(int typeNumber, ClickableElement subElem) : base()
    {
        type = (behaviourType)typeNumber;

        elem = subElem;

        nodeName = elem.elementName;
    }
}
