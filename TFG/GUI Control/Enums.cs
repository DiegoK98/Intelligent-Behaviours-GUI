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
    /// Transforms the error given into a pre-defined message
    /// </summary>
    /// <param name="error"></param>
    /// <returns></returns>
    public static string EnumToString(Error error)
    {
        switch (error)
        {
            case Error.NoEntryState:
                return "ERROR: You can't have a FSM without an Entry State";
            case Error.RepeatedName:
                return "ERROR: You can't have two elements with the same name";
            case Error.MoreThanOneRoot:
                return "ERROR: You can't have a BT with more than one Root";
            default:
                return "ERROR: Unexpected error :(";
        }
    }
}
