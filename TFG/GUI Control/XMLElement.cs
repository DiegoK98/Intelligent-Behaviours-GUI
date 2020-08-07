using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class XMLElement
{
    // General properties

    /// <summary>
    /// Unique identificator
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The type of element
    /// </summary>
    public string elemType { get; set; }

    /// <summary>
    /// Auxiliar type used for when using only <see cref="elemType"/> is not enough
    /// </summary>
    public string secondType { get; set; } = "";

    /// <summary>
    /// Auxiliar type used for when using only <see cref="elemType"/> and <see cref="secondType"/> is not enough
    /// </summary>
    public string thirdType { get; set; } = "";

    /// <summary>
    /// Auxiliar type used for when using only <see cref="elemType"/>, <see cref="secondType"/> and <see cref="thirdType"/> is not enough
    /// </summary>
    public string fourthType { get; set; } = "";

    /// <summary>
    /// Name of the element
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// X position of the element window
    /// </summary>
    public float windowPosX { get; set; }

    /// <summary>
    /// Y position of the element window
    /// </summary>
    public float windowPosY { get; set; }

    // Clickable Elements properties

    /// <summary>
    /// List of nodes of the <see cref="ClickableElement"/>
    /// </summary>
    public List<XMLElement> nodes { get; set; }

    /// <summary>
    /// List of transitions of the <see cref="FSM"/>
    /// </summary>
    public List<XMLElement> transitions { get; set; }

    // Behaviour Nodes properties

    /// <summary>
    /// Parameter for Sequence Nodes
    /// </summary>
    public bool isRandom { get; set; }

    /// <summary>
    /// Parameter for DelayT Nodes
    /// </summary>
    public float delayTime { get; set; }

    /// <summary>
    /// Parameter for LoopN Nodes
    /// </summary>
    public int Nloops { get; set; }

    // Utility Nodes properties

    /// <summary>
    /// Parameter for Variable nodes
    /// </summary>
    public float variableMin { get; set; }

    /// <summary>
    /// Parameter for Variable nodes
    /// </summary>
    public float variableMax { get; set; }

    /// <summary>
    /// Parameter for Curve nodes
    /// </summary>
    public float slope { get; set; }

    /// <summary>
    /// Parameter for Curve nodes
    /// </summary>
    public float exp { get; set; }

    /// <summary>
    /// Parameter for Curve nodes
    /// </summary>
    public float displX { get; set; }

    /// <summary>
    /// Parameter for Curve nodes
    /// </summary>
    public float displY { get; set; }

    // Transitions properties

    /// <summary>
    /// Identificator of the <see cref="TransitionGUI.fromNode"/>
    /// </summary>
    public string fromId { get; set; }

    /// <summary>
    /// Identificator of the <see cref="TransitionGUI.toNode"/>
    /// </summary>
    public string toId { get; set; }

    /// <summary>
    /// The <see cref="PerceptionXML"/> of this <see cref="TransitionGUI"/>'s perception
    /// </summary>
    public PerceptionXML perception { get; set; }

    /// <summary>
    /// Weight for weighted Fusion nodes
    /// </summary>
    public float weight { get; set; }

    // Conversion Methods

    /// <summary>
    /// Creates and returns the <see cref="FSM"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="selectedNode"></param>
    /// <returns></returns>
    public FSM ToFSM(ClickableElement parent, BaseNode selectedNode = null, NodeEditor sender = null)
    {
        FSM fsm = ScriptableObject.CreateInstance<FSM>();
        fsm.InitFSM(sender, parent, this.windowPosX, this.windowPosY, true);
        fsm.identificator = this.Id;
        fsm.elementName = this.name;

        foreach (XMLElement node in this.nodes)
        {
            switch (node.elemType)
            {
                case nameof(FSM):
                    node.ToFSM(fsm, null, sender);
                    break;
                case nameof(BehaviourTree):
                    node.ToBehaviourTree(fsm, null, sender);
                    break;
                case nameof(UtilitySystem):
                    node.ToUtilitySystem(fsm, null, sender);
                    break;
                case nameof(StateNode):
                    StateNode state = node.ToStateNode(fsm);

                    if (node.secondType.Equals(stateType.Entry.ToString()))
                    {
                        fsm.AddEntryState(state);
                    }
                    else
                    {
                        fsm.states.Add(state);
                    }
                    break;
                default:
                    Debug.LogError("Wrong content in saved data");
                    break;
            }
        }

        foreach (XMLElement trans in this.transitions)
        {
            BaseNode node1 = fsm.states.Where(n => n.identificator == trans.fromId).FirstOrDefault();
            BaseNode node2 = fsm.states.Where(n => n.identificator == trans.toId).FirstOrDefault();
            if (node1 != null && node2 != null)
                fsm.AddTransition(trans.ToTransitionGUI(node1, node2));
        }

        if (parent is FSM)
        {
            StateNode node = ScriptableObject.CreateInstance<StateNode>();
            node.InitStateNode(parent, 2, fsm.windowRect.position.x, fsm.windowRect.position.y, fsm);
            node.identificator = this.Id;

            if (this.secondType.Equals(stateType.Entry.ToString()))
            {
                ((FSM)parent).AddEntryState(node);
            }
            else
            {
                ((FSM)parent).states.Add(node);
            }
        }

        if (parent is BehaviourTree)
        {
            BehaviourNode node = ScriptableObject.CreateInstance<BehaviourNode>();
            node.InitBehaviourNode(parent, 2, fsm.windowRect.x, fsm.windowRect.y, fsm);

            ((BehaviourTree)parent).nodes.Add(node);

            if (selectedNode != null)
            {
                TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
                transition.InitTransitionGUI(parent, selectedNode, node);

                ((BehaviourTree)parent).connections.Add(transition);

                selectedNode = node;
            }
        }

        if (parent is UtilitySystem)
        {
            UtilityNode node = ScriptableObject.CreateInstance<UtilityNode>();
            node.InitUtilityNode(sender, parent, utilityType.Action, fsm.windowRect.position.x, fsm.windowRect.position.y, fsm);
            node.identificator = this.Id;

            ((UtilitySystem)parent).nodes.Add(node);
        }

        return fsm;
    }

    /// <summary>
    /// Creates and returns the <see cref="StateNode"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <returns></returns>
    public StateNode ToStateNode(FSM parent)
    {
        StateNode node = ScriptableObject.CreateInstance<StateNode>();
        node.identificator = this.Id;
        node.parent = parent;
        node.nodeName = this.name;
        node.windowRect = new Rect(this.windowPosX, this.windowPosY, StateNode.width, StateNode.height);
        node.type = stateType.Unconnected;

        parent.elementNamer.AddName(node.identificator, node.nodeName);

        return node;
    }

    /// <summary>
    /// Creates and returns the <see cref="TransitionGUI"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public TransitionGUI ToTransitionGUI(BaseNode from, BaseNode to)
    {
        TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
        transition.identificator = this.Id;
        transition.transitionName = this.name;
        transition.width = TransitionGUI.baseWidth;
        transition.height = TransitionGUI.baseHeight;
        transition.weight = this.weight;
        transition.fromNode = from;
        transition.toNode = to;

        transition.rootPerception = this.perception.ToGUIElement();

        return transition;
    }

    /// <summary>
    /// Creates and returns the <see cref="BehaviourTree"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="selectedNode"></param>
    /// <param name="sender"></param>
    /// <returns></returns>
    public BehaviourTree ToBehaviourTree(ClickableElement parent, BaseNode selectedNode = null, NodeEditor sender = null)
    {
        BehaviourTree bt = ScriptableObject.CreateInstance<BehaviourTree>();
        bt.InitBehaviourTree(sender, parent, this.windowPosX, this.windowPosY);
        bt.identificator = this.Id;
        bt.elementName = this.name;

        foreach (XMLElement node in this.nodes)
        {
            switch (node.elemType)
            {
                case nameof(FSM):
                    node.ToFSM(bt, null, sender);
                    break;
                case nameof(BehaviourTree):
                    node.ToBehaviourTree(bt, null, sender);
                    break;
                case nameof(UtilitySystem):
                    node.ToUtilitySystem(bt, null, sender);
                    break;
                case nameof(BehaviourNode):
                    node.ToBehaviourNode(null, bt, parent, sender);
                    break;
                default:
                    Debug.LogError("Wrong content in saved data");
                    break;
            }
        }

        if (parent is FSM)
        {
            StateNode node = ScriptableObject.CreateInstance<StateNode>();
            node.InitStateNode(parent, 2, bt.windowRect.position.x, bt.windowRect.position.y, bt);
            node.identificator = this.Id;

            if (this.secondType.Equals(stateType.Entry.ToString()))
            {
                ((FSM)parent).AddEntryState(node);
            }
            else
            {
                ((FSM)parent).states.Add(node);
            }
        }

        if (parent is BehaviourTree)
        {
            BehaviourNode node = ScriptableObject.CreateInstance<BehaviourNode>();
            node.InitBehaviourNode(parent, 2, bt.windowRect.x, bt.windowRect.y, bt);

            ((BehaviourTree)parent).nodes.Add(node);

            if (selectedNode != null)
            {
                TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
                transition.InitTransitionGUI(parent, selectedNode, node);

                ((BehaviourTree)parent).connections.Add(transition);

                selectedNode = node;
            }
        }

        if (parent is UtilitySystem)
        {
            UtilityNode node = ScriptableObject.CreateInstance<UtilityNode>();
            node.InitUtilityNode(sender, parent, utilityType.Action, bt.windowRect.position.x, bt.windowRect.position.y, bt);
            node.identificator = this.Id;

            ((UtilitySystem)parent).nodes.Add(node);
        }

        return bt;
    }

    /// <summary>
    /// Creates the <see cref="BehaviourNode"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <param name="selectedNode"></param>
    /// <param name="currentTree"></param>
    /// <param name="currentElement"></param>
    public void ToBehaviourNode(BaseNode selectedNode, BehaviourTree currentTree, ClickableElement currentElement, NodeEditor sender = null)
    {
        switch (this.elemType)
        {
            case nameof(FSM):
                this.ToFSM(currentElement, selectedNode, sender);
                break;
            case nameof(BehaviourTree):
                this.ToBehaviourTree(currentElement, selectedNode, sender);
                break;
            case nameof(BehaviourNode):
                BehaviourNode nodeBT = ScriptableObject.CreateInstance<BehaviourNode>();
                nodeBT.InitBehaviourNode(currentTree, (int)Enum.Parse(typeof(behaviourType), this.secondType), this.windowPosX, this.windowPosY);
                nodeBT.nodeName = this.name;
                nodeBT.isRandom = this.isRandom;
                nodeBT.delayTime = this.delayTime;
                nodeBT.Nloops = this.Nloops;

                currentTree.nodes.Add(nodeBT);

                if (selectedNode)
                {
                    TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
                    transition.InitTransitionGUI(currentTree, selectedNode, nodeBT);

                    currentTree.connections.Add(transition);
                }
                else
                {
                    nodeBT.isRoot = true;
                }

                foreach (XMLElement childState in this.nodes)
                {
                    childState.ToBehaviourNode(nodeBT, currentTree, currentTree, sender);
                }
                break;
            default:
                Debug.LogError("Wrong content in saved data");
                break;
        }
    }

    /// <summary>
    /// Creates and returns the <see cref="FSM"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="selectedNode"></param>
    /// <returns></returns>
    public UtilitySystem ToUtilitySystem(ClickableElement parent, BaseNode selectedNode = null, NodeEditor sender = null)
    {
        UtilitySystem utilSystem = ScriptableObject.CreateInstance<UtilitySystem>();
        utilSystem.InitUtilitySystem(sender, parent, this.windowPosX, this.windowPosY);
        utilSystem.identificator = this.Id;
        utilSystem.elementName = this.name;

        foreach (XMLElement node in this.nodes)
        {
            switch (node.elemType)
            {
                case nameof(FSM):
                    node.ToFSM(utilSystem, null, sender);
                    break;
                case nameof(BehaviourTree):
                    node.ToBehaviourTree(utilSystem, null, sender);
                    break;
                case nameof(UtilitySystem):
                    node.ToUtilitySystem(utilSystem, null, sender);
                    break;
                case nameof(UtilityNode):
                    UtilityNode state = node.ToUtilityNode(utilSystem);

                    utilSystem.nodes.Add(state);
                    break;
                default:
                    Debug.LogError("Wrong content in saved data");
                    break;
            }
        }

        foreach (XMLElement trans in this.transitions)
        {
            BaseNode node1 = utilSystem.nodes.Where(n => n.identificator == trans.fromId).FirstOrDefault();
            BaseNode node2 = utilSystem.nodes.Where(n => n.identificator == trans.toId).FirstOrDefault();
            if (node1 != null && node2 != null)
                utilSystem.connections.Add(trans.ToTransitionGUI(node1, node2));
        }

        if (parent is FSM)
        {
            StateNode node = ScriptableObject.CreateInstance<StateNode>();
            node.InitStateNode(parent, 2, utilSystem.windowRect.position.x, utilSystem.windowRect.position.y, utilSystem);
            node.identificator = this.Id;

            if (this.secondType.Equals(stateType.Entry.ToString()))
            {
                ((FSM)parent).AddEntryState(node);
            }
            else
            {
                ((FSM)parent).states.Add(node);
            }
        }

        if (parent is BehaviourTree)
        {
            BehaviourNode node = ScriptableObject.CreateInstance<BehaviourNode>();
            node.InitBehaviourNode(parent, 2, utilSystem.windowRect.x, utilSystem.windowRect.y, utilSystem);

            ((BehaviourTree)parent).nodes.Add(node);

            if (selectedNode != null)
            {
                TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
                transition.InitTransitionGUI(parent, selectedNode, node);

                ((BehaviourTree)parent).connections.Add(transition);

                selectedNode = node;
            }
        }

        if (parent is UtilitySystem)
        {
            UtilityNode node = ScriptableObject.CreateInstance<UtilityNode>();
            node.InitUtilityNode(sender, parent, utilityType.Action, utilSystem.windowRect.position.x, utilSystem.windowRect.position.y, utilSystem);
            node.identificator = this.Id;

            ((UtilitySystem)parent).nodes.Add(node);
        }

        return utilSystem;
    }

    /// <summary>
    /// Creates and returns the <see cref="StateNode"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <returns></returns>
    public UtilityNode ToUtilityNode(UtilitySystem parent)
    {
        UtilityNode node = ScriptableObject.CreateInstance<UtilityNode>();
        node.identificator = this.Id;
        node.parent = parent;
        node.nodeName = this.name;
        node.type = (utilityType)Enum.Parse(typeof(utilityType), this.secondType);
        node.fusionType = (fusionType)Enum.Parse(typeof(fusionType), this.thirdType);
        node.curveType = (curveType)Enum.Parse(typeof(curveType), this.fourthType);

        if (node.type == utilityType.Fusion)
        {
            node.windowRect = new Rect(windowPosX, windowPosY, BaseNode.width, BaseNode.height * 1.7f);
        }
        else if (node.type == utilityType.Curve)
        {
            node.windowRect = new Rect(windowPosX, windowPosY, BaseNode.width, BaseNode.height * 1.5f);
        }
        else
        {
            node.windowRect = new Rect(windowPosX, windowPosY, BaseNode.width, BaseNode.height);
        }

        node.variableMax = this.variableMax;
        node.variableMin = this.variableMin;
        node.slope = this.slope;
        node.exp = this.exp;
        node.displX = this.displX;
        node.displY = this.displY;

        parent.elementNamer.AddName(node.identificator, node.nodeName);

        return node;
    }
}
