using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Transition : GUIElement
{
    public string transitionName = "";

    public BaseNode fromNode;

    public BaseNode toNode;

    public Rect textBox;

    public static int width = 150;

    public static int height = 35;

    /// <summary>
    /// The Transition
    /// </summary>
    /// <param name="name"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public Transition(string name, BaseNode from, BaseNode to)
    {
        transitionName = name;

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
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.LowerCenter;
        style.fontSize = 15;
        transitionName = EditorGUILayout.TextArea(transitionName, style, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.Height(25));

        //En vez de is null deberia checkeear si ha cambiado
        if (parent.focusedObj is null)
        {
            CheckNameExisting(parent, transitionName);
        }
    }

    /// <summary>
    /// The Equals
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (!base.Equals((Transition)other))
            return false;
        if (this.transitionName != ((Transition)other).transitionName)
            return false;
        if (!fromNode.Equals(((Transition)other).fromNode) || !toNode.Equals(((Transition)other).toNode))
            return false;

        return true;
    }
}
