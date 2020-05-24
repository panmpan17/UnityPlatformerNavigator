using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MPJamPack {
    [CreateAssetMenu(menuName="MPJamPack/AudioPreset")]
    public class AudioPreset : ScriptableObject {
        public EnumToAudio[] Audios;

        [System.Serializable]
        public struct EnumToAudio {
            public AudioIDEnum Type;
            public AudioClip Clip;
        }

    #if UNITY_EDITOR
        [CustomPropertyDrawer(typeof(EnumToAudio))]
        public class _PropertyDrawer : PropertyDrawer {
            private const float Height = 18;

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return Height;
            }

            public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
            {
                rect.width /= 2;
                rect.height = Height;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("Type"), GUIContent.none);
                rect.x += rect.width;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("Clip"), GUIContent.none);
            }
        }
    #endif
    }
}