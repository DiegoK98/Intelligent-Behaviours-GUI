using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class NodeEditor : EditorWindow
{
    private Vector2 mousePos;

    private BaseNode selectednode;

    private BaseNode toCreateNode;

    public GUIElement focusedObj;

    private bool makeTransitionMode = false;

    private bool makeBehaviourMode = false;

    private bool makeConnectionMode = false;

    public List<ClickableElement> Elements = new List<ClickableElement>();

    public ClickableElement currentElem;

    private readonly int MAX_N_STATES = 100;

    private Dictionary<string, int> errors = new Dictionary<string, int>();

    public bool popupShown;

    private float widthVariant;

    private static float coupleTransitionsOffset = 20;

    /// <summary>
    /// The ShowEditor
    /// </summary>
    [MenuItem("Window/Node Editor")]
    static void ShowEditor()
    {
        // Close any previous window
        GetWindow<PopupWindow>().Close();
        GetWindow<NodeEditor>().Close();

        // Open a new Editor Window
        GetWindow<NodeEditor>();
    }

    /// <summary>
    /// The OnGUI
    /// </summary>
    private void OnGUI()
    {
        Event e = Event.current;
        mousePos = e.mousePosition;

        ShowTopBar();
        if (currentElem != null)
            ShowOptions();
        ShowErrorByPriority();

        // Draw the curves for everything
        #region Curves Drawing

        if ((makeTransitionMode || makeBehaviourMode || makeConnectionMode) && selectednode != null)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            Rect nodeRect = new Rect(selectednode.windowRect);

            if (makeConnectionMode)
                DrawNodeCurve(mouseRect, nodeRect, true);
            else
                DrawNodeCurve(nodeRect, mouseRect, true);

            Repaint();
        }

        if (currentElem is FSM)
        {
            ((FSM)currentElem).DrawCurves();

            if (!((FSM)currentElem).hasEntryState)
            {
                AddError(Enums.Errors.NoEntryState);
            }
            else
            {
                RemoveError(Enums.Errors.NoEntryState);
            }
        }

        if (currentElem is BehaviourTree)
        {
            ((BehaviourTree)currentElem).DrawCurves();
        }

        #endregion

        // Controls for the events called by the mouse and keyboard
        #region Mouse Click Control

        if (e.type == EventType.MouseDown)
        {
            // Check where it clicked
            int[] results = ClickedOnCheck();

            bool clickedOnElement = Convert.ToBoolean(results[0]);
            bool clickedOnWindow = Convert.ToBoolean(results[1]);
            bool clickedOnLeaf = Convert.ToBoolean(results[2]);
            bool decoratorWithOneChild = Convert.ToBoolean(results[3]);
            bool nodeWithAscendants = Convert.ToBoolean(results[4]);
            bool clickedOnTransition = Convert.ToBoolean(results[5]);
            int selectIndex = results[6];

            // Click derecho
            if (e.button == 1)
            {
                if (!makeTransitionMode && !makeBehaviourMode && !makeConnectionMode)
                {
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
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Load Element from file"), false, LoadElem);
                        }
                        else if (clickedOnWindow)
                        {
                            menu.AddItem(new GUIContent("Make Transition"), false, ContextCallback, new string[] { "makeTransition", selectIndex.ToString() });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, new string[] { "deleteNode", selectIndex.ToString() });

                            if (!((FSM)currentElem).isEntryState(((FSM)currentElem).states[selectIndex]))
                            {
                                menu.AddSeparator("");
                                menu.AddItem(new GUIContent("Convert to Entry State"), false, ContextCallback, new string[] { "entryState", selectIndex.ToString() });
                            }
                        }
                        else if (clickedOnTransition)
                        {
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
                            menu.AddItem(new GUIContent("Load Element from file"), false, LoadElem);
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
                                }

                                menu.AddSeparator("");
                            }

                            if (nodeWithAscendants)
                                menu.AddItem(new GUIContent("Disconnect Node"), false, ContextCallback, new string[] { "disconnectNode", selectIndex.ToString() });
                            else
                                menu.AddItem(new GUIContent("Connect Node"), false, ContextCallback, new string[] { "connectNode", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, new string[] { "deleteNode", selectIndex.ToString() });
                        }
                    }
                    else if (currentElem is null)
                    {
                        if (!clickedOnElement)
                        {
                            menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, new string[] { "FSM", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, new string[] { "BT", selectIndex.ToString() });
                            menu.AddSeparator("");
                            menu.AddItem(new GUIContent("Load Element from file"), false, LoadElem);

                            menu.ShowAsContext();
                            e.Use();
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Save Element to file"), false, SaveElem, Elements[selectIndex]);
                            menu.AddItem(new GUIContent("Export Code"), false, ExportCode, Elements[selectIndex]);
                            menu.AddItem(new GUIContent("Delete Element"), false, ContextCallback, new string[] { "deleteNode", selectIndex.ToString() });
                        }
                    }

                    menu.ShowAsContext();
                    e.Use();
                }
                //Click derecho estando en uno de estos dos modos, lo cancela
                else
                {
                    makeTransitionMode = false;
                    makeBehaviourMode = false;
                    makeConnectionMode = false;
                }
            }

            // Click izquierdo
            else if (e.button == 0)
            {
                GUI.FocusControl(null);
                if (focusedObj != null) focusedObj.isFocused = false;

                if (clickedOnElement)
                {
                    Elements[selectIndex].isFocused = true;
                    focusedObj = Elements[selectIndex];

                    if (Event.current.clickCount == 2)
                    {
                        currentElem = Elements[selectIndex];
                        e.Use();
                    }
                }
                else if (clickedOnTransition && currentElem is FSM)
                {
                    ((FSM)currentElem).transitions[selectIndex].isFocused = true;
                    focusedObj = ((FSM)currentElem).transitions[selectIndex];
                }
                else if (clickedOnWindow && currentElem is FSM)
                {
                    ((FSM)currentElem).states[selectIndex].isFocused = true;
                    focusedObj = ((FSM)currentElem).states[selectIndex];

                    if (Event.current.clickCount == 2 && ((StateNode)focusedObj).subElem != null)
                    {
                        currentElem = ((StateNode)focusedObj).subElem;
                        e.Use();
                    }
                }
                else if (clickedOnWindow && currentElem is BehaviourTree)
                {
                    ((BehaviourTree)currentElem).nodes[selectIndex].isFocused = true;
                    focusedObj = ((BehaviourTree)currentElem).nodes[selectIndex];

                    if (Event.current.clickCount == 2 && ((BehaviourNode)focusedObj).subElem != null)
                    {
                        currentElem = ((BehaviourNode)focusedObj).subElem;
                        e.Use();
                    }
                }
                else
                {
                    focusedObj = null;

                    e.Use();
                }

                if (makeTransitionMode && currentElem is FSM)
                {
                    if (clickedOnWindow && !((FSM)currentElem).states[selectIndex].Equals(selectednode))
                    {
                        if (!((FSM)currentElem).transitions.Exists(t => t.fromNode.Equals(selectednode) && t.toNode.Equals(((FSM)currentElem).states[selectIndex])))
                        {
                            TransitionGUI transition = CreateInstance<TransitionGUI>();
                            transition.InitTransitionGUI("New Transition " + ((FSM)currentElem).transitions.Count, selectednode, ((FSM)currentElem).states[selectIndex]);

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

                if (currentElem is BehaviourTree)
                {
                    if (makeBehaviourMode)
                    {
                        toCreateNode.windowRect.position = new Vector2(mousePos.x, mousePos.y);
                        ((BehaviourTree)currentElem).nodes.Add((BehaviourNode)toCreateNode);

                        TransitionGUI transition = CreateInstance<TransitionGUI>();
                        transition.InitTransitionGUI("", selectednode, toCreateNode);

                        ((BehaviourTree)currentElem).connections.Add(transition);

                        makeBehaviourMode = false;
                        selectednode = null;
                        toCreateNode = null;

                        e.Use();
                    }
                    if (makeConnectionMode)
                    {
                        if (clickedOnWindow && !((BehaviourTree)currentElem).ConnectedCheck(selectednode, ((BehaviourTree)currentElem).nodes[selectIndex]) && !decoratorWithOneChild && !(((BehaviourTree)currentElem).nodes[selectIndex].type == BehaviourNode.behaviourType.Leaf))
                        {
                            TransitionGUI transition = CreateInstance<TransitionGUI>();
                            transition.InitTransitionGUI("", ((BehaviourTree)currentElem).nodes[selectIndex], selectednode);
                            ((BehaviourTree)currentElem).connections.Add(transition);

                            ((BehaviourNode)selectednode).isRootNode = false;
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
                    if (makeTransitionMode)
                    {
                        makeTransitionMode = false;
                        break;
                    }
                    if (focusedObj != null && GUIUtility.keyboardControl == 0)
                    {
                        PopupWindow.InitDelete(this, focusedObj, focusedObj.GetTypeString());
                        e.Use();
                    }
                    break;
                case KeyCode.Escape:
                    if (makeTransitionMode)
                    {
                        makeTransitionMode = false;
                        break;
                    }
                    currentElem = currentElem?.parent;
                    e.Use();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (GUIUtility.keyboardControl != 0)
                    {
                        GUI.FocusControl(null);
                        e.Use();
                    }
                    else if (focusedObj is ClickableElement)
                    {
                        currentElem = (ClickableElement)focusedObj;
                        e.Use();
                    }
                    break;
            }
        }

        #endregion

        // Draw the windows
        #region Windows Drawing (has to be done last)

        BeginWindows();

        if (currentElem is null)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements[i].windowRect = GUI.Window(i, Elements[i].windowRect, DrawElementWindow, Elements[i].GetTypeString(), new GUIStyle(Styles.SubTitleText)
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
                ((FSM)currentElem).states[i].windowRect = GUI.Window(i, ((FSM)currentElem).states[i].windowRect, DrawNodeWindow, ((FSM)currentElem).states[i].GetTypeString(), new GUIStyle(Styles.SubTitleText)
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

                if (((FSM)currentElem).transitions.Exists(t => t.fromNode.Equals(elem.toNode) && t.toNode.Equals(elem.fromNode)))
                {
                    float ang = Vector2.SignedAngle((elem.toNode.windowRect.position - elem.fromNode.windowRect.position), Vector2.right);

                    if (ang > -45 && ang <= 45)
                    {
                        offset.y = coupleTransitionsOffset;
                        offset.x = coupleTransitionsOffset;
                    }
                    else if (ang > 45 && ang <= 135)
                    {
                        offset.x = coupleTransitionsOffset;
                        offset.y = -coupleTransitionsOffset;
                    }
                    else if ((ang > 135 && ang <= 180) || (ang > -180 && ang <= -135))
                    {
                        offset.y = -coupleTransitionsOffset;
                        offset.x = -coupleTransitionsOffset;
                    }
                    else if (ang > -135 && ang <= -45)
                    {
                        offset.x = -coupleTransitionsOffset;
                        offset.y = coupleTransitionsOffset;
                    }
                }

                Vector2 pos = new Vector2(elem.fromNode.windowRect.center.x + (elem.toNode.windowRect.x - elem.fromNode.windowRect.x) / 2,
                                          elem.fromNode.windowRect.center.y + (elem.toNode.windowRect.y - elem.fromNode.windowRect.y) / 2);
                Rect textBox = new Rect(pos.x - 75, pos.y - 15, elem.width, elem.height);
                textBox.position += offset;

                elem.textBox = GUI.Window(i + MAX_N_STATES, textBox, DrawTransitionBox, "", new GUIStyle(Styles.SubTitleText)
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
                if (((BehaviourTree)currentElem).nodes[i].type > BehaviourNode.behaviourType.Selector)
                    displayName = ((BehaviourTree)currentElem).nodes[i].GetTypeString();

                ((BehaviourTree)currentElem).nodes[i].windowRect = GUI.Window(i, ((BehaviourTree)currentElem).nodes[i].windowRect, DrawNodeWindow, displayName, new GUIStyle(Styles.SubTitleText)
                {
                    normal = new GUIStyleState()
                    {
                        background = GetBackground(((BehaviourTree)currentElem).nodes[i])
                    }
                });
            }
        }

        EndWindows();

        //En vez de is null deberia checkear si ha cambiado
        if (focusedObj is null)
        {
            bool repeatedNames = false;

            foreach (ClickableElement elem in Elements)
            {
                if (elem is FSM)
                {
                    foreach (BaseNode node in ((FSM)elem).states)
                    {
                        if (elem.CheckNameExisting(node.nodeName, 1))
                            repeatedNames = true;
                    }

                    foreach (TransitionGUI transition in ((FSM)elem).transitions)
                    {
                        if (elem.CheckNameExisting(transition.transitionName, 1))
                            repeatedNames = true;
                    }
                }
                else if (elem is BehaviourTree)
                {
                    foreach (BaseNode node in ((BehaviourTree)elem).nodes)
                    {
                        if (elem.CheckNameExisting(node.nodeName, 1))
                            repeatedNames = true;
                    }
                }
            }

            if (repeatedNames)
                AddError(Enums.Errors.RepeatedName);
            else
                RemoveError(Enums.Errors.RepeatedName);
        }

        #endregion
    }

    /// <summary>
    /// The OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        GetWindow<PopupWindow>().Close();
    }

    private int[] ClickedOnCheck()
    {
        int clickedOnElement = 0;
        int clickedOnWindow = 0;
        int clickedOnLeaf = 0;
        int decoratorWithOneChild = 0;
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
                if (((FSM)currentElem).transitions[i].textBox.Contains(mousePos))
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

                    if (((BehaviourTree)currentElem).nodes[i].type == BehaviourNode.behaviourType.Leaf)
                        clickedOnLeaf = 1;
                    else if (((BehaviourTree)currentElem).nodes[i].type >= BehaviourNode.behaviourType.LoopN && ((BehaviourTree)currentElem).connections.Exists(t => t.fromNode.Equals(((BehaviourTree)currentElem).nodes[i])))
                        decoratorWithOneChild = 1;
                    break;
                }
            }
        }

        return new int[]
        {
            clickedOnElement,
            clickedOnWindow,
            clickedOnLeaf,
            decoratorWithOneChild,
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
        widthVariant = 0;
        var name = "Node Editor";

        if (currentElem != null)
        {
            ShowButtonRecursive(Styles.TopBarButton, currentElem, "Node Editor");
            if (currentElem != null)
                name = currentElem.elementName;
        }

        var labelWidth = 25 + name.ToCharArray().Length * 6;
        GUI.Label(new Rect(widthVariant, 0, labelWidth, 20), name);
    }

    /// <summary>
    /// Draws the Options button
    /// </summary>
    private void ShowOptions()
    {
        if (GUI.Button(new Rect(position.width - 60, 0, 50, 20), "...", Styles.OptionsButton))
        {
            // Set menu items
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Save Element to file"), false, SaveElem, currentElem);
            menu.AddItem(new GUIContent("Export Code"), false, ExportCode, currentElem);

            menu.ShowAsContext();
        }
    }

    /// <summary>
    /// Shows the buttons with the names of the elements in order of hierarchy
    /// </summary>
    /// <param name="style"></param>
    /// <param name="elem"></param>
    /// <param name="name"></param>
    private void ShowButtonRecursive(GUIStyle style, ClickableElement elem, string name)
    {
        if (elem.parent != null)
        {
            ShowButtonRecursive(style, elem.parent, name);
            name = elem.parent.elementName;
        }
        var buttonWidth = 25 + name.ToCharArray().Length * 6;
        if (GUI.Button(new Rect(widthVariant, 0, buttonWidth, 20), name, style))
        {
            currentElem = elem.parent;
        }
        widthVariant += buttonWidth;
        GUI.Label(new Rect(widthVariant, 0, 15, 20), ">", new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.UpperLeft
        });
        widthVariant += 12;
    }

    /// <summary>
    /// Configures the Texture using the sprite resources and returns it
    /// </summary>
    /// <param name="typeOfItem"></param>
    /// <param name="elem"></param>
    /// <returns></returns>
    private Texture2D GetBackground(GUIElement elem)
    {
        var isFocused = elem.isFocused;
        Color col = Color.white;
        Texture2D originalTexture = null;
        int type;

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

            // FSM Node
            case nameof(StateNode):
                type = (int)((StateNode)elem).type;

                // Nodo normal
                if (((StateNode)elem).subElem == null)
                {
                    switch (type)
                    {
                        case 0:
                            originalTexture = Resources.Load<Texture2D>("Def_Node_Rect");
                            col = Color.grey;
                            break;
                        case 1:
                            originalTexture = Resources.Load<Texture2D>("Entry_Rect");
                            col = Color.green;
                            break;
                        case 2:
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
                    switch (type)
                    {
                        case 0:
                            originalTexture = Resources.Load<Texture2D>("Def_Sub_Rect");
                            col = Color.grey;
                            break;
                        case 1:
                            originalTexture = Resources.Load<Texture2D>("Entry_Sub_Rect");
                            col = Color.green;
                            break;
                        case 2:
                            originalTexture = Resources.Load<Texture2D>("Unconnected_Sub_Rect");
                            col = Color.red;
                            break;
                        default:
                            col = Color.white;
                            break;
                    }
                }
                break;

            // BehaviourTree Node
            case nameof(BehaviourNode):
                type = (int)((BehaviourNode)elem).type;

                switch (type)
                {
                    case 0:
                        originalTexture = Resources.Load<Texture2D>("Sequence_Rect");
                        col = Color.yellow;
                        break;
                    case 1:
                        originalTexture = Resources.Load<Texture2D>("Selector_Rect");
                        col = new Color(1, 0.5f, 0, 1); //orange
                        break;
                    case 2:
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
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        originalTexture = Resources.Load<Texture2D>("Decorator_Rect"); //Hacer un rombo gris
                                                                                       //col = Color.grey;
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
                if (((BehaviourTree)currentElem).ConnectedCheck(selectednode, elem) || selectednode.Equals(elem) || ((BehaviourNode)elem).type == BehaviourNode.behaviourType.Leaf || ((BehaviourNode)elem).type >= BehaviourNode.behaviourType.LoopN && ((BehaviourTree)currentElem).connections.Exists(t => t.fromNode.Equals(elem)))
                {
                    //Make it look transparent when not connectable to connect mode
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i].a = (byte)(pixels[i].a * 64 / 255);
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
        if (currentElem is FSM)
        {
            ((FSM)currentElem).states[id].DrawWindow(this);
            if (((FSM)currentElem).states[id].subElem != null)
                ((FSM)currentElem).states[id].subElem.elementName = ((FSM)currentElem).states[id].nodeName;
        }
        if (currentElem is BehaviourTree)
        {
            ((BehaviourTree)currentElem).nodes[id].DrawWindow(this);
            if (((BehaviourTree)currentElem).nodes[id].subElem != null)
                ((BehaviourTree)currentElem).nodes[id].subElem.elementName = ((BehaviourTree)currentElem).nodes[id].nodeName;
        }

        GUI.DragWindow();
    }

    /// <summary>
    /// The DrawElementWindow
    /// </summary>
    /// <param name="id"></param>
    void DrawElementWindow(int id)
    {
        Elements[id].DrawWindow();
        GUI.DragWindow();
    }

    /// <summary>
    /// The DrawTransitionBox
    /// </summary>
    /// <param name="id"></param>
    void DrawTransitionBox(int id)
    {
        ((FSM)currentElem).transitions[id - MAX_N_STATES].DrawBox();
        GUI.DragWindow();
    }

    /// <summary>
    /// Performs an action depending on the given order
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
            case "makeTransition":
                MakeTransition(index);
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
    /// Exporta el código de un elemento si no hay errores
    /// </summary>
    void ExportCode(object elem)
    {
        if (errors.Count == 0)
        {
            NodeEditorUtilities.GenerateElemScript((ClickableElement)elem);
        }
        else
        {
            PopupWindow.InitExport(this);
        }
    }

    /// <summary>
    /// Guarda el elemento
    /// </summary>
    void SaveElem(object elem)
    {
        NodeEditorUtilities.GenerateElemXML((ClickableElement)elem);
    }

    /// <summary>
    /// Carga el elemento
    /// </summary>
    void LoadElem()
    {
        XMLElement loadedXML = NodeEditorUtilities.LoadSavedData();

        if (loadedXML != null)
        {
            var currentBackup = currentElem;

            switch (loadedXML.elemType)
            {
                case nameof(FSM):
                case nameof(BehaviourTree):
                    PasteElem(mousePos.x, mousePos.y, loadedXML);
                    break;
            }

            currentElem = currentBackup;
        }
    }

    /// <summary>
    /// Deletes the given element
    /// </summary>
    /// <param name="type"></param>
    /// <param name="selectIndex"></param>
    public void Delete(GUIElement elem)
    {
        switch (elem.GetType().ToString())
        {
            case nameof(StateNode):
                StateNode stateNode = (StateNode)elem;
                ((FSM)currentElem).DeleteNode(stateNode);

                focusedObj = null;
                break;

            case nameof(BehaviourNode):
                BehaviourNode behaviourNode = (BehaviourNode)elem;
                ((BehaviourTree)currentElem).DeleteNode(behaviourNode);

                focusedObj = null;
                break;
            case nameof(TransitionGUI):
                TransitionGUI transition = (TransitionGUI)elem;
                ((FSM)currentElem).DeleteTransition(transition);

                focusedObj = null;
                break;

            case nameof(FSM):
                FSM fsm = (FSM)elem;
                Elements.Remove(fsm);

                focusedObj = null;
                break;

            case nameof(BehaviourTree):
                BehaviourTree bt = (BehaviourTree)elem;
                Elements.Remove(bt);

                focusedObj = null;
                break;
        }
    }

    /// <summary>
    /// Draws a stylized bezier curve from start to end
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="isFocused"></param>
    public static void DrawNodeCurve(Rect start, Rect end, bool isFocused, bool hasCouple = false)
    {
        // Check which sides to put the curve on
        float ang = Vector2.SignedAngle((end.position - start.position), Vector2.right);
        Vector3 direction = Vector3.up;

        if (ang > -45 && ang <= 45)
        {
            start.x += start.width / 2;
            end.x -= end.width / 2;
            direction = Vector3.right;

            if (hasCouple)
            {
                start.y += coupleTransitionsOffset;
                end.y += coupleTransitionsOffset;
            }
        }
        else if (ang > 45 && ang <= 135)
        {
            start.y -= start.height / 2;
            end.y += end.height / 2;
            direction = Vector3.down;

            if (hasCouple)
            {
                start.x += coupleTransitionsOffset;
                end.x += coupleTransitionsOffset;
            }
        }
        else if ((ang > 135 && ang <= 180) || (ang > -180 && ang <= -135))
        {
            start.x -= start.width / 2;
            end.x += end.width / 2;
            direction = Vector3.left;

            if (hasCouple)
            {
                start.y -= coupleTransitionsOffset;
                end.y -= coupleTransitionsOffset;
            }
        }
        else if (ang > -135 && ang <= -45)
        {
            start.y += start.height / 2;
            end.y -= end.height / 2;
            direction = Vector3.up;

            if (hasCouple)
            {
                start.x -= coupleTransitionsOffset;
                end.x -= coupleTransitionsOffset;
            }
        }

        // Draw curve

        // Curve parameters
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
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
        int focusFactor = 3;

        if (isFocused)
        {
            shadowCol = new Color(1, 1, 1, 0.1f);
            focusFactor = 10;
        }

        for (int i = 0; i < focusFactor; i++)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);

            // Draw arrow
            Handles.DrawBezier(pos1, endPos, pos1, endPos, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier(pos2, endPos, pos2, endPos, shadowCol, null, (i + 1) * 5);
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);

        // Draw arrow
        Handles.DrawBezier(pos1, endPos, pos1, endPos, Color.black, null, 1);
        Handles.DrawBezier(pos2, endPos, pos2, endPos, Color.black, null, 1);
    }

    private void PasteElem(float posX, float posY, XMLElement elem)
    {
        ClickableElement newElem;

        switch (elem.elemType)
        {
            case nameof(FSM):
                newElem = CreateInstance<FSM>();
                ((FSM)newElem).InitFSM(null, currentElem, posX, posY);
                break;
            case nameof(BehaviourTree):
                newElem = CreateInstance<BehaviourTree>();
                ((BehaviourTree)newElem).InitBehaviourTree(currentElem, posX, posY);
                break;
            default:
                newElem = null;
                break;
        }

        if (newElem != null)
        {
            newElem.identificator = elem.Id;
            newElem.elementName = elem.name;

            if (currentElem is null)
            {
                Elements.Add(newElem);
            }

            if (currentElem is FSM)
            {
                StateNode node = CreateInstance<StateNode>();
                node.InitStateNode(2, newElem.windowRect.position.x, newElem.windowRect.position.y, newElem);
                node.identificator = elem.Id;

                if (elem.secondType.Equals(StateNode.stateType.Entry.ToString()))
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
                node.InitBehaviourNode(2, newElem.windowRect.x, newElem.windowRect.y, newElem);

                ((BehaviourTree)currentElem).nodes.Add(node);

                if (selectednode != null)
                {
                    TransitionGUI transition = CreateInstance<TransitionGUI>();
                    transition.InitTransitionGUI("", selectednode, node);

                    ((BehaviourTree)currentElem).connections.Add(transition);

                    selectednode = node;
                }
            }

            foreach (XMLElement node in elem.nodes)
            {
                currentElem = newElem;

                switch (node.elemType)
                {
                    case nameof(FSM):
                    case nameof(BehaviourTree):
                        PasteElem(node.windowPosX, node.windowPosY, node);
                        break;
                    case nameof(StateNode):
                        StateNode state = CreateInstance<StateNode>();
                        state.InitStateNode(2, node.windowPosX, node.windowPosY);
                        state.identificator = node.Id;
                        state.nodeName = node.name;

                        if (node.secondType.Equals(StateNode.stateType.Entry.ToString()))
                        {
                            ((FSM)currentElem).AddEntryState(state);
                        }
                        else
                        {
                            ((FSM)currentElem).states.Add(state);
                        }
                        break;
                    case nameof(BehaviourNode):
                        PasteTreeNode(node, true, (BehaviourTree)currentElem);
                        break;
                    default:
                        Debug.LogError("Wrong content in saved data");
                        break;
                }
            }

            foreach (string transString in elem.transitions)
            {
                string[] str = transString.Split('#');

                BaseNode node1 = ((FSM)newElem).states.Where(n => n.identificator == str[1]).FirstOrDefault();
                BaseNode node2 = ((FSM)newElem).states.Where(n => n.identificator == str[2]).FirstOrDefault();

                TransitionGUI transition = CreateInstance<TransitionGUI>();
                transition.InitTransitionGUI(str[0], node1, node2);

                ((FSM)newElem).AddTransition(transition);
            }
        }
        else
        {
            Debug.LogError("Wrong content in saved data");
        }
    }

    private void PasteTreeNode(XMLElement state, bool isRoot, BehaviourTree currentTree)
    {
        switch (state.elemType)
        {
            case nameof(FSM):
            case nameof(BehaviourTree):
                PasteElem(state.windowPosX, state.windowPosY, state);
                break;
            case nameof(BehaviourNode):
                BehaviourNode nodeBT = CreateInstance<BehaviourNode>();
                nodeBT.InitBehaviourNode((int)Enum.Parse(typeof(BehaviourNode.behaviourType), state.secondType), state.windowPosX, state.windowPosY);
                nodeBT.nodeName = state.name;
                nodeBT.NProperty = state.NProperty;

                currentTree.nodes.Add(nodeBT);

                if (!isRoot)
                {
                    TransitionGUI transition = CreateInstance<TransitionGUI>();
                    transition.InitTransitionGUI("", selectednode, nodeBT);

                    currentTree.connections.Add(transition);
                }
                else
                {
                    nodeBT.isRootNode = true;
                }

                foreach (XMLElement childState in state.nodes)
                {
                    selectednode = nodeBT;

                    PasteTreeNode(childState, false, currentTree);
                }
                break;
            default:
                Debug.LogError("Wrong content in saved data");
                break;
        }
    }

    /// <summary>
    /// Creates a FSM
    /// </summary>
    private void CreateFSM(int nodeIndex, float posX, float posY)
    {
        FSM newFSM;

        StateNode entryNode = CreateInstance<StateNode>();
        entryNode.InitStateNode(1, 50, 50);

        newFSM = CreateInstance<FSM>();
        newFSM.InitFSM(entryNode, currentElem, posX, posY);

        if (currentElem is null)
        {
            Elements.Add(newFSM);
        }

        if (currentElem is FSM)
        {
            StateNode node = CreateInstance<StateNode>();
            node.InitStateNode(2, newFSM.windowRect.position.x, newFSM.windowRect.position.y, newFSM);

            if (!((FSM)currentElem).hasEntryState)
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
            node.InitBehaviourNode(2, newFSM.windowRect.x, newFSM.windowRect.y, newFSM);

            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeBehaviourMode = true;
        }
    }

    /// <summary>
    /// Creates a BehaviourTree
    /// </summary>
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
            StateNode node = CreateInstance<StateNode>();
            node.InitStateNode(2, newBT.windowRect.position.x, newBT.windowRect.position.y, newBT);

            if (!((FSM)currentElem).hasEntryState)
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
            node.InitBehaviourNode(2, newBT.windowRect.x, newBT.windowRect.y, newBT);

            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeBehaviourMode = true;
        }
    }

    /// <summary>
    /// Creates a Node
    /// </summary>
    private void CreateNode(float posX, float posY)
    {
        StateNode node = CreateInstance<StateNode>();
        node.InitStateNode(2, posX, posY);

        if (!((FSM)currentElem).hasEntryState)
        {
            ((FSM)currentElem).AddEntryState(node);
        }
        else
        {
            ((FSM)currentElem).states.Add(node);
        }
    }

    /// <summary>
    /// Creates a Sequence Node
    /// </summary>
    private void CreateSequence(int nodeIndex, float posX = 50, float posY = 50)
    {
        BehaviourNode node = CreateInstance<BehaviourNode>();
        node.InitBehaviourNode(0, posX, posY);

        if (nodeIndex > -1)
        {
            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeBehaviourMode = true;
        }
        else
        {
            node.isRootNode = true;
            ((BehaviourTree)currentElem).nodes.Add(node);
        }
    }

    /// <summary>
    /// Creates a Selector Node
    /// </summary>
    private void CreateSelector(int nodeIndex, float posX = 50, float posY = 50)
    {
        BehaviourNode node = CreateInstance<BehaviourNode>();
        node.InitBehaviourNode(1, posX, posY);

        if (nodeIndex > -1)
        {
            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeBehaviourMode = true;
        }
        else
        {
            node.isRootNode = true;
            ((BehaviourTree)currentElem).nodes.Add(node);
        }
    }

    /// <summary>
    /// Creates a Leaf Node
    /// </summary>
    /// <param name="type"></param>
    private void CreateLeafNode(int type, int nodeIndex, float posX = 50, float posY = 50)
    {
        BehaviourNode node = CreateInstance<BehaviourNode>();
        node.InitBehaviourNode(type, posX, posY);

        selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
        toCreateNode = node;
        makeBehaviourMode = true;
    }

    /// <summary>
    /// Enter MakeTransition mode (mouse carries the other end of the transition until you click somewhere else)
    /// </summary>
    private void MakeTransition(int selectIndex)
    {
        makeTransitionMode = true;

        if (currentElem is FSM)
            selectednode = ((FSM)currentElem).states[selectIndex];

        if (currentElem is BehaviourTree)
            selectednode = ((BehaviourTree)currentElem).nodes[selectIndex];
    }

    /// <summary>
    /// Popup appears that will delete the clicked node if accepted
    /// </summary>
    private void DeleteNode(int selectIndex)
    {
        if (currentElem is FSM)
        {
            PopupWindow.InitDelete(this, ((FSM)currentElem).states[selectIndex], ((FSM)currentElem).states[selectIndex].GetTypeString());
        }

        if (currentElem is BehaviourTree)
        {
            PopupWindow.InitDelete(this, ((BehaviourTree)currentElem).nodes[selectIndex], ((BehaviourTree)currentElem).nodes[selectIndex].GetTypeString());
        }

        if (currentElem is null)
        {
            PopupWindow.InitDelete(this, Elements[selectIndex], Elements[selectIndex].GetTypeString());
        }
    }

    /// <summary>
    /// Popup appears that will delete the clicked transition if accepted
    /// </summary>
    private void DeleteTransition(int selectIndex)
    {
        PopupWindow.InitDelete(this, ((FSM)currentElem).transitions[selectIndex], ((FSM)currentElem).transitions[selectIndex].GetTypeString());
    }

    /// <summary>
    /// Converts the clicked node into the EntryState
    /// </summary>
    private void ConvertToEntry(int selectIndex)
    {
        ((FSM)currentElem).SetAsEntry(((FSM)currentElem).states[selectIndex]);
    }

    /// <summary>
    /// Disconnects the clicked node
    /// </summary>
    private void DisconnectNode(int selectIndex)
    {
        BehaviourNode selNode = ((BehaviourTree)currentElem).nodes[selectIndex];

        foreach (TransitionGUI tr in ((BehaviourTree)currentElem).connections.FindAll(t => t.toNode.Equals(selNode)))
        {
            ((BehaviourTree)currentElem).DeleteConnection(tr);
        }

        selNode.isRootNode = true;
    }

    /// <summary>
    /// Enter connect mode
    /// </summary>
    /// <param name="nodeIndex"></param>
    private void ConnectNode(int selectIndex)
    {
        selectednode = ((BehaviourTree)currentElem).nodes[selectIndex];
        makeConnectionMode = true;
    }

    /// <summary>
    /// Shows on the bottom left the highest priority error currently happening
    /// </summary>
    private void ShowErrorByPriority()
    {
        var maxPriorityError = "";
        var currentPriority = 0;

        foreach (var error in errors)
        {
            if (error.Value > currentPriority)
            {
                maxPriorityError = error.Key;
                currentPriority = error.Value;
            }
        }

        if (errors.Count > 1)
            maxPriorityError += " (and " + (errors.Count - 1) + " more errors)";

        EditorGUILayout.LabelField(maxPriorityError, new GUIStyle(Styles.ErrorPrompt)
        {
            contentOffset = new Vector2(0, position.height - 20)
        });
    }

    /// <summary>
    /// Add an error that is happening right now
    /// </summary>
    /// <param name="error"></param>
    public void AddError(Enums.Errors error)
    {
        if (!errors.ContainsKey(Enums.EnumToString(error)))
            errors.Add(Enums.EnumToString(error), (int)error);
    }

    /// <summary>
    /// Remove an error that is no longer happening
    /// </summary>
    /// <param name="error"></param>
    public void RemoveError(Enums.Errors error)
    {
        if (errors.ContainsKey(Enums.EnumToString(error)))
            errors.Remove(Enums.EnumToString(error));
    }
}
