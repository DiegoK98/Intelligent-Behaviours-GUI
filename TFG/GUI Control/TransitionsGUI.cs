using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class TransitionGUI : GUIElement
{
    public string transitionName = "";

    public BaseNode fromNode;

    public BaseNode toNode;

    public Rect textBox;

    public static int baseWidth = 200;

    public static int baseHeight = 70;

    public static int areaHeight = 100;

    public int width;

    public int height;

    public PerceptionGUI rootPerception;

    /// <summary>
    /// The InitTransitionGUI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void InitTransitionGUI(BaseNode from, BaseNode to)
    {
        identificator = UniqueID();

        transitionName = uniqueNamer.GenerateUniqueName(identificator, "New Transition ");

        width = baseWidth;
        height = baseHeight;

        fromNode = from;
        toNode = to;

        rootPerception = CreateInstance<PerceptionGUI>();
        rootPerception.InitPerceptionGUI(perceptionType.Push);
    }

    public override XMLElement ToXMLElement(params object[] args)
    {
        XMLElement result = new XMLElement
        {
            name = CleanName(this.transitionName),
            elemType = this.GetType().ToString(),
            windowPosX = this.textBox.x,
            windowPosY = this.textBox.y,
            Id = this.identificator,
            fromId = this.fromNode.identificator,
            toId = this.toNode.identificator,
            perception = this.rootPerception.ToPerceptionXML()
        };

        return result;
    }

    /// <summary>
    /// Gets the type of the element properly written
    /// </summary>
    /// <returns></returns>
    public override string GetTypeString()
    {
        return "Transition";
    }

    /// <summary>
    /// Draws all the elements inside the Transition box
    /// </summary>
    /// <param name="parent"></param>
    public void DrawBox(NodeEditor sender)
    {
        int heightAcc = 0;
        int widthAcc = 0;

        transitionName = CleanName(EditorGUILayout.TextArea(transitionName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(25)));

        // Narrower area than the main rect
        Rect areaRect = new Rect(baseWidth * 0.1f, 40, width * 0.8f, height);

        GUILayout.BeginArea(areaRect);
        try
        {
            PerceptionFoldout(ref heightAcc, ref widthAcc, ref rootPerception, sender);
        }
        finally
        {
            GUILayout.EndArea();
        }

        // Increase the size depending on the open foldouts
        height = baseHeight + heightAcc;
        width = baseWidth + widthAcc;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="parentRect"></param>
    /// <param name="heightAcc"></param>
    /// <param name="widthAcc"></param>
    /// <param name="index"></param>
    /// <param name="treeLevel"></param>
    private void PerceptionFoldout(ref int heightAcc, ref int widthAcc, ref PerceptionGUI currentPerception, NodeEditor sender)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        try
        {
            currentPerception.openFoldout = EditorGUILayout.Foldout(currentPerception.openFoldout, currentPerception.type.ToString() + "Perception");

            if (currentPerception.openFoldout)
            {
                heightAcc += 30;

                if (GUILayout.Button(currentPerception.type.ToString(), EditorStyles.toolbarDropDown))
                {
                    GenericMenu toolsMenu = new GenericMenu();

                    foreach (string name in Enum.GetNames(typeof(perceptionType)))
                    {
                        toolsMenu.AddItem(new GUIContent(name), false, ChangeType, new string[] { name, currentPerception.identificator });
                    }

                    toolsMenu.DropDown(new Rect(0, 40, 0, 0));
                    EditorGUIUtility.ExitGUI();
                }

                switch (currentPerception.type)
                {
                    case perceptionType.Push:
                        heightAcc += 40;
                        GUILayout.Label("You will be able to\nfire this transition\nmanually through code", new GUIStyle(Styles.SubTitleText)
                        {
                            fontStyle = FontStyle.Italic
                        }, GUILayout.Height(50));
                        break;
                    case perceptionType.Timer:
                        heightAcc += 20;

                        GUILayout.BeginHorizontal();
                        try
                        {
                            GUILayout.Label("Time: ", new GUIStyle(Styles.TitleText)
                            {
                                alignment = TextAnchor.MiddleCenter
                            }, GUILayout.Height(20), GUILayout.Width(width * 0.5f));


                            int.TryParse(GUILayout.TextField(currentPerception.timerNumber.ToString(), new GUIStyle(Styles.TitleText)
                            {
                                alignment = TextAnchor.MiddleCenter
                            }, GUILayout.Height(20), GUILayout.Width(20)), out int number);

                            currentPerception.timerNumber = number;
                        }
                        finally
                        {
                            GUILayout.EndHorizontal();
                        }
                        break;
                    case perceptionType.Value:
                        heightAcc += 20;
                        GUILayout.Label("Not implemented yet", Styles.WarningLabel, GUILayout.Height(20));
                        break;
                    case perceptionType.IsInState:
                        heightAcc += 30;

                        GUILayout.Space(5);

                        List<FSM> subFSMsList = sender.currentElem.GetSubElems().Where(e => e is FSM).Cast<FSM>().ToList();

                        GUI.enabled = subFSMsList.Count > 0;

                        try
                        {
                            if (GUILayout.Button(currentPerception.elemName, EditorStyles.toolbarDropDown))
                            {
                                GenericMenu toolsMenu = new GenericMenu();

                                List<string> list = subFSMsList.Select(e => e.elementName).ToList();
                                list.Sort();

                                foreach (string name in list)
                                {
                                    toolsMenu.AddItem(new GUIContent(name), false, (current) =>
                                    {
                                        if (((PerceptionGUI)current).elemName != name)
                                        {
                                            ((PerceptionGUI)current).elemName = name;
                                            ((PerceptionGUI)current).stateName = "Select a State";
                                        }
                                    }, currentPerception);
                                }

                                toolsMenu.DropDown(new Rect(0, 40, 0, 0));
                                EditorGUIUtility.ExitGUI();
                            }

                            if (subFSMsList.Count > 0 && currentPerception.elemName != "Select a FSM" && currentPerception.elemName != "Select a BT" && !string.IsNullOrEmpty(currentPerception.elemName))
                            {
                                heightAcc += 30;

                                GUILayout.Space(5);

                                string auxName = currentPerception.elemName;

                                FSM selectedFSM = subFSMsList.Where(e => e.elementName == auxName).FirstOrDefault();
                                List<StateNode> subStatesList = selectedFSM ? selectedFSM.states.Where(s => s.subElem == null).ToList() : new List<StateNode>();

                                GUI.enabled = subStatesList.Count > 0;

                                if (GUILayout.Button(currentPerception.stateName, EditorStyles.toolbarDropDown))
                                {
                                    GenericMenu toolsMenu = new GenericMenu();

                                    List<string> list = subStatesList.Select(s => s.nodeName).ToList();
                                    list.Sort();

                                    foreach (string name in list)
                                    {
                                        toolsMenu.AddItem(new GUIContent(name), false, (current) =>
                                        {
                                            ((PerceptionGUI)current).stateName = name;
                                        }, currentPerception);
                                    }

                                    toolsMenu.DropDown(new Rect(0, 40, 0, 0));
                                    EditorGUIUtility.ExitGUI();
                                }
                            }
                        }
                        finally
                        {
                            GUI.enabled = true;
                        }

                        break;
                    case perceptionType.BehaviourTreeStatus:
                        heightAcc += 30;

                        GUILayout.Space(5);

                        List<ClickableElement> subBTsList = sender.currentElem.GetSubElems();

                        GUI.enabled = subBTsList.Count > 0;
                        if (GUILayout.Button(currentPerception.elemName, EditorStyles.toolbarDropDown))
                        {
                            GenericMenu toolsMenu = new GenericMenu();

                            List<string> list = subBTsList.Where(e => e is BehaviourTree).Select(e => e.elementName).ToList();
                            list.Sort();

                            foreach (string name in list)
                            {
                                toolsMenu.AddItem(new GUIContent(name), false, (current) =>
                                {
                                    if (((PerceptionGUI)current).elemName != name)
                                    {
                                        ((PerceptionGUI)current).elemName = name;
                                        ((PerceptionGUI)current).status = ReturnValues.Succeed;
                                    }
                                }, currentPerception);
                            }

                            toolsMenu.DropDown(new Rect(0, 40, 0, 0));
                            EditorGUIUtility.ExitGUI();
                        }
                        GUI.enabled = true;

                        if (subBTsList.Count > 0 && currentPerception.elemName != "Select a FSM" && currentPerception.elemName != "Select a BT" && !string.IsNullOrEmpty(currentPerception.elemName))
                        {
                            heightAcc += 30;

                            GUILayout.Space(5);

                            if (GUILayout.Button(currentPerception.status.ToString(), EditorStyles.toolbarDropDown))
                            {
                                GenericMenu toolsMenu = new GenericMenu();

                                foreach (string name in Enum.GetNames(typeof(ReturnValues)))
                                {
                                    toolsMenu.AddItem(new GUIContent(name), false, (current) =>
                                    {
                                        ((PerceptionGUI)current).status = (ReturnValues)Enum.Parse(typeof(ReturnValues), name);
                                    }, currentPerception);
                                }

                                toolsMenu.DropDown(new Rect(0, 40, 0, 0));
                                EditorGUIUtility.ExitGUI();
                            }
                        }
                        break;
                    case perceptionType.And:
                        heightAcc += 60;
                        widthAcc += 20;

                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.firstChild, sender);
                        GUILayout.Label("-AND-", Styles.TitleText, GUILayout.Height(20));
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.secondChild, sender);
                        break;
                    case perceptionType.Or:
                        heightAcc += 60;
                        widthAcc += 20;

                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.firstChild, sender);
                        GUILayout.Label("-OR-", Styles.TitleText, GUILayout.Height(20));
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.secondChild, sender);
                        break;
                    case perceptionType.Custom:
                        heightAcc += 70;
                        widthAcc += 30;

                        GUILayout.BeginHorizontal();
                        try
                        {
                            GUILayout.Label("Name: ", new GUIStyle(Styles.TitleText)
                            {
                                alignment = TextAnchor.MiddleCenter
                            }, GUILayout.Height(20), GUILayout.Width(width * 0.3f));


                            string name = GUILayout.TextField(currentPerception.customName, new GUIStyle(Styles.TitleText)
                            {
                                alignment = TextAnchor.MiddleCenter
                            }, GUILayout.Height(20), GUILayout.Width(100));

                            currentPerception.customName = name;
                        }
                        finally
                        {
                            GUILayout.EndHorizontal();
                        }

                        GUILayout.Label("You will have to code\nthe Check method\nin the generated script", new GUIStyle(Styles.SubTitleText)
                        {
                            fontStyle = FontStyle.Italic
                        }, GUILayout.Height(50));
                        break;
                }
            }
        }
        finally
        {
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }

    /// <summary>
    /// The Equals
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (!base.Equals(other))
            return false;
        if (this.transitionName != ((TransitionGUI)other).transitionName)
            return false;
        if (this.identificator != ((TransitionGUI)other).identificator)
            return false;
        if (!fromNode.Equals(((TransitionGUI)other).fromNode) || !toNode.Equals(((TransitionGUI)other).toNode))
            return false;

        return true;
    }

    public void ChangeType(object param)
    {
        string[] data = (string[])param;
        string newType = data[0];
        string id = data[1];

        ChangeTypeRecursive(ref rootPerception, id, (perceptionType)Enum.Parse(typeof(perceptionType), newType));
    }

    public void ChangeTypeRecursive(ref PerceptionGUI perception, string id, perceptionType newType)
    {
        if (perception.identificator == id)
        {
            switch (perception.type)
            {
                case perceptionType.Timer:
                    if (newType != perceptionType.Timer)
                    {
                        perception.InitPerceptionGUI(newType);
                    }
                    break;
                case perceptionType.Value:
                    if (newType != perceptionType.Value)
                    {
                        perception.InitPerceptionGUI(newType);
                    }
                    break;
                case perceptionType.IsInState:
                    if (newType != perceptionType.IsInState)
                    {
                        perception.InitPerceptionGUI(newType);
                    }
                    break;
                case perceptionType.BehaviourTreeStatus:
                    if (newType != perceptionType.BehaviourTreeStatus)
                    {
                        perception.InitPerceptionGUI(newType);
                    }
                    break;
                case perceptionType.And:
                case perceptionType.Or:
                    if (newType == perceptionType.And || newType == perceptionType.Or)
                    {
                        perception.type = newType;
                    }
                    else
                    {
                        perception.InitPerceptionGUI(newType);
                    }
                    break;
                default:
                    perception.InitPerceptionGUI(newType);
                    break;
            }

            perception.openFoldout = true;
        }
        else
        {
            if (perception.firstChild != null && perception.secondChild != null)
            {
                ChangeTypeRecursive(ref perception.firstChild, id, newType);
                ChangeTypeRecursive(ref perception.secondChild, id, newType);
            }
        }
    }
}
