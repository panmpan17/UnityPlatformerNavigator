using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace MPJamPack {
    [CustomEditor(typeof(PositionTween))]
    public class PositionEditor : Editor
    {
        PositionTween tween;

        SerializedProperty sameInterval, uselocalPosition;

        ReorderableList keyPoints;

        bool editKeyPoint;

        private void OnEnable() {
            tween = (PositionTween) target;

            sameInterval = serializedObject.FindProperty("sameInterval");
            uselocalPosition = serializedObject.FindProperty("uselocalPosition");

            keyPoints = new ReorderableList(serializedObject, serializedObject.FindProperty("keyPoints"),
                true, true, true, true);
            keyPoints.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Key Points");
            keyPoints.elementHeightCallback = (index) => sameInterval.boolValue? 20: 40;
            keyPoints.drawElementCallback = (rect, index, _a, _b) => {
                rect.y += 1;
                rect.height = 18;
                
                SerializedProperty property = keyPoints.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("Position"), GUIContent.none);

                if (!sameInterval.boolValue) {
                    rect.y += 20;
                    rect.height = 18;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("Interval"));
                }
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
            EditorGUILayout.PropertyField(uselocalPosition);
            EditorGUILayout.PropertyField(sameInterval);

            if (sameInterval.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("interval"));

            GUILayout.Space(5);

            EditorGUI.BeginChangeCheck();
            editKeyPoint = EditorUtilities.ToggleButton(editKeyPoint, new GUIContent("Edit Keypoint"));
            if (EditorGUI.EndChangeCheck()) {
                EditorWindow.GetWindow<SceneView>().Repaint();
            }

            GUILayout.Space(5);
            keyPoints.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI() {
            serializedObject.Update();

            if (!editKeyPoint) return;

            for (int i = 0; i < keyPoints.serializedProperty.arraySize; i++)
            {
                SerializedProperty property = keyPoints.serializedProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Position");
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.PositionHandle(property.vector3Value, Quaternion.identity);
                if (EditorGUI.EndChangeCheck()) {
                    if (uselocalPosition.boolValue && tween.transform.parent != null)
                        property.vector3Value = newPos - tween.transform.parent.position;
                    else property.vector3Value = newPos;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(ColorTween))]
    public class ColorEditor : Editor
    {
        ColorTween tween;

        SerializedProperty sameInterval;

        ReorderableList keyPoints;

        private void OnEnable() {
            tween = (ColorTween) target;

            sameInterval = serializedObject.FindProperty("sameInterval");

            keyPoints = new ReorderableList(serializedObject, serializedObject.FindProperty("keyPoints"),
                true, true, true, true);
            keyPoints.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Key Points");
            keyPoints.elementHeightCallback = (index) => sameInterval.boolValue ? 20 : 40;
            keyPoints.drawElementCallback = (rect, index, _a, _b) =>
            {
                rect.y += 1;
                rect.height = 18;

                SerializedProperty property = keyPoints.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("Color"), GUIContent.none);

                if (!sameInterval.boolValue)
                {
                    rect.y += 20;
                    rect.height = 18;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("Interval"));
                }
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("type"));
            // EditorGUILayout.PropertyField(uselocalPosition);
            EditorGUILayout.PropertyField(sameInterval);

            if (sameInterval.boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("interval"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteRenderers"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("graphics"));

            keyPoints.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}