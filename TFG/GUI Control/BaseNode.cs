using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public abstract class BaseNode : GUIElement
{
    public string nodeName = "";

    public readonly long identificator;

    public Rect windowRect;

    public static int width = 150;

    public static int height = 70;

    public int NProperty = 0;

    /// <summary>
    /// The BaseNode
    /// </summary>
    public BaseNode()
    {
        identificator = UniqueID();
    }

    /// <summary>
    /// The Equals
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Draws all the elements inside the Node window
    /// </summary>
    /// <param name="parent"></param>
    public void DrawWindow(NodeEditor parent)
    {
        if (this is BehaviourNode)
        {
            switch (((BehaviourNode)this).type)
            {
                case BehaviourNode.behaviourType.Selector:
                case BehaviourNode.behaviourType.Sequence:
                case BehaviourNode.behaviourType.Leaf:
                    nodeName = EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25));
                    break;
                case BehaviourNode.behaviourType.LoopN:
                case BehaviourNode.behaviourType.DelayT:
                    NProperty = int.Parse(EditorGUILayout.TextArea(NProperty.ToString(), Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
                    break;
            }
        }
        else
        {
            nodeName = EditorGUILayout.TextArea(nodeName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25));
        }
    }
}
