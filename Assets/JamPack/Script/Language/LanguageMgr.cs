using System.Collections.Generic;
using UnityEngine;

namespace MPJamPack {
    public static class LanguageMgr
    {
        static private LanguageData languageData;
		static private List<LanguageText> texts = new List<LanguageText>();

        /// <summary>
        /// Register the LanguageText to Manager
        /// </summary>
        /// <param name="text"></param>
		static public void AddText(LanguageText text) {
			texts.Add(text);
		}

        /// <summary>
        /// Clear all the registered LanguageText component, usally used in changing scene
        /// </summary>
		static public void ClearTexts() {
			texts.Clear();
		}

		static public void AssignLanguageData(LanguageData newData) {
            languageData = newData;

            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i] == null)
                {
                    texts.RemoveAt(i);
                    i--;
                    continue;
                }
                texts[i].Text = GetTextById(texts[i].ID);
            }
		}

        /// <summary>
        /// Return the string accroding to langauge ID
        /// </summary>
        /// <param name="id">LanguageText ID</param>
        /// <returns></returns>
		static public string GetTextById(int id) {
            if (languageData == null) {
            #if UNITY_EDITOR
                Debug.LogErrorFormat("Language data havn't assign");
            #endif
                return "";
            }

            for (int i = 0; i < languageData.Texts.Length; i++) {
                if (languageData.Texts[i].ID == id) return languageData.Texts[i].Text;
                
            }
        #if UNITY_EDITOR
            Debug.LogErrorFormat("Text id '{0}' has no language '{1}'", id, languageData.ID);
        #endif
            return "";
		}
    }
}