using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GUIElement : ScriptableObject
{
    public bool isFocused = false;

    internal static UniqueNamer uniqueNamer;

    public string identificator { get; set; }

    /// <summary>
    /// The UniqueID
    /// </summary>
    /// <returns></returns>
    public string UniqueID()
    {
        return Guid.NewGuid().ToString();
    }

    public abstract string GetTypeString();

    public abstract XMLElement ToXMLElement(params object[] args);

    /// <summary>
    /// Modifies the given string to remove unnecesary spaces and newlines
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string CleanName(string name)
    {
        string result;
        var numberChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        var spacesAndNewlines = new[] { ' ', '\n' };

        result = name.Trim(spacesAndNewlines);
        result = result.TrimStart(numberChars);

        return result;
    }
}
