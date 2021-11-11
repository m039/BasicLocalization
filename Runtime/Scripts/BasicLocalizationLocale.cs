using System;
using System.Collections.Generic;
using UnityEngine;

namespace m039.BasicLocalization
{
    /// <summary>
    /// This class holds all necessary information for the library to figure which <see cref="BasicLocalizationLanguage"/> to use.
    /// </summary>
    [CreateAssetMenu(fileName = "New Locale", menuName = BasicLocalization.ContextMenuRoot + "/Locale", order = 1)]
    public class BasicLocalizationLocale : ScriptableObject
    {
        /// <summary>
        /// The main language code. It is primarily a two letter string, as "en".
        /// </summary>
        public string mainTranslationCode;

        /// <summary>
        /// A list of all codes that identifies this locale.
        /// </summary>
        public List<string> translationCodes;

        public bool HasCode(string arg)
        {
            if (!string.IsNullOrEmpty(mainTranslationCode) &&
                        mainTranslationCode.Equals(arg))
            {
                return true;
            }

            if (translationCodes != null)
            {
                foreach (var code in translationCodes)
                {
                    if (!string.IsNullOrEmpty(code) &&
                        code.Equals(arg))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
