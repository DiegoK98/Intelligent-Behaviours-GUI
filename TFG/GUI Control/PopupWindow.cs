using UnityEngine;
using UnityEditor;
using System;

public class PopupWindow : EditorWindow
{
    enum typeOfPopup
    {
        Delete,
        Export
    }

    static typeOfPopup PopupType;

    static string typeOfElem;

    static NodeEditor senderEditor;

    static GUIElement elem;

    static string repeatedName;

    static int width = 300;

    static int height = 150;

    /// <summary>
    /// Initializer for the popup when deleting an element
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="focusElem"></param>
    /// <param name="type"></param>
    public static void InitDelete(NodeEditor sender, GUIElement focusElem, string type)
    {
        senderEditor = sender;

        PopupType = typeOfPopup.Delete;

        elem = focusElem;
        typeOfElem = type;

        PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
        window.position = new Rect(sender.position.center.x - width / 2, sender.position.center.y - height / 2, width, height);
        window.ShowPopup();
    }

    /// <summary>
    /// Initializer for the popup when exporting an element
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="focusElem"></param>
    /// <param name="type"></param>
    public static void InitExport(NodeEditor sender)
    {
        senderEditor = sender;

        PopupType = typeOfPopup.Export;

        PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
        window.position = new Rect(sender.position.center.x - width / 2, sender.position.center.y - height / 2, width, height);
        window.ShowPopup();
    }

    //public static void InitNameRepeated(NodeEditor sender, string name, Rect rect)
    //{
    //    senderEditor = sender;

    //    PopupType = typeOfPopup.NameRepeat;

    //    repeatedName = name;

    //    PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
    //    window.position = new Rect(sender.position.x + rect.center.x - width / 2, sender.position.y + rect.center.y + height / 2, width, height);
    //    window.ShowPopup();
    //}

    //public static void InitNoEntryState(NodeEditor sender)
    //{
    //    senderEditor = sender;

    //    PopupType = typeOfPopup.NoEntry;

    //    PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
    //    window.position = new Rect(sender.position.center.x - width / 2, sender.position.center.y - height / 2, width, height);
    //    window.ShowPopup();
    //}

    /// <summary>
    /// The OnGUI
    /// </summary>
    void OnGUI()
    {
        switch (PopupType)
        {
            case typeOfPopup.Delete:
                ShowDeletePopup();
                break;
            case typeOfPopup.Export:
                ShowExportPopup();
                break;
        }

        if (Event.current.isKey && Event.current.type == EventType.KeyUp)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    this.Close();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    senderEditor.Delete(elem);
                    this.Close();
                    break;
            }
        }
    }

    /// <summary>
    /// Shows the popup asking if you're sure you wanna delete an element
    /// </summary>
    private void ShowDeletePopup()
    {
        EditorGUILayout.LabelField("Do you want to delete this " + typeOfElem + "?", EditorStyles.boldLabel, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));
        if (senderEditor.currentElem is BehaviourTree)
        {
            int numberOfSons = ((BehaviourTree)senderEditor.currentElem).ChildrenCount(elem);
            if (numberOfSons > 0)
            {
                EditorGUILayout.LabelField(numberOfSons + " child nodes will be deleted as well", Styles.WarningLabel, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));
                GUILayout.Space(20);
            }
        }
        else
        {
            GUILayout.Space(30);
        }

        if (GUILayout.Button("Delete", Styles.DeleteStyle))
        {
            senderEditor.Delete(elem);
            this.Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
    }

    private void ShowExportPopup()
    {
        EditorGUILayout.LabelField("Fix all the errors", EditorStyles.boldLabel, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));

        GUILayout.Space(30);

        if (GUILayout.Button("Ok"))
        {
            this.Close();
        }
    }

    //private void ShowNamePopup()
    //{
    //    CancelStyle = new GUIStyle(GUI.skin.button);
    //    CancelStyle.normal.background = MakeBackground(Color.gray);

    //    GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
    //    labelStyle.normal.textColor = Color.red;

    //    EditorGUILayout.LabelField("This element's name " + repeatedName + " is repeated in another element", labelStyle, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));

    //    GUILayout.Space(30);

    //    if (GUILayout.Button("Cancel", CancelStyle))
    //    {
    //        this.Close();
    //    }
    //}

    /// <summary>
    /// The OnLostFocus
    /// </summary>
    private void OnLostFocus()
    {
        this.Close();
    }
}