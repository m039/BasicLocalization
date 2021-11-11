using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m039.BasicLocalization
{
    /// <summary>
    /// The main idea of this class is taken from Unity Localization package.
    ///
    /// This class helps to identify which language is to pick.
    ///
    /// <see cref="https://docs.unity3d.com/Packages/com.unity.localization@1.0/api/UnityEngine.Localization.Settings.IStartupLocaleSelector.html?q=selectors"/>
    /// </summary>
    [Serializable]
    public abstract class BaseLanguageSelector
#if UNITY_EDITOR
        : IReordableListElement
#endif
    {
        /// <summary>
        /// Selectes a language from the profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public abstract BasicLocalizationLanguage GetLanguage(BasicLocalizationProfile profile);

#if UNITY_EDITOR
        [NonSerialized]
        protected bool IsExpanded;

        public BasicLocalizationProfile CurrentProfile { get; set; }

        public abstract float GetElementHeight();

        public abstract void DrawElement(Rect rect, bool isActive, bool isFocused);
#endif
    }
}
