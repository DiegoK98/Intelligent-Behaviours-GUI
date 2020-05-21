using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class PerceptionGUI : GUIElement
{
    public perceptionType type;

    public int timerNumber;

    public string elemName;

    public string customName;

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
    /// The default initiator
    /// </summary>
    /// <param name="type"></param>
    public void InitPerceptionGUI(perceptionType type)
    {
        identificator = UniqueID();

        this.type = type;

        timerNumber = 0;
        customName = "CustomName";
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
            firstChild.InitPerceptionGUI(perceptionType.Push);
            secondChild = CreateInstance<PerceptionGUI>();
            secondChild.InitPerceptionGUI(perceptionType.Push);
        }
        else
        {
            firstChild = null;
            secondChild = null;
        }
    }

    public PerceptionXML ToPerceptionXML()
    {
        PerceptionXML result = new PerceptionXML
        {
            Id = this.identificator,
            type = this.type,
            timerNumber = this.timerNumber,
            customName = this.customName,
            elemName = this.elemName,
            stateName = this.stateName,
            status = this.status,
            openFoldout = this.openFoldout
        };

        if (this.firstChild != null)
            result.firstChild = this.firstChild.ToPerceptionXML();
        if (this.secondChild != null)
            result.secondChild = this.secondChild.ToPerceptionXML();

        return result;
    }

    public override XMLElement ToXMLElement(params object[] args)
    {
        throw new NotImplementedException();
    }
}
