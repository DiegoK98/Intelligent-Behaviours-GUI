using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GUIElement : ScriptableObject
{
    public bool isFocused = false;

    public long identificator;

    /// <summary>
    /// The UniqueID
    /// </summary>
    /// <returns></returns>
    public long UniqueID()
    {
        long i = 1;

        foreach (byte b in Guid.NewGuid().ToByteArray())
        {
            i *= ((int)b + 1);
        }

        long number = (DateTime.Now.Ticks / 10) % 1000000000;

        return number;
    }

    public abstract string GetTypeString();

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
