using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

public class PopupWindow : EditorWindow
{
    enum typeOfPopup
    {
        Delete,
        FailedExport
    }

    /// <summary>
    /// Type of popup
    /// </summary>
    static typeOfPopup PopupType;

    /// <summary>
    /// Type of the <see cref="ClickableElement"/> that will be deleted
    /// </summary>
    static string typeOfElem;

    /// <summary>
    /// The <see cref="NodeEditor"/> window that is calling for this <see cref="PopupWindow"/> to be shown
    /// </summary>
    static NodeEditor senderEditor;

    /// <summary>
    /// The List of <see cref="GUIElement"/> that will be deleted
    /// </summary>
    static List<GUIElement> elems;

    /// <summary>
    /// Width of the <see cref="PopupWindow"/>
    /// </summary>
    static int width = 300;

    /// <summary>
    /// Height of the <see cref="PopupWindow"/>
    /// </summary>
    static int height = 150;

    /// <summary>
    /// Initializer for the <see cref="PopupWindow"/> when deleting a <see cref="GUIElement"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="focusedElems"></param>
    /// <param name="type"></param>
    public static void InitDelete(NodeEditor sender, params GUIElement[] focusedElems)
    {
        senderEditor = sender;

        PopupType = typeOfPopup.Delete;

        elems = new List<GUIElement>(focusedElems);
        typeOfElem = elems[0].GetTypeString();

        PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
        window.position = new Rect(sender.position.center.x - width / 2, sender.position.center.y - height / 2, width, height);
        window.ShowPopup();
    }

    /// <summary>
    /// Initializer for the <see cref="PopupWindow"/> when failed at exporting a <see cref="ClickableElement"/>
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="focusElem"></param>
    /// <param name="type"></param>
    public static void InitExport(NodeEditor sender)
    {
        senderEditor = sender;

        PopupType = typeOfPopup.FailedExport;

        PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
        window.position = new Rect(sender.position.center.x - width / 2, sender.position.center.y - height / 2, width, height);
        window.ShowPopup();
    }

    /// <summary>
    /// Called once every frame
    /// </summary>
    void OnGUI()
    {
        switch (PopupType)
        {
            case typeOfPopup.Delete:
                ShowDeletePopup();
                break;
            case typeOfPopup.FailedExport:
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
                    foreach (GUIElement elem in elems)
                        senderEditor.Delete(elem);
                    this.Close();
                    break;
            }
        }
    }

    /// <summary>
    /// Shows the <see cref="PopupWindow"/> asking if you're sure you wanna delete the <see cref="elems"/>
    /// </summary>
    private void ShowDeletePopup()
    {
        string text = elems.Count == 1 ? "Do you want to delete this " + typeOfElem + "?" : "Do you want to delete these elements?";

        EditorGUILayout.LabelField(text, EditorStyles.boldLabel, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));
        if (senderEditor.currentElem is BehaviourTree)
        {
            int numberOfSons = 0;

            foreach(GUIElement elem in elems.Where(e => e is BehaviourNode))
            {
                numberOfSons += ((BehaviourTree)senderEditor.currentElem).ChildrenCount((BehaviourNode)elem);
            }

            if (numberOfSons > 0)
            {
                EditorGUILayout.LabelField(numberOfSons + " child nodes will also be deleted", Styles.WarningLabel, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));
                GUILayout.Space(20);
            }
        }
        else
        {
            GUILayout.Space(30);
        }

        if (GUILayout.Button("Delete", Styles.DeleteStyle))
        {
            foreach (GUIElement elem in elems)
                senderEditor.Delete(elem);
            this.Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            this.Close();
        }
    }

    /// <summary>
    /// Shows the <see cref="PopupWindow"/> telling you that you should fix all errors before exporting the <see cref="ClickableElement"/>
    /// </summary>
    private void ShowExportPopup()
    {
        EditorGUILayout.LabelField("Fix all the errors", EditorStyles.boldLabel, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));

        GUILayout.Space(30);

        if (GUILayout.Button("Ok"))
        {
            this.Close();
        }
    }

    private void OnLostFocus()
    {
        this.Close();
    }
}