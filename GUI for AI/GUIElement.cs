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

    protected void CheckNameExisting(NodeEditor parent, string name)
    {
        int totalCount = 0;

        totalCount += parent.Elements.FindAll(e => e.elementName == name).Count;

        if (parent.currentElem is FSM)
        {
            totalCount += ((FSM)parent.currentElem).states.FindAll(e => e.nodeName == name).Count;
            totalCount += ((FSM)parent.currentElem).transitions.FindAll(e => e.transitionName == name).Count;
        }
        else if (parent.currentElem is BehaviourTree)
        {
            totalCount += ((BehaviourTree)parent.currentElem).states.FindAll(e => e.nodeName == name).Count;
        }

        if (totalCount > 1)
        {
            parent.AddError(Enums.Errors.RepeatedName);
        }
        else
        {
            parent.RemoveError(Enums.Errors.RepeatedName);
        }
    }
}
