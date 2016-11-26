using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngineInternal;

namespace rr.level {

    public enum GameObjectParsingState {
        Begin,
        ReadingName,
        NonRelevant,
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

    public struct GameObjectRef {
        public Component target;
        public string targetKey;
        public string refName;
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

        private List<GameObjectRef> gameObjRefs;

        public void LoadLevel() {
            gameObjRefs = new List<GameObjectRef>();
            var serializedLevel = currentLevel.SerializedLevel;
            var serialGameObjects = serializedLevel.Split(';');
            GameObject currentGO;
            foreach(var sgo in serialGameObjects) {
                currentGO = ParseGameObject(sgo);
            }

            foreach(var goRef in gameObjRefs) {
                currentGO = GameObject.Find(goRef.refName);
                                
                if(currentGO != null) {
                    var args = new object[] { currentGO };
                    goRef.target.GetType().InvokeMember(goRef.targetKey, System.Reflection.BindingFlags.SetField, null, goRef.target, args);
                }
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

            serialization += "go:" + go.name;

            if (go.transform.parent != null)
                serialization += ":parent:" + go.transform.parent.name;

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
            var path = AssetDatabase.GetAssetPath(prefab);
            serialization += "go:" + go.name;
            serialization += ":pf:";
            serialization += path;

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
                    serialization += property.propertyPath + "=" + GetPropertyStringValue(property);
                    propCount++;
                }

                property.Next(false);
            }
            while (property.propertyPath.Length > 0) ;

                serialization += "]";

            return serialization;
        }

        protected void SetComponentValue(Component comp, string key, string value)
        {
            if (key.StartsWith("m_")) {
                key = key.Substring(2);
                key = Char.ToLower(key[0]) + key.Substring(1);
            }

            var prop = comp.GetType().GetProperty(key);
            var type = prop.PropertyType;

            object result = null;
            GameObjectRef goRef;
            GameObject go;

            if(type == typeof(GameObject)) {
                go = GameObject.Find(value);
                if (go != null)
                    result = go;
                else {
                    goRef = new GameObjectRef();
                    goRef.target = comp;
                    goRef.targetKey = key;
                    goRef.refName = value;
                    gameObjRefs.Add(goRef);
                    result = null;
                }
            }
            else if (type == typeof(float)) {
                result = float.Parse(value);
            }
            else if (type == typeof(int)) {
                result = int.Parse(value);
            }
            else if (type == typeof(string)) {
                result = value;
            }
            else if (type == typeof(float)) {
                result = float.Parse(value);
            }
            else {
                result = ParseSpecificType(type, value);
            }

            if (result != null) {
                var args = new object[] { result };
                comp.GetType().InvokeMember(key, System.Reflection.BindingFlags.SetProperty, null, comp, args);
            }
        }

        protected object ParseSpecificType(Type type, string value) {
            var length = value.Length;
            char currentChar;
            string word = "";
            if (type == typeof(Vector2)) {
                Vector2 vector = Vector2.zero;
                for (int i = 0; i < length; i++) {
                    currentChar = value[i];
                    if (currentChar == ',') {
                        vector.x = float.Parse(word);
                        word = "";
                    }
                    else {
                        word += currentChar;
                    }
                }

                vector.y = float.Parse(word);

                return vector;
            }
            else if (type == typeof(Vector3)) {
                Vector3 vector = Vector3.zero;
                bool firstValue = true;
                for (int i = 0; i < length; i++) {
                    currentChar = value[i];
                    if (currentChar == ',') {
                        if (firstValue) {
                            vector.x = float.Parse(word);
                            firstValue = false;
                        }
                        else {
                            vector.y = float.Parse(word);
                        }
                        word = "";
                    }
                    else {
                        word += currentChar;
                    }
                }

                vector.z = float.Parse(word);
                return vector;
            }
            else if (type == typeof(Quaternion)) {
                Quaternion quaternion = Quaternion.identity;
                int valueCount = 0;
                for (int i = 0; i < length; i++) {
                    currentChar = value[i];
                    if (currentChar == ',') {
                        if (valueCount == 0) {
                            quaternion.x = float.Parse(word);
                            valueCount++;
                        }
                        else if (valueCount == 1) {
                            quaternion.y = float.Parse(word);
                            valueCount++;
                        }
                        else {
                            quaternion.z = float.Parse(word);
                        }
                        word = "";
                    }
                    else {
                        word += currentChar;
                    }
                }
                quaternion.w = float.Parse(word);

                return quaternion;
            }
            else
                return null;
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
            var tempGo = new GameObject();
            PropertyModification mod;
            Component comp = null;

            GameObjectParsingState state = GameObjectParsingState.Begin;
            string name = "", parent ="";
            string prefabPath = "";
            bool inParenthesis = false;

            Debug.Log("--- BEGIN PARSING --");

            while(index < charCount)
            {
                currentChar = serialized.ElementAt(index);
                if (inParenthesis && currentChar != ')') {
                    word += currentChar;
                    index++;
                    continue;
                }

                switch (currentChar)
                {
                    case ':':
                        switch(state) {
                            case GameObjectParsingState.Begin:
                                word = "";
                                state = GameObjectParsingState.ReadingName;
                                Debug.Log("read name");
                                break;
                            case GameObjectParsingState.ReadingName:
                                name = word;
                                Debug.Log("name = " + name);
                                state = GameObjectParsingState.NonRelevant;
                                word = "";
                                break;
                            case GameObjectParsingState.ReadingPrefab:
                                prefabPath = word;
                                Debug.Log("prefab = " + prefabPath);
                                state = GameObjectParsingState.NonRelevant;
                                word = "";
                                break;
                            case GameObjectParsingState.ReadingParent:
                                parent = word;
                                Debug.Log("parent = " + parent);
                                state = GameObjectParsingState.NonRelevant;
                                word = "";
                                break;
                            case GameObjectParsingState.InComponentList:
                                word = "";
                                Debug.Log("read component name");
                                state = GameObjectParsingState.ReadingComponentName;
                                break;
                            case GameObjectParsingState.ReadingComponentName:
                                Debug.Log("component is " + word);
                                if (word != "Transform") {
                                    comp = APIUpdaterRuntimeServices.AddComponent(tempGo, "LevelManager.cs (503, 37)", word);
                                }
                                else
                                    comp = tempGo.GetComponent<Transform>();

                                componentList.Add(comp);
                                word = "";
                                break;
                            default:
                                if(word == "pf") {
                                    Debug.Log("read prefab");
                                    state = GameObjectParsingState.ReadingPrefab;
                                }
                                else if(word == "parent") {
                                    Debug.Log("read parent");
                                    state = GameObjectParsingState.ReadingParent;
                                }

                                word = "";
                            break;
                        }
                        break;
                    case '[':
                        if(word == "comps") {
                            Debug.Log("Enter component list");
                            state = GameObjectParsingState.InComponentList;
                        }
                        else if (word == "props") {
                            Debug.Log("Enter properties list");
                            state = GameObjectParsingState.InPropertyList;
                        }
                        else if (word == "mods") {
                            Debug.Log("Enter modifcations list");
                            state = GameObjectParsingState.InModificationList;
                        }
                        word = "";
                        break;
                    case ']':
                        if (state == GameObjectParsingState.ReadingComponent || state == GameObjectParsingState.ReadingModification) {
                            Debug.Log("Exiting comp / mod list");
                            state = GameObjectParsingState.End;
                        }
                        else if (state == GameObjectParsingState.ReadingProperty) {
                            Debug.Log("Exiting properties list");
                            state = GameObjectParsingState.ReadingComponent;
                        }
                        readingValue = false;
                        break;
                    case ',':
                        if (state == GameObjectParsingState.ReadingComponent) {
                            Debug.Log("Component is read");
                            state = GameObjectParsingState.InComponentList;
                        }
                        else if (state == GameObjectParsingState.ReadingModification)
                        {
                            Debug.Log("Modification is read");
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
                            Debug.Log("Property is read");
                            state = GameObjectParsingState.InPropertyList;
                            if (readingValue)
                            {
                                SetComponentValue(comp, key, word);
                                key = "";
                                word = "";
                            }
                        }

                        readingValue = false;
                        break;
                    case '=':
                        key = word;
                        if(state == GameObjectParsingState.InPropertyList) {
                            Debug.Log("Start reading property");
                            state = GameObjectParsingState.ReadingProperty;
                        }
                        else if (state == GameObjectParsingState.InModificationList) {
                            Debug.Log("Start reading modification");
                            state = GameObjectParsingState.ReadingModification;
                        }
                            
                        word = "";
                        readingValue = true;
                        break;
                    case '(':
                        Debug.Log("ENTER parenthesis");
                        inParenthesis = true;
                        break;
                    case ')':
                        Debug.Log("EXIT parenthesis");
                        inParenthesis = false;
                        break;
                    default:
                        word += currentChar;
                        break;
                }

                index++;
            }
            
            if(prefabPath != "")
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                go = GameObject.Instantiate(prefab);

                if (parent != "")
                    go.transform.parent = GameObject.Find(parent).transform;

                go.name = name;
                PrefabUtility.ConnectGameObjectToPrefab(go, prefab);
                PrefabUtility.SetPropertyModifications(go, mods.ToArray());
            }
            else
            {
                go = new GameObject();
                go.name = name;

                if (parent != "")
                    go.transform.parent = GameObject.Find(parent).transform;

                Component clone;
                foreach(var c in componentList)
                {

                    if (c.GetType() == typeof(Transform))
                        clone = go.transform;
                    else
                        clone = go.AddComponent(c.GetType());

                    EditorUtility.CopySerialized(c, clone);
                }
            }

            GameObject.DestroyImmediate(tempGo);

            Debug.Log("--- END PARSING --");
            return go;
        }
    }
}
