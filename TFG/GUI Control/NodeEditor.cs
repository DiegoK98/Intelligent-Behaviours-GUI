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

    /// <summary>
    /// The ShowEditor
    /// </summary>
    [MenuItem("Window/Node Editor")]
    static void ShowEditor()
    {
        PopupWindow.ClosePopup(GetWindow<PopupWindow>());
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
                        if (!clickedOnWindow && !clickedOnTransition && ((BehaviourTree)currentElem).nodes.Count == 0)
                        {
                            menu.AddItem(new GUIContent("Add Sequence"), false, ContextCallback, new string[] { "Sequence", selectIndex.ToString() });
                            menu.AddItem(new GUIContent("Add Selector"), false, ContextCallback, new string[] { "Selector", selectIndex.ToString() });
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

                            menu.ShowAsContext();
                            e.Use();
                        }
                        else
                        {
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

                    if (Event.current.clickCount == 2 && ((StateNode)focusedObj).elem != null)
                    {
                        currentElem = ((StateNode)focusedObj).elem;
                        e.Use();
                    }
                }
                else if (clickedOnWindow && currentElem is BehaviourTree)
                {
                    ((BehaviourTree)currentElem).nodes[selectIndex].isFocused = true;
                    focusedObj = ((BehaviourTree)currentElem).nodes[selectIndex];

                    if (Event.current.clickCount == 2 && ((BehaviourNode)focusedObj).elem != null)
                    {
                        currentElem = ((BehaviourNode)focusedObj).elem;
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
                            TransitionsGUI transition = new TransitionsGUI("New Transition " + ((FSM)currentElem).transitions.Count, selectednode, ((FSM)currentElem).states[selectIndex]);

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

                        TransitionsGUI transition = new TransitionsGUI("", selectednode, toCreateNode);

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
                            TransitionsGUI transition = new TransitionsGUI("", ((BehaviourTree)currentElem).nodes[selectIndex], selectednode);
                            ((BehaviourTree)currentElem).connections.Add(transition);

                            ((BehaviourTree)currentElem).nodes[selectIndex].isRootNode = false;
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
                case KeyCode.S:
                    if (GUIUtility.keyboardControl == 0)
                    {
                        foreach (ClickableElement elem in Elements)
                        {
                            NodeEditorUtilities.CreateElem(elem);
                        }
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
                GUIStyle style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
                style.normal.background = GetBackground(Elements[i]);

                Elements[i].windowRect = GUI.Window(i, Elements[i].windowRect, DrawElementWindow, Elements[i].GetTypeString(), style);
            }
        }

        if (currentElem is FSM)
        {
            for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
            {
                GUIStyle style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
                style.normal.background = GetBackground(((FSM)currentElem).states[i]);

                ((FSM)currentElem).states[i].windowRect = GUI.Window(i, ((FSM)currentElem).states[i].windowRect, DrawNodeWindow, ((FSM)currentElem).states[i].GetTypeString(), style);
            }

            for (int i = 0; i < ((FSM)currentElem).transitions.Count; i++)
            {
                TransitionsGUI elem = ((FSM)currentElem).transitions[i];

                Vector2 pos = new Vector2(elem.fromNode.windowRect.center.x + (elem.toNode.windowRect.x - elem.fromNode.windowRect.x) / 2,
                                          elem.fromNode.windowRect.center.y + (elem.toNode.windowRect.y - elem.fromNode.windowRect.y) / 2);
                Rect textBox = new Rect(pos.x - 75, pos.y - 15, TransitionsGUI.width, TransitionsGUI.height);

                GUIStyle style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
                style.normal.background = GetBackground(elem);

                elem.textBox = GUI.Window(i + MAX_N_STATES, textBox, DrawTransitionBox, "", style);
            }
        }

        if (currentElem is BehaviourTree)
        {
            for (int i = 0; i < ((BehaviourTree)currentElem).nodes.Count; i++)
            {
                GUIStyle style = new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 10
                };
                style.normal.background = GetBackground(((BehaviourTree)currentElem).nodes[i]);

                ((BehaviourTree)currentElem).nodes[i].windowRect = GUI.Window(i, ((BehaviourTree)currentElem).nodes[i].windowRect, DrawNodeWindow, ((BehaviourTree)currentElem).nodes[i].GetTypeString(), style);
            }
        }

        EndWindows();

        #endregion
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
        GUIStyle style = GUI.skin.button;
        style.hover.textColor = Color.grey;
        style.alignment = TextAnchor.MiddleLeft;

        // Top Bar
        widthVariant = 0;
        var name = "Node Editor";

        if (currentElem != null)
        {
            ShowButtonRecursive(style, currentElem, "Node Editor");
            if (currentElem != null)
                name = currentElem.elementName;
        }

        var labelWidth = 25 + name.ToCharArray().Length * 6;
        GUI.Label(new Rect(widthVariant, 0, labelWidth, 20), name, new GUIStyle(GUI.skin.label));
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
        GUI.Label(new Rect(widthVariant, 0, 15, 20), ">", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft });
        widthVariant += 12;
    }

    /// <summary>
    /// The OnDestroy
    /// </summary>
    private void OnDestroy()
    {
        PopupWindow.ClosePopup(GetWindow<PopupWindow>());
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
                if (((StateNode)elem).elem == null)
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
                        if (((BehaviourNode)elem).elem == null) //Es un nodo normal
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
            case nameof(TransitionsGUI):
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
            ((FSM)currentElem).states[id].DrawWindow(this);
        if (currentElem is BehaviourTree)
            ((BehaviourTree)currentElem).nodes[id].DrawWindow(this);
        GUI.DragWindow();
    }

    /// <summary>
    /// The DrawElementWindow
    /// </summary>
    /// <param name="id"></param>
    void DrawElementWindow(int id)
    {
        Elements[id].DrawWindow(this);
        GUI.DragWindow();
    }

    /// <summary>
    /// The DrawTransitionBox
    /// </summary>
    /// <param name="id"></param>
    void DrawTransitionBox(int id)
    {
        ((FSM)currentElem).transitions[id - MAX_N_STATES].DrawBox(this);
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
                CreateFSM(index);
                break;
            case "BT":
                CreateBT(index);
                break;
            case "Node":
                CreateNode();
                break;
            case "Sequence":
                CreateSequence(index);
                break;
            case "Selector":
                CreateSelector(index);
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
            case nameof(TransitionsGUI):
                TransitionsGUI transition = (TransitionsGUI)elem;
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
    public static void DrawNodeCurve(Rect start, Rect end, bool isFocused)
    {
        // Check which sides to put the curve on
        float ang = Vector2.SignedAngle((end.position - start.position), Vector2.right);
        Vector3 direction = Vector3.up;

        if (ang > -45 && ang <= 45)
        {
            start.x += start.width / 2;
            end.x -= end.width / 2;
            direction = Vector3.right;
        }
        else if (ang > 45 && ang <= 135)
        {
            start.y -= start.height / 2;
            end.y += end.height / 2;
            direction = Vector3.down;
        }
        else if ((ang > 135 && ang <= 180) || (ang > -180 && ang <= -135))
        {
            start.x -= start.width / 2;
            end.x += end.width / 2;
            direction = Vector3.left;
        }
        else if (ang > -135 && ang <= -45)
        {
            start.y += start.height / 2;
            end.y -= end.height / 2;
            direction = Vector3.up;
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

    /// <summary>
    /// Creates a FSM
    /// </summary>
    private void CreateFSM(int nodeIndex)
    {
        StateNode entryNode = new StateNode(1, 50, 50);

        ClickableElement newFSM = new FSM(entryNode, currentElem, mousePos.x, mousePos.y);

        if (currentElem is null)
        {
            Elements.Add(newFSM);
        }

        if (currentElem is FSM)
        {
            StateNode node = new StateNode(2, newFSM.windowRect.position.x, newFSM.windowRect.position.y, newFSM);

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
            BehaviourNode node = new BehaviourNode(2, newFSM)
            {
                windowRect = newFSM.windowRect
            };

            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeBehaviourMode = true;
        }
    }

    /// <summary>
    /// Creates a BehaviourTree
    /// </summary>
    private void CreateBT(int nodeIndex)
    {
        ClickableElement newBT = new BehaviourTree(currentElem, mousePos.x, mousePos.y);

        if (currentElem is null)
        {
            Elements.Add(newBT);
        }

        if (currentElem is FSM)
        {
            var type = 1;
            if (((FSM)currentElem).states.Exists(n => n.type == StateNode.stateType.Entry))
            {
                type = 2;
            }

            StateNode node = new StateNode(type, newBT.windowRect.position.x, newBT.windowRect.position.y, newBT);

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
            BehaviourNode node = new BehaviourNode(2, newBT)
            {
                windowRect = newBT.windowRect
            };

            selectednode = ((BehaviourTree)currentElem).nodes[nodeIndex];
            toCreateNode = node;
            makeBehaviourMode = true;
        }
    }

    /// <summary>
    /// Creates a Node
    /// </summary>
    private void CreateNode()
    {
        StateNode node = new StateNode(2, mousePos.x, mousePos.y);

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
    private void CreateSequence(int nodeIndex)
    {
        BehaviourNode node = new BehaviourNode(0, mousePos.x, mousePos.y);

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
    private void CreateSelector(int nodeIndex)
    {
        BehaviourNode node = new BehaviourNode(1, mousePos.x, mousePos.y);

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
    private void CreateLeafNode(int type, int nodeIndex)
    {
        BehaviourNode node = new BehaviourNode(type, mousePos.x, mousePos.y);

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

        foreach (TransitionsGUI tr in ((BehaviourTree)currentElem).connections.FindAll(t => t.toNode.Equals(selNode)))
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
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.contentOffset = new Vector2(0, position.height - 20);

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

        EditorGUILayout.LabelField(maxPriorityError, style);
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
