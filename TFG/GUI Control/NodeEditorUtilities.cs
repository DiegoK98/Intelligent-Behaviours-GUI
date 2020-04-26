using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class NodeEditorUtilities
{
    /// <summary>
    /// C#'s Script Icon [The one MonoBhevaiour Scripts have].
    /// </summary>
    private static Texture2D scriptIcon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);

    static string tab = "    ";
    static string actionsEnding = "Action";
    static string conditionsEnding = "SuccessCheck";
    static string classEnding = "_class";
    static string subFSMEnding = "_SubFSM";
    static string subBtEnding = "_SubBT";

    /// <summary>
    /// Creates a new C# for a FSM.
    /// </summary>
    public static void GenerateElemCode(ClickableElement elem)
    {
        string templatePath = "none";

        switch (elem.GetType().ToString())
        {
            case nameof(FSM):
                templatePath = "FSM_Template.cs";
                break;
            case nameof(BehaviourTree):
                templatePath = "BT_Template.cs";
                break;
        }
        string[] guids = AssetDatabase.FindAssets(templatePath);
        if (guids.Length == 0)
        {
            Debug.LogWarning(templatePath + ".txt not found in asset database");
            return;
        }
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);

        CreateAsset(CleanName(elem.elementName) + ".cs", path, elem);
    }

    private static void CreateAsset(string initialName, string templatePath, ClickableElement elem)
    {
        if (!AssetDatabase.IsValidFolder("Assets/AI Scripts"))
            AssetDatabase.CreateFolder("Assets", "AI Scripts");
        Object o = CreateScript("Assets/AI Scripts/" + initialName, templatePath, elem);
        ProjectWindowUtil.ShowCreatedAsset(o);
    }

    /// <summary>
    /// Creates Script from Template's path.
    /// </summary>
    private static UnityEngine.Object CreateScript(string pathName, string templatePath, ClickableElement elem)
    {
        string templateText = string.Empty;

        UTF8Encoding encoding = new UTF8Encoding(true, false);

        if (File.Exists(templatePath))
        {
            // Read procedures
            StreamReader reader = new StreamReader(templatePath);
            templateText = reader.ReadToEnd();
            reader.Close();

            // Replace the tags with the corresponding parts
            string className = Path.GetFileNameWithoutExtension(pathName);
            List<ClickableElement> subElems = new List<ClickableElement>();

            templateText = templateText.Replace("#SCRIPTNAME#", className);

            switch (elem.GetType().ToString())
            {
                case nameof(FSM):
                    templateText = templateText.Replace("#ENDING#", "_FSM");
                    templateText = templateText.Replace("#FSMCREATE#", GetFSMCreate(elem, "_FSM", false, ref subElems));
                    templateText = GetAllSubElemsRecursive(templateText, ref subElems);
                    templateText = templateText.Replace("#SUBELEMCREATE#", string.Empty);
                    break;
                case nameof(BehaviourTree):
                    templateText = templateText.Replace("#ENDING#", "_BT");
                    templateText = templateText.Replace("#BTCREATE#", GetBTCreate(elem, "_BT", false, ref subElems));
                    templateText = GetAllSubElemsRecursive(templateText, ref subElems);
                    templateText = templateText.Replace("#SUBELEMCREATE#", string.Empty);
                    break;
            }
            templateText = templateText.Replace("#ACTIONS#", GetMethods(elem));

            // SubFSM
            templateText = templateText.Replace("#SUBELEM1#", GetSubElemDecl(elem, subElems));
            templateText = templateText.Replace("#SUBELEM2#", GetSubElemInit(elem, subElems));
            templateText = templateText.Replace("#SUBELEM3#", GetSubFSMUpdate(elem, subElems));

            /// You can replace as many tags you make on your templates, just repeat Replace function
            /// e.g.:
            /// templateText = templateText.Replace("#NEWTAG#", "MyText");

            // Write procedures
            StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
            writer.Write(templateText);
            writer.Close();

            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }
        else
        {
            Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
            return null;
        }
    }

    //This name cleaning shit should be done in the GUI and not here
    private static string CleanName(string name)
    {
        string result;

        result = string.Concat(name.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !char.IsSymbol(c)));

        return result;
    }

    private static string GetAllSubElemsRecursive(string templateText, ref List<ClickableElement> subElems)
    {
        List<ClickableElement> subElemsCopy = new List<ClickableElement>();
        foreach (ClickableElement sub in subElems)
        {
            if (sub is FSM)
                templateText = templateText.Replace("#SUBELEMCREATE#", GetFSMCreate(sub, subFSMEnding, true, ref subElemsCopy));
            if (sub is BehaviourTree)
                templateText = templateText.Replace("#SUBELEMCREATE#", GetBTCreate(sub, subBtEnding, true, ref subElemsCopy));
        }
        if (subElemsCopy.Count > 0)
        {
            templateText = GetAllSubElemsRecursive(templateText, ref subElemsCopy);
            subElems.AddRange(subElemsCopy);
        }

        return templateText;
    }

    private static string GetSubElemDecl(ClickableElement elem, List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;

        foreach (ClickableElement sub in subElems)
        {
            string engineEnding = sub is FSM ? subFSMEnding : sub is BehaviourTree ? subBtEnding : "";
            string type = sub is FSM ? "StateMachineEngine" : sub is BehaviourTree ? "BehaviourTreeEngine" : "";
            string elemName = CleanName(sub.elementName);
            result += "private " + type + " " + elemName + engineEnding + ";\n" + tab;
        }

        return result;
    }

    private static string GetSubElemInit(ClickableElement elem, List<ClickableElement> subElems)
    {

        string result = string.Empty;

        for (int i = subElems.Count - 1; i >= 0; i--)
        {
            string engineEnding = subElems[i] is FSM ? subFSMEnding : subElems[i] is BehaviourTree ? subBtEnding : "";
            string elemName = CleanName(subElems[i].elementName) + engineEnding;
            result += "Create" + elemName + "();\n" + tab + tab;
        }

        return result;
    }

    private static string GetFSMCreate(ClickableElement elem, string engineEnding, bool isSub, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string machineName = className + engineEnding;
        string createdName = isSub ? machineName : "StateMachine";
        string isSubStr = isSub ? "true" : "false";
        string templateSub = "\n"
        + tab + "private void Create" + createdName + "()\n"
        + tab + "{\n"
        + tab + tab + machineName + " = new StateMachineEngine(" + isSubStr + ");\n"
        + tab + tab + "\n"
        + tab + tab + "// Perceptions\n"
        + tab + tab + "// Modify or add new Perceptions, see the guide for more\n"
        + tab + tab + "#PERCEPIONS#\n"
        + tab + tab + "// States\n"
        + tab + tab + "#STATES#\n"
        + tab + tab + "// Transitions#TRANSITIONS#\n"
        + tab + "}";

        if (isSub)
            templateSub += "#SUBELEMCREATE#";

        templateSub = templateSub.Replace("#PERCEPIONS#", GetPerceptions(elem, engineEnding));
        templateSub = templateSub.Replace("#STATES#", GetStates(elem, engineEnding, ref subElems));
        templateSub = templateSub.Replace("#TRANSITIONS#", GetTransitions(elem, engineEnding));

        return templateSub;
    }

    private static string GetBTCreate(ClickableElement elem, string engineEnding, bool isSub, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string machineName = className + engineEnding;
        string createdName = isSub ? machineName : "BehaviourTree";
        string isSubStr = isSub ? "true" : "false";
        string templateSub = "\n"
        + tab + "private void Create" + createdName + "()\n"
        + tab + "{\n"
        + tab + tab + machineName + " = new BehaviourTreeEngine(" + isSubStr + ");\n"
        + tab + tab + "\n"
        + tab + tab + "// Nodes\n"
        + tab + tab + "#NODES#\n"
        + tab + tab + "// Child adding#CHILDS#\n"
        + tab + tab + "// SetRoot\n"
        + tab + tab + "#SETROOT#\n"
        + tab + "}";

        if (isSub)
            templateSub += "#SUBELEMCREATE#";

        templateSub = templateSub.Replace("#NODES#", GetNodes(elem, engineEnding, ref subElems));
        templateSub = templateSub.Replace("#CHILDS#", GetChilds(elem, ref subElems));
        templateSub = templateSub.Replace("#SETROOT#", GetSetRoot(elem, engineEnding));

        return templateSub;
    }

    private static string GetSubFSMUpdate(ClickableElement elem, List<ClickableElement> subElems)
    {
        string result = string.Empty;

        foreach (ClickableElement sub in subElems)
        {
            string engineEnding = sub is FSM ? subFSMEnding : sub is BehaviourTree ? subBtEnding : "";
            string elemName = CleanName(sub.elementName);
            result += "\n" + tab + tab + elemName + engineEnding + ".Update();";
        }

        return result;
    }

    #region FSM
    private static string GetPerceptions(ClickableElement elem, string engineEnding)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (TransitionsGUI transition in ((FSM)elem).transitions)
        {
            string transitionName = CleanName(transition.transitionName);
            result += "Perception " + transitionName + "Perception = " + machineName + ".CreatePerception<PushPerception>();\n" + tab + tab;
        }

        return result;
    }

    private static string GetStates(ClickableElement elem, string engineEnding, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (StateNode node in ((FSM)elem).states)
        {
            string nodeName = CleanName(node.nodeName);
            if (node.elem is FSM)
            {
                result += "State " + nodeName + " = " + machineName + ".CreateSubStateMachine(\"" + node.nodeName + "\", " + nodeName + subFSMEnding + ");\n" + tab + tab;
                subElems.Add(node.elem);
            }
            else if (node.elem is BehaviourTree)
            {
                result += "State " + nodeName + " = " + machineName + ".CreateSubStateMachine(\"" + node.nodeName + "\", " + nodeName + subBtEnding + ");\n" + tab + tab;
                subElems.Add(node.elem);

            }
            else if (node.type == StateNode.stateType.Entry)
            {
                result += "State " + nodeName + " = " + machineName + ".CreateEntryState(\"" + node.nodeName + "\", " + nodeName + actionsEnding + ");\n" + tab + tab;
            }
            else
            {
                result += "State " + nodeName + " = " + machineName + ".CreateState(\"" + node.nodeName + "\", " + nodeName + actionsEnding + ");\n" + tab + tab;
            }
        }

        return result;
    }

    private static string GetTransitions(ClickableElement elem, string engineEnding)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (TransitionsGUI transition in ((FSM)elem).transitions)
        {
            string transitionName = CleanName(transition.transitionName);
            string fromNodeName = CleanName(transition.fromNode.nodeName);
            string toNodeName = CleanName(transition.toNode.nodeName);
            if (((StateNode)transition.fromNode).elem != null)
            {
                result += "\n" + tab + tab + machineName + ".CreateExitTransition(\"" + transition.transitionName + "\", " + fromNodeName + ", " + transitionName + "Perception, " + toNodeName + ");";
            }
            else
            {
                result += "\n" + tab + tab + machineName + ".CreateTransition(\"" + transition.transitionName + "\", " + fromNodeName + ", " + transitionName + "Perception, " + toNodeName + ");";
            }
        }

        if (elem.parent is BehaviourTree)
            result += "\n" + tab + tab + machineName + ".CreateExitTransition(\"" + machineName + " Exit" + "\", null /*Change this for a node*/, null /*Change this for a perception*/, ReturnValues.Succeed);";

        return result;
    }

    #endregion

    #region Behaviour Tree

    private static string GetNodes(ClickableElement elem, string engineEnding, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.FindAll(n => n.type <= BehaviourNode.behaviourType.Leaf))
        {
            string nodeName = CleanName(node.nodeName);

            switch (node.type)
            {
                case BehaviourNode.behaviourType.Selector:
                    result += "SelectorNode " + nodeName + " = " + machineName + ".CreateSelectorNode(\"" + node.nodeName + "\");\n" + tab + tab;
                    break;
                case BehaviourNode.behaviourType.Sequence:
                    result += "SequenceNode " + nodeName + " = " + machineName + ".CreateSequenceNode(\"" + node.nodeName + "\", false);\n" + tab + tab;
                    break;
                case BehaviourNode.behaviourType.Leaf:
                    if (node.elem is FSM)
                    {
                        result += "LeafNode " + nodeName + " = " + machineName + ".CreateSubBehaviour(\"" + node.nodeName + "\", " + nodeName + subFSMEnding + ");\n" + tab + tab;
                        subElems.Add(node.elem);
                    }
                    else if (node.elem is BehaviourTree)
                    {
                        result += "LeafNode " + nodeName + " = " + machineName + ".CreateSubBehaviour(\"" + node.nodeName + "\", " + nodeName + subBtEnding + ");\n" + tab + tab;
                        subElems.Add(node.elem);
                    }
                    else
                    {
                        result += "LeafNode " + nodeName + " = " + machineName + ".CreateLeafNode(\"" + node.nodeName + "\", " + nodeName + actionsEnding + ", " + nodeName + conditionsEnding + ");\n" + tab + tab;
                    }
                    break;
            }
        }

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.FindAll(n => n.type > BehaviourNode.behaviourType.Leaf))
        {
            string nodeName = CleanName(node.nodeName);
            TransitionsGUI decoratorConnection = ((BehaviourTree)elem).connections.Where(t => node.Equals(t.fromNode)).FirstOrDefault();
            string subNodeName = CleanName(decoratorConnection.toNode.nodeName);
            TransitionsGUI decoratorConnectionsub;

            switch (((BehaviourNode)decoratorConnection.toNode).type)
            {
                case BehaviourNode.behaviourType.LoopN:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "LoopN_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case BehaviourNode.behaviourType.LoopUntilFail:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "LoopUntilFail_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case BehaviourNode.behaviourType.Inverter:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Inverter_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case BehaviourNode.behaviourType.DelayT:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Timer_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case BehaviourNode.behaviourType.Succeeder:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Succeeder_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case BehaviourNode.behaviourType.Conditional:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Conditional_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
            }

            switch (node.type)
            {
                case BehaviourNode.behaviourType.LoopN:
                    string loopNodeName = "LoopN_" + subNodeName;
                    result += "LoopDecoratorNode " + loopNodeName + " = " + machineName + ".CreateLoopNode(\"" + loopNodeName + "\", " + subNodeName + ", " + node.NProperty + ");\n" + tab + tab;
                    break;
                case BehaviourNode.behaviourType.LoopUntilFail:
                    string loopUntilNodeName = "LoopUntilFail_" + subNodeName;
                    result += "LoopUntilFailDecoratorNode " + loopUntilNodeName + " = " + machineName + ".CreateLoopUntilFailNode(\"" + loopUntilNodeName + "\", " + subNodeName + ");\n" + tab + tab;
                    break;
                case BehaviourNode.behaviourType.Inverter:
                    string InverterNodeName = "Inverter_" + subNodeName;
                    result += "InverterDecoratorNode " + InverterNodeName + " = " + machineName + ".CreateInverterNode(\"" + InverterNodeName + "\", " + subNodeName + ");\n" + tab + tab;
                    break;
                case BehaviourNode.behaviourType.DelayT:
                    string TimerNodeName = "Timer_" + subNodeName;
                    result += "TimerDecoratorNode " + TimerNodeName + " = " + machineName + ".CreateTimerNode(\"" + TimerNodeName + "\", " + subNodeName + ", " + node.NProperty + ");\n" + tab + tab;
                    break;
                case BehaviourNode.behaviourType.Succeeder:
                    string SucceederNodeName = "Succeeder_" + subNodeName;
                    result += "SucceederDecoratorNode " + SucceederNodeName + " = " + machineName + ".CreateSucceederNode(\"" + SucceederNodeName + "\", " + subNodeName + ");\n" + tab + tab;
                    break;
                case BehaviourNode.behaviourType.Conditional:
                    string ConditionalNodeName = "Conditional_" + subNodeName;
                    result += "ConditionalDecoratorNode " + ConditionalNodeName + " = " + machineName + ".CreateConditionalNode(\"" + ConditionalNodeName + "\", " + subNodeName + ", null /*Change this for a perception*/);\n" + tab + tab;
                    break;
            }
        }

        return result;
    }

    private static string GetChilds(ClickableElement elem, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.Where(n => n.type < BehaviourNode.behaviourType.Leaf && ((BehaviourTree)elem).ChildrenCount(n) > 0))
        {
            string nodeName = CleanName(node.nodeName);
            result += "\n" + tab + tab;
            foreach (BehaviourNode toNode in ((BehaviourTree)elem).connections.FindAll(t => node.Equals(t.fromNode)).Select(o => o.toNode))
            {
                string toNodeName = CleanName(toNode.nodeName);
                TransitionsGUI decoratorConnection = ((BehaviourTree)elem).connections.Where(t => toNode.Equals(t.fromNode)).FirstOrDefault();
                if (decoratorConnection != null)
                {
                    string subNodeName = CleanName(decoratorConnection.toNode.nodeName);
                    TransitionsGUI decoratorConnectionsub;

                    switch (((BehaviourNode)decoratorConnection.toNode).type)
                    {
                        case BehaviourNode.behaviourType.LoopN:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "LoopN_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case BehaviourNode.behaviourType.LoopUntilFail:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "LoopUntilFail_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case BehaviourNode.behaviourType.Inverter:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Inverter_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case BehaviourNode.behaviourType.DelayT:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Timer_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case BehaviourNode.behaviourType.Succeeder:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Succeeder_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case BehaviourNode.behaviourType.Conditional:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Conditional_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                    }

                    switch (toNode.type)
                    {
                        case BehaviourNode.behaviourType.LoopN:
                            toNodeName = "LoopN_" + subNodeName;
                            break;
                        case BehaviourNode.behaviourType.LoopUntilFail:
                            toNodeName = "LoopUntilFail_" + subNodeName;
                            break;
                        case BehaviourNode.behaviourType.Inverter:
                            toNodeName = "Inverter_" + subNodeName;
                            break;
                        case BehaviourNode.behaviourType.DelayT:
                            toNodeName = "Timer_" + subNodeName;
                            break;
                        case BehaviourNode.behaviourType.Succeeder:
                            toNodeName = "Succeeder_" + subNodeName;
                            break;
                        case BehaviourNode.behaviourType.Conditional:
                            toNodeName = "Conditional_" + subNodeName;
                            break;
                    }
                }

                result += nodeName + ".AddChild(" + toNodeName + ");\n" + tab + tab;
            }
        }

        return result;
    }

    private static string GetSetRoot(ClickableElement elem, string engineEnding)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes)
        {
            if (node.isRootNode)
                result += machineName + ".SetRootNode(" + CleanName(node.nodeName) + ");";
        }

        return result;
    }

    #endregion

    private static string GetMethods(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;

        switch (elem.GetType().ToString())
        {
            case nameof(FSM):
                foreach (StateNode node in ((FSM)elem).states)
                {
                    if (node.elem != null)
                    {
                        result += GetMethods(node.elem);
                    }
                    else
                    {
                        string nodeName = CleanName(node.nodeName);
                        result += "\n" + tab +
                          "private void " + nodeName + actionsEnding + "()\n"
                          + tab + "{\n"
                          + tab + tab + "\n"
                          + tab + "}\n"
                          + tab;
                    }
                }
                break;
            case nameof(BehaviourTree):
                foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.FindAll(n => n.type == BehaviourNode.behaviourType.Leaf))
                {
                    if (node.elem != null)
                    {
                        result += GetMethods(node.elem);
                    }
                    else
                    {
                        string nodeName = CleanName(node.nodeName);
                        result += "\n" + tab +
                          "private void " + nodeName + actionsEnding + "()\n"
                          + tab + "{\n"
                          + tab + tab + "\n"
                          + tab + "}\n"
                          + tab;

                        result += "\n" + tab +
                          "private ReturnValues " + nodeName + conditionsEnding + "()\n"
                          + tab + "{\n"
                          + tab + tab + "//Write here the code for the success check for " + nodeName + "\n"
                          + tab + tab + "return ReturnValues.Failed;\n"
                          + tab + "}\n"
                          + tab;
                    }
                }
                break;
        }

        return result;
    }
}
