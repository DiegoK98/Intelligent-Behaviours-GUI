using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionXML
{
    public string Id;

    public perceptionType type;

    public int timerNumber;

    public string customName;

    public string elemName;

    public string stateName;

    public ReturnValues status;

    public bool openFoldout;

    public PerceptionXML firstChild;

    public PerceptionXML secondChild;

    public PerceptionGUI ToGUIElement()
    {
        PerceptionGUI result = ScriptableObject.CreateInstance<PerceptionGUI>();
        result.identificator = this.Id;
        result.type = this.type;
        result.timerNumber = this.timerNumber;
        result.customName = this.customName;
        result.elemName = this.elemName;
        result.stateName = this.stateName;
        result.status = this.status;
        result.openFoldout = this.openFoldout;

        if (this.firstChild != null)
            result.firstChild = this.firstChild.ToGUIElement();
        if (this.secondChild != null)
            result.secondChild = this.secondChild.ToGUIElement();

        return result;
    }
}