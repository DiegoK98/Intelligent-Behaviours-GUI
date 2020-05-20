﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class PerceptionGUI : GUIElement
{
    public bool isSecondChild;

    public int treeLevel;

    public perceptionType type;

    public int timerNumber;

    public string elemName;

    public string stateName;

    public ReturnValues status;

    public bool openFoldout;

    public PerceptionGUI firstChild;

    public PerceptionGUI secondChild;

    /// <summary>
    /// The Equals
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public override bool Equals(object other)
    {
        if (!base.Equals(other))
            return false;
        if (this.identificator != ((PerceptionGUI)other).identificator)
            return false;

        return true;
    }

    public override string GetTypeString()
    {
        return type.ToString() + " Perception";
    }

    /// <summary>
    /// The InitTransitionGUI
    /// </summary>
    /// <param name="name"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public void InitPerceptionGUI(bool isSecondChild, int treeLevel, perceptionType type)
    {
        identificator = UniqueID();

        this.isSecondChild = isSecondChild;
        this.treeLevel = treeLevel;
        this.type = type;

        timerNumber = 0;
        openFoldout = false;

        if (type == perceptionType.IsInState)
        {
            elemName = "Select a FSM";
        }
        else if (type == perceptionType.BehaviourTreeStatus)
        {
            elemName = "Select a BT";
        }
        else
        {
            elemName = "";
        }

        stateName = "Select a State";

        status = ReturnValues.Succeed;

        if (type == perceptionType.And || type == perceptionType.Or)
        {
            firstChild = CreateInstance<PerceptionGUI>();
            firstChild.InitPerceptionGUI(false, treeLevel + 1, perceptionType.Push);
            secondChild = CreateInstance<PerceptionGUI>();
            secondChild.InitPerceptionGUI(true, treeLevel + 1, perceptionType.Push);
        }
        else
        {
            firstChild = null;
            secondChild = null;
        }
    }
}