using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMLElement
{
    /// General properties
    public string Id { get; set; }

    public string elemType { get; set; }

    public string secondType { get; set; } = "";

    public string name { get; set; }

    public float windowPosX { get; set; }

    public float windowPosY { get; set; }

    /// Decorator Nodes properties
    public int NProperty { get; set; }

    /// Transitions properties
    public string fromId { get; set; }

    public string toId { get; set; }

    public PerceptionXML perception { get; set; }

    /// Lists
    public List<XMLElement> nodes { get; set; }

    public List<XMLElement> transitions { get; set; }

    public TransitionGUI ToTransitionGUI(BaseNode from, BaseNode to)
    {
        TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
        transition.identificator = this.Id;
        transition.transitionName = this.name;
        transition.width = TransitionGUI.baseWidth;
        transition.height = TransitionGUI.baseHeight;
        transition.fromNode = from;
        transition.toNode = to;

        transition.rootPerception = this.perception.ToGUIElement();

        return transition;
    }
}
