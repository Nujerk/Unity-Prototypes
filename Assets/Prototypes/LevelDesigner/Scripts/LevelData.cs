using UnityEngine;
using System.Collections;

namespace rr.level {

    [CreateAssetMenu(menuName="Level/New Level")]
    public class LevelData : ScriptableObject {
        [SerializeField]
        [HideInInspector]
        private string _serializedLevel;

        public string SerializedLevel {
            get {
                return _serializedLevel;
            }
            set {
                _serializedLevel = value;
            }
        }
    }
}


