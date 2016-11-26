using UnityEngine;
using System.Collections;
using UnityEditor;

namespace DataUIEditor
{
    public class DataEditor : EditorWindow
    {
        private Object folder;
        private string file = "";

        [MenuItem("Window/Data Editor")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DataEditor));
        }

        void OnGUI()
        {
            if(folder != null)
            {

            }

            GUILayout.Label("Settings", EditorStyles.boldLabel);
            folder = EditorGUILayout.ObjectField("Data Folder :", folder, typeof(TextAsset), null);
            
        }
    }

}
