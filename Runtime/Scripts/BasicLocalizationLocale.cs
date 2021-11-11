using System;
using System.Collections.Generic;
using UnityEngine;

namespace m039.BasicLocalization
{
    [CreateAssetMenu(fileName = "New Locale", menuName = BasicLocalization.ContextMenuRoot + "/Locale", order = 1)]
    public class BasicLocalizationLocale : ScriptableObject
    {
        public string mainTranslationCode;

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
