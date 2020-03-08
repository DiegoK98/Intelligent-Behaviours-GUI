using UnityEngine;
using UnityEditor;
using System;

public class PopupWindow : EditorWindow
{
    enum typeOfPopup
    {
        Delete,
        NameRepeat
    }

    static typeOfPopup PopupType;

    static string typeOfElem;

    static GUIStyle DeleteStyle;
    static GUIStyle CancelStyle;

    static NodeEditor senderEditor;
    static int locIndex;
    static string repeatedName;

    static int width = 250;
    static int height = 100;

    public static void InitDelete(NodeEditor sender, int index, string type)
    {
        PopupType = typeOfPopup.Delete;

        senderEditor = sender;

        locIndex = index;
        typeOfElem = type;

        PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
        window.position = new Rect(sender.position.center.x - width / 2, sender.position.center.y - height / 2, width, height);
        window.ShowPopup();
    }

    public static void InitNameRepeated(NodeEditor sender, string name, Rect rect)
    {
        PopupType = typeOfPopup.NameRepeat;

        repeatedName = name;

        PopupWindow window = ScriptableObject.CreateInstance<PopupWindow>();
        window.position = new Rect(sender.position.x + rect.center.x - width / 2, sender.position.y + rect.center.y + height / 2, width, height);
        window.ShowPopup();
    }

    public static void ClosePopup(PopupWindow popup)
    {
        popup.Close();
    }

    void OnGUI()
    {
        if (PopupType == typeOfPopup.Delete)
        {
            ShowDeletePopup();
        }
        else if (PopupType == typeOfPopup.NameRepeat)
        {
            ShowNamePopup();
        }

        if (Event.current.isKey && Event.current.type == EventType.KeyUp)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    this.Close();
                    break;
            }
        }
    }

    private void ShowDeletePopup()
    {
        DeleteStyle = new GUIStyle(GUI.skin.button);
        DeleteStyle.normal.background = MakeBackground(Color.red);

        CancelStyle = new GUIStyle(GUI.skin.button);
        CancelStyle.normal.background = MakeBackground(Color.gray);

        EditorGUILayout.LabelField("Do you want to delete this " + typeOfElem + "?", EditorStyles.boldLabel, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));
        if (senderEditor.currentElem is BehaviourTree)
        {
            int numberOfSons = ((BehaviourTree)senderEditor.currentElem).SonsCount(locIndex);
            if (numberOfSons > 0)
            {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
                labelStyle.normal.textColor = Color.red;

                EditorGUILayout.LabelField(numberOfSons + " child nodes will be deleted as well", labelStyle, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));
            }
        }

        GUILayout.Space(30);

        if (GUILayout.Button("Delete", DeleteStyle))
        {
            senderEditor.Delete(typeOfElem, locIndex);
            this.Close();
        }
        if (GUILayout.Button("Cancel", CancelStyle))
        {
            this.Close();
        }
    }

    private void ShowNamePopup()
    {
        CancelStyle = new GUIStyle(GUI.skin.button);
        CancelStyle.normal.background = MakeBackground(Color.gray);

        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.normal.textColor = Color.red;

        EditorGUILayout.LabelField("This element's name " + repeatedName + " is repeated in another element", labelStyle, GUILayout.Width(this.position.width - 10), GUILayout.ExpandHeight(true));

        GUILayout.Space(30);

        if (GUILayout.Button("Cancel", CancelStyle))
        {
            this.Close();
        }
    }

    //Habrá que cambiarlo
    private Texture2D MakeBackground(Color col)
    {
        Color[] pix = new Color[2 * 2];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }

        Texture2D result = new Texture2D(2, 2);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }

    private void OnLostFocus()
    {
        this.Close();
    }
}