using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class TransitionGUI : GUIElement
{
    public enum perceptionType
    {
        Push,
        Timer,
        Value,
        IsInState,
        BehaviourTreeStatus,
        And,
        Or,
        Custom
    }

    public string transitionName = "";

    public BaseNode fromNode;

    public BaseNode toNode;

    public Rect textBox;

    public static int baseWidth = 170;

    public static int baseHeight = 70;

    public static int areaHeight = 100;

    public int width;

    public int height;

    // These parameters are for each percetion, ordered so they don't get mixed in the tree of perceptions
    // Make serializable

    public Dictionary<string, perceptionType> orderedTypes;

    public Dictionary<string, int> orderedTimerNumber;

    public Dictionary<string, bool> orderedOpenFoldout;

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

        orderedTypes = new Dictionary<string, perceptionType>();

        orderedTimerNumber = new Dictionary<string, int>();

        orderedOpenFoldout = new Dictionary<string, bool>();
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

        int index = 0;

        transitionName = CleanName(EditorGUILayout.TextArea(transitionName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(25)));

        // Narrower area than the main rect
        Rect areaRect = new Rect(baseWidth * 0.1f, 40, width * 0.8f, height);

        GUILayout.BeginArea(areaRect);
        try
        {
            PerceptionFoldout(ref heightAcc, ref widthAcc, ref index, 0);
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
    private void PerceptionFoldout(ref int heightAcc, ref int widthAcc, ref int index, int treeLevel)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        try
        {
            string indexLvl = index.ToString() + "#" + treeLevel.ToString();

            if (!orderedTypes.ContainsKey(indexLvl))
            {
                orderedTypes[indexLvl] = perceptionType.Push;
            }
            if (!orderedTimerNumber.ContainsKey(indexLvl))
            {
                orderedTimerNumber[indexLvl] = 0;
            }
            if (!orderedOpenFoldout.ContainsKey(indexLvl))
            {
                orderedOpenFoldout[indexLvl] = false;
            }

            orderedOpenFoldout[indexLvl] = EditorGUILayout.Foldout(orderedOpenFoldout[indexLvl], orderedTypes[indexLvl].ToString() + "Perception");

            if (orderedOpenFoldout[indexLvl])
            {
                heightAcc += 30;

                if (GUILayout.Button(orderedTypes[indexLvl].ToString(), EditorStyles.toolbarDropDown))
                {
                    GenericMenu toolsMenu = new GenericMenu();

                    foreach (string name in Enum.GetNames(typeof(perceptionType)))
                    {
                        toolsMenu.AddItem(new GUIContent(name), false, ChangeType, new string[] { name, index.ToString(), treeLevel.ToString() });
                    }

                    toolsMenu.DropDown(new Rect(0, 40, 0, 0));
                    EditorGUIUtility.ExitGUI();
                }

                switch (orderedTypes[indexLvl])
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

                            int aux;

                            int.TryParse(GUILayout.TextField(orderedTimerNumber[indexLvl].ToString(), new GUIStyle(Styles.TitleText)
                            {
                                alignment = TextAnchor.MiddleCenter
                            }, GUILayout.Height(20), GUILayout.Width(20)), out aux);

                            orderedTimerNumber[indexLvl] = aux;
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
                        heightAcc += 20;
                        GUILayout.Label("Not implemented yet", Styles.WarningLabel, GUILayout.Height(20));
                        break;
                    case perceptionType.BehaviourTreeStatus:
                        heightAcc += 20;
                        GUILayout.Label("Not implemented yet", Styles.WarningLabel, GUILayout.Height(20));
                        break;
                    case perceptionType.And:
                        heightAcc += 60;
                        widthAcc += 20;

                        index++;
                        treeLevel++;
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref index, treeLevel);
                        GUILayout.Label("-AND-", Styles.TitleText, GUILayout.Height(20));
                        index++;
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref index, treeLevel);
                        break;
                    case perceptionType.Or:
                        heightAcc += 60;
                        widthAcc += 20;

                        index++;
                        treeLevel++;
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref index, treeLevel);
                        GUILayout.Label("-OR-", Styles.TitleText, GUILayout.Height(20));
                        index++;
                        PerceptionFoldout(ref heightAcc, ref widthAcc, ref index, treeLevel);
                        break;
                    case perceptionType.Custom:
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
        int index = int.Parse(data[1]);
        int treeLevel = int.Parse(data[2]);
        string key = index.ToString() + "#" + treeLevel.ToString();

        orderedTypes[key] = (perceptionType)Enum.Parse(typeof(perceptionType), newType);
        orderedTimerNumber[key] = 0;

        foreach (string indexLvl in orderedTypes.Keys)
        {
            string[] numbers = indexLvl.Split('#');
            if (int.Parse(numbers[1]) > treeLevel)
            {
                orderedTypes[indexLvl] = perceptionType.Push;
                orderedTimerNumber[indexLvl] = 0;
                orderedOpenFoldout[indexLvl] = false;
            }
        }
    }
}
