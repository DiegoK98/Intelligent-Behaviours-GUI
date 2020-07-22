using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Error
{
    //The higher the number, the higher its priority
    // Number must be unique
    NoEntryState = 4,
    MoreThanOneRoot = 3,
    RepeatedName = 2
}

public enum Warning
{
    NoFactors = 3,
    WeightZero = 2,
    UnconnectedNode = 1
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
    public static string ErrorToString(Error error, ClickableElement current)
    {
        string prompt = "Error at " + (current ? current.elementName : "unknown") + ": ";

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
            default:
                prompt += "Unknown error :(";
                break;
        }

        return prompt;
    }

    /// <summary>
    /// Transforms the <paramref name="warning"/> into a pre-defined message
    /// </summary>
    /// <param name="warning"></param>
    /// <param name="current"></param>
    /// <returns></returns>
    public static string WarningToString(Warning warning, ClickableElement current)
    {
        string prompt = "Warning at " + (current ? current.elementName : "unknown") + ": ";

        switch (warning)
        {
            case Warning.NoFactors:
                prompt += "You have at least one Action node without any Factors connected to it";
                break;
            case Warning.WeightZero:
                prompt += "Having a Factor with a weight value of zero means it will be ignored";
                break;
            case Warning.UnconnectedNode:
                prompt += "At least one node is disconnected from the entry state";
                break;
            default:
                prompt += "Unknown warning :(";
                break;
        }

        return prompt;
    }
}
