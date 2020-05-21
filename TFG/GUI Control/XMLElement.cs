using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public StateNode ToStateNode()
    {
        StateNode node = ScriptableObject.CreateInstance<StateNode>();
        node.identificator = this.Id;
        node.nodeName = this.name;
        node.windowRect = new Rect(this.windowPosX, this.windowPosY, StateNode.width, StateNode.height);
        node.type = stateType.Unconnected;

        return node;
    }

    public FSM ToFSM(ClickableElement parent, BaseNode selectedNode = null)
    {
        FSM fsm = ScriptableObject.CreateInstance<FSM>();
        fsm.identificator = this.Id;
        fsm.elementName = this.name;
        fsm.windowRect = new Rect(this.windowPosX, this.windowPosY, FSM.width, FSM.height);
        fsm.type = ClickableElement.elementType.FSM;
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
            node.InitStateNode(2, fsm.windowRect.position.x, fsm.windowRect.position.y, fsm);
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
            node.InitBehaviourNode(2, fsm.windowRect.x, fsm.windowRect.y, fsm);

            ((BehaviourTree)parent).nodes.Add(node);

            if (selectedNode != null)
            {
                TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
                transition.InitTransitionGUI("", selectedNode, node);

                ((BehaviourTree)parent).connections.Add(transition);

                selectedNode = node;
            }
        }

        return fsm;
    }

    public BehaviourTree ToBehaviourTree(ClickableElement parent, BaseNode selectedNode = null)
    {
        BehaviourTree bt = ScriptableObject.CreateInstance<BehaviourTree>();
        bt.identificator = this.Id;
        bt.elementName = this.name;
        bt.windowRect = new Rect(this.windowPosX, this.windowPosY, BehaviourTree.width, BehaviourTree.height);
        bt.type = ClickableElement.elementType.BT;
        bt.parent = parent;

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
            node.InitStateNode(2, bt.windowRect.position.x, bt.windowRect.position.y, bt);
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
            node.InitBehaviourNode(2, bt.windowRect.x, bt.windowRect.y, bt);

            ((BehaviourTree)parent).nodes.Add(node);

            if (selectedNode != null)
            {
                TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
                transition.InitTransitionGUI("", selectedNode, node);

                ((BehaviourTree)parent).connections.Add(transition);

                selectedNode = node;
            }
        }

        return bt;
    }

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
                nodeBT.InitBehaviourNode((int)Enum.Parse(typeof(BehaviourNode.behaviourType), this.secondType), this.windowPosX, this.windowPosY);
                nodeBT.nodeName = this.name;
                nodeBT.NProperty = this.NProperty;

                currentTree.nodes.Add(nodeBT);

                if (selectedNode)
                {
                    TransitionGUI transition = ScriptableObject.CreateInstance<TransitionGUI>();
                    transition.InitTransitionGUI("", selectedNode, nodeBT);

                    currentTree.connections.Add(transition);
                }
                else
                {
                    nodeBT.isRootNode = true;
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
