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

    public BehaviourNode(int typeNumber) : base()
    {
        type = (behaviourType)typeNumber;

        switch (typeNumber)
        {
            case 0:
                nodeName = "New Sequence";
                break;
            case 1:
                nodeName = "New Selector";
                break;
            case 2:
                nodeName = "New Leaf";
                break;
        }
    }
}
