using UnityEngine;

namespace m039.BasicLocalization
{
    public static class BasicLocalizationPlayerPrefUtils
    {
        const string CurrentLanguageKey = "localization.current_language_key";

        static internal void Save(BasicLocalizationProfile profile, BasicLocalizationLanguage language)
        {
            if (language == null || profile == null)
                return;

            PlayerPrefs.SetString(CurrentLanguageKey, language.LanguageId);
            PlayerPrefs.Save();
        }

        static internal BasicLocalizationLanguage Restore(BasicLocalizationProfile profile)
        {
            if (profile == null)
                return null;

            var key = CurrentLanguageKey;

            if (PlayerPrefs.HasKey(key))
            {
                var idString = PlayerPrefs.GetString(key);
                if (!string.IsNullOrEmpty(idString))
                {
                    var languages = profile.languages;

                    if (languages != null)
                    {
                        foreach (var language in languages)
                        {
                            if (language != null && language.LanguageId.Equals(idString))
                            {
                                return language;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
