using System.Collections.Generic;
using UnityEngine;

namespace m039.BasicLocalization
{
    /// <summary>
    /// This class contains main public API for the Basic Localization package.
    ///
    /// It has access to the current selected localization profile (<see cref="BasicLocalizationProfile"/>).
    ///
    /// If you need, you can also override the current selected profily using <see cref="OverrideCurrentProfile(BasicLocalizationProfile)"/>.
    /// </summary>
    public static class BasicLocalization
    {
        public const string ContextMenuRoot = "Basic Localization";

        static BasicLocalizationLanguage _currentLanguage;

        static BasicLocalizationProfile _overridenProfile;

        #region Event Callbacks

        public delegate void OnLanguageChangedDelegate(BasicLocalizationLanguage language);

        /// <summary>
        /// Subscribe to this event to get notifications when the language changes.
        /// </summary>
        static public event OnLanguageChangedDelegate OnLanguageChanged;

        #endregion

        /// <summary>
        /// Gets a localized text for a specified key reference.
        /// </summary>
        /// <param name="keyReference"></param>
        /// <returns></returns>
        public static string GetTranslation(BasicLocalizationKeyReference keyReference)
        {
            return GetTranslation(GetCurrentProfile(), keyReference);
        }

        /// <summary>
        /// Gets a localized text for a specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetTranslation(string key)
        {
            return GetTranslation(GetCurrentProfile(), new BasicLocalizationKeyReference
            {
                key = key
            });
        }

        static string GetTranslation(BasicLocalizationProfile profile, BasicLocalizationKeyReference keyReference)
        {
            return profile?.GetTranslation(GetCurrentLanguage(profile), keyReference);
        }

        /// <summary>
        /// Returns a current selected language in the current localization profile.
        /// </summary>
        /// <returns></returns>
        public static BasicLocalizationLanguage GetCurrentLanguage()
        {
            var profile = GetCurrentProfile();
            return GetCurrentLanguage(profile);
        }

        /// <summary>
        /// This function travers the current selected profile and picks all its languages.
        /// </summary>
        /// <returns></returns>
        public static List<BasicLocalizationLanguage> GetAvailableLanguages()
        {
            var profile = GetCurrentProfile();
            return profile.languages;
        }

        /// <summary>
        /// Selects a language specified by the index. It works best with <see cref="GetAvailableLanguages"/>.
        /// </summary>
        /// <param name="index"></param>
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

        /// <summary>
        /// Selects a next language in the current localization profile.
        /// </summary>
        public static void SelectNextLanguage()
        {
            SelectLanguageWithOffset(+1);
        }

        /// <summary>
        /// Selects a previous language in the current localization profile.
        /// </summary>
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

        /// <summary>
        /// By default, there is only one localization profile for the whole application,
        /// but you can override this behaviour by this function.
        /// </summary>
        /// <param name="overrideProfile">A new localization profile or <code>null</code> if you want to reset the profile back.</param>
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
