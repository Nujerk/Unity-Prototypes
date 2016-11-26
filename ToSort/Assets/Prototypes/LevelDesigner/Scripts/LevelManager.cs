using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace rr.level {

    public enum GameObjectParsingState
    {
        Begin,
        ReadingName,
        ReadingPrefab,
        ReadingParent,
        InComponentList,
        InPropertyList,
        InModificationList,
        ReadingComponent,
        ReadingProperty,
        ReadingModification,
        ReadingComponentName,
        End
    }

    public class LevelManager : MonoBehaviour {

        private static Quaternion s_DefaultQuaternion = new Quaternion(0, 0, 0, 1);
        private static Quaternion s_DefaultRotation = new Quaternion(0, 0, 0, 1);
        private static Vector3 s_DefaultVector3 = Vector3.zero;
        private static Vector3 s_DefaultLocalPosition = Vector3.zero;
        private static Vector3 s_DefaultLocalScale = Vector3.one;
        private static Vector2 s_DefaultVector2 = Vector2.zero;
        private static Vector4 s_DefaultVector4 = Vector4.zero;

        public string storageFolder = "Assets/Data/Levels/";
        public LevelData currentLevel;
        public bool debugMode = false;

        private static List<string> s_UselessProperties = new List<string> {
            "m_GameObject",
            "m_ObjectHideFlags",
            "m_PrefabParentObject",
            "m_PrefabInternal",
            "m_Children",
            "m_Father",
            "",
        };

        public void LoadLevel() {
            var serializedLevel = currentLevel.SerializedLevel;
            var serialGameObjects = serializedLevel.Split(';');
            GameObject currentGO;
            foreach(var sgo in serialGameObjects) {
                currentGO = ParseGameObject(sgo);
            }
        }

        public void SaveLevel() {
            var serializedLevel = "";
            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var sort = from go in gameObjects
                       orderby GetHierarchyDepth(go), go.transform.GetSiblingIndex()
                       select go;

            foreach (GameObject go in sort) {
                if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
                    continue;

                if (EditorUtility.IsPersistent(go.transform.root.gameObject))
                    continue;

                if (go.GetComponent<LevelManager>() != null)
                    continue;

                var prefabType = PrefabUtility.GetPrefabType(go);
                if(prefabType == PrefabType.PrefabInstance) {
                    serializedLevel += SerializePrefabInstance(go);
                }
                else if(prefabType == PrefabType.None) {
                    serializedLevel += SerializeGameObject(go);
                }
                else {
                    Debug.Log("ERROR : Not Handled Prefab Type - " + prefabType.ToString());
                }
            }

            if (debugMode)
                Debug.Log(serializedLevel);

            serializedLevel = serializedLevel.Replace("\n", "");

            if (currentLevel != null && AssetDatabase.LoadAssetAtPath(storageFolder + "/" + currentLevel, typeof(LevelData)) != null) {
                currentLevel.SerializedLevel = serializedLevel;
            }
            else if(!AssetDatabase.IsValidFolder(storageFolder)) {
                Debug.LogError("The selected folder ( " + storageFolder + " ) doesn't exist. Please create it before to save the level.");
                return;
            }
            else {
                var levelData = ScriptableObject.CreateInstance<LevelData>();
                
                var path = AssetDatabase.GenerateUniqueAssetPath(storageFolder + "/LevelData.asset");
                levelData.SerializedLevel = serializedLevel;
                AssetDatabase.CreateAsset(levelData, path);
                currentLevel = levelData;
            }

            EditorUtility.SetDirty(currentLevel);
            AssetDatabase.SaveAssets();
        }

        protected string SerializeGameObject(GameObject go) {
            var serialization = "";

            serialization += "go:\"" + go.name + "\"";

            if (go.transform.parent != null)
                serialization += ":parent:\"" + go.transform.parent.name + "\"";

            var components = go.GetComponents<Component>();
            Component comp;
            serialization += ":comps[";

            for(var i = 0; i < components.Length; i ++) {
                comp = components[i];
                
                if(i > 0)
                serialization += ",";
                serialization += SerializeComponent(comp);
            }
            serialization += "];\n";

            return serialization;
        }

        protected string SerializePrefabInstance(GameObject go) {
            var serialization = "";

            var prefab = PrefabUtility.GetPrefabParent(go) as GameObject;
            var id = prefab.GetInstanceID();
            serialization += "go:" + go.name;
            serialization += ":pf:";
            serialization += id;

            if(go.transform.parent != null)
                serialization += ":parent:" + go.transform.parent.name;

            var mods = PrefabUtility.GetPropertyModifications(go);
            if(mods.Length > 0) {
                serialization += ":mods[";
                PropertyModification mod;
                SerializedObject serializedObj;
                var serializedCount = 0;
                for (var i = 0; i < mods.Length; i++) {
                    mod = mods[i];
                    if (mod.propertyPath == "m_Name")
                        continue;

                    var type = mod.target.GetType();
                    if(type != typeof(GameObject)) {
                        serializedObj = new SerializedObject(prefab.GetComponent(type.Name));
                    }
                    else {
                        serializedObj = new SerializedObject(prefab);
                    }
                    
                    var prop = serializedObj.FindProperty(mod.propertyPath);

                    var value = GetPropertyStringValue(prop);
                    if (value == mod.value)
                        continue;

                    if (serializedCount > 0)
                        serialization += ",";

                    serialization += mod.propertyPath + "=" + mod.value;
                    serializedCount++;
                }
                serialization += "]";
            }

            serialization += ";\n";

            return serialization;
        }

        protected string GetPropertyStringValue(SerializedProperty prop) {
            if(!prop.isArray) {
                switch (prop.propertyType) {
                    case SerializedPropertyType.Float:
                        return prop.floatValue.ToString();
                    case SerializedPropertyType.Integer:
                        return prop.intValue.ToString();
                    case SerializedPropertyType.Boolean:
                        return prop.boolValue.ToString();
                    case SerializedPropertyType.String:
                        return prop.stringValue;
                    case SerializedPropertyType.Vector2:
                        return "(" + prop.vector2Value.x + "," + prop.vector2Value.y + ")";
                    case SerializedPropertyType.Vector3:
                        return "(" + prop.vector3Value.x + "," + prop.vector3Value.y + "," + prop.vector3Value.z + ")";
                    case SerializedPropertyType.Vector4:
                        return "(" + prop.vector4Value.x + "," + prop.vector4Value.y + "," + prop.vector4Value.z + "," + prop.vector4Value.w + ")";
                    case SerializedPropertyType.Quaternion:
                        return "(" + prop.quaternionValue.x + "," + prop.quaternionValue.y + "," + prop.quaternionValue.z + "," + prop.quaternionValue.w + ")";
                    case SerializedPropertyType.ObjectReference:
                        return prop.objectReferenceValue != null ? prop.objectReferenceValue.name : "null";
                    default:
                        return "N/A";
                }
            }
            else {
                var arrayLength = prop.arraySize;
                var serializedArray = "[";
                for(var i = 0; i < arrayLength; i++) {
                    if (i > 0)
                        serializedArray += ",";

                    serializedArray += GetPropertyStringValue(prop.GetArrayElementAtIndex(i));
                }
                serializedArray += "]";

                return serializedArray;
            }
        }

        protected int GetHierarchyDepth(GameObject go, int depth = 0) {
            var parent = go.transform.parent;
            if (parent != null) {
                return GetHierarchyDepth(parent.gameObject, ++depth);
            }
            else
                return depth;
        }

        protected bool IsUselessProperty(string name) {
            return s_UselessProperties.Contains(name);
        }

        protected bool IsDefaultValue(SerializedProperty prop) {
            if(prop.isArray) {
                return prop.arraySize == 0;
            }
            else {
                switch (prop.name) {
                    case "m_LocalRotation":
                        return prop.quaternionValue == s_DefaultRotation;
                    case "m_LocalPosition":
                        return prop.vector3Value == s_DefaultLocalPosition;
                    case "m_LocalScale":
                        return prop.vector3Value == s_DefaultLocalScale;
                    default:
                        switch (prop.propertyType) {
                            case SerializedPropertyType.Float:
                                return prop.floatValue == 0f;
                            case SerializedPropertyType.Integer:
                                return prop.intValue == 0;
                            case SerializedPropertyType.Boolean:
                                return prop.boolValue == false;
                            case SerializedPropertyType.String:
                                return prop.stringValue == "";
                            case SerializedPropertyType.Vector2:
                                return prop.vector2Value == s_DefaultVector2;
                            case SerializedPropertyType.Vector3:
                                return prop.vector3Value == s_DefaultVector3;
                            case SerializedPropertyType.Vector4:
                                return prop.vector4Value == s_DefaultVector4;
                            case SerializedPropertyType.Quaternion:
                                return prop.quaternionValue == new Quaternion(0, 0, 0, 1);
                            case SerializedPropertyType.ObjectReference:
                                return prop.objectReferenceValue == null ? true : false;
                            default:
                                return false;
                        }
                }
            }
        }

        protected string SerializeComponent(Component comp) {
            var serialization = "";

            serialization += "comp:" + comp.GetType().Name;
            var sObj = new SerializedObject(comp);
            var property = sObj.GetIterator();
            property.Next(true);
            serialization += ":props[";
            var propCount = 0;
            do {
                if (!IsUselessProperty(property.propertyPath) && !IsDefaultValue(property)) {
                    if (propCount > 0)
                        serialization += ",";
                    serialization += property.propertyPath + ":" + GetPropertyStringValue(property);
                    propCount++;
                }

                property.Next(false);
            }
            while (property.propertyPath.Length > 0) ;

                serialization += "]";

            return serialization;
        }

        protected T GetValueFromString<T>(string value)
        {
            T result = default(T);

            return result;
        }

        protected GameObject ParseGameObject(string serialized) {
            GameObject go = null;

            var charCount = serialized.Length;
            var index = 0;
            string word = "";
            char currentChar;
            var componentList = new List<Component>();
            var mods = new List<PropertyModification>();
            bool readingValue = false;
            string key = "";

            PropertyModification mod;
            Component comp = null;

            GameObjectParsingState state = GameObjectParsingState.Begin;
            string name = "", parent ="";
            int prefabID = 0;

            while(index < charCount)
            {
                currentChar = serialized.ElementAt(index);
                switch (currentChar)
                {
                    case ':':
                        switch(state) {
                            case GameObjectParsingState.Begin:
                                word = "";
                                state = GameObjectParsingState.ReadingName;
                                break;
                            case GameObjectParsingState.ReadingName:
                                name = word;
                                word = "";
                                break;
                            case GameObjectParsingState.ReadingPrefab:
                                prefabID = int.Parse(word);
                                word = "";
                                break;
                            case GameObjectParsingState.ReadingParent:
                                parent = word;
                                word = "";
                                break;
                            case GameObjectParsingState.InComponentList:
                                word = "";
                                state = GameObjectParsingState.ReadingComponentName;
                                break;

                            default:
                                if(word == "pf")
                                    state = GameObjectParsingState.ReadingPrefab;
                                else if(word == "parent")
                                    state = GameObjectParsingState.ReadingParent;
                            break;
                        }
                        break;
                    case '[':
                        if(word == "comps") 
                            state = GameObjectParsingState.InComponentList;
                        else if (word == "props")
                            state = GameObjectParsingState.InPropertyList;
                        else if (word == "mods")
                            state = GameObjectParsingState.InModificationList;
                        break;
                    case ']':
                        if (state == GameObjectParsingState.ReadingComponent || state == GameObjectParsingState.ReadingModification)
                            state = GameObjectParsingState.End;
                        else if (state == GameObjectParsingState.ReadingProperty)
                            state = GameObjectParsingState.ReadingComponent;

                        readingValue = false;
                        break;
                    case ',':
                        if (state == GameObjectParsingState.ReadingComponent)
                            state = GameObjectParsingState.InComponentList;
                        else if (state == GameObjectParsingState.ReadingModification)
                        {
                            state = GameObjectParsingState.InModificationList;
                            if(readingValue)
                            {
                                mod = new PropertyModification();
                                mod.propertyPath = key;
                                mod.value = word;
                                mods.Add(mod);

                                key = "";
                                word = "";
                            }
                        }
                        else if (state == GameObjectParsingState.ReadingProperty)
                        {
                            state = GameObjectParsingState.InPropertyList;
                            if (readingValue)
                            {
                                
                                var args = new object[] { word };
                                var member = comp.GetType().GetMember(key);
                                var value = GetValueFromString<>(word);
                                comp.GetType().InvokeMember(key, System.Reflection.BindingFlags.SetField, null, comp, args);
                            }
                        }

                        readingValue = false;
                        break;
                    case '=':
                        readingValue = true;
                        break;
                    default:
                        word += currentChar;
                        break;
                }

                index++;
            }
            
            if(prefabID != 0)
            {
                var prefab = EditorUtility.InstanceIDToObject(prefabID) as GameObject;
                go = GameObject.Instantiate(prefab);

                go.name = name;
                PrefabUtility.SetPropertyModifications(go, mods.ToArray());
            }
            else
            {
                go = new GameObject();
                if (parent != "")
                    go.transform.parent = GameObject.Find(parent).transform;

                foreach(var comp in componentList)
                {
                    var clone = go.AddComponent(comp.GetType());
                    EditorUtility.CopySerialized(comp, clone);
                }
            }

            return go;
        }
    }
}
