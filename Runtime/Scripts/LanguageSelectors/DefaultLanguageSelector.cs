#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace m039.BasicLocalization
{
    public class DefaultLanguageSelector : BaseLanguageSelector
    {
        public override BasicLocalizationLanguage GetLanguage(BasicLocalizationProfile profile)
        {
            return profile?.defaultLanguage;
        }

#if UNITY_EDITOR

        public override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void DrawElement(Rect rect, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(nameof(DefaultLanguageSelector)));
        }

#endif

    }
}
