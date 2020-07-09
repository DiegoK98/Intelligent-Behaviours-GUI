using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public abstract class BaseNode : GUIElement
{
    /// <summary>
    /// Name of the <see cref="BaseNode"/>
    /// </summary>
    public string nodeName = "";

    /// <summary>
    /// The <see cref="ClickableElement"/> that is contained in this node
    /// </summary>
    public ClickableElement subElem;

    /// <summary>
    /// Width of the <see cref="GUIElement.windowRect"/>
    /// </summary>
    public static int width = 140;

    /// <summary>
    /// Height of the <see cref="GUIElement.windowRect"/>
    /// </summary>
    public static int height = 63;

    /// <summary>
    /// The <see cref="ClickableElement"/> in which this <see cref="BaseNode"/> exists
    /// </summary>
    protected ClickableElement parent;

    /// <summary>
    /// The Initializer for the <seealso cref="BaseNode"/>
    /// </summary>
    public void InitBaseNode(ClickableElement parent)
    {
        identificator = UniqueID();
        this.parent = parent;
    }

    /// <summary>
    /// Compares this <see cref="BaseNode"/> with <paramref name="other"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (!base.Equals(other))
            return false;
        if (this.nodeName != ((BaseNode)other).nodeName)
            return false;
        if (this.identificator != ((BaseNode)other).identificator)
            return false;

        return true;
    }

    /// <summary>
    /// Draws all the elements inside the <see cref="BaseNode"/>
    /// </summary>
    public override abstract void DrawWindow();
}
