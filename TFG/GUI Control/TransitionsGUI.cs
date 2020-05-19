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

    public static int baseWidth = 170;

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
    public void InitTransitionGUI(string name, BaseNode from, BaseNode to)
    {
        identificator = UniqueID();

        transitionName = name;

        width = baseWidth;
        height = baseHeight;

        fromNode = from;
        toNode = to;

        rootPerception = CreateInstance<PerceptionGUI>();
        rootPerception.InitPerceptionGUI(false, 0, Enums.perceptionType.Push);
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
    public void DrawBox()
    {
        int heightAcc = 0;
        int widthAcc = 0;

        transitionName = CleanName(EditorGUILayout.TextArea(transitionName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(25)));

        // Narrower area than the main rect
        Rect areaRect = new Rect(baseWidth * 0.1f, 40, width * 0.8f, height);

        GUILayout.BeginArea(areaRect);
        try
        {
            PerceptionFoldout(ref heightAcc, ref widthAcc, ref rootPerception);
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
    private void PerceptionFoldout(ref int heightAcc, ref int widthAcc, ref PerceptionGUI currentPerception)
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

                    foreach (string name in Enum.GetNames(typeof(Enums.perceptionType)))
                    {
                        toolsMenu.AddItem(new GUIContent(name), false, ChangeType, new string[] { name, currentPerception.identificator });
                    }

                    toolsMenu.DropDown(new Rect(0, 40, 0, 0));
                    EditorGUIUtility.ExitGUI();
                }

                switch (currentPerception.type)
                {
                    case Enums.perceptionType.Push:
                        heightAcc += 40;
                        GUILayout.Label("You will be able to\nfire this transition\nmanually through code", new GUIStyle(Styles.SubTitleText)
                        {
                            fontStyle = FontStyle.Italic
                        }, GUILayout.Height(50));
                        break;
                    case Enums.perceptionType.Timer:
                        heightAcc += 20;

                        GUILayout.BeginHorizontal();
                        try
                        {
                            GUILayout.Label("Time: ", new GUIStyle(Styles.TitleText)
                            {
                                alignment = TextAnchor.MiddleCenter
                            }, GUILayout.Height(20), GUILayout.Width(width * 0.5f));

                            int aux;

                            int.TryParse(GUILayout.TextField(currentPerception.timerNumber.ToString(), new GUIStyle(Styles.TitleText)
                            {
                                alignment = TextAnchor.MiddleCenter
                            }, GUILayout.Height(20), GUILayout.Width(20)), out aux);

                            currentPerception.timerNumber = aux;
                        }
                        finally
                        {
                            GUILayout.EndHorizontal();
                        }
                        break;
                    case Enums.perceptionType.Value:
                        heightAcc += 20;
                        GUILayout.Label("Not implemented yet", Styles.WarningLabel, GUILayout.Height(20));
                        break;
                    case Enums.perceptionType.IsInState:
                        heightAcc += 20;
                        GUILayout.Label("Not implemented yet", Styles.WarningLabel, GUILayout.Height(20));
                        break;
                    case Enums.perceptionType.BehaviourTreeStatus:
                        heightAcc += 20;
                        GUILayout.Label("Not implemented yet", Styles.WarningLabel, GUILayout.Height(20));
                        break;
                    case Enums.perceptionType.And:
                        heightAcc += 60;
                        widthAcc += 20;

                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.firstChild);
                        GUILayout.Label("-AND-", Styles.TitleText, GUILayout.Height(20));
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.secondChild);
                        break;
                    case Enums.perceptionType.Or:
                        heightAcc += 60;
                        widthAcc += 20;

                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.firstChild);
                        GUILayout.Label("-OR-", Styles.TitleText, GUILayout.Height(20));
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref currentPerception.secondChild);
                        break;
                    case Enums.perceptionType.Custom:
                        heightAcc += 50;
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

        ChangeTypeRecursive(ref rootPerception, id, (Enums.perceptionType)Enum.Parse(typeof(Enums.perceptionType), newType));
    }

    public void ChangeTypeRecursive(ref PerceptionGUI perception, string id, Enums.perceptionType newType)
    {
        if (perception.identificator == id)
        {
            switch (perception.type)
            {
                case Enums.perceptionType.Timer:
                    if (newType != Enums.perceptionType.Timer)
                    {
                        perception.InitPerceptionGUI(perception.isSecondChild, perception.treeLevel, newType);
                    }
                    break;
                case Enums.perceptionType.And:
                case Enums.perceptionType.Or:
                    if (newType == Enums.perceptionType.And || newType == Enums.perceptionType.Or)
                    {
                        perception.type = newType;
                    }
                    else
                    {
                        perception.InitPerceptionGUI(perception.isSecondChild, perception.treeLevel, newType);
                    }
                    break;
                default:
                    perception.InitPerceptionGUI(perception.isSecondChild, perception.treeLevel, newType);
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
