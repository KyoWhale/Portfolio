using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

// [CustomPropertyDrawer(typeof(DialogNodeData))]
// public class DialogNodeDataDrawer : PropertyDrawer
// {
//     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//     {
//         EditorGUI.BeginProperty(position, label, property);

//         Rect parentIndexRect = new Rect(position.x, position.y, position.width, position.height);
//         Rect childIndicesRect = new Rect(position.x, position.y, position.width, position.height);
//         EditorGUI.PropertyField(parentIndexRect, property.FindPropertyRelative("parentIndex"));
//         EditorGUI.PropertyField(childIndicesRect, property.FindPropertyRelative("childIndices"));

//         EditorGUI.EndProperty();
//     }
// }

[CustomEditor(typeof(DialogData))]
public class DialogDataEditor : Editor
{
    public static readonly string baseFileSavePath = "Assets/00Data/Dialog";

    [MenuItem("Assets/Create/Dialog/DialogGraph", priority = int.MinValue+2)]
    private static void Init()
    {
        DialogData data = ScriptableObject.CreateInstance<DialogData>();
        CreateDialog(data);
        FocusDialog(data);
        ShowGraphWindow(data);
    }
    
    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        if (GUILayout.Button("Show GraphView", GUILayout.Height(40)))
        {
            DialogData dialogData = Selection.activeObject as DialogData;
            ShowGraphWindow(dialogData);
        }

        GUILayout.Space(10);
        base.OnInspectorGUI();
    }

    private static void CreateDialog(DialogData newDialog)
    {
        int numbering = 0;
        string postFileSavePath = ".asset";

        string fileSavePath;
        Object existing;
        do
        {
            fileSavePath = baseFileSavePath + "/DialogGraph " + (++numbering).ToString() + postFileSavePath;
            existing = AssetDatabase.LoadAssetAtPath<DialogData>(fileSavePath);
        } 
        while (existing != null);

        AssetDatabase.CreateAsset(newDialog, fileSavePath);
        AssetDatabase.CreateFolder(baseFileSavePath, "DialogGraph " + numbering);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void FocusDialog(DialogData newDialog)
    {
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newDialog;
    }

    private static void ShowGraphWindow(DialogData newDialog)
    {
        DialogGraphViewWindow.ShowGraph(newDialog);
    }
}
