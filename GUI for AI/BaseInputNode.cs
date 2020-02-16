using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInputNode : BaseNode
{
    public virtual string getResult()
    {
        return "None";
    }

    public override void DrawCurves()
    {
        
    }
}
