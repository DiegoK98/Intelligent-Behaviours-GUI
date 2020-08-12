using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class NodeEditor : EditorWindow
{
    /// <summary>
    /// Name of the window that will show on top
    /// </summary>
    const string editorTitle = "Intelligent Behaviours GUI";

    /// <summary>
    /// Stores the position of the cursor in every GUI iteration
    /// </summary>
    private Vector2 mousePos;

    /// <summary>
    /// Stores the <see cref="BaseNode"/> that is currently being used for <see cref="makeTransitionMode"/>, <see cref="makeAttachedNode"/> and <see cref="makeConnectionMode"/>
    /// </summary>
    private BaseNode selectednode;

    /// <summary>
    /// Stores the <see cref="TransitionGUI"/> that is currently being reconnected
    /// </summary>
    private TransitionGUI selectedTransition;

    /// <summary>
    /// The list of <see cref="GUIElement"/> that are currently selected by the user
    /// </summary>
    public List<GUIElement> focusedObjects = new List<GUIElement>();

    /// <summary>
    /// The list of copied <see cref="GUIElement"/> that will be pasted
    /// </summary>
    public List<GUIElement> clipboard = new List<GUIElement>();

    /// <summary>
    /// The list of references to every <see cref="GUIElement"/> that will be deleted after pasting the cut objects
    /// </summary>
    public List<GUIElement> cutObjects = new List<GUIElement>();

    /// <summary>
    /// The <see cref="ClickableElement"/> from which <see cref="cutObjects"/> were cut from
    /// </summary>
    public ClickableElement cutFromElement;

    /// <summary>
    /// The <see cref="BaseNode"/> that is being created in a <see cref="BehaviourTree"/> or <see cref="UtilitySystem"/>. Used to keep track of it while in <see cref="makeAttachedNode"/>
    /// </summary>
    private BaseNode toCreateNode;

    /// <summary>
    /// True when the user is in the process of creating a new <see cref="TransitionGUI"/>
    /// </summary>
    private bool makeTransitionMode = false;

    /// <summary>
    /// True when the user is in the process of reconnecting a <see cref="TransitionGUI"/>
    /// </summary>
    private bool reconnectTransitionMode = false;

    /// <summary>
    /// True when the user is in the process of creating a new <see cref="BaseNode"/> from the <see cref="selectednode"/>, so it gets attached
    /// </summary>
    private bool makeAttachedNode = false;

    /// <summary>
    /// True when the user is in the process of connecting a <see cref="BaseNode"/>
    /// </summary>
    private bool makeConnectionMode = false;

    /// <summary>
    /// List of <see cref="ClickableElement"/> in the editor
    /// </summary>
    public List<ClickableElement> Elements = new List<ClickableElement>();

    /// <summary>
    /// Active <see cref="ClickableElement"/> that is open
    /// </summary>
    public ClickableElement currentElem;

    /// <summary>
    /// The <see cref="UniqueNamer"/> for managing the names of the elements in the editor
    /// </summary>
    public UniqueNamer editorNamer;

    /// <summary>
    /// True if the <see cref="PopupWindow"/> is on screen
    /// </summary>
    public bool popupShown;

    /// <summary>
    /// Variable width of the top bar depending on the length of what is being displayed
    /// </summary>
    private float topBarOffset;

    /// <summary>
    /// Fixed offset for the <see cref="TransitionGUI.windowRect"/> when there's two of them in the same pair of nodes
    /// </summary>
    private static float pairTransitionsOffset = 20;

    private static bool CtrlDown = false;

    /// <summary>
    /// This will be called when the user opens the window
    /// </summary>
    [MenuItem("Window/" + editorTitle)]
    static void ShowEditor()
    {
        // We clear any UndoSteps from previous uses of the GUI
        NodeEditorUtilities.ClearUndoSteps();

        // Close any previous window
        GetWindow<PopupWindow>().Close();
        GetWindow<NodeEditor>().Close();

        // Open a new Editor Window
        // And reset the editorNamer
        GetWindow<NodeEditor>(editorTitle).editorNamer = CreateInstance<UniqueNamer>();
    }

    /// <summary>
    /// Called once every frame
    /// </summary>
    private void OnGUI()
    {
        Event e = Event.current;
        mousePos = e.mousePosition;

        // Show the GUI elements that are over the windows
        #region GUI overlay elements

        ShowTopBar();
        if (currentElem != null)
        {
            GUI.enabled = NodeEditorUtilities.undoStepsSaved > 0;
            if (GUI.Button(new Rect(position.width - 80, 0, 25, 20), "<-", Styles.TopBarButton))
            {
                Undo();
            }

            GUI.enabled = NodeEditorUtilities.redoStepsSaved > 0;
            if (GUI.Button(new Rect(position.width - 55, 0, 25, 20), "->", Styles.TopBarButton))
            {
                Redo();
            }

            GUI.enabled = true;
        }
        ShowOptions();
        ShowErrorsAndWarnings();

        #endregion

        // Draw the curves for everything
        #region Curves Drawing

        if ((makeTransitionMode || makeAttachedNode || makeConnectionMode) && selectednode != null)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            Rect nodeRect = new Rect(selectednode.windowRect);

            if ((currentElem is BehaviourTree && makeConnectionMode) || (currentElem is UtilitySystem && makeAttachedNode))
                DrawNodeCurve(mouseRect, nodeRect, true);
            else
                DrawNodeCurve(nodeRect, mouseRect, true);
        }

        if (reconnectTransitionMode)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            Rect nodeRect = new Rect(selectedTransition.fromNode.windowRect);

            if (!cutObjects.Contains(selectedTransition))
                cutObjects.Add(selectedTransition);

            DrawNodeCurve(nodeRect, mouseRect, true);
        }

        if (currentElem is FSM)
        {
            ((FSM)currentElem).DrawCurves();

            if (!((FSM)currentElem).HasEntryState)
            {
                currentElem.AddError(Error.NoEntryState);
            }
            else
            {
                currentElem.RemoveError(Error.NoEntryState);
            }

            if (((FSM)currentElem).states.Exists(n => n.type == stateType.Unconnected))
            {
                currentElem.AddWarning(Warning.UnconnectedNode);
            }
            else
            {
                currentElem.RemoveWarning(Warning.UnconnectedNode);
            }
        }

        if (currentElem is BehaviourTree)
        {
            ((BehaviourTree)currentElem).DrawCurves();

            if (((BehaviourTree)currentElem).nodes.Where(n => n.isRoot).Count() > 1)
            {
                currentElem.AddError(Error.MoreThanOneRoot);
            }
            else
            {
                currentElem.RemoveError(Error.MoreThanOneRoot);
            }
        }

        if (currentElem is UtilitySystem)
        {
            ((UtilitySystem)currentElem).DrawCurves();

            if (((UtilitySystem)currentElem).nodes.Exists(n => n.type == utilityType.Action && !((UtilitySystem)currentElem).connections.Exists(t => t.toNode.Equals(n))))
            {
                currentElem.AddWarning(Warning.NoFactors);
            }
            else
            {
                currentElem.RemoveWarning(Warning.NoFactors);
            }

            if (((UtilitySystem)currentElem).nodes.Exists(n => n.type == utilityType.Fusion && n.fusionType == fusionType.Weighted && ((UtilitySystem)currentElem).connections.Exists(t => t.toNode.Equals(n) && t.weight == 0)))
            {
                currentElem.AddWarning(Warning.WeightZero);
            }
            else
            {
                currentElem.RemoveWarning(Warning.WeightZero);
            }
        }

        #endregion

        // Control for the events called by the mouse and keyboard
        #region Mouse Click Control

        if (e.type == EventType.MouseDown)
        {
            // Check where it clicked
            int[] results = ClickedOnCheck();

            bool clickedOnElement = Convert.ToBoolean(results[0]);
            bool clickedOnWindow = Convert.ToBoolean(results[1]);
            bool clickedOnLeaf = Convert.ToBoolean(results[2]);
            bool clickedOnVariable = Convert.ToBoolean(results[3]);
            bool decoratorWithOneChild = Convert.ToBoolean(results[4]);
            bool actionWithOneFactor = Convert.ToBoolean(results[5]);
            bool curveWithOneFactor = Convert.ToBoolean(results[6]);
            bool nodeWithAscendants = Convert.ToBoolean(results[7]);
            bool clickedOnTransition = Convert.ToBoolean(results[8]);
            int selectIndex = results[9];

            // Click derecho
            if (e.button == 1)
            {
                if (!makeTransitionMode && !reconnectTransitionMode && !makeAttachedNode && !makeConnectionMode)
                {
                    if (!clickedOnElement && !clickedOnWindow && !clickedOnTransition)
                    {
                        focusedObjects.Clear();
                    }

                    // Set menu items
                    GenericMenu menu = new GenericMenu();

                    if (currentElem is FSM)
                    {
                        if (!clickedOnWindow && !clickedOnTransition)
                        {
                            menu.AddItem(new GUIContent("Add Node"), false, ContextCallback, new string[] { "Node", selectIndex.ToString() });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, new string[] { "FSM", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, new string[] { "BT", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add Utility System"), false, ContextCallback, new string[] { "US", selectIndex.ToString() });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Load Element from file"), false, LoadSaveFile);
                        }
                        else if (clickedOnWindow)
                        {
                            menu.AddItem(new GUIContent("Make Transition"), false, ContextCallback, new string[] { "makeTransition", selectIndex.ToString() });

                            if (!((FSM)currentElem).isEntryState(((FSM)currentElem).states[selectIndex]))
                            {
                                menu.AddSeparator("");
                                menu.AddItem(new GUIContent("Convert to Entry State"), false, ContextCallback, new string[] { "entryState", selectIndex.ToString() });
                            }

                            if (((FSM)currentElem).states[selectIndex].subElem != null)
                            {
                                menu.AddSeparator("");
                                menu.AddItem(new GUIContent("Save Element to file"), false, SaveElem, ((FSM)currentElem).states[selectIndex].subElem);
                                menu.AddItem(new GUIContent("Export Code"), false, ExportCode, ((FSM)currentElem).states[selectIndex].subElem);
                            }

                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, new string[] { "deleteNode", selectIndex.ToString() });
                        }
                        else if (clickedOnTransition)
                        {
                            menu.AddItem(new GUIContent("Reconnect Transition"), false, ContextCallback, new string[] { "reconnectTransition", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Flip Transition"), false, ContextCallback, new string[] { "flipTransition", selectIndex.ToString() });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Delete Transition"), false, ContextCallback, new string[] { "deleteTransition", selectIndex.ToString() });
                        }
                    }
                    else if (currentElem is BehaviourTree)
                    {
                        if (!clickedOnWindow && !clickedOnTransition)
                        {
                            if (((BehaviourTree)currentElem).nodes.Count == 0)
                            {
                                menu.AddItem(new GUIContent("Add Sequence"), false, ContextCallback, new string[] { "Sequence", selectIndex.ToString() });
                                menu.AddItem(new GUIContent("Add Selector"), false, ContextCallback, new string[] { "Selector", selectIndex.ToString() });
                                menu.AddSeparator("");
                            }
                            menu.AddItem(new GUIContent("Load Element from file"), false, LoadSaveFile);
                        }
                        else if (clickedOnWindow)
                        {
                            if (!clickedOnLeaf)
                            {
                                if (decoratorWithOneChild)
                                {
                                    menu.AddDisabledItem(new GUIContent("Add Sequence"));
                                    menu.AddDisabledItem(new GUIContent("Add Selector"));
                                    menu.AddSeparator("");
                                    menu.AddDisabledItem(new GUIContent("Add Leaf Node"));
                                    menu.AddDisabledItem(new GUIContent("Decorator Nodes/Add Loop (N)"));
                                    menu.AddDisabledItem(new GUIContent("Decorator Nodes/Add LoopU (Until Fail)"));
                                    menu.AddDisabledItem(new GUIContent("Decorator Nodes/Add Inverter"));
                                    menu.AddDisabledItem(new GUIContent("Decorator Nodes/Add Timer"));
                                    menu.AddDisabledItem(new GUIContent("Decorator Nodes/Add Succeeder"));
                                    menu.AddDisabledItem(new GUIContent("Decorator Nodes/Add Conditional"));
                                    menu.AddDisabledItem(new GUIContent("Add FSM"));
                                    menu.AddDisabledItem(new GUIContent("Add BT"));
                                    menu.AddDisabledItem(new GUIContent("Add Utility System"));
                                }
                                else
                                {
                                    menu.AddItem(new GUIContent("Add Sequence"), false, ContextCallback, new string[] { "Sequence", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Add Selector"), false, ContextCallback, new string[] { "Selector", selectIndex.ToString() });
                                    menu.AddSeparator("");
                                    menu.AddItem(new GUIContent("Add Leaf Node"), false, ContextCallback, new string[] { "leafNode", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Decorator Nodes/Add Loop (N)"), false, ContextCallback, new string[] { "loopN", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Decorator Nodes/Add LoopU (Until Fail)"), false, ContextCallback, new string[] { "loopUFail", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Decorator Nodes/Add Inverter"), false, ContextCallback, new string[] { "inverter", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Decorator Nodes/Add Timer"), false, ContextCallback, new string[] { "timer", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Decorator Nodes/Add Succeeder"), false, ContextCallback, new string[] { "succeeder", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Decorator Nodes/Add Conditional"), false, ContextCallback, new string[] { "conditional", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, new string[] { "FSM", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, new string[] { "BT", selectIndex.ToString() });
                                    menu.AddItem(new GUIContent("Add Utility System"), false, ContextCallback, new string[] { "US", selectIndex.ToString() });
                                }

                                menu.AddSeparator("");
                            }
                            else if (((BehaviourTree)currentElem).nodes[selectIndex].subElem != null)
                            {
                                menu.AddItem(new GUIContent("Save Element to file"), false, SaveElem, ((BehaviourTree)currentElem).nodes[selectIndex].subElem);
                                menu.AddItem(new GUIContent("Export Code"), false, ExportCode, ((BehaviourTree)currentElem).nodes[selectIndex].subElem);
                            }

                            if (nodeWithAscendants)
                                menu.AddItem(new GUIContent("Disconnect Node"), false, ContextCallback, new string[] { "disconnectNode", selectIndex.ToString() });
                            else
                                menu.AddItem(new GUIContent("Connect Node"), false, ContextCallback, new string[] { "connectNode", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, new string[] { "deleteNode", selectIndex.ToString() });
                        }
                    }
                    else if (currentElem is UtilitySystem)
                    {
                        if (!clickedOnWindow && !clickedOnTransition)
                        {
                            menu.AddItem(new GUIContent("Add Action"), false, ContextCallback, new string[] { "Action", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, new string[] { "FSM", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, new string[] { "BT", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add Utility System"), false, ContextCallback, new string[] { "US", selectIndex.ToString() });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Load Element from file"), false, LoadSaveFile);
                        }
                        else if (clickedOnWindow)
                        {
                            if (((UtilitySystem)currentElem).nodes[selectIndex].type != utilityType.Action)
                            {
                                menu.AddItem(new GUIContent("Connect Node"), false, ContextCallback, new string[] { "connectNode", selectIndex.ToString() });
                            }

                            if (!clickedOnVariable && !actionWithOneFactor && !curveWithOneFactor)
                            {
                                menu.AddItem(new GUIContent("Factors/Add Variable"), false, ContextCallback, new string[] { "Variable", selectIndex.ToString() });
                                menu.AddItem(new GUIContent("Factors/Add Fusion"), false, ContextCallback, new string[] { "Fusion", selectIndex.ToString() });
                                menu.AddItem(new GUIContent("Factors/Add Curve"), false, ContextCallback, new string[] { "Curve", selectIndex.ToString() });
                            }
                            else if (((UtilitySystem)currentElem).nodes[selectIndex].subElem != null)
                            {
                                menu.AddItem(new GUIContent("Save Element to file"), false, SaveElem, ((UtilitySystem)currentElem).nodes[selectIndex].subElem);
                                menu.AddItem(new GUIContent("Export Code"), false, ExportCode, ((UtilitySystem)currentElem).nodes[selectIndex].subElem);
                            }

                            menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, new string[] { "deleteNode", selectIndex.ToString() });
                        }
                    }
                    else if (currentElem is null)
                    {
                        if (!clickedOnElement)
                        {
                            menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, new string[] { "FSM", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, new string[] { "BT", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add Utility System"), false, ContextCallback, new string[] { "US", selectIndex.ToString() });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Load Element from file"), false, LoadSaveFile);
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Save Element to file"), false, SaveElem, Elements[selectIndex]);
                            menu.AddItem(new GUIContent("Export Code"), false, ExportCode, Elements[selectIndex]);
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Delete Element"), false, ContextCallback, new string[] { "deleteNode", selectIndex.ToString() });
                        }
                    }

                    if (focusedObjects.Count > 0)
                    {
                        menu.AddItem(new GUIContent("Cut"), false, Cut);
                        menu.AddItem(new GUIContent("Copy"), false, Copy);
                    }

                    if (clipboard.Count > 0)
                    {
                        menu.AddItem(new GUIContent("Paste"), false, Paste);
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Paste"));
                    }

                    menu.ShowAsContext();
                    e.Use();
                }
                //Click derecho estando en uno de estos dos modos, lo cancela
                else
                {
                    makeTransitionMode = false;
                    reconnectTransitionMode = false;
                    cutObjects.Remove(selectedTransition);
                    selectedTransition = null;
                    makeAttachedNode = false;
                    makeConnectionMode = false;
                }
            }

            // Click izquierdo
            else if (e.button == 0)
            {
                GUI.FocusControl(null);
                if (!CtrlDown)
                {
                    focusedObjects.Clear();
                }

                if (clickedOnElement)
                {
                    if (focusedObjects.Contains(Elements[selectIndex]))
                    {
                        focusedObjects.Remove(Elements[selectIndex]);
                    }
                    else
                    {
                        focusedObjects.Add(Elements[selectIndex]);
                    }

                    if (Event.current.clickCount == 2)
                    {
                        focusedObjects.Clear();
                        currentElem = Elements[selectIndex];
                        NodeEditorUtilities.ClearUndoSteps();
                        e.Use();
                    }
                }
                else if (clickedOnTransition)
                {
                    if (currentElem is FSM)
                    {
                        if (focusedObjects.Contains(((FSM)currentElem).transitions[selectIndex]))
                        {
                            focusedObjects.Remove(((FSM)currentElem).transitions[selectIndex]);
                        }
                        else
                        {
                            focusedObjects.Add(((FSM)currentElem).transitions[selectIndex]);
                        }
                    }
                    if (currentElem is UtilitySystem)
                    {
                        if (focusedObjects.Contains(((UtilitySystem)currentElem).connections[selectIndex]))
                        {
                            focusedObjects.Remove(((UtilitySystem)currentElem).connections[selectIndex]);
                        }
                        else
                        {
                            focusedObjects.Add(((UtilitySystem)currentElem).connections[selectIndex]);
                        }
                    }
                }
                else if (clickedOnWindow)
                {
                    if (currentElem is FSM)
                    {
                        if (focusedObjects.Contains(((FSM)currentElem).states[selectIndex]))
                        {
                            focusedObjects.Remove(((FSM)currentElem).states[selectIndex]);
                        }
                        else
                        {
                            focusedObjects.Add(((FSM)currentElem).states[selectIndex]);
                        }

                        if (Event.current.clickCount == 2 && ((FSM)currentElem).states[selectIndex].subElem != null)
                        {
                            currentElem = ((FSM)currentElem).states[selectIndex].subElem;
                            NodeEditorUtilities.ClearUndoSteps();
                            e.Use();
                        }
                    }
                    else if (currentElem is BehaviourTree)
                    {
                        if (focusedObjects.Contains(((BehaviourTree)currentElem).nodes[selectIndex]))
                        {
                            focusedObjects.Remove(((BehaviourTree)currentElem).nodes[selectIndex]);
                            focusedObjects.Remove(((BehaviourTree)currentElem).connections.Where(c => c.toNode.Equals(((BehaviourTree)currentElem).nodes[selectIndex])).FirstOrDefault());
                        }
                        else
                        {
                            focusedObjects.Add(((BehaviourTree)currentElem).nodes[selectIndex]);

                            GUIElement transToAdd = ((BehaviourTree)currentElem).connections.Where(c => c.toNode.Equals(((BehaviourTree)currentElem).nodes[selectIndex])).FirstOrDefault();
                            if (transToAdd)
                                focusedObjects.Add(transToAdd);
                        }

                        if (Event.current.clickCount == 2 && ((BehaviourTree)currentElem).nodes[selectIndex].subElem != null)
                        {
                            currentElem = ((BehaviourTree)currentElem).nodes[selectIndex].subElem;
                            NodeEditorUtilities.ClearUndoSteps();
                            e.Use();
                        }
                    }
                    else if (currentElem is UtilitySystem)
                    {
                        if (focusedObjects.Contains(((UtilitySystem)currentElem).nodes[selectIndex]))
                        {
                            focusedObjects.Remove(((UtilitySystem)currentElem).nodes[selectIndex]);
                        }
                        else
                        {
                            focusedObjects.Add(((UtilitySystem)currentElem).nodes[selectIndex]);
                        }

                        if (Event.current.clickCount == 2 && ((UtilitySystem)currentElem).nodes[selectIndex].subElem != null)
                        {
                            currentElem = ((UtilitySystem)currentElem).nodes[selectIndex].subElem;
                            NodeEditorUtilities.ClearUndoSteps();
                            e.Use();
                        }
                    }
                }
                else
                {
                    focusedObjects.Clear();

                    e.Use();
                }

                if (currentElem is FSM)
                {
                    if (makeTransitionMode)
                    {
                        if (clickedOnWindow && !((FSM)currentElem).states[selectIndex].Equals(selectednode))
                        {
                            if (!((FSM)currentElem).transitions.Exists(t => t.fromNode.Equals(selectednode) && t.toNode.Equals(((FSM)currentElem).states[selectIndex])))
                            {
                                NodeEditorUtilities.GenerateUndoStep(currentElem);

                                TransitionGUI transition = CreateInstance<TransitionGUI>();
                                transition.InitTransitionGUI(currentElem, selectednode, ((FSM)currentElem).states[selectIndex]);

                                ((FSM)currentElem).AddTransition(transition);
                            }

                            makeTransitionMode = false;
                            selectednode = null;
                        }

                        if (!clickedOnWindow)
                        {
                            makeTransitionMode = false;
                            selectednode = null;
                        }

                        e.Use();
                    }
                    if (reconnectTransitionMode)
                    {
                        if (clickedOnWindow
                            && !(((FSM)currentElem).states[selectIndex].Equals(selectedTransition.toNode) || ((FSM)currentElem).states[selectIndex].Equals(selectedTransition.fromNode))
                            && !((FSM)currentElem).transitions.Exists(t => t.fromNode.Equals(selectedTransition.fromNode) && t.toNode.Equals(((FSM)currentElem).states[selectIndex])))
                        {
                            NodeEditorUtilities.GenerateUndoStep(currentElem);

                            selectedTransition.toNode = ((FSM)currentElem).states[selectIndex];
                            ((FSM)currentElem).CheckConnected();
                        }

                        reconnectTransitionMode = false;
                        cutObjects.Remove(selectedTransition);
                        selectedTransition = null;

                        e.Use();
                    }
                }

                if (currentElem is BehaviourTree)
                {
                    if (makeAttachedNode)
                    {
                        NodeEditorUtilities.GenerateUndoStep(currentElem);

                        toCreateNode.windowRect.position = new Vector2(mousePos.x, mousePos.y);
                        ((BehaviourTree)currentElem).nodes.Add((BehaviourNode)toCreateNode);

                        TransitionGUI transition = CreateInstance<TransitionGUI>();
                        transition.InitTransitionGUI(currentElem, selectednode, toCreateNode);

                        ((BehaviourTree)currentElem).connections.Add(transition);

                        makeAttachedNode = false;
                        selectednode = null;
                        toCreateNode = null;

                        e.Use();
                    }
                    if (makeConnectionMode)
                    {
                        if (clickedOnWindow && !((BehaviourTree)currentElem).ConnectedCheck((BehaviourNode)selectednode, ((BehaviourTree)currentElem).nodes[selectIndex]) && !decoratorWithOneChild && !(((BehaviourTree)currentElem).nodes[selectIndex].type == behaviourType.Leaf))
                        {
                            NodeEditorUtilities.GenerateUndoStep(currentElem);

                            TransitionGUI transition = CreateInstance<TransitionGUI>();
                            transition.InitTransitionGUI(currentElem, ((BehaviourTree)currentElem).nodes[selectIndex], selectednode);
                            ((BehaviourTree)currentElem).connections.Add(transition);

                            ((BehaviourNode)selectednode).isRoot = false;
                        }

                        makeConnectionMode = false;
                        selectednode = null;

                        e.Use();
                    }
                }

                if (currentElem is UtilitySystem)
                {
                    if (makeAttachedNode)
                    {
                        NodeEditorUtilities.GenerateUndoStep(currentElem);

                        toCreateNode.windowRect.position = new Vector2(mousePos.x, mousePos.y);
                        ((UtilitySystem)currentElem).nodes.Add((UtilityNode)toCreateNode);

                        TransitionGUI transition = CreateInstance<TransitionGUI>();
                        transition.InitTransitionGUI(currentElem, toCreateNode, selectednode);

                        ((UtilitySystem)currentElem).connections.Add(transition);

                        makeAttachedNode = false;
                        selectednode = null;
                        toCreateNode = null;

                        e.Use();
                    }
                    if (makeConnectionMode)
                    {
                        if (clickedOnWindow && !((UtilitySystem)currentElem).ConnectedCheck((UtilityNode)selectednode, ((UtilitySystem)currentElem).nodes[selectIndex]) && !actionWithOneFactor && !curveWithOneFactor && !(((UtilitySystem)currentElem).nodes[selectIndex].type == utilityType.Variable))
                        {
                            NodeEditorUtilities.GenerateUndoStep(currentElem);

                            TransitionGUI transition = CreateInstance<TransitionGUI>();
                            transition.InitTransitionGUI(currentElem, selectednode, ((UtilitySystem)currentElem).nodes[selectIndex]);
                            ((UtilitySystem)currentElem).connections.Add(transition);
                        }

                        makeConnectionMode = false;
                        selectednode = null;

                        e.Use();
                    }
                }
            }
        }

        #endregion
        #region Key Press Control

        if (e.type == EventType.KeyUp)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Delete:
                    if (makeTransitionMode || reconnectTransitionMode)
                    {
                        makeTransitionMode = false;
                        reconnectTransitionMode = false;
                        cutObjects.Remove(selectedTransition);
                        selectedTransition = null;
                        break;
                    }
                    if (focusedObjects.Count > 0 && GUIUtility.keyboardControl == 0)
                    {
                        PopupWindow.InitDelete(focusedObjects.ToArray());
                        e.Use();
                    }
                    break;
                case KeyCode.Escape:
                    if (makeTransitionMode || reconnectTransitionMode || makeAttachedNode || makeConnectionMode)
                    {
                        makeTransitionMode = false;
                        makeAttachedNode = false;
                        makeConnectionMode = false;
                        focusedObjects.Clear();

                        reconnectTransitionMode = false;
                        cutObjects.Remove(selectedTransition);
                        selectedTransition = null;
                        break;
                    }
                    currentElem = currentElem?.parent;
                    NodeEditorUtilities.ClearUndoSteps();
                    e.Use();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (GUIUtility.keyboardControl != 0)
                    {
                        GUI.FocusControl(null);
                        e.Use();
                    }
                    else if (focusedObjects.LastOrDefault() is ClickableElement)
                    {
                        currentElem = (ClickableElement)focusedObjects.LastOrDefault();
                        e.Use();
                    }
                    else if (((BaseNode)focusedObjects.LastOrDefault())?.subElem != null)
                    {
                        currentElem = ((BaseNode)focusedObjects.LastOrDefault()).subElem;
                        e.Use();
                    }
                    focusedObjects.Clear();
                    NodeEditorUtilities.ClearUndoSteps();
                    break;
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                    CtrlDown = false;
                    break;
            }
        }

        if (e.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                    CtrlDown = true;
                    break;
                case KeyCode.S:
                    if (CtrlDown) SaveElem(currentElem);
                    break;
                case KeyCode.C:
                    if (CtrlDown) Copy();
                    break;
                case KeyCode.X:
                    if (CtrlDown) Cut();
                    break;
                case KeyCode.V:
                    if (CtrlDown) Paste();
                    break;
                case KeyCode.Z:
                    if (CtrlDown)
                    {
                        Undo();
                        e.Use();
                    }
                    break;
                case KeyCode.Y:
                    if (CtrlDown)
                    {
                        Redo();
                        e.Use();
                    }
                    break;
            }
        }

        #endregion

        // Draw the windows
        #region Windows Drawing (has to be done after key/mouse control)

        BeginWindows();

        if (currentElem is null)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                // Write how many errors are in this element
                string errorAbb = "";
                int errorCount = Elements[i].GetErrors().Count;
                int warningCount = Elements[i].GetWarnings().Count;

                if (errorCount + warningCount > 0)
                {
                    errorAbb += "\n (";

                    if (errorCount > 0)
                    {
                        errorAbb += errorCount + " error/s";
                    }
                    if (warningCount > 0)
                    {
                        if (errorCount > 0)
                            errorAbb += ", ";
                        errorAbb += warningCount + " warning/s";
                    }

                    errorAbb += ")";
                }

                Elements[i].windowRect = GUI.Window(i, Elements[i].windowRect, DrawElementWindow, Elements[i].GetTypeString() + errorAbb, new GUIStyle(Styles.SubTitleText)
                {
                    normal = new GUIStyleState()
                    {
                        background = GetBackground(Elements[i])
                    }
                });
            }
        }

        if (currentElem is FSM)
        {
            for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
            {
                // Write how many errors are in this element
                string errorAbb = "";
                int errorCount = ((FSM)currentElem).states[i].subElem ? ((FSM)currentElem).states[i].subElem.GetErrors().Count : 0;
                int warningCount = ((FSM)currentElem).states[i].subElem ? ((FSM)currentElem).states[i].subElem.GetWarnings().Count : 0;

                if (errorCount + warningCount > 0)
                {
                    errorAbb += "\n (";

                    if (errorCount > 0)
                    {
                        errorAbb += errorCount + " error/s";
                    }
                    if (warningCount > 0)
                    {
                        if (errorCount > 0)
                            errorAbb += ", ";
                        errorAbb += warningCount + " warning/s";
                    }

                    errorAbb += ")";
                }

                ((FSM)currentElem).states[i].windowRect = GUI.Window(i, ((FSM)currentElem).states[i].windowRect, DrawNodeWindow, ((FSM)currentElem).states[i].GetTypeString() + errorAbb, new GUIStyle(Styles.SubTitleText)
                {
                    normal = new GUIStyleState()
                    {
                        background = GetBackground(((FSM)currentElem).states[i])
                    }
                });
            }

            for (int i = 0; i < ((FSM)currentElem).transitions.Count; i++)
            {
                Vector2 offset = Vector2.zero;
                TransitionGUI elem = ((FSM)currentElem).transitions[i];

                if (elem.fromNode is null || elem.toNode is null)
                    break;

                // If there is two transitions with the same two nodes (->)
                //                                                     (<-)
                if (((FSM)currentElem).transitions.Exists(t => t.fromNode.Equals(elem.toNode) && t.toNode.Equals(elem.fromNode)))
                {
                    float ang = Vector2.SignedAngle((elem.toNode.windowRect.position - elem.fromNode.windowRect.position), Vector2.right);

                    if (ang > -45 && ang <= 45)
                    {
                        offset.y = pairTransitionsOffset;
                        offset.x = pairTransitionsOffset;
                    }
                    else if (ang > 45 && ang <= 135)
                    {
                        offset.x = pairTransitionsOffset;
                        offset.y = -pairTransitionsOffset;
                    }
                    else if ((ang > 135 && ang <= 180) || (ang > -180 && ang <= -135))
                    {
                        offset.y = -pairTransitionsOffset;
                        offset.x = -pairTransitionsOffset;
                    }
                    else if (ang > -135 && ang <= -45)
                    {
                        offset.x = -pairTransitionsOffset;
                        offset.y = pairTransitionsOffset;
                    }
                }

                Vector2 pos = new Vector2(elem.fromNode.windowRect.center.x + (elem.toNode.windowRect.x - elem.fromNode.windowRect.x) / 2,
                                          elem.fromNode.windowRect.center.y + (elem.toNode.windowRect.y - elem.fromNode.windowRect.y) / 2);
                Rect transitionRect = new Rect(pos.x - 75, pos.y - 15, elem.width, elem.height);
                transitionRect.position += offset;

                elem.windowRect = GUI.Window(i + ((FSM)currentElem).states.Count, transitionRect, DrawTransitionBox, "", new GUIStyle(Styles.SubTitleText)
                {
                    normal = new GUIStyleState()
                    {
                        background = GetBackground(elem)
                    }
                });
            }
        }

        if (currentElem is BehaviourTree)
        {
            for (int i = 0; i < ((BehaviourTree)currentElem).nodes.Count; i++)
            {
                string displayName = "";
                if (((BehaviourTree)currentElem).nodes[i].type > behaviourType.Selector)
                    displayName = ((BehaviourTree)currentElem).nodes[i].GetTypeString();

                // Write how many errors are in this element
                int errorCount = ((BehaviourTree)currentElem).nodes[i].subElem ? ((BehaviourTree)currentElem).nodes[i].subElem.GetErrors().Count : 0;
                int warningCount = ((BehaviourTree)currentElem).nodes[i].subElem ? ((BehaviourTree)currentElem).nodes[i].subElem.GetWarnings().Count : 0;

                if (errorCount + warningCount > 0)
                {
                    displayName += "\n (";

                    if (errorCount > 0)
                    {
                        displayName += errorCount + " error/s";
                    }
                    if (warningCount > 0)
                    {
                        if (errorCount > 0)
                            displayName += ", ";
                        displayName += warningCount + " warning/s";
                    }

                    displayName += ")";
                }

                ((BehaviourTree)currentElem).nodes[i].windowRect = GUI.Window(i, ((BehaviourTree)currentElem).nodes[i].windowRect, DrawNodeWindow, displayName, new GUIStyle(Styles.SubTitleText)
                {
                    normal = new GUIStyleState()
                    {
                        background = GetBackground(((BehaviourTree)currentElem).nodes[i])
                    }
                });
            }
        }

        if (currentElem is UtilitySystem)
        {
            for (int i = 0; i < ((UtilitySystem)currentElem).nodes.Count; i++)
            {
                string displayName = "";
                if (((UtilitySystem)currentElem).nodes[i].type == utilityType.Action)
                {
                    displayName = ((UtilitySystem)currentElem).nodes[i].GetTypeString();

                    // Write how many errors are in this element
                    int errorCount = ((UtilitySystem)currentElem).nodes[i].subElem ? ((UtilitySystem)currentElem).nodes[i].subElem.GetErrors().Count : 0;
                    int warningCount = ((UtilitySystem)currentElem).nodes[i].subElem ? ((UtilitySystem)currentElem).nodes[i].subElem.GetWarnings().Count : 0;

                    if (errorCount + warningCount > 0)
                    {
                        displayName += "\n (";

                        if (errorCount > 0)
                        {
                            displayName += errorCount + " error/s";
                        }
                        if (warningCount > 0)
                        {
                            if (errorCount > 0)
                                displayName += ", ";
                            displayName += warningCount + " warning/s";
                        }

                        displayName += ")";
                    }
                }

                ((UtilitySystem)currentElem).nodes[i].windowRect = GUI.Window(i, ((UtilitySystem)currentElem).nodes[i].windowRect, DrawNodeWindow, displayName, new GUIStyle(Styles.SubTitleText)
                {
                    normal = new GUIStyleState()
                    {
                        background = GetBackground(((UtilitySystem)currentElem).nodes[i])
                    }
                });
            }

            foreach (TransitionGUI elem in ((UtilitySystem)currentElem).connections.Where(t => ((UtilityNode)t.toNode).type == utilityType.Fusion && ((UtilityNode)t.toNode).fusionType == fusionType.Weighted))
            {
                if (elem.fromNode is null || elem.toNode is null)
                    break;

                Vector2 pos = new Vector2(elem.fromNode.windowRect.center.x + (elem.toNode.windowRect.x - elem.fromNode.windowRect.x) / 2,
                                          elem.fromNode.windowRect.center.y + (elem.toNode.windowRect.y - elem.fromNode.windowRect.y) / 2);
                Rect transitionRect = new Rect(pos.x, pos.y - 30, 70, 25);

                elem.windowRect = GUI.Window(((UtilitySystem)currentElem).connections.IndexOf(elem) + ((UtilitySystem)currentElem).nodes.Count, transitionRect, DrawTransitionBox, "", new GUIStyle(Styles.SubTitleText)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = new GUIStyleState()
                    {
                        background = GetBackground(elem)
                    }
                });
            }
        }

        EndWindows();

        #endregion

        // Check if there are repeated names
        #region Repeated Names

        bool repeatedNames = false;

        if (currentElem is FSM)
        {
            foreach (BaseNode node in ((FSM)currentElem).states)
            {
                if (currentElem.CheckNameExisting(node.nodeName, 1))
                    repeatedNames = true;
            }

            foreach (TransitionGUI transition in ((FSM)currentElem).transitions)
            {
                if (currentElem.CheckNameExisting(transition.transitionName, 1))
                    repeatedNames = true;
            }

            if (repeatedNames)
                currentElem.AddError(Error.RepeatedName);
            else
                currentElem.RemoveError(Error.RepeatedName);
        }
        else if (currentElem is BehaviourTree)
        {
            foreach (BaseNode node in ((BehaviourTree)currentElem).nodes)
            {
                if (currentElem.CheckNameExisting(node.nodeName, 1))
                    repeatedNames = true;
            }

            if (repeatedNames)
                currentElem.AddError(Error.RepeatedName);
            else
                currentElem.RemoveError(Error.RepeatedName);
        }
        else if (currentElem is UtilitySystem)
        {
            foreach (BaseNode node in ((UtilitySystem)currentElem).nodes)
            {
                if (currentElem.CheckNameExisting(node.nodeName, 1))
                    repeatedNames = true;
            }

            if (repeatedNames)
                currentElem.AddError(Error.RepeatedName);
            else
                currentElem.RemoveError(Error.RepeatedName);
        }

        Repaint();

        #endregion
    }

    private void OnDestroy()
    {
        NodeEditorUtilities.ClearUndoSteps();
        GetWindow<PopupWindow>().Close();
    }

    /// <summary>
    /// Checks what the <see cref="mousePos"/> overlaps with and returns the necessary info
    /// </summary>
    /// <returns>An array of booleans (ints of 2 values) and 1 int are used to determine what element the user clicked on</returns>
    private int[] ClickedOnCheck()
    {
        int clickedOnElement = 0;
        int clickedOnWindow = 0;
        int clickedOnLeaf = 0;
        int clickedOnVariable = 0;
        int decoratorWithOneChild = 0;
        int actionWithOneFactor = 0;
        int curveWithOneFactor = 0;
        int nodeWithAscendants = 0;
        int clickedOnTransition = 0;
        int selectIndex = -1;

        if (currentElem is null)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                if (Elements[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnElement = 1;
                    break;
                }
            }
        }

        if (currentElem is FSM)
        {
            for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
            {
                if (((FSM)currentElem).states[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnWindow = 1;
                    break;
                }
            }

            for (int i = 0; i < ((FSM)currentElem).transitions.Count; i++)
            {
                if (((FSM)currentElem).transitions[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnTransition = 1;
                    break;
                }
            }
        }

        if (currentElem is BehaviourTree)
        {
            for (int i = 0; i < ((BehaviourTree)currentElem).nodes.Count; i++)
            {
                if (((BehaviourTree)currentElem).nodes[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnWindow = 1;

                    if (((BehaviourTree)currentElem).connections.Exists(t => t.toNode.Equals(((BehaviourTree)currentElem).nodes[i])))
                        nodeWithAscendants = 1;

                    if (((BehaviourTree)currentElem).nodes[i].type == behaviourType.Leaf)
                        clickedOnLeaf = 1;

                    else if (((BehaviourTree)currentElem).nodes[i].type >= behaviourType.LoopN && ((BehaviourTree)currentElem).connections.Exists(t => t.fromNode.Equals(((BehaviourTree)currentElem).nodes[i])))
                        decoratorWithOneChild = 1;

                    break;
                }
            }
        }

        if (currentElem is UtilitySystem)
        {
            for (int i = 0; i < ((UtilitySystem)currentElem).nodes.Count; i++)
            {
                if (((UtilitySystem)currentElem).nodes[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnWindow = 1;

                    if (((UtilitySystem)currentElem).nodes[i].type == utilityType.Variable)
                        clickedOnVariable = 1;

                    else if (((UtilitySystem)currentElem).nodes[i].type == utilityType.Action && ((UtilitySystem)currentElem).connections.Exists(t => t.toNode.Equals(((UtilitySystem)currentElem).nodes[i])))
                        actionWithOneFactor = 1;

                    else if (((UtilitySystem)currentElem).nodes[i].type == utilityType.Curve && ((UtilitySystem)currentElem).connections.Exists(t => t.toNode.Equals(((UtilitySystem)currentElem).nodes[i])))
                        curveWithOneFactor = 1;

                    break;
                }
            }

            for (int i = 0; i < ((UtilitySystem)currentElem).connections.Count; i++)
            {
                if (((UtilitySystem)currentElem).connections[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnTransition = 1;
                    break;
                }
            }
        }

        return new int[]
        {
            clickedOnElement,
            clickedOnWindow,
            clickedOnLeaf,
            clickedOnVariable,
            decoratorWithOneChild,
            actionWithOneFactor,
            curveWithOneFactor,
            nodeWithAscendants,
            clickedOnTransition,
            selectIndex
        };
    }

    /// <summary>
    /// Draws the top bar elements
    /// </summary>
    private void ShowTopBar()
    {
        // Top Bar
        topBarOffset = 0;
        float yOffset = 0;
        var name = editorTitle;

        if (currentElem != null)
        {
            ShowButtonRecursive(Styles.TopBarButton, currentElem, editorTitle, yOffset);
            if (currentElem != null)
                name = currentElem.elementName;
        }

        var labelWidth = 25 + name.ToCharArray().Length * 6;
        GUI.Label(new Rect(topBarOffset, yOffset, labelWidth, 20), name);
    }

    /// <summary>
    /// Draws the Options button and its content
    /// </summary>
    private void ShowOptions()
    {
        if (GUI.Button(new Rect(position.width - 30, 0, 25, 20), "...", Styles.TopBarButton))
        {
            // Set menu items
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Manual de usuario"), false, () => Application.OpenURL("file:///" + Application.dataPath + "/Intelligent%20Behaviours%20GUI%20Package/Guia%20de%20uso.pdf"));
            menu.AddSeparator("");

            if (currentElem != null)
            {
                menu.AddItem(new GUIContent("Save Element to file"), false, SaveElem, currentElem);
                menu.AddItem(new GUIContent("Export Code"), false, ExportCode, currentElem);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Save Element to file"), false);
                menu.AddDisabledItem(new GUIContent("Export Code"), false);
            }

            menu.ShowAsContext();
        }
    }

    /// <summary>
    /// Shows the buttons with the names of the elements in order of hierarchy
    /// </summary>
    /// <param name="style"></param>
    /// <param name="elem"></param>
    /// <param name="name"></param>
    private void ShowButtonRecursive(GUIStyle style, ClickableElement elem, string name, float yOffset)
    {
        if (elem.parent != null)
        {
            ShowButtonRecursive(style, elem.parent, name, yOffset);
            name = elem.parent.elementName;
        }
        var buttonWidth = 25 + name.ToCharArray().Length * 6;
        if (GUI.Button(new Rect(topBarOffset, yOffset, buttonWidth, 20), name, style))
        {
            currentElem = elem.parent;
        }
        topBarOffset += buttonWidth;
        GUI.Label(new Rect(topBarOffset, yOffset, 15, 20), ">", new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft
        });
        topBarOffset += 12;
    }

    /// <summary>
    /// Configures the Texture using the sprite resources and returns it
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    private Texture2D GetBackground(GUIElement elem)
    {
        var isFocused = focusedObjects.Contains(elem);
        var isCut = cutObjects.Contains(elem);
        Color col = Color.white;
        Texture2D originalTexture = null;

        switch (elem.GetType().ToString())
        {
            // FSM
            case nameof(FSM):
                originalTexture = Resources.Load<Texture2D>("FSM_Rect");
                col = Color.blue;
                break;

            // BT
            case nameof(BehaviourTree):
                originalTexture = Resources.Load<Texture2D>("BT_Rect");
                col = Color.cyan;
                break;

            // US
            case nameof(UtilitySystem):
                originalTexture = Resources.Load<Texture2D>("US_Rect");
                col = new Color(0, 0.75f, 0, 1); //dark green
                break;

            // FSM Node
            case nameof(StateNode):
                stateType StType = ((StateNode)elem).type;

                // Nodo normal
                if (((StateNode)elem).subElem == null)
                {
                    switch (StType)
                    {
                        case stateType.Default:
                            originalTexture = Resources.Load<Texture2D>("Def_Node_Rect");
                            col = Color.grey;
                            break;
                        case stateType.Entry:
                            originalTexture = Resources.Load<Texture2D>("Entry_Rect");
                            col = Color.green;
                            break;
                        case stateType.Unconnected:
                            originalTexture = Resources.Load<Texture2D>("Unconnected_Node_Rect");
                            col = Color.red;
                            break;
                        default:
                            col = Color.white;
                            break;
                    }
                }
                // Nodo con sub-elemento
                else
                {
                    switch (StType)
                    {
                        case stateType.Default:
                            originalTexture = Resources.Load<Texture2D>("Def_Sub_Rect");
                            col = Color.grey;
                            break;
                        case stateType.Entry:
                            originalTexture = Resources.Load<Texture2D>("Entry_Sub_Rect");
                            col = Color.green;
                            break;
                        case stateType.Unconnected:
                            originalTexture = Resources.Load<Texture2D>("Unconnected_Sub_Rect");
                            col = Color.red;
                            break;
                        default:
                            col = Color.white;
                            break;
                    }
                }
                break;

            // BehaviourNode
            case nameof(BehaviourNode):
                behaviourType BNType = ((BehaviourNode)elem).type;

                switch (BNType)
                {
                    case behaviourType.Sequence:
                        originalTexture = Resources.Load<Texture2D>("Sequence_Rect");
                        col = Color.yellow;
                        break;
                    case behaviourType.Selector:
                        originalTexture = Resources.Load<Texture2D>("Selector_Rect");
                        col = new Color(1, 0.5f, 0, 1); //orange
                        break;
                    case behaviourType.Leaf:
                        if (((BehaviourNode)elem).subElem == null) //Es un nodo normal
                        {
                            originalTexture = Resources.Load<Texture2D>("Leaf_Rect");
                        }
                        else //Es un subelemento
                        {
                            originalTexture = Resources.Load<Texture2D>("Leaf_Sub_Rect");
                        }
                        col = new Color(0, 0.75f, 0, 1); //dark green
                        break;
                    case behaviourType.LoopN:
                    case behaviourType.LoopUntilFail:
                    case behaviourType.Inverter:
                    case behaviourType.DelayT:
                    case behaviourType.Succeeder:
                    case behaviourType.Conditional:
                        originalTexture = Resources.Load<Texture2D>("Decorator_Rect"); //Hacer un rombo gris
                                                                                       //col = Color.grey;
                        break;
                    default:
                        col = Color.white;
                        break;
                }
                break;

            // UtilityNode
            case nameof(UtilityNode):
                utilityType UNType = ((UtilityNode)elem).type;

                switch (UNType)
                {
                    case utilityType.Variable:
                        originalTexture = Resources.Load<Texture2D>("Variable_Rect");
                        col = new Color(1, 0.5f, 0, 1); //orange
                        break;
                    case utilityType.Fusion:
                        originalTexture = Resources.Load<Texture2D>("Fusion_Rect");
                        col = Color.yellow;
                        break;
                    case utilityType.Action:
                        if (((UtilityNode)elem).subElem == null) //Es un nodo normal
                        {
                            originalTexture = Resources.Load<Texture2D>("Leaf_Rect");
                        }
                        else //Es un subelemento
                        {
                            originalTexture = Resources.Load<Texture2D>("Leaf_Sub_Rect");
                        }
                        col = new Color(0, 0.75f, 0, 1); //dark green
                        break;
                    case utilityType.Curve:
                        if (((UtilityNode)elem).openFoldout)
                        {
                            originalTexture = Resources.Load<Texture2D>("Curve_Rect_Unfolded");
                        }
                        else
                        {
                            originalTexture = Resources.Load<Texture2D>("Curve_Rect_Folded");
                        }
                        col = Color.blue;
                        break;
                    default:
                        col = Color.white;
                        break;
                }
                break;

            // FSM Transition
            case nameof(TransitionGUI):
                originalTexture = Resources.Load<Texture2D>("Transition_Rect");
                col = Color.yellow;
                break;
            default:
                col = Color.clear;
                break;
        }

        // Copy the texture, so we don't override its original colors permanently
        Texture2D resultTexture = originalTexture is null ? null : new Texture2D(originalTexture.width, originalTexture.height);

        // If no texture has been found, use a simple colored Rect
        if (originalTexture == null)
        {
            Color[] pix = new Color[2 * 2];

            //Make it look semitransparent when not selected
            if (!isFocused)
                col.a = 0.5f;
            if (isCut)
                col.a = 0.2f;

            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            resultTexture = new Texture2D(2, 2);
            resultTexture.SetPixels(pix);
            resultTexture.Apply();
        }
        else
        {
            Color32[] pixels = originalTexture.GetPixels32();

            if (makeConnectionMode)
            {
                if (currentElem is BehaviourTree)
                {
                    if (((BehaviourTree)currentElem).ConnectedCheck((BehaviourNode)selectednode, (BehaviourNode)elem) || selectednode.Equals(elem) || ((BehaviourNode)elem).type == behaviourType.Leaf || ((BehaviourNode)elem).type >= behaviourType.LoopN && ((BehaviourTree)currentElem).connections.Exists(t => t.fromNode.Equals(elem)))
                    {
                        //Make it look transparent when not connectable to connect mode
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            pixels[i].a = (byte)(pixels[i].a * 64 / 255);
                        }
                    }
                }
                if (currentElem is UtilitySystem)
                {
                    if (elem is UtilityNode)
                    {
                        if (((UtilitySystem)currentElem).ConnectedCheck((UtilityNode)selectednode, (UtilityNode)elem) ||
                            selectednode.Equals(elem) || ((UtilityNode)elem).type == utilityType.Variable ||
                            ((UtilityNode)elem).type == utilityType.Action && ((UtilitySystem)currentElem).connections.Exists(t => t.toNode.Equals(elem)) ||
                            ((UtilityNode)elem).type == utilityType.Curve && ((UtilitySystem)currentElem).connections.Exists(t => t.toNode.Equals(elem)))
                        {
                            //Make it look transparent when not connectable to connect mode
                            for (int i = 0; i < pixels.Length; i++)
                            {
                                pixels[i].a = (byte)(pixels[i].a * 64 / 255);
                            }
                        }
                    }
                    else
                    {
                        //Make it look transparent when not connectable to connect mode
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            pixels[i].a = (byte)(pixels[i].a * 64 / 255);
                        }
                    }
                }
            }
            else if (!isFocused)
            {
                //Make it look semitransparent when not selected
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i].a = (byte)(pixels[i].a * 127 / 255);
                }
            }

            if (isCut)
            {
                //Make it look even more transparent when it's being cut
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i].a = (byte)(pixels[i].a * 64 / 255);
                }
            }

            resultTexture.SetPixels32(pixels);
            resultTexture.Apply();
        }

        return resultTexture;
    }

    /// <summary>
    /// The DrawNodeWindow
    /// </summary>
    /// <param name="id"></param>
    void DrawNodeWindow(int id)
    {
        if (currentElem is FSM && ((FSM)currentElem).states.Count > id)
        {
            ((FSM)currentElem).states[id].DrawWindow();
            if (((FSM)currentElem).states[id].subElem != null)
                ((FSM)currentElem).states[id].subElem.elementName = ((FSM)currentElem).states[id].nodeName;
        }
        if (currentElem is BehaviourTree && ((BehaviourTree)currentElem).nodes.Count > id)
        {
            ((BehaviourTree)currentElem).nodes[id].DrawWindow();
            if (((BehaviourTree)currentElem).nodes[id].subElem != null)
                ((BehaviourTree)currentElem).nodes[id].subElem.elementName = ((BehaviourTree)currentElem).nodes[id].nodeName;
        }
        if (currentElem is UtilitySystem && ((UtilitySystem)currentElem).nodes.Count > id)
        {
            ((UtilitySystem)currentElem).nodes[id].DrawWindow();
            if (((UtilitySystem)currentElem).nodes[id].subElem != null)
                ((UtilitySystem)currentElem).nodes[id].subElem.elementName = ((UtilitySystem)currentElem).nodes[id].nodeName;
        }

        GUI.DragWindow();
    }

    /// <summary>
    /// The DrawElementWindow
    /// </summary>
    /// <param name="id"></param>
    void DrawElementWindow(int id)
    {
        if (Elements.Count > id)
            Elements[id].DrawWindow();

        GUI.DragWindow();
    }

    /// <summary>
    /// The DrawTransitionBox
    /// </summary>
    /// <param name="id"></param>
    void DrawTransitionBox(int id)
    {
        if (currentElem is FSM && ((FSM)currentElem).transitions.Count > id - ((FSM)currentElem).states.Count)
        {
            ((FSM)currentElem).transitions[id - ((FSM)currentElem).states.Count].DrawBox();
        }
        if (currentElem is UtilitySystem && ((UtilitySystem)currentElem).connections.Count > id - ((UtilitySystem)currentElem).nodes.Count)
        {
            ((UtilitySystem)currentElem).connections[id - ((UtilitySystem)currentElem).nodes.Count].DrawBox();
        }

        GUI.DragWindow();
    }

    /// <summary>
    /// Performs an action depending on the given <paramref name="data"/>
    /// </summary>
    /// <param name="data"></param>
    void ContextCallback(object data)
    {
        string[] dataList = (string[])data;
        string order = dataList[0];
        int index = int.Parse(dataList[1]);

        switch (order)
        {
            case "FSM":
                CreateFSM(index, mousePos.x, mousePos.y);
                break;
            case "BT":
                CreateBT(index, mousePos.x, mousePos.y);
                break;
            case "US":
                CreateUS(index, mousePos.x, mousePos.y);
                break;
            case "Node":
                CreateNode(mousePos.x, mousePos.y);
                break;
            case "Sequence":
                CreateSequence(index, mousePos.x, mousePos.y);
                break;
            case "Selector":
                CreateSelector(index, mousePos.x, mousePos.y);
                break;
            case "leafNode":
                CreateLeafNode(2, index);
                break;
            case "loopN":
                CreateLeafNode(3, index);
                break;
            case "loopUFail":
                CreateLeafNode(4, index);
                break;
            case "inverter":
                CreateLeafNode(5, index);
                break;
            case "timer":
                CreateLeafNode(6, index);
                break;
            case "succeeder":
                CreateLeafNode(7, index);
                break;
            case "conditional":
                CreateLeafNode(8, index);
                break;
            case "Variable":
                CreateVariable(index, mousePos.x, mousePos.y);
                break;
            case "Fusion":
                CreateFusion(index, mousePos.x, mousePos.y);
                break;
            case "Action":
                CreateAction(mousePos.x, mousePos.y);
                break;
            case "Curve":
                CreateCurve(index, mousePos.x, mousePos.y);
                break;
            case "makeTransition":
                MakeTransition(index);
                break;
            case "reconnectTransition":
                ReconnectTransition(index);
                break;
            case "flipTransition":
                FlipTransition(index);
                break;
            case "deleteNode":
                DeleteNode(index);
                break;
            case "deleteTransition":
                DeleteTransition(index);
                break;
            case "entryState":
                ConvertToEntry(index);
                break;
            case "disconnectNode":
                DisconnectNode(index);
                break;
            case "connectNode":
                ConnectNode(index);
                break;
        }
    }

    /// <summary>
    /// Calls the utility function to export the code of the <paramref name="elem"/> if there's no errors
    /// </summary>
    void ExportCode(object elem)
    {
        if (((ClickableElement)elem).GetErrors().Count == 0)
        {
            NodeEditorUtilities.GenerateElemScript((ClickableElement)elem);
        }
        else
        {
            PopupWindow.InitExport();
        }
    }

    /// <summary>
    /// Calls the utility function to save the <paramref name="elem"/>
    /// </summary>
    void SaveElem(object elem)
    {
        NodeEditorUtilities.GenerateElemXMLFile((ClickableElement)elem);
    }

    /// <summary>
    /// Opens the explorer to select a file, and loads it
    /// </summary>
    void LoadSaveFile()
    {
        XMLElement loadedXML = NodeEditorUtilities.LoadSavedData();

        NodeEditorUtilities.GenerateUndoStep(currentElem);

        LoadElem(loadedXML, currentElem);
    }

    /// <summary>
    /// Loads the XMLElement to the <paramref name="element"/>
    /// </summary>
    ClickableElement LoadElem(XMLElement loadedXML, ClickableElement element, bool withMousePos = true)
    {
        if (loadedXML != null)
        {
            var currentBackup = element;

            ClickableElement newElem;
            switch (loadedXML.elemType)
            {
                case nameof(FSM):
                    newElem = loadedXML.ToFSM(element, null);
                    break;
                case nameof(BehaviourTree):
                    newElem = loadedXML.ToBehaviourTree(element, null);
                    break;
                case nameof(UtilitySystem):
                    newElem = loadedXML.ToUtilitySystem(element, null);
                    break;
                default:
                    Debug.LogError("Wrong content in saved data");
                    return null;
            }

            if (withMousePos)
            {
                newElem.windowRect.x = mousePos.x;
                newElem.windowRect.y = mousePos.y;
            }
            Elements.Add(newElem);
            editorNamer.AddName(newElem.identificator, newElem.elementName);

            element = currentBackup;

            return newElem;
        }

        return null;
    }

    /// <summary>
    /// Deletes the <paramref name="elem"/>
    /// </summary>
    /// <param name="elem"></param>
    public void Delete(GUIElement elem, ClickableElement element = null, bool skipUndoStep = false)
    {
        if (!skipUndoStep)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);
        }

        if (element is null)
        {
            element = currentElem;
        }

        switch (elem.GetType().ToString())
        {
            case nameof(StateNode):
                StateNode stateNode = (StateNode)elem;
                ((FSM)element).DeleteNode(stateNode);

                focusedObjects.Remove(stateNode);
                break;

            case nameof(BehaviourNode):
                BehaviourNode behaviourNode = (BehaviourNode)elem;
                ((BehaviourTree)element).DeleteNode(behaviourNode);

                focusedObjects.Remove(behaviourNode);
                break;

            case nameof(UtilityNode):
                UtilityNode utilityNode = (UtilityNode)elem;
                ((UtilitySystem)element).DeleteNode(utilityNode);

                focusedObjects.Remove(utilityNode);
                break;

            case nameof(TransitionGUI):
                if (element is FSM)
                {
                    TransitionGUI transition = (TransitionGUI)elem;
                    ((FSM)element).DeleteTransition(transition);

                    focusedObjects.Remove(transition);
                }
                break;

            case nameof(FSM):
                FSM fsm = (FSM)elem;
                Elements.Remove(fsm);

                editorNamer.RemoveName(fsm.identificator);

                focusedObjects.Remove(fsm);
                break;

            case nameof(BehaviourTree):
                BehaviourTree bt = (BehaviourTree)elem;
                Elements.Remove(bt);

                editorNamer.RemoveName(bt.identificator);

                focusedObjects.Remove(bt);
                break;
            case nameof(UtilitySystem):
                UtilitySystem us = (UtilitySystem)elem;
                Elements.Remove(us);

                editorNamer.RemoveName(us.identificator);

                focusedObjects.Remove(us);
                break;
        }
    }

    /// <summary>
    /// Draws a stylized bezier curve from <paramref name="start"/> to <paramref name="end"/>
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="isFocused"></param>
    /// <param name="isDouble"></param>
    public static void DrawNodeCurve(Rect start, Rect end, bool isFocused, bool isDouble = false)
    {
        // We add an offset so the start and end points are centered in their respective nodes

        start.x += start.width / 2;
        start.y += start.height / 2;

        end.x += end.width / 2;
        end.y += end.height / 2;

        // Check which sides to put the curve on

        float ang = Vector2.SignedAngle((end.position - start.position), Vector2.right);
        Vector3 direction = Vector3.right;

        if (ang > -45 && ang <= 45)
        {
            direction = Vector3.right;

            start.x += start.width / 2;
            end.x -= end.width / 2;

            if (isDouble)
            {
                start.y += pairTransitionsOffset;
                end.y += pairTransitionsOffset;
            }
        }
        else if (ang > 45 && ang <= 135)
        {
            direction = Vector3.down;

            start.y -= start.height / 2;
            end.y += end.height / 2;

            if (isDouble)
            {
                start.x += pairTransitionsOffset;
                end.x += pairTransitionsOffset;
            }
        }
        else if ((ang > 135 && ang <= 180) || (ang > -180 && ang <= -135))
        {
            direction = Vector3.left;

            start.x -= start.width / 2;
            end.x += end.width / 2;

            if (isDouble)
            {
                start.y -= pairTransitionsOffset;
                end.y -= pairTransitionsOffset;
            }
        }
        else if (ang > -135 && ang <= -45)
        {
            direction = Vector3.up;

            start.y += start.height / 2;
            end.y -= end.height / 2;

            if (isDouble)
            {
                start.x -= pairTransitionsOffset;
                end.x -= pairTransitionsOffset;
            }
        }

        // Draw curve
        // Curve parameters
        Vector3 startPos = new Vector3(start.x, start.y, 0);
        Vector3 endPos = new Vector3(end.x, end.y, 0);
        Vector3 startTan = startPos + direction * 50;
        Vector3 endTan = endPos - direction * 50;

        // Arrow parameters
        Vector3 pos1 = endPos - direction * 10;
        Vector3 pos2 = endPos - direction * 10;

        if (direction == Vector3.up || direction == Vector3.down)
        {
            pos1.x += 6;
            pos2.x -= 6;
        }
        else
        {
            pos1.y += 6;
            pos2.y -= 6;
        }

        // Color
        Color shadowCol = new Color(0, 0, 0, 0.06f);

        // Factor that determines how big the shadow/light of the curve will be
        int focusFactor = 3;

        // If the curve is focused it will look white, and with a bigger shadow to look more bright
        if (isFocused)
        {
            shadowCol = new Color(1, 1, 1, 0.1f);
            focusFactor = 10;
        }

        // Draw the whole thing a number of times with increasing width
        // so it looks like a shadow when unfocused and like a backlight when focused
        for (int i = 0; i < focusFactor; i++)
        {
            // Draw the curve
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);

            // Draw arrow
            Handles.DrawBezier(pos1, endPos, pos1, endPos, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier(pos2, endPos, pos2, endPos, shadowCol, null, (i + 1) * 5);
        }

        // Draw it with width 1 (sharper looking)
        // Draw the curve
        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);

        // Draw arrow
        Handles.DrawBezier(pos1, endPos, pos1, endPos, Color.black, null, 1);
        Handles.DrawBezier(pos2, endPos, pos2, endPos, Color.black, null, 1);
    }

    /// <summary>
    /// Creates a <see cref="FSM"/>
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateFSM(int nodeIndex, float posX, float posY)
    {
        FSM newFSM = CreateInstance<FSM>();
        newFSM.InitFSM(currentElem, posX, posY);

        if (currentElem is null)
        {
            Elements.Add(newFSM);
        }

        if (currentElem is FSM)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            StateNode node = CreateInstance<StateNode>();
            node.InitStateNode(currentElem, stateType.Unconnected, newFSM.windowRect.position.x, newFSM.windowRect.position.y, newFSM);

            if (!((FSM)currentElem).HasEntryState)
            {
                ((FSM)currentElem).AddEntryState(node);
            }
            else
            {
                ((FSM)currentElem).states.Add(node);
            }
        }

        if (currentElem is BehaviourTree)
        {
            BehaviourNode node = CreateInstance<BehaviourNode>();
            node.InitBehaviourNode(currentElem, behaviourType.Leaf, newFSM.windowRect.x, newFSM.windowRect.y, newFSM);

            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeAttachedNode = true;
        }

        if (currentElem is UtilitySystem)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            UtilityNode node = CreateInstance<UtilityNode>();
            node.InitUtilityNode(currentElem, utilityType.Action, posX, posY, newFSM);

            ((UtilitySystem)currentElem).nodes.Add(node);
        }
    }

    /// <summary>
    /// Creates a <see cref="BehaviourTree"/>
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateBT(int nodeIndex, float posX, float posY)
    {
        BehaviourTree newBT = CreateInstance<BehaviourTree>();
        newBT.InitBehaviourTree(currentElem, posX, posY);

        if (!string.IsNullOrEmpty(name))
        {
            newBT.elementName = name;
        }

        if (currentElem is null)
        {
            Elements.Add(newBT);
        }

        if (currentElem is FSM)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            StateNode node = CreateInstance<StateNode>();
            node.InitStateNode(currentElem, stateType.Unconnected, newBT.windowRect.position.x, newBT.windowRect.position.y, newBT);

            if (!((FSM)currentElem).HasEntryState)
            {
                ((FSM)currentElem).AddEntryState(node);
            }
            else
            {
                ((FSM)currentElem).states.Add(node);
            }
        }

        if (currentElem is BehaviourTree)
        {
            BehaviourNode node = CreateInstance<BehaviourNode>();
            node.InitBehaviourNode(currentElem, behaviourType.Leaf, newBT.windowRect.x, newBT.windowRect.y, newBT);

            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeAttachedNode = true;
        }

        if (currentElem is UtilitySystem)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            UtilityNode node = CreateInstance<UtilityNode>();
            node.InitUtilityNode(currentElem, utilityType.Action, posX, posY, newBT);

            ((UtilitySystem)currentElem).nodes.Add(node);
        }
    }

    /// <summary>
    /// Creates a <see cref="UtilitySystem"/>
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateUS(int nodeIndex, float posX, float posY)
    {
        UtilitySystem newUS = CreateInstance<UtilitySystem>();
        newUS.InitUtilitySystem(currentElem, posX, posY);

        if (!string.IsNullOrEmpty(name))
        {
            newUS.elementName = name;
        }

        if (currentElem is null)
        {
            Elements.Add(newUS);
        }

        if (currentElem is FSM)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            StateNode node = CreateInstance<StateNode>();
            node.InitStateNode(currentElem, stateType.Unconnected, newUS.windowRect.position.x, newUS.windowRect.position.y, newUS);

            if (!((FSM)currentElem).HasEntryState)
            {
                ((FSM)currentElem).AddEntryState(node);
            }
            else
            {
                ((FSM)currentElem).states.Add(node);
            }
        }

        if (currentElem is BehaviourTree)
        {
            BehaviourNode node = CreateInstance<BehaviourNode>();
            node.InitBehaviourNode(currentElem, behaviourType.Leaf, newUS.windowRect.x, newUS.windowRect.y, newUS);

            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeAttachedNode = true;
        }

        if (currentElem is UtilitySystem)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            UtilityNode node = CreateInstance<UtilityNode>();
            node.InitUtilityNode(currentElem, utilityType.Action, posX, posY, newUS);

            ((UtilitySystem)currentElem).nodes.Add(node);
        }
    }

    /// <summary>
    /// Creates a <see cref="StateNode"/>
    /// </summary>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateNode(float posX, float posY)
    {
        NodeEditorUtilities.GenerateUndoStep(currentElem);

        StateNode node = CreateInstance<StateNode>();
        node.InitStateNode(currentElem, stateType.Unconnected, posX, posY);

        if (!((FSM)currentElem).HasEntryState)
        {
            ((FSM)currentElem).AddEntryState(node);
        }
        else
        {
            ((FSM)currentElem).states.Add(node);
        }
    }

    /// <summary>
    /// Creates a <see cref="BehaviourNode"/> of type Sequence
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateSequence(int nodeIndex, float posX = 50, float posY = 50)
    {
        BehaviourNode node = CreateInstance<BehaviourNode>();
        node.InitBehaviourNode(currentElem, behaviourType.Sequence, posX, posY);

        if (nodeIndex > -1)
        {
            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeAttachedNode = true;
        }
        else
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            node.isRoot = true;
            ((BehaviourTree)currentElem).nodes.Add(node);
        }
    }

    /// <summary>
    /// Creates a <see cref="BehaviourNode"/> of type Selector
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateSelector(int nodeIndex, float posX = 50, float posY = 50)
    {
        BehaviourNode node = CreateInstance<BehaviourNode>();
        node.InitBehaviourNode(currentElem, behaviourType.Selector, posX, posY);

        if (nodeIndex > -1)
        {
            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeAttachedNode = true;
        }
        else
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            node.isRoot = true;
            ((BehaviourTree)currentElem).nodes.Add(node);
        }
    }

    /// <summary>
    /// Creates a <see cref="BehaviourNode"/> of type Leaf
    /// </summary>
    /// <param name="type"></param>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateLeafNode(int type, int nodeIndex, float posX = 50, float posY = 50)
    {
        BehaviourNode node = CreateInstance<BehaviourNode>();
        node.InitBehaviourNode(currentElem, (behaviourType)type, posX, posY);

        selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
        toCreateNode = node;
        makeAttachedNode = true;
    }

    /// <summary>
    /// Creates a <see cref="UtilityNode"/> of type Variable
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateVariable(int nodeIndex, float posX = 50, float posY = 50)
    {
        UtilityNode node = CreateInstance<UtilityNode>();
        node.InitUtilityNode(currentElem, utilityType.Variable, posX, posY);

        selectednode = ((UtilitySystem)currentElem).nodes[nodeIndex];
        toCreateNode = node;
        makeAttachedNode = true;
    }

    /// <summary>
    /// Creates a <see cref="UtilityNode"/> of type Fusion
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateFusion(int nodeIndex, float posX = 50, float posY = 50)
    {
        UtilityNode node = CreateInstance<UtilityNode>();
        node.InitUtilityNode(currentElem, utilityType.Fusion, posX, posY);

        selectednode = ((UtilitySystem)currentElem).nodes[nodeIndex];
        toCreateNode = node;
        makeAttachedNode = true;
    }

    /// <summary>
    /// Creates a <see cref="UtilityNode"/> of type Action
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateAction(float posX = 50, float posY = 50)
    {
        NodeEditorUtilities.GenerateUndoStep(currentElem);

        UtilityNode node = CreateInstance<UtilityNode>();
        node.InitUtilityNode(currentElem, utilityType.Action, posX, posY);

        ((UtilitySystem)currentElem).nodes.Add(node);
    }

    /// <summary>
    /// Creates a <see cref="UtilityNode"/> of type Curve
    /// </summary>
    /// <param name="nodeIndex"></param>
    /// <param name="posX"></param>
    /// <param name="posY"></param>
    private void CreateCurve(int nodeIndex, float posX = 50, float posY = 50)
    {
        UtilityNode node = CreateInstance<UtilityNode>();
        node.InitUtilityNode(currentElem, utilityType.Curve, posX, posY);

        selectednode = ((UtilitySystem)currentElem).nodes[nodeIndex];
        toCreateNode = node;
        makeAttachedNode = true;
    }

    /// <summary>
    /// Enter <see cref="makeTransitionMode"/> (mouse carries the other end of the transition until you click somewhere else)
    /// </summary>
    /// <param name="selectIndex"></param>
    private void MakeTransition(int selectIndex)
    {
        makeTransitionMode = true;

        if (currentElem is FSM)
            selectednode = ((FSM)currentElem).states[selectIndex];

        if (currentElem is BehaviourTree)
            selectednode = ((BehaviourTree)currentElem).nodes[selectIndex];
    }

    /// <summary>
    /// Enter <see cref="makeTransitionMode"/> like in <see cref="MakeTransition(int)"/> but with a transition selected for reconnection
    /// </summary>
    /// <param name="selectIndex"></param>
    private void ReconnectTransition(int selectIndex)
    {
        reconnectTransitionMode = true;

        if (currentElem is FSM)
        {
            selectedTransition = ((FSM)currentElem).transitions[selectIndex];
        }
    }

    /// <summary>
    /// Flip the transition (fromNode is toNode and viceversa)
    /// </summary>
    /// <param name="selectIndex"></param>
    private void FlipTransition(int selectIndex)
    {
        if (currentElem is FSM)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem);

            TransitionGUI transition = ((FSM)currentElem).transitions[selectIndex];

            TransitionGUI coupleTransition = ((FSM)currentElem).transitions.Find(t => t.fromNode.Equals(transition.toNode) && t.toNode.Equals(transition.fromNode));

            if (coupleTransition != null)
            {
                BaseNode tempNode1 = coupleTransition.fromNode;

                coupleTransition.fromNode = coupleTransition.toNode;
                coupleTransition.toNode = tempNode1;
            }

            BaseNode tempNode2 = transition.fromNode;

            transition.fromNode = transition.toNode;
            transition.toNode = tempNode2;

            ((FSM)currentElem).CheckConnected();
        }
    }

    /// <summary>
    /// Makes a <see cref="PopupWindow"/> appear that will delete the clicked node if accepted
    /// </summary>
    private void DeleteNode(int selectIndex)
    {
        if (currentElem is FSM)
        {
            PopupWindow.InitDelete(((FSM)currentElem).states[selectIndex]);
        }

        if (currentElem is BehaviourTree)
        {
            PopupWindow.InitDelete(((BehaviourTree)currentElem).nodes[selectIndex]);
        }

        if (currentElem is UtilitySystem)
        {
            PopupWindow.InitDelete(((UtilitySystem)currentElem).nodes[selectIndex]);
        }

        if (currentElem is null)
        {
            PopupWindow.InitDelete(Elements[selectIndex]);
        }
    }

    /// <summary>
    /// Makes a <see cref="PopupWindow"/> appear that will delete the clicked transition if accepted
    /// </summary>
    private void DeleteTransition(int selectIndex)
    {
        PopupWindow.InitDelete(((FSM)currentElem).transitions[selectIndex]);
    }

    /// <summary>
    /// Converts the clicked node into Entry State
    /// </summary>
    private void ConvertToEntry(int selectIndex)
    {
        NodeEditorUtilities.GenerateUndoStep(currentElem);

        ((FSM)currentElem).SetAsEntry(((FSM)currentElem).states[selectIndex]);
    }

    /// <summary>
    /// Disconnects the clicked node
    /// </summary>
    private void DisconnectNode(int selectIndex)
    {
        NodeEditorUtilities.GenerateUndoStep(currentElem);

        BehaviourNode selNode = ((BehaviourTree)currentElem).nodes[selectIndex];

        foreach (TransitionGUI tr in ((BehaviourTree)currentElem).connections.FindAll(t => t.toNode.Equals(selNode)))
        {
            ((BehaviourTree)currentElem).DeleteConnection(tr);
        }

        selNode.isRoot = true;
    }

    /// <summary>
    /// Enter <see cref="makeConnectionMode"/>
    /// </summary>
    /// <param name="nodeIndex"></param>
    private void ConnectNode(int selectIndex)
    {
        if (currentElem is BehaviourTree)
            selectednode = ((BehaviourTree)currentElem).nodes[selectIndex];
        if (currentElem is UtilitySystem)
            selectednode = ((UtilitySystem)currentElem).nodes[selectIndex];
        makeConnectionMode = true;
    }

    /// <summary>
    /// Shows the errors that currently exist on the bottom left corner of the window
    /// </summary>
    private void ShowErrorsAndWarnings()
    {
        string maxPriorityErrorOrWarning = "";
        int currentPriority = 0;
        bool errorShown = false;
        GUIStyle style = Styles.ErrorPrompt;

        // We use a list of KeyValuePairs so we can have duplicate keys
        List<KeyValuePair<ClickableElement, Error>> errors = new List<KeyValuePair<ClickableElement, Error>>();
        List<KeyValuePair<ClickableElement, Warning>> warnings = new List<KeyValuePair<ClickableElement, Warning>>();

        // Add the errors and warnings to the lists
        if (currentElem)
        {
            errors.AddRange(currentElem.GetErrors());
            warnings.AddRange(currentElem.GetWarnings());
        }
        else
        {
            foreach (ClickableElement elem in Elements)
            {
                errors.AddRange(elem.GetErrors());
                warnings.AddRange(elem.GetWarnings());
            }
        }

        // Select the highest priority warning to show
        foreach (var warning in warnings)
        {
            if ((int)warning.Value > currentPriority)
            {
                maxPriorityErrorOrWarning = Enums.WarningToString(warning.Value, warning.Key);
                currentPriority = (int)warning.Value;
            }
        }

        currentPriority = 0;

        // Select the highest priority error to show overriding the warning selected before. If there is no errors, the selected warning will remain
        foreach (var error in errors)
        {
            if ((int)error.Value > currentPriority)
            {
                errorShown = true;
                maxPriorityErrorOrWarning = Enums.ErrorToString(error.Value, error.Key);
                currentPriority = (int)error.Value;
            }
        }

        // Write the "(...)" part at the end of the prompt
        if (errors.Count + warnings.Count > 1)
        {
            maxPriorityErrorOrWarning += " (";

            if (errors.Count > (errorShown ? 1 : 0))
            {
                maxPriorityErrorOrWarning += "+" + (errors.Count - (errorShown ? 1 : 0)) + " error/s";
            }
            if (warnings.Count > (errorShown ? 0 : 1))
            {
                if (errors.Count > 1)
                    maxPriorityErrorOrWarning += ", ";
                maxPriorityErrorOrWarning += "+" + (warnings.Count - (errorShown ? 0 : 1)) + " warning/s";
            }

            maxPriorityErrorOrWarning += ")";
        }

        if (!string.IsNullOrEmpty(maxPriorityErrorOrWarning))
        {
            // If the shown prompt is an error, make the text background look red, if not, orange
            if (errorShown)
                style.normal.background = GetLabelBackground(new Color(1, 0, 0, 0.6f));
            else
                style.normal.background = GetLabelBackground(new Color(1, 0.8f, 0, 0.6f));
            style.contentOffset = new Vector2(5, 5);
            EditorGUI.LabelField(new Rect(new Vector2(0, position.height - 25), maxSize), maxPriorityErrorOrWarning, style);
        }
    }

    public Texture2D GetLabelBackground(Color color)
    {
        // Grey colored background for the label
        Texture2D backgroundTex = new Texture2D(2, 2);
        Color[] pix = new Color[2 * 2];

        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = color; // grey with 0.2 alpha
        }

        backgroundTex.SetPixels(pix);
        backgroundTex.Apply();

        return backgroundTex;
    }

    /// <summary>
    /// Takes all the copied elements and gives them a new identificator
    /// </summary>
    /// <param name="elements"></param>
    private void ReIdentifyElements(List<GUIElement> elements)
    {
        foreach (GUIElement elem in elements)
        {
            elem.identificator = GUIElement.UniqueID();

            if (elem is BaseNode && ((BaseNode)elem).subElem != null)
            {
                if (((BaseNode)elem).subElem is FSM)
                {
                    ReIdentifyElements(((FSM)((BaseNode)elem).subElem).states.Cast<GUIElement>().ToList());
                    ReIdentifyElements(((FSM)((BaseNode)elem).subElem).transitions.Cast<GUIElement>().ToList());
                }
                if (((BaseNode)elem).subElem is BehaviourTree)
                {
                    ReIdentifyElements(((BehaviourTree)((BaseNode)elem).subElem).nodes.Cast<GUIElement>().ToList());
                    ReIdentifyElements(((BehaviourTree)((BaseNode)elem).subElem).connections.Cast<GUIElement>().ToList());
                }
                if (((BaseNode)elem).subElem is UtilitySystem)
                {
                    ReIdentifyElements(((UtilitySystem)((BaseNode)elem).subElem).nodes.Cast<GUIElement>().ToList());
                    ReIdentifyElements(((BehaviourTree)((BaseNode)elem).subElem).connections.Cast<GUIElement>().ToList());
                }
            }
        }
    }

    private void Copy()
    {
        clipboard = focusedObjects.Select(o => o.CopyElement(currentElem)).ToList();
        foreach (GUIElement elem in clipboard)
        {
            if (elem is TransitionGUI)
            {
                ((TransitionGUI)elem).fromNode = (BaseNode)clipboard.Find(n => n.identificator == ((TransitionGUI)elem).fromNode.identificator);
                ((TransitionGUI)elem).toNode = (BaseNode)clipboard.Find(n => n.identificator == ((TransitionGUI)elem).toNode.identificator);

                if (((TransitionGUI)elem).fromNode is StateNode && ((TransitionGUI)elem).toNode is StateNode)
                {
                    ((StateNode)((TransitionGUI)elem).fromNode).nodeTransitions.Add((TransitionGUI)elem);
                    ((StateNode)((TransitionGUI)elem).toNode).nodeTransitions.Add((TransitionGUI)elem);
                }
            }
        }

        // Reidentify after to make sure any references to other GUIElements are properly copied
        ReIdentifyElements(clipboard);

        cutObjects.Clear();
    }

    private void Cut()
    {
        Copy();
        cutObjects = new List<GUIElement>(focusedObjects);
        cutFromElement = currentElem;
    }

    private void Paste()
    {
        if (currentElem is null)
        {
            if (clipboard.Any(e => !(e is ClickableElement || (e is BaseNode && ((BaseNode)e).subElem != null))))
            {
                Debug.LogError("[ERROR] Couldn't paste these elements in this place");
            }
            else
            {
                if (cutObjects.Count > 0)
                    foreach (GUIElement elem in cutObjects)
                        Delete(elem, cutFromElement);

                foreach (GUIElement elem in clipboard)
                {
                    ClickableElement clickElem;

                    if (elem is ClickableElement)
                    {
                        clickElem = (ClickableElement)elem;
                    }
                    else
                    {
                        clickElem = ((BaseNode)elem).subElem;
                    }

                    clickElem.parent = null;

                    Elements.Add(clickElem);
                    // El currentelem null no hace el check para los names, pero deberia
                    //currentElem.elementNamer.AddName(clickElem.identificator, clickElem.elementName);
                }
            }
        }

        if (currentElem is FSM)
        {
            if (clipboard.Any(e => !(e is StateNode || e is TransitionGUI || e is ClickableElement || (e is BaseNode && ((BaseNode)e).subElem != null))))
            {
                Debug.LogError("[ERROR] Couldn't paste these elements in this place");
            }
            else
            {
                if (cutObjects.Count > 0)
                    foreach (GUIElement elem in cutObjects)
                        Delete(elem, cutFromElement);

                foreach (GUIElement elem in clipboard)
                {
                    if (elem is TransitionGUI)
                    {
                        ((FSM)currentElem).transitions.Add((TransitionGUI)elem);
                        currentElem.elementNamer.AddName(elem.identificator, ((TransitionGUI)elem).transitionName);
                    }
                    else
                    {
                        StateNode newElem;

                        if (elem is ClickableElement)
                        {
                            ((ClickableElement)elem).parent = currentElem;
                            currentElem.elementNamer.AddName(elem.identificator, ((ClickableElement)elem).elementName);

                            newElem = CreateInstance<StateNode>();
                            newElem.InitStateNode(currentElem, stateType.Unconnected, ((ClickableElement)elem).windowRect.x, ((ClickableElement)elem).windowRect.y, (ClickableElement)elem);
                        }
                        else
                        {
                            if (elem is StateNode)
                            {
                                newElem = (StateNode)elem;

                                currentElem.elementNamer.AddName(newElem.identificator, newElem.nodeName);
                            }
                            else
                            {
                                ((BaseNode)elem).parent = currentElem;

                                newElem = CreateInstance<StateNode>();
                                newElem.InitStateNode(currentElem, stateType.Unconnected, ((BaseNode)elem).windowRect.x, ((BaseNode)elem).windowRect.y, ((BaseNode)elem).subElem);
                            }
                        }

                        newElem.type = stateType.Unconnected;

                        ((FSM)currentElem).states.Add(newElem);

                        if (!((FSM)currentElem).HasEntryState)
                        {
                            ((FSM)currentElem).SetAsEntry(newElem);
                        }

                        ((FSM)currentElem).CheckConnected();
                    }
                }
            }
        }

        if (currentElem is BehaviourTree)
        {
            if (clipboard.Any(e => !(e is BehaviourNode || e is TransitionGUI || e is ClickableElement || (e is BaseNode && ((BaseNode)e).subElem != null))))
            {
                Debug.LogError("[ERROR] Couldn't paste these elements in this place");
            }
            else
            {
                if (cutObjects.Count > 0)
                    foreach (GUIElement elem in cutObjects)
                        Delete(elem, cutFromElement);

                foreach (GUIElement elem in clipboard)
                {
                    if (elem is TransitionGUI)
                    {
                        ((BehaviourTree)currentElem).connections.Add((TransitionGUI)elem);
                        currentElem.elementNamer.AddName(elem.identificator, ((TransitionGUI)elem).transitionName);
                    }
                    else
                    {
                        BehaviourNode newElem;

                        if (elem is ClickableElement)
                        {
                            ((ClickableElement)elem).parent = currentElem;
                            currentElem.elementNamer.AddName(elem.identificator, ((ClickableElement)elem).elementName);

                            newElem = CreateInstance<BehaviourNode>();
                            newElem.InitBehaviourNode(currentElem, behaviourType.Leaf, ((ClickableElement)elem).windowRect.x, ((ClickableElement)elem).windowRect.y, (ClickableElement)elem);
                        }
                        else
                        {
                            if (elem is BehaviourNode)
                            {
                                newElem = (BehaviourNode)elem;

                                currentElem.elementNamer.AddName(newElem.identificator, newElem.nodeName);
                            }
                            else
                            {
                                ((BaseNode)elem).parent = currentElem;

                                newElem = CreateInstance<BehaviourNode>();
                                newElem.InitBehaviourNode(currentElem, behaviourType.Leaf, ((BaseNode)elem).windowRect.x, ((BaseNode)elem).windowRect.y, ((BaseNode)elem).subElem);
                            }
                        }

                        ((BehaviourTree)currentElem).nodes.Add(newElem);
                    }
                }
            }
        }

        if (currentElem is UtilitySystem)
        {
            if (clipboard.Any(e => !(e is UtilityNode || e is TransitionGUI || e is ClickableElement || (e is BaseNode && ((BaseNode)e).subElem != null))))
            {
                Debug.LogError("[ERROR] Couldn't paste these elements in this place");
            }
            else
            {
                if (cutObjects.Count > 0)
                    foreach (GUIElement elem in cutObjects)
                        Delete(elem, cutFromElement);

                foreach (GUIElement elem in clipboard)
                {
                    if (elem is TransitionGUI)
                    {
                        ((UtilitySystem)currentElem).connections.Add((TransitionGUI)elem);
                        currentElem.elementNamer.AddName(elem.identificator, ((TransitionGUI)elem).transitionName);
                    }
                    else
                    {
                        UtilityNode newElem;

                        if (elem is ClickableElement)
                        {
                            ((ClickableElement)elem).parent = currentElem;
                            currentElem.elementNamer.AddName(elem.identificator, ((ClickableElement)elem).elementName);

                            newElem = CreateInstance<UtilityNode>();
                            newElem.InitUtilityNode(currentElem, utilityType.Action, ((ClickableElement)elem).windowRect.x, ((ClickableElement)elem).windowRect.y, (ClickableElement)elem);
                        }
                        else
                        {
                            if (elem is UtilityNode)
                            {
                                newElem = (UtilityNode)elem;

                                currentElem.elementNamer.AddName(newElem.identificator, newElem.nodeName);
                            }
                            else
                            {
                                ((BaseNode)elem).parent = currentElem;

                                newElem = CreateInstance<UtilityNode>();
                                newElem.InitUtilityNode(currentElem, utilityType.Action, ((BaseNode)elem).windowRect.x, ((BaseNode)elem).windowRect.y, ((BaseNode)elem).subElem);
                            }
                        }

                        ((UtilitySystem)currentElem).nodes.Add(newElem);
                    }
                }
            }
        }

        if (cutObjects.Count > 0)
        {
            clipboard.Clear();
        }

        cutObjects.Clear();

        NodeEditorUtilities.GenerateUndoStep(currentElem);
    }

    private void Undo()
    {
        if (NodeEditorUtilities.undoStepsSaved > 0)
        {
            NodeEditorUtilities.GenerateRedoStep(currentElem);
            Delete(currentElem, currentElem.parent, true);
            currentElem = LoadElem(NodeEditorUtilities.LoadUndoStep(), currentElem.parent, false);
        }
    }

    private void Redo()
    {
        if (NodeEditorUtilities.redoStepsSaved > 0)
        {
            NodeEditorUtilities.GenerateUndoStep(currentElem, false);
            Delete(currentElem, currentElem.parent, true);
            currentElem = LoadElem(NodeEditorUtilities.LoadRedoStep(), currentElem.parent, false);
        }
    }
}
