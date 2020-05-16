using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    public perceptionType type;

    public BaseNode fromNode;

    public BaseNode toNode;

    public Rect textBox;

    public static int baseWidth = 150;

    public static int baseHeight = 70;

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
        type = perceptionType.Push;

        fromNode = from;
        toNode = to;
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
    public void DrawBox(NodeEditor parent)
    {
        transitionName = CleanName(EditorGUILayout.TextArea(transitionName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(25)));

        GUILayout.BeginArea(new Rect(textBox.width * 0.1f, textBox.height - 30, textBox.width * 0.8f, baseHeight * 0.3f));

        if (GUILayout.Button(type.ToString(), EditorStyles.toolbarDropDown))
        {
            GenericMenu toolsMenu = new GenericMenu();

            foreach (string name in Enum.GetNames(typeof(perceptionType)))
            {
                toolsMenu.AddItem(new GUIContent(name), false, ChangeType, name);
            }

            // Offset menu from right of editor window
            toolsMenu.DropDown(new Rect(0, 0, 0, 0));
            EditorGUIUtility.ExitGUI();
        }

        GUILayout.EndArea();
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
        string newType = param.ToString();

        type = (perceptionType)Enum.Parse(typeof(perceptionType), newType);
    }
}
