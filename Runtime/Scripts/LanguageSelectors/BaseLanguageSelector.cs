using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m039.BasicLocalization
{
    [Serializable]
    public abstract class BaseLanguageSelector
#if UNITY_EDITOR
        : IReordableListElement
#endif
    {
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
