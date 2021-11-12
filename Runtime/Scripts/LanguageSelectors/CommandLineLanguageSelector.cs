using System;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif
namespace m039.BasicLocalization
{
    /// <summary>
    /// Picks a language using a command line argument.
    /// </summary>
    public class CommandLineLanguageSelector : BaseLanguageSelector
    {
        public string commandLineArguments = "-language=";

        public override BasicLocalizationLanguage GetLanguage(BasicLocalizationProfile profile)
        {
            if (string.IsNullOrEmpty(commandLineArguments))
                return null;

            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith(commandLineArguments, StringComparison.OrdinalIgnoreCase))
                {
                    var argValue = arg.Substring(commandLineArguments.Length);
                    var foundLanguage = FindLanguage(profile, argValue);

                    if (foundLanguage != null)
                        Debug.LogFormat("Found a matching locale({0}) for command line argument: `{1}`.", argValue, foundLanguage);
                    else
                        Debug.LogWarningFormat("Could not find a matching locale for command line argument: `{0}`", argValue);

                    return foundLanguage;
                }
            }

            return null;
        }

        BasicLocalizationLanguage FindLanguage(BasicLocalizationProfile profile, string arg)
        {
            if (profile.languages == null)
                return null;

            arg = arg.Trim();

            foreach (var language in profile.languages)
            {
                if (language.locale != null && language.locale.HasCode(arg))
                {
                    return language;                   
                }
            }

            return null;
        }

#if UNITY_EDITOR

        public override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight * (IsExpanded? 2: 1); 
        }

        public override void DrawElement(Rect rect, bool isActive, bool isFocused)
        {
            var foldoutRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            IsExpanded = EditorGUI.Foldout(foldoutRect, IsExpanded, ObjectNames.NicifyVariableName(nameof(CommandLineLanguageSelector)));
            if (IsExpanded)
            {
                var newRect = new Rect(foldoutRect.x, rect.y + EditorGUIUtility.singleLineHeight, rect.width, EditorGUIUtility.singleLineHeight);
                var oldIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel += 2;

                EditorGUI.BeginChangeCheck();

                commandLineArguments = EditorGUI.TextField(newRect, "Command Line Argument", commandLineArguments);

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
