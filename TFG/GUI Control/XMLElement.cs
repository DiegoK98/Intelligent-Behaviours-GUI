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
    /// Parameter for DelayT and LoopN Nodes
    /// </summary>
    public int NProperty { get; set; }

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

    // Conversion Methods

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
        transition.fromNode = from;
        transition.toNode = to;

        transition.rootPerception = this.perception.ToGUIElement();

        return transition;
    }

    /// <summary>
    /// Creates and returns the <see cref="StateNode"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <returns></returns>
    public StateNode ToStateNode()
    {
        StateNode node = ScriptableObject.CreateInstance<StateNode>();
        node.identificator = this.Id;
        node.nodeName = this.name;
        node.windowRect = new Rect(this.windowPosX, this.windowPosY, StateNode.width, StateNode.height);
        node.type = stateType.Unconnected;

        return node;
    }

    /// <summary>
    /// Creates and returns the <see cref="FSM"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="selectedNode"></param>
    /// <returns></returns>
    public FSM ToFSM(ClickableElement parent, BaseNode selectedNode = null)
    {
        FSM fsm = ScriptableObject.CreateInstance<FSM>();
        fsm.identificator = this.Id;
        fsm.elementName = this.name;
        fsm.windowRect = new Rect(this.windowPosX, this.windowPosY, FSM.width, FSM.height);
        fsm.parent = parent;

        foreach (XMLElement node in this.nodes)
        {
            switch (node.elemType)
            {
                case nameof(FSM):
                    node.ToFSM(fsm);
                    break;
                case nameof(BehaviourTree):
                    node.ToBehaviourTree(fsm);
                    break;
                case nameof(StateNode):
                    StateNode state = node.ToStateNode();

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

        return fsm;
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

        foreach (XMLElement node in this.nodes)
        {
            switch (node.elemType)
            {
                case nameof(FSM):
                    node.ToFSM(bt);
                    break;
                case nameof(BehaviourTree):
                    node.ToBehaviourTree(bt);
                    break;
                case nameof(BehaviourNode):
                    node.ToBehaviourNode(null, bt, parent);
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

        return bt;
    }

    /// <summary>
    /// Creates the <see cref="BehaviourNode"/> corresponding to this <see cref="XMLElement"/>
    /// </summary>
    /// <param name="selectedNode"></param>
    /// <param name="currentTree"></param>
    /// <param name="currentElement"></param>
    private void ToBehaviourNode(BaseNode selectedNode, BehaviourTree currentTree, ClickableElement currentElement)
    {
        switch (this.elemType)
        {
            case nameof(FSM):
                this.ToFSM(currentElement, selectedNode);
                break;
            case nameof(BehaviourTree):
                this.ToBehaviourTree(currentElement, selectedNode);
                break;
            case nameof(BehaviourNode):
                BehaviourNode nodeBT = ScriptableObject.CreateInstance<BehaviourNode>();
                nodeBT.InitBehaviourNode(currentTree, (int)Enum.Parse(typeof(BehaviourNode.behaviourType), this.secondType), this.windowPosX, this.windowPosY);
                nodeBT.nodeName = this.name;
                nodeBT.isRandom = this.isRandom;
                nodeBT.NProperty = this.NProperty;

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
                    childState.ToBehaviourNode(nodeBT, currentTree, currentTree);
                }
                break;
            default:
                Debug.LogError("Wrong content in saved data");
                break;
        }
    }
}
