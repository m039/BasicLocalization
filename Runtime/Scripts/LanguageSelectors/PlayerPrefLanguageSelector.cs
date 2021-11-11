using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m039.BasicLocalization
{
    /// <summary>
    /// Picks a language based on the information from Player Prefs.
    /// </summary>
    public class PlayerPrefLanguageSelector : BaseLanguageSelector
    {
        public override BasicLocalizationLanguage GetLanguage(BasicLocalizationProfile profile)
        {
            return BasicLocalizationPlayerPrefUtils.Restore(profile);
        }

#if UNITY_EDITOR

        public override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void DrawElement(Rect rect, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(nameof(PlayerPrefLanguageSelector)));
        }

#endif
    }
}
