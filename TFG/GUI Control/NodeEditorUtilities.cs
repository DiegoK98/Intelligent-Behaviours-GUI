using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

public class NodeEditorUtilities
{
    /// <summary>
    /// C# Script Icon
    /// </summary>
    private readonly static Texture2D scriptIcon = (EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D);

    /// <summary>
    /// 4 blank spaces to imitate pressing tab
    /// </summary>
    static readonly string tab = "    ";

    /// <summary>
    /// Text to add at the end of an Action's name
    /// </summary>
    static readonly string actionsEnding = "Action";

    /// <summary>
    /// Text to add at the end of a Conditions method's name
    /// </summary>
    static readonly string conditionsEnding = "SuccessCheck";

    /// <summary>
    /// Text to add at the end of the CreateMethod of a <see cref="FSM"/>
    /// </summary>
    static readonly string FSMCreateName = "StateMachine";

    /// <summary>
    /// Template for the CreateMethod of a <see cref="FSM"/>
    /// </summary>
    static readonly string FSMCreateTemplate = "\n"
        + tab + "private void Create#CREATENAME#()\n"
        + tab + "{\n"
        + tab + tab + "#MACHNAME# = new StateMachineEngine(#ISSUB#);\n"
        + tab + tab + "\n"
        + tab + tab + "// Perceptions\n"
        + tab + tab + "// Modify or add new Perceptions, see the guide for more\n"
        + tab + tab + "#PERCEPIONS#\n"
        + tab + tab + "// States\n"
        + tab + tab + "#STATES#\n"
        + tab + tab + "// Transitions#TRANSITIONS#\n"
        + tab + "}";

    /// <summary>
    /// Text to add at the end of the CreateMethod of a <see cref="BehaviourTree"/>
    /// </summary>
    static readonly string BTCreateName = "BehaviourTree";

    /// <summary>
    /// Template for the CreateMethod of a <see cref="BehaviourTree"/>
    /// </summary>
    static readonly string BTCreateTemplate = "\n"
        + tab + "private void Create#CREATENAME#()\n"
        + tab + "{\n"
        + tab + tab + "#MACHNAME# = new BehaviourTreeEngine(#ISSUB#);\n"
        + tab + tab + "\n"
        + tab + tab + "// Nodes\n"
        + tab + tab + "#NODES#\n"
        + tab + tab + "#CHILDS#\n"
        + tab + tab + "// SetRoot\n"
        + tab + tab + "#SETROOT#\n"
        + tab + "}";


    /// <summary>
    /// Text to add at the end of a FSM's name
    /// </summary>
    static readonly string FSMEnding = "_FSM";

    /// <summary>
    /// Text to add at the end of a BT's name
    /// </summary>
    static readonly string BTEnding = "_BT";

    /// <summary>
    /// Text to add at the end of a subFSM's name
    /// </summary>
    static readonly string subFSMEnding = "_SubFSM";

    /// <summary>
    /// Text to add at the end of a subBT's name
    /// </summary>
    static readonly string subBTEnding = "_SubBT";

    /// <summary>
    /// Name for the saves Folder
    /// </summary>
    static readonly string savesFolderName = "Saves (Intelligent Behaviours)";

    /// <summary>
    /// Name for the scripts Folder
    /// </summary>
    static readonly string scriptsFolderName = "Scripts (Intelligent Behaviours)";

    /// <summary>
    /// The <see cref="UniqueNamer"/> for managing the names of the variables of the perceptions
    /// </summary>
    static UniqueNamer uniqueNamer;

    /// <summary>
    /// Generates a new C# script for an <paramref name="elem"/>
    /// </summary>
    /// <param name="elem"></param>
    public static void GenerateElemScript(ClickableElement elem)
    {
        uniqueNamer = ScriptableObject.CreateInstance<UniqueNamer>();

        string path = "none";

        switch (elem.GetType().ToString())
        {
            case nameof(FSM):
                path = "FSM_Template.cs";
                break;
            case nameof(BehaviourTree):
                path = "BT_Template.cs";
                break;
        }
        string[] guids = AssetDatabase.FindAssets(path);
        if (guids.Length == 0)
        {
            Debug.LogWarning(path + ".txt not found in asset database");
            return;
        }
        string templatePath = AssetDatabase.GUIDToAssetPath(guids[0]);

        // Create Asset
        if (!AssetDatabase.IsValidFolder("Assets/" + scriptsFolderName))
            AssetDatabase.CreateFolder("Assets", scriptsFolderName);

        string scriptPath = EditorUtility.SaveFilePanel("Select a folder for the script", "Assets/" + scriptsFolderName, CleanName(elem.elementName) + ".cs", "CS");

        if (!string.IsNullOrEmpty(scriptPath))
        {
            UnityEngine.Object o = CreateScriptFromTemplate(scriptPath, templatePath, elem);
            AssetDatabase.Refresh();
            ProjectWindowUtil.ShowCreatedAsset(o);
        }
    }

    /// <summary>
    /// Generates a new XML file for an <paramref name="elem"/>
    /// </summary>
    /// <param name="elem"></param>
    public static void GenerateElemXMLFile(ClickableElement elem)
    {
        // Create Asset
        if (!AssetDatabase.IsValidFolder("Assets/" + savesFolderName))
            AssetDatabase.CreateFolder("Assets", savesFolderName);

        string path = EditorUtility.SaveFilePanel("Select a folder for the save file", "Assets/" + savesFolderName, CleanName(elem.elementName) + "_savedData.xml", "XML");

        if (!string.IsNullOrEmpty(path))
        {
            UnityEngine.Object o = CreateXMLFromElem(path, elem);
            AssetDatabase.Refresh();
            ProjectWindowUtil.ShowCreatedAsset(o);
        }
    }

    /// <summary>
    /// Shows the File Panel, and returns the <see cref="XMLElement"/> corresponding to the XML file that the user selects
    /// </summary>
    /// <returns></returns>
    public static XMLElement LoadSavedData()
    {
        string path = EditorUtility.OpenFilePanel("Open a save file", "Assets/Intelligent Behaviours Saves", "XML");

        if (string.IsNullOrEmpty(path))
            return null;

        return LoadXML(path);
    }

    /// <summary>
    /// Creates Script from <paramref name="templatePath"/>
    /// </summary>
    /// <param name="pathName"></param>
    /// <param name="templatePath"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static UnityEngine.Object CreateScriptFromTemplate(string pathName, string templatePath, object obj)
    {
        string templateText = string.Empty;

        string folderPath = pathName.Substring(0, pathName.LastIndexOf("/") + 1);

        UTF8Encoding encoding = new UTF8Encoding(true, false);

        if (File.Exists(templatePath))
        {
            // Read procedures
            StreamReader reader = new StreamReader(templatePath);
            templateText = reader.ReadToEnd();
            reader.Close();

            if (obj is ClickableElement)
            {
                ClickableElement elem = (ClickableElement)obj;

                // Replace the tags with the corresponding parts
                List<ClickableElement> subElems = new List<ClickableElement>();

                templateText = templateText.Replace("#SCRIPTNAME#", CleanName(elem.elementName));

                switch (elem.GetType().ToString())
                {
                    case nameof(FSM):
                        templateText = templateText.Replace("#ENDING#", FSMEnding);
                        templateText = templateText.Replace("#FSMCREATE#", GetCreateMethod(elem, false, ref subElems, folderPath));
                        templateText = GetAllSubElemsRecursive(templateText, ref subElems, folderPath);
                        templateText = templateText.Replace("#SUBELEMCREATE#", string.Empty);
                        break;
                    case nameof(BehaviourTree):
                        templateText = templateText.Replace("#ENDING#", BTEnding);
                        templateText = templateText.Replace("#BTCREATE#", GetCreateMethod(elem, false, ref subElems));
                        templateText = GetAllSubElemsRecursive(templateText, ref subElems, folderPath);
                        templateText = templateText.Replace("#SUBELEMCREATE#", string.Empty);
                        break;
                }
                templateText = templateText.Replace("#ACTIONS#", GetActionMethods(elem));

                // SubFSM
                templateText = templateText.Replace("#SUBELEM1#", GetSubElemDecl(subElems));
                templateText = templateText.Replace("#SUBELEM2#", GetSubElemInit(subElems));
                templateText = templateText.Replace("#SUBELEM3#", GetSubElemUpdate(subElems));
            }
            else if (obj is string)
            {
                string elemName = obj.ToString();

                templateText = templateText.Replace("#CUSTOMNAME#", elemName);
            }

            /// You can replace as many tags you make on your templates, just repeat Replace function
            /// e.g.:
            /// templateText = templateText.Replace("#NEWTAG#", "MyText");

            // Write procedures
            StreamWriter writer = new StreamWriter(Path.GetFullPath(pathName), false, encoding);
            writer.Write(templateText);
            writer.Close();

            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        }
        else
        {
            Debug.LogError(string.Format("The template file was not found: {0}", templatePath));
            return null;
        }
    }

    /// <summary>
    /// Creates XML object from <paramref name="elem"/>, serializes it to a file and saves it in <paramref name="pathName"/>
    /// </summary>
    /// <param name="pathName"></param>
    /// <param name="elem"></param>
    /// <returns></returns>
    private static UnityEngine.Object CreateXMLFromElem(string pathName, ClickableElement elem)
    {
        var data = elem.ToXMLElement();

        // Serialize to XML
        using (var stream = new FileStream(pathName, FileMode.Create))
        {
            XmlSerializer serial = new XmlSerializer(typeof(XMLElement));
            serial.Serialize(stream, data);
        }

        return null;
    }

    /// <summary>
    /// Loads an XML file and converts it to <see cref="XMLElement"/>
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private static XMLElement LoadXML(string fileName)
    {
        XmlSerializer serial = new XmlSerializer(typeof(XMLElement));

        serial.UnknownNode += new XmlNodeEventHandler(UnknownNode);
        serial.UnknownAttribute += new XmlAttributeEventHandler(UnknownAttribute);

        FileStream fs = new FileStream(fileName, FileMode.Open);

        return (XMLElement)serial.Deserialize(fs);
    }

    /// <summary>
    /// Event that is called when an node is unknown in the serialization
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void UnknownNode(object sender, XmlNodeEventArgs e)
    {
        Debug.LogError("[XMLSerializer] Unknown Node:" + e.Name + "\t" + e.Text);
    }

    /// <summary>
    /// Event that is called when an attribute is unknown in the serialization
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void UnknownAttribute(object sender, XmlAttributeEventArgs e)
    {
        System.Xml.XmlAttribute attr = e.Attr;
        Debug.LogError("[XMLSerializer] Unknown attribute " +
        attr.Name + "='" + attr.Value + "'");
    }

    /// <summary>
    /// Modifies the <paramref name="name"/> to be usable in code
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private static string CleanName(string name)
    {
        string result;
        var numberChars = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        var spacesAndNewlines = new[] { ' ', '\n' };

        result = name.Trim(spacesAndNewlines);
        result = string.Concat(result.Where(c => !char.IsWhiteSpace(c) && !char.IsPunctuation(c) && !char.IsSymbol(c)));
        result = result.TrimStart(numberChars);

        return result;
    }

    /// <summary>
    /// Writes the Create method for the <paramref name="elem"/>
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="isSub"></param>
    /// <param name="subElems"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    private static string GetCreateMethod(ClickableElement elem, bool isSub, ref List<ClickableElement> subElems, string folderPath = null)
    {
        // Set the strings to what they need to be
        string className = CleanName(elem.elementName);
        string engineEnding = "";
        string createName = "";
        string templateSub = "";
        if (elem is FSM)
        {
            engineEnding = isSub ? subFSMEnding : FSMEnding;
            createName = FSMCreateName;
            templateSub = FSMCreateTemplate;
        }
        else if (elem is BehaviourTree)
        {
            engineEnding = isSub ? subBTEnding : BTEnding;
            createName = BTCreateName;
            templateSub = BTCreateTemplate;
        }
        string machineName = className + engineEnding;
        string createdName = isSub ? machineName : createName;
        string isSubStr = isSub ? "true" : "false";

        // Adjust the template names
        templateSub = templateSub.Replace("#CREATENAME#", createdName);
        templateSub = templateSub.Replace("#MACHNAME#", machineName);
        templateSub = templateSub.Replace("#ISSUB#", isSubStr);

        if (isSub)
            templateSub += "#SUBELEMCREATE#";

        // Fill the template with the content
        if (elem is FSM)
        {
            templateSub = templateSub.Replace("#PERCEPIONS#", GetPerceptions(elem, engineEnding, folderPath));
            templateSub = templateSub.Replace("#STATES#", GetStates(elem, engineEnding, ref subElems));
            templateSub = templateSub.Replace("#TRANSITIONS#", GetTransitions(elem, engineEnding));
        }
        else if (elem is BehaviourTree)
        {
            templateSub = templateSub.Replace("#NODES#", GetNodes(elem, engineEnding, ref subElems));
            templateSub = templateSub.Replace("#CHILDS#", GetChilds(elem, ref subElems));
            templateSub = templateSub.Replace("#SETROOT#", GetSetRoot(elem, engineEnding));
        }

        return templateSub;
    }

    /// <summary>
    /// Writes the Create Method for all <paramref name="subElems"/>
    /// </summary>
    /// <param name="templateText"></param>
    /// <param name="subElems"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    private static string GetAllSubElemsRecursive(string templateText, ref List<ClickableElement> subElems, string folderPath = null)
    {
        List<ClickableElement> subElemsCopy = new List<ClickableElement>();
        foreach (ClickableElement sub in subElems)
        {
            if (sub is FSM)
                templateText = templateText.Replace("#SUBELEMCREATE#", GetCreateMethod(sub, true, ref subElemsCopy, folderPath));
            if (sub is BehaviourTree)
                templateText = templateText.Replace("#SUBELEMCREATE#", GetCreateMethod(sub, true, ref subElemsCopy));
        }
        if (subElemsCopy.Count > 0)
        {
            templateText = GetAllSubElemsRecursive(templateText, ref subElemsCopy, folderPath);
            subElems.AddRange(subElemsCopy);
        }

        return templateText;
    }

    /// <summary>
    /// Writes the declaration of all <paramref name="subElems"/>
    /// </summary>
    /// <param name="subElems"></param>
    /// <returns></returns>
    private static string GetSubElemDecl(List<ClickableElement> subElems)
    {
        string result = string.Empty;

        foreach (ClickableElement sub in subElems)
        {
            string engineEnding = sub is FSM ? subFSMEnding : sub is BehaviourTree ? subBTEnding : "";
            string type = sub is FSM ? "StateMachineEngine" : sub is BehaviourTree ? "BehaviourTreeEngine" : "";
            string elemName = CleanName(sub.elementName);
            result += "private " + type + " " + elemName + engineEnding + ";\n" + tab;
        }

        return result;
    }

    /// <summary>
    /// Writes the initialization of all <paramref name="subElems"/>
    /// </summary>
    /// <param name="subElems"></param>
    /// <returns></returns>
    private static string GetSubElemInit(List<ClickableElement> subElems)
    {

        string result = string.Empty;

        for (int i = subElems.Count - 1; i >= 0; i--)
        {
            string engineEnding = subElems[i] is FSM ? subFSMEnding : subElems[i] is BehaviourTree ? subBTEnding : "";
            string elemName = CleanName(subElems[i].elementName) + engineEnding;
            result += "Create" + elemName + "();\n" + tab + tab;
        }

        return result;
    }

    /// <summary>
    /// Writes the Update call of all <paramref name="subElems"/>
    /// </summary>
    /// <param name="subElems"></param>
    /// <returns></returns>
    private static string GetSubElemUpdate(List<ClickableElement> subElems)
    {
        string result = string.Empty;

        foreach (ClickableElement sub in subElems)
        {
            string engineEnding = sub is FSM ? subFSMEnding : sub is BehaviourTree ? subBTEnding : "";
            string elemName = CleanName(sub.elementName);
            result += "\n" + tab + tab + elemName + engineEnding + ".Update();";
        }

        return result;
    }

    #region FSM
    /// <summary>
    /// Writes all the <see cref="Perception"/> declaration and initialization
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="engineEnding"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    private static string GetPerceptions(ClickableElement elem, string engineEnding, string folderPath = null)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (TransitionGUI transition in ((FSM)elem).transitions)
        {
            string transitionName = CleanName(transition.transitionName);

            result += RecursivePerceptions(transition.rootPerception, transitionName, machineName, folderPath);
        }

        return result;
    }

    /// <summary>
    /// Recursive method for <see cref="GetPerceptions(ClickableElement, string, string)"/>
    /// </summary>
    /// <param name="perception"></param>
    /// <param name="transitionName"></param>
    /// <param name="machineName"></param>
    /// <param name="folderPath"></param>
    /// <returns></returns>
    private static string RecursivePerceptions(PerceptionGUI perception, string transitionName, string machineName, string folderPath)
    {
        string res = "";
        string auxAndOr = "";

        if (perception.type == perceptionType.And || perception.type == perceptionType.Or)
        {
            auxAndOr = perception.type.ToString();
            res += RecursivePerceptions(perception.firstChild, transitionName, machineName, folderPath);
            res += RecursivePerceptions(perception.secondChild, transitionName, machineName, folderPath);
        }

        string typeName;
        if (perception.type == perceptionType.Custom)
        {
            typeName = CleanName(perception.customName);

            string scriptName = typeName + "Perception.cs";

            // Generate the script for the custom perception if it doesn't exist already

            string[] assets = AssetDatabase.FindAssets(scriptName);
            if (assets.Length == 0)
            {
                string path = "CustomPerception_Template.cs";

                string[] guids = AssetDatabase.FindAssets(path);
                if (guids.Length == 0)
                {
                    Debug.LogWarning(path + ".txt not found in asset database");
                }
                else
                {
                    string templatePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    UnityEngine.Object o = CreateScriptFromTemplate(folderPath + scriptName, templatePath, typeName);
                }
            }
        }
        else
        {
            typeName = perception.type.ToString();
        }

        res += "Perception " + uniqueNamer.AddName(perception.identificator, typeName + "Perception") + " = " + machineName + ".Create" + auxAndOr + "Perception<" + typeName + "Perception" + ">(" + GetPerceptionParameters(perception) + ");\n" + tab + tab;

        return res;
    }

    /// <summary>
    /// Returns the corresponding parameters for the <paramref name="perception"/>
    /// </summary>
    /// <param name="perception"></param>
    /// <returns></returns>
    private static string GetPerceptionParameters(PerceptionGUI perception)
    {
        string result = "";

        switch (perception.type)
        {
            case perceptionType.Timer:
                result = perception.timerNumber.ToString();
                break;
            case perceptionType.IsInState:
                result = CleanName(perception.elemName) + subFSMEnding + ", " + "\"" + perception.stateName + "\"";
                break;
            case perceptionType.BehaviourTreeStatus:
                result = CleanName(perception.elemName) + subBTEnding + ", " + "ReturnValues." + perception.status.ToString();
                break;
            case perceptionType.And:
            case perceptionType.Or:
                result = uniqueNamer.GetName(perception.firstChild.identificator) + ", " + uniqueNamer.GetName(perception.secondChild.identificator);
                break;
            case perceptionType.Custom:
                result = "new " + uniqueNamer.GetName(perception.identificator) + "()";
                break;
        }

        return result;
    }

    /// <summary>
    /// Writes all the <see cref="State"/> declaration and initialization
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="engineEnding"></param>
    /// <param name="subElems"></param>
    /// <returns></returns>
    private static string GetStates(ClickableElement elem, string engineEnding, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (StateNode node in ((FSM)elem).states)
        {
            string nodeName = CleanName(node.nodeName);
            if (node.subElem is FSM)
            {
                result += "State " + nodeName + " = " + machineName + ".CreateSubStateMachine(\"" + node.nodeName + "\", " + nodeName + subFSMEnding + ");\n" + tab + tab;
                subElems.Add(node.subElem);
            }
            else if (node.subElem is BehaviourTree)
            {
                result += "State " + nodeName + " = " + machineName + ".CreateSubStateMachine(\"" + node.nodeName + "\", " + nodeName + subBTEnding + ");\n" + tab + tab;
                subElems.Add(node.subElem);

            }
            else if (node.type == stateType.Entry)
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

    /// <summary>
    /// Writes all the <see cref="Transition"/> declaration and initialization
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="engineEnding"></param>
    /// <returns></returns>
    private static string GetTransitions(ClickableElement elem, string engineEnding)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (TransitionGUI transition in ((FSM)elem).transitions)
        {
            string transitionName = CleanName(transition.transitionName);
            string fromNodeName = CleanName(transition.fromNode.nodeName);
            string toNodeName = CleanName(transition.toNode.nodeName);

            string typeName = "";

            if (transition.rootPerception.type == perceptionType.Custom)
            {
                typeName = transition.rootPerception.customName;
            }
            else
            {
                typeName = transition.rootPerception.type.ToString();
            }

            if (((StateNode)transition.fromNode).subElem != null)
            {
                string ending;

                if (((StateNode)transition.fromNode).subElem is FSM)
                    ending = "_SubFSM";
                else
                    ending = "_SubBT";

                string subClassName = CleanName(((StateNode)transition.fromNode).subElem.elementName);

                result += "\n" + tab + tab + subClassName + ending + ".CreateExitTransition(\"" + transition.transitionName + "\", " + fromNodeName + ", " + uniqueNamer.GetName(transition.rootPerception.identificator) + ", " + toNodeName + ");";
            }
            else
            {
                result += "\n" + tab + tab + machineName + ".CreateTransition(\"" + transition.transitionName + "\", " + fromNodeName + ", " + uniqueNamer.GetName(transition.rootPerception.identificator) + ", " + toNodeName + ");";
            }
        }

        if (elem.parent is BehaviourTree)
            result += "\n" + tab + tab + machineName + ".CreateExitTransition(\"" + machineName + " Exit" + "\", null /*Change this for a node*/, null /*Change this for a perception*/, ReturnValues.Succeed);";

        return result;
    }

    #endregion

    #region Behaviour Tree
    /// <summary>
    /// Writes all the <see cref="TreeNode"/> declaration and initialization, except for Decorators
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="engineEnding"></param>
    /// <param name="subElems"></param>
    /// <returns></returns>
    private static string GetNodes(ClickableElement elem, string engineEnding, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.FindAll(n => n.type <= behaviourType.Leaf))
        {
            string nodeName = CleanName(node.nodeName);

            switch (node.type)
            {
                case behaviourType.Selector:
                    result += "SelectorNode " + nodeName + " = " + machineName + ".CreateSelectorNode(\"" + node.nodeName + "\");\n" + tab + tab;
                    break;
                case behaviourType.Sequence:
                    result += "SequenceNode " + nodeName + " = " + machineName + ".CreateSequenceNode(\"" + node.nodeName + "\", " + (node.isRandom ? "true" : "false") + ");\n" + tab + tab;
                    break;
                case behaviourType.Leaf:
                    if (node.subElem is FSM)
                    {
                        result += "LeafNode " + nodeName + " = " + machineName + ".CreateSubBehaviour(\"" + node.nodeName + "\", " + nodeName + subFSMEnding + ");\n" + tab + tab;
                        subElems.Add(node.subElem);
                    }
                    else if (node.subElem is BehaviourTree)
                    {
                        result += "LeafNode " + nodeName + " = " + machineName + ".CreateSubBehaviour(\"" + node.nodeName + "\", " + nodeName + subBTEnding + ");\n" + tab + tab;
                        subElems.Add(node.subElem);
                    }
                    else
                    {
                        result += "LeafNode " + nodeName + " = " + machineName + ".CreateLeafNode(\"" + node.nodeName + "\", " + nodeName + actionsEnding + ", " + nodeName + conditionsEnding + ");\n" + tab + tab;
                    }
                    break;
            }
        }

        // Decorator nodes
        // We check every node from the root so it is written in order in the generated code

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.FindAll(o => o.isRoot))
        {
            RecursiveDecorators(ref result, machineName, elem, node);
        }

        return result;
    }

    /// <summary>
    /// Checks if <paramref name="node"/> is a decorator and if it is, it writes its declaration and initialization
    /// </summary>
    /// <param name="result"></param>
    /// <param name="machineName"></param>
    /// <param name="elem"></param>
    /// <param name="node"></param>
    private static void RecursiveDecorators(ref string result, string machineName, ClickableElement elem, BehaviourNode node)
    {
        foreach (BehaviourNode childNode in ((BehaviourTree)elem).connections.FindAll(o => o.fromNode.Equals(node)).Select(o => o.toNode))
        {
            RecursiveDecorators(ref result, machineName, elem, childNode);
        }

        if (node.type > behaviourType.Leaf)
        {
            string nodeName = CleanName(node.nodeName);
            TransitionGUI decoratorConnection = ((BehaviourTree)elem).connections.Where(t => node.Equals(t.fromNode)).FirstOrDefault();
            string subNodeName = CleanName(decoratorConnection.toNode.nodeName);
            TransitionGUI decoratorConnectionsub;

            switch (((BehaviourNode)decoratorConnection.toNode).type)
            {
                case behaviourType.LoopN:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "LoopN_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case behaviourType.LoopUntilFail:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "LoopUntilFail_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case behaviourType.Inverter:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Inverter_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case behaviourType.DelayT:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Timer_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case behaviourType.Succeeder:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Succeeder_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
                case behaviourType.Conditional:
                    decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                    subNodeName = "Conditional_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                    break;
            }

            switch (node.type)
            {
                case behaviourType.LoopN:
                    string loopNodeName = "LoopN_" + subNodeName;
                    result += "LoopDecoratorNode " + loopNodeName + " = " + machineName + ".CreateLoopNode(\"" + loopNodeName + "\", " + subNodeName + ", " + node.NProperty + ");\n" + tab + tab;
                    break;
                case behaviourType.LoopUntilFail:
                    string loopUntilNodeName = "LoopUntilFail_" + subNodeName;
                    result += "LoopUntilFailDecoratorNode " + loopUntilNodeName + " = " + machineName + ".CreateLoopUntilFailNode(\"" + loopUntilNodeName + "\", " + subNodeName + ");\n" + tab + tab;
                    break;
                case behaviourType.Inverter:
                    string InverterNodeName = "Inverter_" + subNodeName;
                    result += "InverterDecoratorNode " + InverterNodeName + " = " + machineName + ".CreateInverterNode(\"" + InverterNodeName + "\", " + subNodeName + ");\n" + tab + tab;
                    break;
                case behaviourType.DelayT:
                    string TimerNodeName = "Timer_" + subNodeName;
                    result += "TimerDecoratorNode " + TimerNodeName + " = " + machineName + ".CreateTimerNode(\"" + TimerNodeName + "\", " + subNodeName + ", " + node.NProperty + ");\n" + tab + tab;
                    break;
                case behaviourType.Succeeder:
                    string SucceederNodeName = "Succeeder_" + subNodeName;
                    result += "SucceederDecoratorNode " + SucceederNodeName + " = " + machineName + ".CreateSucceederNode(\"" + SucceederNodeName + "\", " + subNodeName + ");\n" + tab + tab;
                    break;
                case behaviourType.Conditional:
                    string ConditionalNodeName = "Conditional_" + subNodeName;
                    result += "ConditionalDecoratorNode " + ConditionalNodeName + " = " + machineName + ".CreateConditionalNode(\"" + ConditionalNodeName + "\", " + subNodeName + ", null /*Change this for a perception*/);\n" + tab + tab;
                    break;
            }
        }
    }

    /// <summary>
    /// Writes all the AddChild methods that are necessary
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="subElems"></param>
    /// <returns></returns>
    private static string GetChilds(ClickableElement elem, ref List<ClickableElement> subElems)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.Where(n => n.type < behaviourType.Leaf && ((BehaviourTree)elem).ChildrenCount(n) > 0))
        {
            string nodeName = CleanName(node.nodeName);
            result += "\n" + tab + tab;
            foreach (BehaviourNode toNode in ((BehaviourTree)elem).connections.FindAll(t => node.Equals(t.fromNode)).Select(o => o.toNode))
            {
                string toNodeName = CleanName(toNode.nodeName);
                TransitionGUI decoratorConnection = ((BehaviourTree)elem).connections.Where(t => toNode.Equals(t.fromNode)).FirstOrDefault();
                if (decoratorConnection != null)
                {
                    string subNodeName = CleanName(decoratorConnection.toNode.nodeName);
                    TransitionGUI decoratorConnectionsub;

                    switch (((BehaviourNode)decoratorConnection.toNode).type)
                    {
                        case behaviourType.LoopN:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "LoopN_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case behaviourType.LoopUntilFail:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "LoopUntilFail_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case behaviourType.Inverter:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Inverter_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case behaviourType.DelayT:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Timer_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case behaviourType.Succeeder:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Succeeder_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                        case behaviourType.Conditional:
                            decoratorConnectionsub = ((BehaviourTree)elem).connections.Where(t => decoratorConnection.toNode.Equals(t.fromNode)).FirstOrDefault();
                            subNodeName = "Conditional_" + CleanName(decoratorConnectionsub.toNode.nodeName);
                            break;
                    }

                    switch (toNode.type)
                    {
                        case behaviourType.LoopN:
                            toNodeName = "LoopN_" + subNodeName;
                            break;
                        case behaviourType.LoopUntilFail:
                            toNodeName = "LoopUntilFail_" + subNodeName;
                            break;
                        case behaviourType.Inverter:
                            toNodeName = "Inverter_" + subNodeName;
                            break;
                        case behaviourType.DelayT:
                            toNodeName = "Timer_" + subNodeName;
                            break;
                        case behaviourType.Succeeder:
                            toNodeName = "Succeeder_" + subNodeName;
                            break;
                        case behaviourType.Conditional:
                            toNodeName = "Conditional_" + subNodeName;
                            break;
                    }
                }

                result += nodeName + ".AddChild(" + toNodeName + ");\n" + tab + tab;
            }
        }

        if (string.IsNullOrEmpty(result))
            return result;

        return "// Child adding" + result;
    }

    /// <summary>
    /// Writes all the SetRootNode methods that are necessary
    /// </summary>
    /// <param name="elem"></param>
    /// <param name="engineEnding"></param>
    /// <returns></returns>
    private static string GetSetRoot(ClickableElement elem, string engineEnding)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;
        string machineName = className + engineEnding;

        foreach (BehaviourNode node in ((BehaviourTree)elem).nodes)
        {
            if (node.isRoot)
                result += machineName + ".SetRootNode(" + CleanName(node.nodeName) + ");";
        }

        if (elem.parent is BehaviourTree)
        {
            result += "\n" + tab + tab +
                      "\n" + tab + tab + "// Exit Transition" +
                      "\n" + tab + tab + machineName + ".CreateExitTransition(\"" + machineName + " Exit" + "\");";
        }

        return result;
    }

    #endregion

    /// <summary>
    /// Writes all the methods for the Actions of the <see cref="State"/>
    /// </summary>
    /// <param name="elem"></param>
    /// <returns></returns>
    private static string GetActionMethods(ClickableElement elem)
    {
        string className = CleanName(elem.elementName);
        string result = string.Empty;

        switch (elem.GetType().ToString())
        {
            case nameof(FSM):
                foreach (StateNode node in ((FSM)elem).states)
                {
                    if (node.subElem != null)
                    {
                        result += GetActionMethods(node.subElem);
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
                foreach (BehaviourNode node in ((BehaviourTree)elem).nodes.FindAll(n => n.type == behaviourType.Leaf))
                {
                    if (node.subElem != null)
                    {
                        result += GetActionMethods(node.subElem);
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
