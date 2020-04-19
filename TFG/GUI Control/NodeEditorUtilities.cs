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

    /// <summary>
    /// Creates a new C# for a FSM.
    /// </summary>
    public static void CreateElem(ClickableElement elem)
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
        CreateFromTemplate(
            elem.elementName + ".cs",
            path,
            elem
        );
    }

    private static void CreateFromTemplate(string initialName, string templatePath, ClickableElement elem)
    {
        //ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
        //    0,
        //    ScriptableObject.CreateInstance<DoCreateCodeFile>(),
        //    initialName,
        //    scriptIcon,
        //    templatePath
        //);

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
            string className = CleanName(Path.GetFileNameWithoutExtension(pathName));

            templateText = templateText.Replace("#SCRIPTNAME#", className);

            switch (elem.GetType().ToString())
            {
                case nameof(FSM):
                    templateText = templateText.Replace("#PERCEPIONS#", GetPerceptions(elem));
                    templateText = templateText.Replace("#STATES#", GetStates(elem));
                    templateText = templateText.Replace("#TRANSITIONS#", GetTransitions(elem));
                    break;
                case nameof(BehaviourTree):
                    templateText = templateText.Replace("#NODES#", GetNodes(elem));
                    templateText = templateText.Replace("#CHILDS#", GetChilds(elem));
                    templateText = templateText.Replace("#SETROOT#", GetSetRoot(elem));
                    break;
            }
            templateText = templateText.Replace("#ACTIONS#", GetMethods(elem));
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

    private static string GetPerceptions(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + "_FSM";

        foreach (TransitionsGUI transition in ((FSM)elem).transitions)
        {
            string transitionName = CleanName(transition.transitionName);
            result += "Perception " + transitionName + "Perception = " + machineName + ".CreatePerception<PushPerception>();\n" + tab + tab;
        }

        return result;
    }

    private static string GetStates(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + "_FSM";

        foreach (StateNode node in ((FSM)elem).states)
        {
            string nodeName = CleanName(node.nodeName);
            if (node.type == StateNode.stateType.Entry)
                result += "State " + nodeName + " = " + machineName + ".CreateEntryState(\"" + node.nodeName + "\", " + nodeName + actionsEnding + ");\n" + tab + tab;
            else
                result += "State " + nodeName + " = " + machineName + ".CreateState(\"" + node.nodeName + "\", " + nodeName + actionsEnding + ");\n" + tab + tab;
        }

        return result;
    }

    private static string GetTransitions(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + "_FSM";

        foreach (TransitionsGUI transition in ((FSM)elem).transitions)
        {
            string transitionName = CleanName(transition.transitionName);
            string fromNodeName = CleanName(transition.toNode.nodeName);
            string toNodeName = CleanName(transition.fromNode.nodeName);
            result += "\n" + tab + tab + machineName + ".CreateTransition(\"" + transition.transitionName + "\", " + fromNodeName + ", " + transitionName + "Perception, " + toNodeName + ");";
        }

        return result;
    }

    private static string GetNodes(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + "_BT";

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes)
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
                    result += "LeafNode " + nodeName + " = " + machineName + ".CreateLeafNode(\"" + node.nodeName + "\", " + nodeName + actionsEnding + ", " + nodeName + conditionsEnding + ");\n" + tab + tab;
                    break;
            }
        }

        return result;
    }

    private static string GetChilds(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.Where(n => n.type != BehaviourNode.behaviourType.Leaf))
        {
            string nodeName = CleanName(node.nodeName);
            result += "\n" + tab + tab;
            foreach (BehaviourNode toNode in ((BehaviourTree)elem).connections.FindAll(t => node.Equals(t.fromNode)).Select(o => o.toNode))
            {
                string toNodeName = CleanName(toNode.nodeName);
                result += nodeName + ".AddChild(" + toNodeName + ");\n" + tab + tab;
            }
        }

        return result;
    }

    private static string GetSetRoot(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + "_BT";
        string nodeName = "";

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes)
        {
            if (!node.isRootNode)
                continue;
            nodeName = CleanName(node.nodeName);
            break;
        }
        result += machineName + ".SetRootNode(" + nodeName + ");";

        return result;
    }

    private static string GetMethods(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;

        switch (elem.GetType().ToString())
        {
            case nameof(FSM):
                foreach (StateNode node in ((FSM)elem).states)
                {
                    string nodeName = CleanName(node.nodeName);
                    result += "\n" + tab + "private void " + nodeName + actionsEnding + "()\n"
                      + tab + "{\n"
                      + tab + tab + "\n"
                      + tab + "}\n" + tab;
                }
                break;
            case nameof(BehaviourTree):
                foreach (BehaviourNode node in ((BehaviourTree)elem).nodes)
                {
                    string nodeName = CleanName(node.nodeName);
                    result += "\n" + tab + "private void " + nodeName + actionsEnding + "()\n"
                      + tab + "{\n"
                      + tab + tab + "\n"
                      + tab + "}\n" + tab;

                    result += "\n" + tab + "private ReturnValues " + nodeName + conditionsEnding + "()\n"
                      + tab + "{\n"
                      + tab + tab + "//Write here the code for the success check for " + nodeName + "\n"
                      + tab + tab + "return ReturnValues.Failed;\n"
                      + tab + "}\n" + tab;
                }
                break;
        }

        return result;
    }
}
