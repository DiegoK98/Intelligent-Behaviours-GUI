﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Error
{
    //The higher the number, the higher its priority
    // Number must be unique
    NoFactors = 5,
    NoEntryState = 4,
    MoreThanOneRoot = 3,
    RepeatedName = 2
}

public enum stateType
{
    Default,
    Entry,
    Unconnected
}

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

public class Enums
{
    /// <summary>
    /// Transforms the <paramref name="error"/> into a pre-defined message
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static string EnumToString(Error error, ClickableElement current)
    {
        string prompt = current ? "Error at " + current.elementName + ": " : "Error at unknown";

        switch (error)
        {
            case Error.NoEntryState:
                prompt += "You can't have a FSM without an Entry State";
                break;
            case Error.RepeatedName:
                prompt += "You can't have two elements with the same name";
                break;
            case Error.MoreThanOneRoot:
                prompt += "You can't have a BT with more than one Root";
                break;
            case Error.NoFactors:
                prompt += "You can't have an Action without any Factors";
                break;
            default:
                prompt += "Unexpected error :(";
                break;
        }

        return prompt;
    }
}
