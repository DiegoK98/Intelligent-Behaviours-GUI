using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class BaseNode : GUIElement
{
    public string nodeName = "";
    public readonly long identificator;

    public Rect windowRect;
    public static int width = 150;
    public static int height = 70;

    public int NProperty = 0;

    public BaseNode()
    {
        identificator = UniqueID();
    }

    public override bool Equals(object other)
    {
        if (!base.Equals((BaseNode)other))
            return false;
        if (this.nodeName != ((BaseNode)other).nodeName)
            return false;
        if (this.identificator != ((BaseNode)other).identificator)
            return false;

        return true;
    }

    public void DrawWindow(NodeEditor parent)
    {
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.LowerCenter;
        style.fontSize = 15;

        GUIStyle style2 = new GUIStyle();
        style2.alignment = TextAnchor.LowerCenter;
        style2.fontSize = 15;

        if (this is BehaviourNode)
        {
            switch (((BehaviourNode)this).type)
            {
                case BehaviourNode.behaviourType.Selector:
                case BehaviourNode.behaviourType.Sequence:
                case BehaviourNode.behaviourType.Leaf:
                    nodeName = EditorGUILayout.TextArea(nodeName, style, GUILayout.ExpandWidth(true), GUILayout.Height(25));
                    break;
                case BehaviourNode.behaviourType.LoopN:
                case BehaviourNode.behaviourType.DelayT:
                    NProperty = int.Parse(EditorGUILayout.TextArea(NProperty.ToString(), style2, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                    break;
            }
        }
        else
        {
            nodeName = EditorGUILayout.TextArea(nodeName, style, GUILayout.ExpandWidth(true), GUILayout.Height(25));
        }

        //En vez de is null deberia checkeear si ha cambiado
        if (parent.focusedObj is null)
        {
            CheckNameExisting(parent, nodeName);
        }
    }
}
