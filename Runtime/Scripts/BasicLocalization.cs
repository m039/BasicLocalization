using System.Collections.Generic;
using UnityEngine;

namespace m039.BasicLocalization
{
    public static class BasicLocalization
    {
        public const string ContextMenuRoot = "Basic Localization";


        static BasicLocalizationLanguage _currentLanguage;

        static BasicLocalizationProfile _overridenProfile;

        #region Event Callbacks

        public delegate void OnLanguageChangedDelegate(BasicLocalizationLanguage language);

        static public event OnLanguageChangedDelegate OnLanguageChanged;

        #endregion

        public static string GetTranslation(BasicLocalizationKeyReference keyReference)
        {
            return GetTranslation(GetCurrentProfile(), keyReference);
        }

        public static string GetTranslation(string key)
        {
            var profile = GetCurrentProfile();
            return profile.GetTranslation(GetCurrentLanguage(profile), key);
        }

        static string GetTranslation(BasicLocalizationProfile profile, BasicLocalizationKeyReference keyReference)
        {
            return profile?.GetTranslation(GetCurrentLanguage(profile), keyReference);
        }

        public static BasicLocalizationLanguage GetCurrentLanguage()
        {
            var profile = GetCurrentProfile();
            return GetCurrentLanguage(profile);
        }

        public static List<BasicLocalizationLanguage> GetAvailableLanguages()
        {
            var profile = GetCurrentProfile();
            return profile.languages;
        }

        public static void SelectLanguageAt(int index)
        {
            var profile = GetCurrentProfile();
            var languages = profile.languages;
            var currentLanguage = GetCurrentLanguage(profile);

            if (languages == null ||
                languages.Count <= 0 ||
                currentLanguage == null ||
                index < 0 || index > languages.Count)
                return;

            SelectLanguage(profile, languages[index]);
        }

        public static void SelectNextLanguage()
        {
            SelectLanguageWithOffset(+1);
        }

        public static void SelectPreviousLanguage()
        {
            SelectLanguageWithOffset(-1);
        }

        static void SelectLanguageWithOffset(int offset)
        {
            var profile = GetCurrentProfile();
            var languages = profile.languages;
            var currentLanguage = GetCurrentLanguage(profile);

            if (languages == null || languages.Count <= 0 || currentLanguage == null)
                return;

            var currentIndex = languages.IndexOf(currentLanguage);
            if (currentIndex == -1)
                return;

            var index = 0;

            if (offset == 1)
            {
                index = currentIndex + offset;

                if (index >= languages.Count)
                {
                    index = 0;
                }

            } else if (offset == -1)
            {
                index = currentIndex + offset;

                if (index < 0)
                {
                    index = languages.Count - 1;
                }
            }

            SelectLanguage(profile, languages[index]);
        }

        static void SelectLanguage(BasicLocalizationProfile profile, BasicLocalizationLanguage newLanguage)
        {
            var currentLanguage = GetCurrentLanguage(profile);

            if (newLanguage != null && !newLanguage.Equals(currentLanguage))
            {
                _currentLanguage = newLanguage;
                BasicLocalizationPlayerPrefUtils.Save(profile, _currentLanguage);
                OnLanguageChanged?.Invoke(newLanguage);
            }
        }

        public static void OverrideCurrentProfile(BasicLocalizationProfile overrideProfile)
        {
            var _oldLanguage = _currentLanguage;

            _overridenProfile = overrideProfile;
            _currentLanguage = null; // Clean cache.
            _currentLanguage = GetCurrentLanguage(_overridenProfile ?? BasicLocalizationProfile.Instance);

            if (_oldLanguage != _currentLanguage)
            {
                OnLanguageChanged?.Invoke(_currentLanguage);
            }
        }

        static BasicLocalizationLanguage GetCurrentLanguage(BasicLocalizationProfile profile)
        {
            if (_currentLanguage != null)
                return _currentLanguage;

            if (profile != null && profile.languageSelectors != null)
            {
                foreach (var languageSelector in profile.languageSelectors)
                {
                    var selectedLanguage = languageSelector.GetLanguage(profile);
                    if (selectedLanguage != null)
                    {
                        _currentLanguage = selectedLanguage;
                        break;
                    }
                }
            }

            return _currentLanguage;
        }

#if UNITY_EDITOR
        static BasicLocalizationProfile _sCurrentSceneProfile;

        static BasicLocalizationProfile GetCurrentSceneProfile()
        {
            UnityEditor.EditorApplication.delayCall -= ClearCurrentSceneProfile;
            UnityEditor.EditorApplication.delayCall += ClearCurrentSceneProfile;

            if (_sCurrentSceneProfile == null)
            {
                var manager = GameObject.FindObjectOfType<BasicLocalizationManager>();
                if (manager != null)
                {
                    _sCurrentSceneProfile = manager.Profile;
                }
            }

            if (_sCurrentSceneProfile != null)
                return _sCurrentSceneProfile;

            return _sCurrentSceneProfile = BasicLocalizationProfile.GetProfileFromConfig();
        }

        static void ClearCurrentSceneProfile()
        {
            _sCurrentSceneProfile = null;
        }
#endif

        static internal BasicLocalizationProfile GetCurrentProfile()
        {
#if UNITY_EDITOR
            return GetCurrentSceneProfile();
#else
            if (_overridenProfile != null)
                return _overridenProfile;

            return BasicLocalizationProfile.Instance;
#endif
        }
    }
}
