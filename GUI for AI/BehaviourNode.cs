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
        Leaf
    }

    public behaviourType type;

    static int uniqueNameID = 0;

    public BehaviourNode(int typeNumber) : base()
    {
        type = (behaviourType)typeNumber;

        nodeName = "New " + type + " Node " + uniqueNameID++;
    }
}
