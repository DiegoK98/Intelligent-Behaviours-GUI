﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enums
{
    public enum Errors
    {
        //The higher the number, the higher its priority
        NoEntryState = 3,
        RepeatedName = 2
    }

    /// <summary>
    /// Transforms the error given into a pre-defined message
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static string EnumToString(Errors error)
    {
        switch (error)
        {
            case Errors.NoEntryState:
                return "ERROR: You can't have a FSM without an Entry State";
            case Errors.RepeatedName:
                return "ERROR: You can't have two elements with the same name";
            default:
                return "ERROR: Unexpected error :(";
        }
    }
}