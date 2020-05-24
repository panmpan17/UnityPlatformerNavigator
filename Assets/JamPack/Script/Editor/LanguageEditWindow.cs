using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MPJamPack {
    public class LanguageEditWindow : EditorWindow
    {
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Number = "0123456789";
        private const string Symbol = "!\"#$%^'()*+,-./:;<=>?@[]\\^_`{}|~";

        static private Color IDLabelBGColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        static private Color LineColor = new Color(0.7f, 0.7f, 0.7f, 1);

        private bool addAlphabet = true, addNumber = true, addSymbol = true;

        [MenuItem("Window/Language Editor")]
        static private void OpenEditorWindow() {
            GetWindow<LanguageEditWindow>("Language Editor");
        }

        Vector2 scrollViewPos = Vector3.zero;
        LanguageData[] allDatas;
        int[] allIDs;

        private void Awake() {
            string[] files = AssetDatabase.FindAssets("t:LanguageData");
            allDatas = new LanguageData[files.Length];

            List<int> idList = new List<int>();
            for (int i = 0; i < files.Length; i++) {
                string path = AssetDatabase.GUIDToAssetPath(files[i]);
                LanguageData data = AssetDatabase.LoadAssetAtPath<LanguageData>(path);
                allDatas[i] = data;

                for (int j = 0; j < data.Texts.Length; j++) {
                    if (!idList.Contains(data.Texts[j].ID)) idList.Add(data.Texts[j].ID);
                }
            }

            idList.Sort();
            allIDs = idList.ToArray();
        }

        private void ScanTextInEveryLanguageData() {
            List<char> allChars = new List<char>();
            if (addAlphabet) allChars.AddRange(Alphabet.ToCharArray());
            if (addNumber) allChars.AddRange(Number.ToCharArray());
            if (addSymbol) allChars.AddRange(Symbol.ToCharArray());

            for (int i = 0; i < allDatas.Length; i++) {
                for (int j = 0; j < allDatas[i].Texts.Length; j++) {
                    char[] chars = allDatas[i].Texts[j].Text.ToCharArray();
                    foreach (char chr in chars) {
                        if (!allChars.Contains(chr)) allChars.Add(chr);
                    }
                }
            }

            string allCharString = new string(allChars.ToArray());
            Debug.Log(allCharString);
        }

        private void DrawRow(int ID) {
            int[] textIndex = new int[allDatas.Length];
            int lineCount = 0;

            for (int i = 0; i < allDatas.Length; i++) {
                bool assigned = false;
                for (int j = 0; j < allDatas[i].Texts.Length; j++) {
                    if (allDatas[i].Texts[j].ID == ID) {
                        assigned = true;
                        textIndex[i] = j;

                        int count = allDatas[i].Texts[j].Text.Split(
                            new string[] { "\n" }, System.StringSplitOptions.None).Length;
                        
                        if (count > lineCount) lineCount = count;
                        break;
                    }
                }

                if (!assigned) {
                    Array.Resize(ref allDatas[i].Texts, allDatas[i].Texts.Length + 1);
                    int last = allDatas[i].Texts.Length - 1;
                    allDatas[i].Texts[last].ID = ID;
                    allDatas[i].Texts[last].Text = "";
                    textIndex[i] = last;
                }
            }

            Rect rowRect = EditorGUILayout.GetControlRect(false, (lineCount * 15) + 3);

            Rect labelRect = rowRect;
            labelRect.width = 50;
            labelRect.height -= 3;
            labelRect.y++;

            Handles.DrawSolidRectangleWithOutline(labelRect, IDLabelBGColor, IDLabelBGColor);
            EditorGUI.LabelField(labelRect, ID.ToString());

            for (int i = 0; i < allDatas.Length; i++) {
                labelRect.x += labelRect.width + 2;
                labelRect.width = 200;

                if (textIndex[i] != -1) {
                    allDatas[i].Texts[textIndex[i]].Text = EditorGUI.TextArea(labelRect, allDatas[i].Texts[textIndex[i]].Text);
                }
            }
            // EditorGUI.LabelField.
            // EditorGUI.GetPropertyHeight()

            Rect hrRect = rowRect;
            hrRect.y += rowRect.height - 1;
            hrRect.height = 1;
            Handles.DrawSolidRectangleWithOutline(hrRect, LineColor, LineColor);
        }

        private void OnGUI() {
            addAlphabet = EditorGUILayout.Toggle("Auto Include alphabet", addAlphabet);
            addNumber = EditorGUILayout.Toggle("Auto Include number", addNumber);
            addSymbol = EditorGUILayout.Toggle("Audo Include symbol", addSymbol);
            if (GUILayout.Button("Scan Text in Every LanguageData")) {
                ScanTextInEveryLanguageData();
            }

            scrollViewPos = EditorGUILayout.BeginScrollView(scrollViewPos, false, true);
            for (int i = 0; i < allIDs.Length; i++) {
                DrawRow(allIDs[i]);
            }
            EditorGUILayout.EndScrollView();
        }

    }
}