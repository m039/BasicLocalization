#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace m039.BasicLocalization
{
    public interface IReordableListElement
    {
        public BasicLocalizationProfile CurrentProfile { get; set; }

        public abstract float GetElementHeight();

        public abstract void DrawElement(Rect rect, bool isActive, bool isFocused);
    }
}

#endif
