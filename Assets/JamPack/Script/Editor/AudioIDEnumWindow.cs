using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEditor;

namespace MPJamPack {
    public class AudioIDEnumWindow : EditorWindow {
        private const string TypeCSFilePath = "JamPack/Script/Audio/AudioIDEnum.cs";

        private const string FileHead = "namespace MPJamPack {\n\tpublic enum AudioIDEnum {\n";
        private const string FileFoot = "\t}\n}";

        [MenuItem("Window/Audio ID Enum")]
        private static void ShowWindow() {
            GetWindow<AudioIDEnumWindow>("Audio I Enum Window");
        }

        static bool awaked = false;

        string newAudioIDEnumName = "";

        bool fileMissing = false;
        bool changes = false;
        List<EditorAudioIDEnum> types;

        private void Awake() {
            awaked = true;

            types = new List<EditorAudioIDEnum>();

            TextAsset asset = (TextAsset) AssetDatabase.LoadAssetAtPath("Assets/" + TypeCSFilePath, typeof(TextAsset));
            fileMissing = asset == null;

            if (!fileMissing) {
                string content = asset.text;
                string[] lines = content.Split('\n');

                for (int i = 0; i < lines.Length; i++) {
                    if (!lines[i].EndsWith("{") && !lines[i].EndsWith("}")) {
                        string line = lines[i].Replace("\t", "").Replace(" ", "").Replace(",", "");
                        string[] data = line.Split('=');

                        int value;
                        if (int.TryParse(data[1], out value)) {
                            types.Add(new EditorAudioIDEnum(data[0], value));
                        }
                    }
                }
            }
        }

        private void OnGUI() {
            if (!awaked) Awake();

            GUILayout.Space(5);
            if (GUILayout.Button("Refresh")) {
                Awake();
                return;
            }
            GUILayout.Space(5);

            if (fileMissing) {
                if (GUILayout.Button("Generate File")) {
                    File.WriteAllText(Application.dataPath + "/" + TypeCSFilePath,
                                      FileHead + "\t\tMissingType = 0,\n" + FileFoot);
                    AssetDatabase.Refresh();
                    Awake();
                }
            }
            else {

                EditorGUILayout.BeginHorizontal();
                newAudioIDEnumName = EditorGUILayout.TextField(newAudioIDEnumName);
                if (GUILayout.Button("Add")) {
                    if (AddNewAudioIDEnum(newAudioIDEnumName)) {
                        newAudioIDEnumName = "";
                        changes = true;
                        return;
                    }
                }

                EditorGUILayout.EndHorizontal();

                GUI.enabled = changes;
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Discard Changes")) {
                    Awake();

                    changes = false;
                    return;
                }

                if (GUILayout.Button("Save")) {
                    string content = FileHead;
                    for (var i = 0; i < types.Count; i++) {
                        content += string.Format("\t\t{0} = {1},\n", types[i].Name, types[i].Value);
                    }
                    content += FileFoot;

                    File.WriteAllText(Application.dataPath + "/" + TypeCSFilePath, content);
                    AssetDatabase.Refresh();
                    Awake();

                    changes = false;
                    return;
                }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;

                GUILayout.Space(5);
                for (int i = 0; i < types.Count; i++) {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("x", GUILayout.Width(30))) {
                        types.RemoveAt(i);
                        return;
                    }

                    EditorGUI.BeginChangeCheck();
                    types[i].Name = EditorGUILayout.TextField(types[i].Name, GUILayout.Width(150));
                    if (EditorGUI.EndChangeCheck()) changes = true;
                    EditorGUILayout.LabelField(types[i].Value.ToString());
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        protected bool AddNewAudioIDEnum(string name) {
            int bigestValue = 0;
            for (int i = 0; i < types.Count; i++) {
                if (types[i].Name == name) return false;

                if (types[i].Value > bigestValue) {
                    bigestValue = types[i].Value;
                }
            }

            types.Add(new EditorAudioIDEnum(name, bigestValue + 1));

            return true;
        }

        private class EditorAudioIDEnum {
            public string Name;
            public int Value;

            public EditorAudioIDEnum(string name, int value) {
                Name = name;
                Value = value;
            }
        }
    }
}