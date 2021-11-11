using System.Globalization;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace m039.BasicLocalization
{
    /// <summary>
    /// Selects a best suitable language from the system.
    /// </summary>
    public class SystemLanguageSelector : BaseLanguageSelector
    {
        public override BasicLocalizationLanguage GetLanguage(BasicLocalizationProfile profile)
        {
            if (profile == null || profile.languages == null || profile.languages.Count <= 0)
                return null;

            BasicLocalizationLanguage findLanguage1()
            {
                var cultureInfo = CultureInfo.CurrentUICulture;
                var language = FindLanguage(profile, cultureInfo?.Name);
                if (language == null)
                {
                    cultureInfo = cultureInfo.Parent;
                    while (cultureInfo != CultureInfo.InvariantCulture && language == null)
                    {
                        language = FindLanguage(profile, cultureInfo.Name);
                        cultureInfo = cultureInfo.Parent;
                    }
                }

                return language;
            }

            BasicLocalizationLanguage findLanguage2()
            {
                var systemLanguage = Application.systemLanguage;
                if (systemLanguage != SystemLanguage.Unknown)
                {
                    return FindLanguage(profile, systemLanguage);
                }

                return null;
            }

            // For some reason CurrentUICulture in WebGL returns invariant language.

            var finders = new System.Func<BasicLocalizationLanguage>[]
            {
#if UNITY_WEBGL
                findLanguage2,
                findLanguage1
#else
                findLanguage1,
                findLanguage2
#endif
            };

            foreach (var finder in finders)
            {
                var language = finder();
                if (language != null)
                    return language;
            }

            return null;
        }

        static BasicLocalizationLanguage FindLanguage(BasicLocalizationProfile profile, SystemLanguage systemLanguage)
        {
            return FindLanguage(profile, GetSystemLanguageCultureCode(systemLanguage));
        }

        static BasicLocalizationLanguage FindLanguage(BasicLocalizationProfile profile, CultureInfo cultureInfo)
        {
            if (cultureInfo == null)
                return null;

            return FindLanguage(profile, cultureInfo.Name);
        }

        static BasicLocalizationLanguage FindLanguage(BasicLocalizationProfile profile, string code)
        {
            foreach (var language in profile.languages)
            {
                if (language.locale != null && language.locale.HasCode(code))
                {
                    return language;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a SystemLanguage enum into a CultureInfo Code.
        /// </summary>
        /// <param name="lang">The SystemLanguage enum to convert into a Code.</param>
        /// <returns>The language Code or an empty string if the value could not be converted.</returns>
        static string GetSystemLanguageCultureCode(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Afrikaans: return "af";
                case SystemLanguage.Arabic: return "ar";
                case SystemLanguage.Basque: return "eu";
                case SystemLanguage.Belarusian: return "be";
                case SystemLanguage.Bulgarian: return "bg";
                case SystemLanguage.Catalan: return "ca";
                case SystemLanguage.Chinese: return "zh-CN";
                case SystemLanguage.ChineseSimplified: return "zh-hans";
                case SystemLanguage.ChineseTraditional: return "zh-hant";
                case SystemLanguage.SerboCroatian: return "hr";
                case SystemLanguage.Czech: return "cs";
                case SystemLanguage.Danish: return "da";
                case SystemLanguage.Dutch: return "nl";
                case SystemLanguage.English: return "en";
                case SystemLanguage.Estonian: return "et";
                case SystemLanguage.Faroese: return "fo";
                case SystemLanguage.Finnish: return "fi";
                case SystemLanguage.French: return "fr";
                case SystemLanguage.German: return "de";
                case SystemLanguage.Greek: return "el";
                case SystemLanguage.Hebrew: return "he";
                case SystemLanguage.Hungarian: return "hu";
                case SystemLanguage.Icelandic: return "is";
                case SystemLanguage.Indonesian: return "id";
                case SystemLanguage.Italian: return "it";
                case SystemLanguage.Japanese: return "ja";
                case SystemLanguage.Korean: return "ko";
                case SystemLanguage.Latvian: return "lv";
                case SystemLanguage.Lithuanian: return "lt";
                case SystemLanguage.Norwegian: return "no";
                case SystemLanguage.Polish: return "pl";
                case SystemLanguage.Portuguese: return "pt";
                case SystemLanguage.Romanian: return "ro";
                case SystemLanguage.Russian: return "ru";
                case SystemLanguage.Slovak: return "sk";
                case SystemLanguage.Slovenian: return "sl";
                case SystemLanguage.Spanish: return "es";
                case SystemLanguage.Swedish: return "sv";
                case SystemLanguage.Thai: return "th";
                case SystemLanguage.Turkish: return "tr";
                case SystemLanguage.Ukrainian: return "uk";
                case SystemLanguage.Vietnamese: return "vi";
                default: return "";
            }
        }

#if UNITY_EDITOR
        public override float GetElementHeight()
        {
            return EditorGUIUtility.singleLineHeight;
        }
        
        public override void DrawElement(Rect rect, bool isActive, bool isFocused)
        {
            EditorGUI.LabelField(rect, ObjectNames.NicifyVariableName(nameof(SystemLanguageSelector)));
        }
#endif
    }
}
