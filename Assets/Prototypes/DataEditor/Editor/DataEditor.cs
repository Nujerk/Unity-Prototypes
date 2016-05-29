using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class DataEditor : EditorWindow {

    [MenuItem("Window/Data Editor")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(DataEditor));
        data = Resources.FindObjectsOfTypeAll<GameDataObject>();
        types = new List<string>();
        foreach (var obj in data)
        {
            if (!types.Contains(obj.GetType().Name))
                types.Add(obj.GetType().Name);
        }
    }

    private static Object[] data;
    private static List<string> types;

    void OnGUI()
    {
        if (data == null)
            return;

        // The actual window code goes here
        Rect r = EditorGUILayout.BeginHorizontal();

        Rect v = EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Types");

        var scrollStyle = new GUIStyle();
        EditorGUILayout.BeginScrollView(Vector2.zero, false, true);
        foreach(var type in types)
        {
            EditorGUILayout.LabelField(type);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        v = EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Instances");
        EditorGUILayout.EndVertical();

        v = EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("Fields");
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

    }
}
