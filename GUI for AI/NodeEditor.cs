using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeEditor : EditorWindow
{
    private Vector2 mousePos;

    private BaseNode selectednode;

    private GUIElement focusedObj;

    private bool makeTransitionMode = false;

    private List<ClickableElement> Elements = new List<ClickableElement>();

    private ClickableElement currentElem;

    [MenuItem("Window/Node Editor")]
    static void ShowEditor()
    {
        NodeEditor editor = EditorWindow.GetWindow<NodeEditor>();
    }

    private void OnGUI()
    {
        Event e = Event.current;

        mousePos = e.mousePosition;

        if (e.button == 1 && !makeTransitionMode)
        {
            if (e.type == EventType.MouseDown)
            {
                bool clickedOnWindow = false;
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

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (!clickedOnWindow && !clickedOnTransition && currentElem is BehaviourTree)
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

                    menu.AddItem(new GUIContent("Add Leaf Node"), false, ContextCallback, "leafNode");
                    menu.AddSeparator("");
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

            for (int i = 0; i < Elements?.Count; i++)
            {
                if (Elements[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnElement = true;
                    break;
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
                if (focusedObj != null) focusedObj.isFocused = false;
                Elements[selectIndex].isFocused = true;
                focusedObj = Elements[selectIndex];

                if (Event.current.clickCount == 2)
                {
                    currentElem = Elements[selectIndex];
                }
            }
            else if (clickedOnTransition && currentElem is FSM)
            {
                if (focusedObj != null) focusedObj.isFocused = false;
                ((FSM)currentElem).transitions[selectIndex].isFocused = true;
                focusedObj = ((FSM)currentElem).transitions[selectIndex];

                e.Use();
            }
            else if (clickedOnWindow && currentElem is FSM)
            {
                if (focusedObj != null) focusedObj.isFocused = false;
                ((FSM)currentElem).states[selectIndex].isFocused = true;
                focusedObj = ((FSM)currentElem).states[selectIndex];
            }
            else if (clickedOnWindow && currentElem is BehaviourTree)
            {
                if (focusedObj != null) focusedObj.isFocused = false;
                ((BehaviourTree)currentElem).states[selectIndex].isFocused = true;
                focusedObj = ((BehaviourTree)currentElem).states[selectIndex];
            }
            else
            {
                if (focusedObj != null) focusedObj.isFocused = false;

                focusedObj = null;

                e.Use();
            }

            if (makeTransitionMode && currentElem is FSM)
            {
                if (clickedOnWindow && !((FSM)currentElem).states[selectIndex].Equals(selectednode))
                {
                    Transition transition = new Transition("New Transition", selectednode, ((FSM)currentElem).states[selectIndex]);
                    transition.textBox = DrawTextBox(transition);

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
        }

        if (makeTransitionMode && selectednode != null)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            Rect nodeRect = new Rect(selectednode.windowRect);
            nodeRect.x = selectednode.windowRect.x + nodeRect.width / 2;

            DrawNodeCurve(nodeRect, mouseRect, true);

            Repaint();
        }

        if (currentElem is FSM)
        {
            foreach (StateNode n in ((FSM)currentElem).states)
            {
                n.DrawCurves();
            }
        }

        BeginWindows();

        if (currentElem is null)
        {
            for (int i = 0; i < Elements.Count; i++)
            {
                GUIStyle style = new GUIStyle();
                style.contentOffset = new Vector2(0, -20);
                style.normal.background = MakeBackground(2, 2, "Elements", (int)Elements[i].type, Elements[i].isFocused);

                Elements[i].windowRect = GUI.Window(i, Elements[i].windowRect, DrawElementWindow, Elements[i].elementName, style);
            }
        }
        else if (currentElem is FSM)
        {
            for (int i = 0; i < ((FSM)currentElem).states.Count; i++)
            {
                GUIStyle style = new GUIStyle();
                style.contentOffset = new Vector2(0, -20);
                style.normal.background = MakeBackground(2, 2, "FSM", (int)((FSM)currentElem).states[i].type, ((FSM)currentElem).states[i].isFocused);

                ((FSM)currentElem).states[i].windowRect = GUI.Window(i, ((FSM)currentElem).states[i].windowRect, DrawNodeWindow, ((FSM)currentElem).states[i].nodeName, style);
            }
        }
        else if (currentElem is BehaviourTree)
        {
            for (int i = 0; i < ((BehaviourTree)currentElem).states.Count; i++)
            {
                GUIStyle style = new GUIStyle();
                style.contentOffset = new Vector2(0, -20);
                style.normal.background = MakeBackground(2, 2, "BT", (int)((BehaviourTree)currentElem).states[i].type, ((BehaviourTree)currentElem).states[i].isFocused);

                ((BehaviourTree)currentElem).states[i].windowRect = GUI.Window(i, ((BehaviourTree)currentElem).states[i].windowRect, DrawNodeWindow, ((BehaviourTree)currentElem).states[i].nodeName, style);
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
                            {
                                ((FSM)currentElem).DeleteNode((StateNode)focusedObj);

                                if (((FSM)currentElem).isEntryState((StateNode)focusedObj))
                                {
                                    Elements.Remove(currentElem);
                                    currentElem = null;
                                }
                            }

                            if (currentElem is BehaviourTree)
                            {
                                ((BehaviourTree)currentElem).DeleteNode((BehaviourNode)focusedObj);
                            }

                            focusedObj = null;

                            e.Use();
                        }
                        if (focusedObj is Transition)
                        {
                            if (currentElem is FSM)
                            {
                                ((FSM)currentElem).DeleteTransition((Transition)focusedObj);
                            }

                            focusedObj = null;

                            e.Use();
                        }
                        if (focusedObj is FSM)
                        {
                            Elements.Remove((FSM)focusedObj);

                            focusedObj = null;

                            e.Use();
                        }
                        if (focusedObj is BehaviourTree)
                        {
                            Elements.Remove((BehaviourTree)focusedObj);

                            focusedObj = null;

                            e.Use();
                        }
                        break;
                    case KeyCode.Escape:
                        currentElem = null;

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

    //Habrá que cambiarlo
    private Texture2D MakeBackground(int width, int height, string typeOfItem, int type, bool isFocused)
    {
        Color col;

        switch (typeOfItem)
        {
            case "Elements":
                switch (type)
                {
                    case 0:
                        col = Color.blue;
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
            default:
                col = Color.clear;
                break;
        }

        if (!isFocused)
            col.a = 0.5f;

        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    void DrawNodeWindow(int id)
    {
        if (currentElem is FSM)
            ((FSM)currentElem).states[id].DrawWindow();
        if (currentElem is BehaviourTree)
            ((BehaviourTree)currentElem).states[id].DrawWindow();
        GUI.DragWindow();
    }

    void DrawElementWindow(int id)
    {
        Elements[id].DrawWindow();
        GUI.DragWindow();
    }

    void ContextCallback(object obj)
    {
        string clb = obj.ToString();

        switch (clb)
        {
            case "FSM":
                StateNode entryNode = new StateNode(1);
                entryNode.windowRect = new Rect(50, 50, 200, 100);

                ClickableElement newFSM = new FSM("New FSM", entryNode);
                newFSM.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);

                Elements.Add(newFSM);
                break;
            case "BT":
                ClickableElement newBT = new BehaviourTree("New BT");
                newBT.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);

                Elements.Add(newBT);
                break;
            case "Node":
                if (currentElem is FSM)
                {
                    var type = 1;
                    if (((FSM)currentElem).states.Exists(n => n.type == StateNode.stateType.Entry))
                    {
                        type = 2;
                    }

                    StateNode node = new StateNode(type);
                    node.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);

                    ((FSM)currentElem).states.Add(node);
                }
                break;
            case "Sequence":
                if (currentElem is BehaviourTree)
                {
                    BehaviourNode node = new BehaviourNode(0);
                    node.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);

                    ((BehaviourTree)currentElem).states.Add(node);
                }
                break;
            case "Selector":
                if (currentElem is BehaviourTree)
                {
                    BehaviourNode node = new BehaviourNode(1);
                    node.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);

                    ((BehaviourTree)currentElem).states.Add(node);
                }
                break;
            case "leafNode":
                if (currentElem is BehaviourTree)
                {
                    BehaviourNode node = new BehaviourNode(2);
                    node.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);

                    ((BehaviourTree)currentElem).states.Add(node);
                }
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
                else if (currentElem is BehaviourTree)
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
                        StateNode selNode = ((FSM)currentElem).states[selectIndex];

                        ((FSM)currentElem).DeleteNode(selNode);

                        if (((FSM)currentElem).isEntryState(selNode))
                        {
                            Elements.Remove(currentElem);
                            currentElem = null;
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

                    if (clickedOnWindow)
                    {
                        BehaviourNode selNode = ((BehaviourTree)currentElem).states[selectIndex];

                        ((BehaviourTree)currentElem).DeleteNode(selNode);
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
                        Transition transition = ((FSM)currentElem).transitions[selectIndex];

                        ((FSM)currentElem).DeleteTransition(transition);
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

    public static void DrawNodeCurve(Rect start, Rect end, bool isFocused)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
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
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }

    public static Rect DrawTextBox(Transition trans)
    {
        Vector2 pos = new Vector2(trans.fromNode.windowRect.center.x + (trans.toNode.windowRect.x - trans.fromNode.windowRect.x) / 2, trans.fromNode.windowRect.center.y + (trans.toNode.windowRect.y - trans.fromNode.windowRect.y) / 2);
        Rect textBox = new Rect(pos.x - 75, pos.y - 15, 150, 30);

        GUIStyle style = new GUIStyle();

        style.alignment = TextAnchor.UpperCenter;

        EditorGUI.LabelField(textBox, trans.transitionName, style);

        return textBox;
    }
}
