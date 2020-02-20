using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class NodeEditor : EditorWindow
{
    private List<Node> windows = new List<Node>();

    private Vector2 mousePos;

    private Node selectednode;

    private bool makeTransitionMode = false;

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
                bool clickedOnCurve = false;
                int selectIndex = -1;

                for (int i = 0; i < windows.Count; i++)
                {
                    if (windows[i].windowRect.Contains(mousePos))
                    {
                        selectIndex = i;
                        clickedOnWindow = true;
                        break;
                    }
                }
                if (!clickedOnWindow)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Add Node"), false, ContextCallback, "Node");

                    menu.ShowAsContext();
                    e.Use();
                }
                else if (clickedOnCurve)
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Delete Transition"), false, ContextCallback, "deleteTransition");

                    menu.ShowAsContext();
                    e.Use();
                }
                else
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Make Transition"), false, ContextCallback, "makeTransition");
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Delete Node"), false, ContextCallback, "deleteNode");

                    menu.ShowAsContext();
                    e.Use();
                }
            }
        }
        else if (e.button == 0 && e.type == EventType.MouseDown && makeTransitionMode)
        {
            bool clickedOnWindow = false;
            int selectIndex = -1;

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnWindow = true;
                    break;
                }
            }
            if (clickedOnWindow && !windows[selectIndex].Equals(selectednode))
            {
                windows[selectIndex].SetInput(selectednode, mousePos);
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

        if (makeTransitionMode && selectednode != null)
        {
            Rect mouseRect = new Rect(e.mousePosition.x, e.mousePosition.y, 10, 10);
            Rect nodeRect = new Rect(selectednode.windowRect);
            nodeRect.x = selectednode.windowRect.x + nodeRect.width / 2;

            DrawNodeCurve(nodeRect, mouseRect);

            Repaint();
        }

        foreach (Node n in windows)
        {
            n.DrawCurves();
        }

        BeginWindows();

        for (int i = 0; i < windows.Count; i++)
        {
            windows[i].windowRect = GUI.Window(i, windows[i].windowRect, DrawNodeWindow, windows[i].stateName);
        }

        EndWindows();
    }

    void DrawNodeWindow(int id)
    {
        windows[id].DrawWindow();
        GUI.DragWindow();
    }

    void ContextCallback(object obj)
    {
        string clb = obj.ToString();

        if (clb.Equals("Node"))
        {
            Node node = new Node();
            node.windowRect = new Rect(mousePos.x, mousePos.y, 200, 100);

            windows.Add(node);
        }
        else if (clb.Equals("makeTransition"))
        {
            bool clickedOnWindow = false;
            int selectIndex = -1;

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnWindow = true;
                    break;
                }
            }

            if (clickedOnWindow)
            {
                selectednode = windows[selectIndex];
                makeTransitionMode = true;
            }
        }
        else if (clb.Equals("deleteNode"))
        {
            bool clickedOnWindow = false;
            int selectIndex = -1;

            for (int i = 0; i < windows.Count; i++)
            {
                if (windows[i].windowRect.Contains(mousePos))
                {
                    selectIndex = i;
                    clickedOnWindow = true;
                    break;
                }
            }

            if (clickedOnWindow)
            {
                Node selNode = windows[selectIndex];
                windows.RemoveAt(selectIndex);

                foreach (Node n in windows)
                {
                    n.NodeDeleted(selNode);
                }
            }
        }
    }

    public static void DrawNodeCurve(Rect start, Rect end)
    {
        Vector3 startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
        Vector3 endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;
        Color shadowCol = new Color(0, 0, 0, 0.06f);

        for (int i = 0; i < 3; i++)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 1);
    }
}
