using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIElement : ScriptableObject
{
    public bool isFocused = false;

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
}
