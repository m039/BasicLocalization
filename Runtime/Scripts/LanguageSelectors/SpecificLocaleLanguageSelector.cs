using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m039.BasicLocalization
{
    /// <summary>
    /// Selects a language which has a specified locale.
    /// </summary>
    public class SpecificLocaleLanguageSelector : BaseLanguageSelector
    {
        public BasicLocalizationLocale locale;

        public override BasicLocalizationLanguage GetLanguage(BasicLocalizationProfile profile)
        {
            if (locale != null &&
                !string.IsNullOrEmpty(locale.mainTranslationCode) &&
                profile != null &&
                profile.languages != null &&
                profile.languages.Count > 0)
            {
                foreach (var language in profile.languages)
                {
                    if (locale.mainTranslationCode.Equals(language?.locale?.mainTranslationCode)) {
                        return language;
                    }
                }
            }

            return null;
        }

#if UNITY_EDITOR

        public override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * (IsExpanded? 2 : 1);
        }

        public override void DrawElement(Rect rect, bool isActive, bool isFocused)
        {
            var foldoutRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            IsExpanded = EditorGUI.Foldout(foldoutRect, IsExpanded, ObjectNames.NicifyVariableName(nameof(SpecificLocaleLanguageSelector)));
            if (IsExpanded)
            {
                var newRect = new Rect(foldoutRect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight);
                var oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel += 2;

                EditorGUI.BeginChangeCheck();

                locale = (BasicLocalizationLocale) EditorGUI.ObjectField(newRect, "Locale", locale, typeof(BasicLocalizationLocale), false);

                EditorGUI.indentLevel = oldIndent;

                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(CurrentProfile);
                }
            }
        }

#endif
    }
}
