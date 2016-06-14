using UnityEngine;
using System.Collections;
using UnityEditor;
using rr.level;

[CustomEditor(typeof(LevelManager))]
public class LevelManagerEditor : Editor {

    private SerializedProperty currentLevel;
    private SerializedProperty storageFolder;
    private SerializedProperty debugMode;

    void OnEnable() {
        currentLevel = serializedObject.FindProperty("currentLevel");
        storageFolder = serializedObject.FindProperty("storageFolder");
        debugMode = serializedObject.FindProperty("debugMode");
    }

    public override void OnInspectorGUI() {

        serializedObject.Update();
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(storageFolder, new GUIContent("Storage Folder"));
        if(GUILayout.Button(new GUIContent("Browse..."), GUILayout.Width(75))) {
            var newPath = EditorUtility.OpenFolderPanel("Choose storage folder...", storageFolder.stringValue, "");
            if(newPath != null && newPath.Length > 0)
                storageFolder.stringValue = newPath.Replace(Application.dataPath, "Assets");
            EditorUtility.SetDirty(this);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(currentLevel, new GUIContent("Current Level"));
        EditorGUILayout.PropertyField(debugMode, new GUIContent("Debug Mode"));
        serializedObject.ApplyModifiedProperties();



        var levelMgr = (LevelManager)target;

        if(GUILayout.Button("Save")) {
            levelMgr.SaveLevel();
        }

        if (GUILayout.Button("Load")) {
            levelMgr.LoadLevel();
        }
    }

}
