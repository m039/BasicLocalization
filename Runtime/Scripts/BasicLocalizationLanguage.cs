using UnityEngine;

namespace m039.BasicLocalization
{
    public class BasicLocalizationLanguage : ScriptableObject
    {
        public static BasicLocalizationLanguage Create(string language)
        {
            var result = CreateInstance<BasicLocalizationLanguage>();

            result.language = language;
            result.name = result.LanguageId;

            return result;
        }

        #region Inspector

        public string language;

        public BasicLocalizationLocale locale;

        #endregion

        public string LanguageId => language;
    }
}
