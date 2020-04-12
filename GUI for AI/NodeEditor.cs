using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class NodeEditor : EditorWindow
{
    private Vector2 mousePos;

    private BaseNode selectednode;
    private BaseNode toCreateNode;

    public GUIElement focusedObj;

    private bool makeTransitionMode = false;
    private bool makeBehaviourMode = false;

    public List<ClickableElement> Elements = new List<ClickableElement>();

    public ClickableElement currentElem;
    private readonly int MAX_N_STATES = 100;

    private Dictionary<string, int> errors = new Dictionary<string, int>();

    public bool popupShown;

    private float widthVariant;

    [MenuItem("Window/Node Editor")]
    static void ShowEditor()
    {
        PopupWindow.ClosePopup(GetWindow<PopupWindow>());
        GetWindow<NodeEditor>();
    }

    private void OnGUI()
    {
        Event e = Event.current;

        mousePos = e.mousePosition;

        ShowTopBar();
        if (e.button == 1 && !makeTransitionMode && !makeBehaviourMode)
        {
            if (e.type == EventType.MouseDown)
            {
                bool clickedOnWindow = false;
                bool clickedOnLeaf = false;
                bool clickedOnTransition = false;
                int selectIndex = -1;

                if (currentElem is FSM)
                {
                    for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
                    {
                        if (((FSM)currentElem).states[i].windowRect.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }

                    for (int i = 0; i < ((FSM)currentElem).transitions.Count; i++)
                    {
                        if (((FSM)currentElem).transitions[i].textBox.Contains(mousePos))
                        {
                            clickedOnTransition = true;
                            break;
                        }
                    }
                }

                if (currentElem is BehaviourTree)
                {
                    for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
                    {
                        if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnWindow = true;
                            if (((BehaviourTree)currentElem).states[i].type == BehaviourNode.behaviourType.Leaf) clickedOnLeaf = true;
                            break;
                        }
                    }
                }

                if (!clickedOnWindow && !clickedOnTransition && currentElem is null)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, "FSM");
                    menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, "BT");

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (!clickedOnWindow && !clickedOnTransition && currentElem is FSM)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Add Node"), false, ContextCallback, "Node");
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, "FSM");
                    menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, "BT");

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (!clickedOnWindow && !clickedOnTransition && currentElem is BehaviourTree && ((BehaviourTree)currentElem).states.Count == 0)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Add Sequence"), false, ContextCallback, "Sequence");
                    menu.AddItem(new GUIContent("Add Selector"), false, ContextCallback, "Selector");

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (clickedOnTransition)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Delete Transition"), false, ContextCallback, "deleteTransition");

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (clickedOnWindow && currentElem is FSM)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Make Transition"), false, ContextCallback, "makeTransition");
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "deleteNode");

                    if (!((FSM)currentElem).isEntryState(((FSM)currentElem).states[selectIndex]))
                    {
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Convert to Entry State"), false, ContextCallback, "entryState");
                    }

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (clickedOnWindow && currentElem is BehaviourTree)
                {
                    GenericMenu menu = new GenericMenu();

                    if (!clickedOnLeaf)
                    {
                        menu.AddItem(new GUIContent("Add Sequence"), false, ContextCallback, "Sequence");
                        menu.AddItem(new GUIContent("Add Selector"), false, ContextCallback, "Selector");
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Add Leaf Node"), false, ContextCallback, "leafNode");
                        menu.AddItem(new GUIContent("Add FSM"), false, ContextCallback, "FSM");
                        menu.AddItem(new GUIContent("Add BT"), false, ContextCallback, "BT");

                        menu.AddSeparator("");
                    }

                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "deleteNode");

                    menu.ShowAsContext();
                    e.Use();
                }
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDown)
        {
            bool clickedOnElement = false;
            bool clickedOnWindow = false;
            bool clickedOnTransition = false;
            int selectIndex = -1;

            if (currentElem is null)
            {
                for (int i = 0; i < Elements?.Count; i++)
                {
                    if (Elements[i].windowRect.Contains(mousePos))
                    {
                        selectIndex = i;
                        clickedOnElement = true;
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
                        clickedOnWindow = true;
                        break;
                    }
                }

                for (int i = 0; i < ((FSM)currentElem).transitions.Count; i++)
                {
                    if (((FSM)currentElem).transitions[i].textBox.Contains(mousePos))
                    {
                        selectIndex = i;
                        clickedOnTransition = true;
                        break;
                    }
                }
            }

            if (currentElem is BehaviourTree)
            {
                for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
                {
                    if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                    {
                        selectIndex = i;
                        clickedOnWindow = true;
                        break;
                    }
                }
            }

            if (clickedOnElement)
            {
                GUI.FocusControl(null);
                if (focusedObj != null) focusedObj.isFocused = false;
                Elements[selectIndex].isFocused = true;
                focusedObj = Elements[selectIndex];

                if (Event.current.clickCount == 2)
                {
                    currentElem = Elements[selectIndex];
                    e.Use();
                }

                //if (GUI.GetNameOfFocusedControl() is null)
                //{
                //    e.Use();
                //}
            }
            else if (clickedOnTransition && currentElem is FSM)
            {
                GUI.FocusControl(null);
                if (focusedObj != null) focusedObj.isFocused = false;
                ((FSM)currentElem).transitions[selectIndex].isFocused = true;
                focusedObj = ((FSM)currentElem).transitions[selectIndex];

                //if (GUI.GetNameOfFocusedControl() is null)
                //{
                //    e.Use();
                //}
            }
            else if (clickedOnWindow && currentElem is FSM)
            {
                GUI.FocusControl(null);
                if (focusedObj != null) focusedObj.isFocused = false;
                ((FSM)currentElem).states[selectIndex].isFocused = true;
                focusedObj = ((FSM)currentElem).states[selectIndex];

                if (Event.current.clickCount == 2 && ((StateNode)focusedObj).elem != null)
                {
                    currentElem = ((StateNode)focusedObj).elem;
                    e.Use();
                }

                //if (GUI.GetNameOfFocusedControl() is null)
                //{
                //    e.Use();
                //}
            }
            else if (clickedOnWindow && currentElem is BehaviourTree)
            {
                GUI.FocusControl(null);
                if (focusedObj != null) focusedObj.isFocused = false;
                ((BehaviourTree)currentElem).states[selectIndex].isFocused = true;
                focusedObj = ((BehaviourTree)currentElem).states[selectIndex];

                if (Event.current.clickCount == 2 && ((BehaviourNode)focusedObj).elem != null)
                {
                    currentElem = ((BehaviourNode)focusedObj).elem;
                    e.Use();
                }

                //if (GUI.GetNameOfFocusedControl() is null)
                //{
                //    e.Use();
                //}
            }
            else
            {
                GUI.FocusControl(null);
                if (focusedObj != null) focusedObj.isFocused = false;

                focusedObj = null;

                e.Use();
            }

            if (makeTransitionMode && currentElem is FSM)
            {
                if (clickedOnWindow && !((FSM)currentElem).states[selectIndex].Equals(selectednode))
                {
                    Transition transition = new Transition("New Transition " + ((FSM)currentElem).transitions.Count, selectednode, ((FSM)currentElem).states[selectIndex]);

                    ((FSM)currentElem).AddTransition(transition);

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

            if (makeBehaviourMode && currentElem is BehaviourTree)
            {
                toCreateNode.windowRect.position = new Vector2(mousePos.x, mousePos.y);
                ((BehaviourTree)currentElem).states.Add((BehaviourNode)toCreateNode);

                Transition transition = new Transition("", selectednode, toCreateNode);

                ((BehaviourTree)currentElem).transitions.Add(transition);

                makeBehaviourMode = false;
                selectednode = null;
                toCreateNode = null;

                e.Use();
            }
        }

        if ((makeTransitionMode || makeBehaviourMode) && selectednode != null)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            Rect nodeRect = new Rect(selectednode.windowRect);

            DrawNodeCurve(nodeRect, mouseRect, true);

            Repaint();
        }

        if (currentElem is FSM)
        {
            foreach (StateNode n in ((FSM)currentElem).states)
            {
                n.DrawCurves();
            }

            if (!((FSM)currentElem).hasEntryState)
            {
                if (!errors.ContainsKey(Enums.EnumToString(Enums.Errors.NoEntryState)))
                    errors.Add(Enums.EnumToString(Enums.Errors.NoEntryState), (int)Enums.Errors.NoEntryState);
            }
        }

        if (currentElem is BehaviourTree)
        {
            ((BehaviourTree)currentElem).DrawCurves();
        }

        ShowErrorByPriority();

        BeginWindows();

        if (currentElem is null)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 10;
                style.normal.background = GetBackground(Elements[i].windowRect.size, "Elements", (int)Elements[i].type, Elements[i].isFocused);

                Elements[i].windowRect = GUI.Window(i, Elements[i].windowRect, DrawElementWindow, "FSM", style);
            }
        }
        else if (currentElem is FSM)
        {
            for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
            {
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 10;
                style.normal.background = GetBackground(((FSM)currentElem).states[i].windowRect.size, "FSM", (int)((FSM)currentElem).states[i].type, ((FSM)currentElem).states[i].isFocused);

                ((FSM)currentElem).states[i].windowRect = GUI.Window(i, ((FSM)currentElem).states[i].windowRect, DrawNodeWindow, "Node", style);
            }

            for (int i = 0; i < ((FSM)currentElem).transitions.Count; i++)
            {
                Transition elem = ((FSM)currentElem).transitions[i];

                Vector2 pos = new Vector2(elem.fromNode.windowRect.center.x + (elem.toNode.windowRect.x - elem.fromNode.windowRect.x) / 2,
                                          elem.fromNode.windowRect.center.y + (elem.toNode.windowRect.y - elem.fromNode.windowRect.y) / 2);
                Rect textBox = new Rect(pos.x - 75, pos.y - 15, Transition.width, Transition.height);

                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 10;
                style.normal.background = GetBackground(elem.textBox.size, "Transition", 0, ((FSM)currentElem).transitions[i].isFocused);

                elem.textBox = GUI.Window(i + MAX_N_STATES, textBox, DrawTransitionBox, "", style);
            }
        }
        else if (currentElem is BehaviourTree)
        {
            for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
            {
                GUIStyle style = new GUIStyle();
                style.alignment = TextAnchor.MiddleCenter;
                style.fontSize = 10;
                style.normal.background = GetBackground(((BehaviourTree)currentElem).states[i].windowRect.size, "BT", (int)((BehaviourTree)currentElem).states[i].type, ((BehaviourTree)currentElem).states[i].isFocused);

                ((BehaviourTree)currentElem).states[i].windowRect = GUI.Window(i, ((BehaviourTree)currentElem).states[i].windowRect, DrawNodeWindow, ((BehaviourTree)currentElem).states[i].type.ToString(), style);
            }
        }

        EndWindows();

        if (e.isKey && e.type == EventType.KeyUp)
        {
            if (!makeTransitionMode)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Delete:
                        if (focusedObj is BaseNode)
                        {
                            if (currentElem is FSM)
                                PopupWindow.InitDelete(this, ((FSM)currentElem).states.IndexOf((StateNode)focusedObj), "Node");
                            else
                                PopupWindow.InitDelete(this, ((BehaviourTree)currentElem).states.IndexOf((BehaviourNode)focusedObj), "Node");

                            e.Use();
                        }
                        if (focusedObj is Transition)
                        {
                            PopupWindow.InitDelete(this, ((FSM)currentElem).transitions.IndexOf((Transition)focusedObj), "Transition");

                            e.Use();
                        }
                        if (focusedObj is FSM)
                        {
                            PopupWindow.InitDelete(this, Elements.IndexOf((FSM)focusedObj), "FSM");

                            e.Use();
                        }
                        if (focusedObj is BehaviourTree)
                        {
                            PopupWindow.InitDelete(this, Elements.IndexOf((BehaviourTree)focusedObj), "Behaviour Tree");

                            e.Use();
                        }
                        break;
                    case KeyCode.Escape:
                        currentElem = currentElem?.parent;

                        e.Use();
                        break;
                }
            }
            else
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Escape:
                    case KeyCode.Delete:
                        makeTransitionMode = false;
                        break;
                }
            }
        }
    }

    private void ShowTopBar()
    {
        GUIStyle style = GUI.skin.button;
        style.hover.textColor = Color.grey;
        style.alignment = TextAnchor.MiddleLeft;

        if (currentElem != null)
        {
            widthVariant = 0;
            ShowButtonRecursive(style, currentElem, "Node Editor");
        }
    }

    private void ShowButtonRecursive(GUIStyle style, ClickableElement elem, string name)
    {
        if (elem.parent != null)
        {
            ShowButtonRecursive(style, elem.parent, name);
            name = elem.parent.elementName;
        }
        var buttonWidth = 25 + name.ToCharArray().Length * 6;
        GUI.Label(new Rect(widthVariant, 0, 15, 20), ">", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperLeft });
        widthVariant += 12;
        if (GUI.Button(new Rect(widthVariant, 0, buttonWidth, 20), name, style))
        {
            currentElem = elem.parent;
        }
        widthVariant += buttonWidth;
    }

    private void OnDestroy()
    {
        PopupWindow.ClosePopup(GetWindow<PopupWindow>());
    }

    //Get a sprite from the resources
    private Texture2D GetBackground(Vector2 size, string typeOfItem, int type, bool isFocused)
    {
        Color col;
        Texture2D result;

        switch (typeOfItem)
        {
            case "Elements":
                switch (type)
                {
                    case 0:
                        col = Color.blue;
                        result = Resources.Load<Texture2D>("FSM_Rect");
                        break;
                    case 1:
                        col = Color.cyan;
                        break;
                    default:
                        col = Color.white;
                        break;
                }
                break;
            case "FSM":
                switch (type)
                {
                    case 0:
                        col = Color.grey;
                        break;
                    case 1:
                        col = Color.green;
                        break;
                    case 2:
                        col = Color.red;
                        break;
                    default:
                        col = Color.white;
                        break;
                }
                break;
            case "BT":
                switch (type)
                {
                    case 0:
                        col = Color.yellow;
                        break;
                    case 1:
                        col = new Color(1, 0.5f, 0, 1); //orange
                        break;
                    case 2:
                        col = new Color(0, 0.75f, 0, 1); //dark green
                        break;
                    default:
                        col = Color.white;
                        break;
                }
                break;
            case "Transition":
                col = Color.yellow;
                break;
            default:
                col = Color.clear;
                break;
        }

        if (!isFocused)
            col.a = 0.5f;

        Color[] pix = new Color[2 * 2];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }

        result = new Texture2D(2, 2);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    void DrawNodeWindow(int id)
    {
        if (currentElem is FSM)
            ((FSM)currentElem).states[id].DrawWindow(this);
        if (currentElem is BehaviourTree)
            ((BehaviourTree)currentElem).states[id].DrawWindow(this);
        GUI.DragWindow();
    }

    void DrawElementWindow(int id)
    {
        Elements[id].DrawWindow(this);
        GUI.DragWindow();
    }

    void DrawTransitionBox(int id)
    {
        ((FSM)currentElem).transitions[id - MAX_N_STATES].DrawBox(this);
        GUI.DragWindow();
    }

    void ContextCallback(object obj)
    {
        string clb = obj.ToString();

        switch (clb)
        {
            case "FSM":
                CreateFSM();
                break;
            case "BT":
                CreateBT();
                break;
            case "Node":
                CreateNode();
                break;
            case "Sequence":
                CreateSequence();
                break;
            case "Selector":
                CreateSelector();
                break;
            case "leafNode":
                CreateLeafNode();
                break;
            case "makeTransition":
                bool clickedOnWindow = false;
                int selectIndex = -1;

                if (currentElem is FSM)
                {
                    for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
                    {
                        if (((FSM)currentElem).states[i].windowRect.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }

                    if (clickedOnWindow)
                    {
                        selectednode = ((FSM)currentElem).states[selectIndex];
                        makeTransitionMode = true;
                    }
                }
                if (currentElem is BehaviourTree)
                {
                    for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
                    {
                        if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }

                    if (clickedOnWindow)
                    {
                        selectednode = ((BehaviourTree)currentElem).states[selectIndex];
                        makeTransitionMode = true;
                    }
                }
                break;
            case "deleteNode":
                clickedOnWindow = false;
                selectIndex = -1;

                if (currentElem is FSM)
                {
                    for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
                    {
                        if (((FSM)currentElem).states[i].windowRect.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }

                    if (clickedOnWindow)
                    {
                        PopupWindow.InitDelete(this, selectIndex, "Node");
                    }
                }

                if (currentElem is BehaviourTree)
                {
                    for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
                    {
                        if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }

                    if (clickedOnWindow)
                    {
                        PopupWindow.InitDelete(this, selectIndex, "Node");
                    }
                }
                break;
            case "deleteTransition":
                bool clickedOnTransition = false;
                selectIndex = -1;

                if (currentElem is FSM)
                {
                    for (int i = 0; i < ((FSM)currentElem).transitions.Count; i++)
                    {
                        if (((FSM)currentElem).transitions[i].textBox.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnTransition = true;
                            break;
                        }
                    }

                    if (clickedOnTransition)
                    {
                        PopupWindow.InitDelete(this, selectIndex, "Transition");
                    }
                }
                break;
            case "entryState":
                clickedOnWindow = false;
                selectIndex = -1;

                if (currentElem is FSM)
                {
                    for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
                    {
                        if (((FSM)currentElem).states[i].windowRect.Contains(mousePos))
                        {
                            selectIndex = i;
                            clickedOnWindow = true;
                            break;
                        }
                    }

                    if (clickedOnWindow)
                    {
                        StateNode selNode = ((FSM)currentElem).states[selectIndex];

                        ((FSM)currentElem).SetAsEntry(selNode);
                    }
                }
                break;
        }
    }

    public void Delete(string type, int selectIndex)
    {
        switch (type)
        {
            case "Node":
                if (currentElem is FSM)
                {
                    StateNode selNode = ((FSM)currentElem).states[selectIndex];

                    ((FSM)currentElem).DeleteNode(selNode);
                }
                if (currentElem is BehaviourTree)
                {
                    BehaviourNode selNode = ((BehaviourTree)currentElem).states[selectIndex];

                    ((BehaviourTree)currentElem).DeleteNode(selNode);
                }

                focusedObj = null;

                break;
            case "Transition":
                Transition transition = ((FSM)currentElem).transitions[selectIndex];

                ((FSM)currentElem).DeleteTransition(transition);

                focusedObj = null;
                break;
            case "FSM":
                FSM fsm = (FSM)Elements[selectIndex];

                Elements.Remove(fsm);

                focusedObj = null;
                break;
            case "Behaviour Tree":
                BehaviourTree bt = (BehaviourTree)Elements[selectIndex];

                Elements.Remove(bt);

                focusedObj = null;
                break;
        }
    }

    public static void DrawNodeCurve(Rect start, Rect end, bool isFocused)
    {
        //Check which sides to put the curve on
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

        //Draw curve

        //Curve parameters
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + direction * 50;
        Vector3 endTan = endPos - direction * 50;

        //Arrow parameters
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

        //Color
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

            //Draw arrow
            Handles.DrawBezier(pos1, endPos, pos1, endPos, shadowCol, null, (i + 1) * 5);
            Handles.DrawBezier(pos2, endPos, pos2, endPos, shadowCol, null, (i + 1) * 5);
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);

        //Draw arrow
        Handles.DrawBezier(pos1, endPos, pos1, endPos, Color.black, null, 1);
        Handles.DrawBezier(pos2, endPos, pos2, endPos, Color.black, null, 1);
    }

    private void CreateBT()
    {
        ClickableElement newBT = new BehaviourTree(currentElem, mousePos.x, mousePos.y);

        if (currentElem is null)
        {
            Elements.Add(newBT);
        }
        else if (currentElem is FSM)
        {
            var type = 1;
            if (((FSM)currentElem).states.Exists(n => n.type == StateNode.stateType.Entry))
            {
                type = 2;
            }

            StateNode node = new StateNode(type, newBT.windowRect.position.x, newBT.windowRect.position.y, newBT);

            ((FSM)currentElem).states.Add(node);
        }
        else if (currentElem is BehaviourTree)
        {
            bool clickedOnSequence = false;
            int nodeIndex = -1;

            for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
            {
                if (((BehaviourTree)currentElem).states[i].type == BehaviourNode.behaviourType.Leaf)
                    continue;
                if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                {
                    nodeIndex = i;
                    clickedOnSequence = true;
                    break;
                }
            }

            BehaviourNode node = new BehaviourNode(2, newBT)
            {
                windowRect = newBT.windowRect
            };

            if (clickedOnSequence)
            {
                selectednode = ((BehaviourTree)currentElem).states[nodeIndex];
                toCreateNode = node;
                makeBehaviourMode = true;
            }
            else
            {
                Debug.Log("Shouldn't happen. You didn't click on the node and still got the context menu for when you do");
            }
        }

        //NodeEditorUtilities.CreateFSM(newBT.elementName);
    }

    private void CreateFSM()
    {
        StateNode entryNode = new StateNode(1, 50, 50);

        ClickableElement newFSM = new FSM(entryNode, currentElem, mousePos.x, mousePos.y);

        if (currentElem is null)
        {
            Elements.Add(newFSM);
        }
        else if (currentElem is FSM)
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
        else if (currentElem is BehaviourTree)
        {
            bool clickedOnSequence = false;
            int nodeIndex = -1;

            for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
            {
                if (((BehaviourTree)currentElem).states[i].type == BehaviourNode.behaviourType.Leaf)
                    continue;
                if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                {
                    nodeIndex = i;
                    clickedOnSequence = true;
                    break;
                }
            }

            BehaviourNode node = new BehaviourNode(2, newFSM)
            {
                windowRect = newFSM.windowRect
            };

            if (clickedOnSequence)
            {
                selectednode = ((BehaviourTree)currentElem).states[nodeIndex];
                toCreateNode = node;
                makeBehaviourMode = true;
            }
            else
            {
                Debug.Log("Shouldn't happen. You didn't click on the node and still got the context menu for when you do");
            }
        }

        //NodeEditorUtilities.CreateFSM(newFSM.elementName);
    }

    private void CreateNode()
    {
        if (currentElem is FSM)
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

        //NodeEditorUtilities.CreateNode(node.elementName);
    }

    private void CreateSequence()
    {
        if (currentElem is BehaviourTree)
        {
            bool clickedOnSequence = false;
            int nodeIndex = -1;

            for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
            {
                if (((BehaviourTree)currentElem).states[i].type == BehaviourNode.behaviourType.Leaf)
                    continue;
                if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                {
                    nodeIndex = i;
                    clickedOnSequence = true;
                    break;
                }
            }

            BehaviourNode node = new BehaviourNode(0, mousePos.x, mousePos.y);

            if (clickedOnSequence)
            {
                selectednode = ((BehaviourTree)currentElem).states[nodeIndex];
                toCreateNode = node;
                makeBehaviourMode = true;
            }
            else
            {
                ((BehaviourTree)currentElem).states.Add(node);
            }
        }

        //NodeEditorUtilities.CreateNode(node.elementName);
    }

    private void CreateSelector()
    {
        if (currentElem is BehaviourTree)
        {
            bool clickedOnSelector = false;
            int nodeIndex = -1;

            for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
            {
                if (((BehaviourTree)currentElem).states[i].type == BehaviourNode.behaviourType.Leaf)
                    continue;
                if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                {
                    nodeIndex = i;
                    clickedOnSelector = true;
                    break;
                }
            }

            BehaviourNode node = new BehaviourNode(1, mousePos.x, mousePos.y);

            if (clickedOnSelector)
            {
                selectednode = ((BehaviourTree)currentElem).states[nodeIndex];
                toCreateNode = node;
                makeBehaviourMode = true;
            }
            else
            {
                ((BehaviourTree)currentElem).states.Add(node);
            }
        }

        //NodeEditorUtilities.CreateNode(node.elementName);
    }

    private void CreateLeafNode()
    {
        if (currentElem is BehaviourTree)
        {
            bool clickedOnSequence = false;
            int nodeIndex = -1;

            for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
            {
                if (((BehaviourTree)currentElem).states[i].type == BehaviourNode.behaviourType.Leaf)
                    continue;
                if (((BehaviourTree)currentElem).states[i].windowRect.Contains(mousePos))
                {
                    nodeIndex = i;
                    clickedOnSequence = true;
                    break;
                }
            }

            BehaviourNode node = new BehaviourNode(2, mousePos.x, mousePos.y);

            if (clickedOnSequence)
            {
                selectednode = ((BehaviourTree)currentElem).states[nodeIndex];
                toCreateNode = node;
                makeBehaviourMode = true;
            }
            else
            {
                Debug.Log("Shouldn't happen. You didn't click on the node and still got the context menu for when you do");
            }
        }
    }

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
}
