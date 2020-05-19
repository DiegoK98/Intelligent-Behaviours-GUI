using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XMLElement
{
    public string Id { get; set; }

    public string elemType { get; set; }

    public string secondType { get; set; } = "";

    public string name { get; set; }

    public int NProperty { get; set; }

    public float windowPosX { get; set; }

    public float windowPosY { get; set; }

    public List<XMLElement> nodes { get; set; }

    public List<string> transitions { get; set; }
}
