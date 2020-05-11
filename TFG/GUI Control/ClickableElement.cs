using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ClickableElement : GUIElement
{
    public enum elementType
    {
        FSM,
        BT
    }

    public Rect windowRect;

    public static int width = 150;

    public static int height = 70;

    public string elementName = "";

    public elementType type;

    public ClickableElement parent;

    /// <summary>
    /// Draws all the elements inside the Element window
    /// </summary>
    /// <param name="parent"></param>
    public void DrawWindow()
    {
        elementName = CleanName(EditorGUILayout.TextArea(elementName, Styles.TitleText, GUILayout.ExpandWidth(true), GUILayout.Height(25)));
    }

    /// <summary>
    /// Checks if a given name exists in this element
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public bool CheckNameExisting(string name, int threshold)
    {
        int repeatedNames = 0;

        if (this is FSM)
        {
            foreach (StateNode node in ((FSM)this).states)
            {

                if (node.nodeName == name)
                {
                    repeatedNames++;
                }
                if (node.subElem != null)
                {
                    if (node.subElem.CheckNameExisting(name, 0))
                        repeatedNames++;
                }
            }
            foreach (TransitionGUI transition in ((FSM)this).transitions)
            {
                if (transition.transitionName == name)
                {
                    repeatedNames++;
                }
            }
        }
        else if (this is BehaviourTree)
        {
            foreach (BehaviourNode node in ((BehaviourTree)this).nodes)
            {
                if (node.nodeName == name)
                {
                    repeatedNames++;
                }
            }
        }

        return repeatedNames > threshold;
    }
}
