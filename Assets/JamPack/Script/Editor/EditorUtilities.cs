using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MPJamPack {
    public class EditorUtilities : MonoBehaviour
    {
        private static GUIStyle buttonPressedStyle;

        public static bool ToggleButton(bool toggled, GUIContent btnContent) {
            if (buttonPressedStyle == null) {
                buttonPressedStyle = new GUIStyle("Button");
                buttonPressedStyle.padding = new RectOffset(0, 0, 5, 5);
                buttonPressedStyle.margin = new RectOffset(0, 0, 0, 0);
            }

            return GUILayout.Toggle(toggled, btnContent, buttonPressedStyle);
        }
    }
}