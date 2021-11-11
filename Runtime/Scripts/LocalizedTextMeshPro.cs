using UnityEngine;
using TMPro;

namespace m039.BasicLocalization
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu(BasicLocalization.ContextMenuRoot + "/Localized TextMeshPro")]
    public class LocalizedTextMeshPro : MonoBehaviour
    {

        #region Inspector

        public BasicLocalizationKeyReference keyReference;

        public string fallback;

        #endregion

        protected virtual void Start()
        {
            UpdateText();
        }

        protected virtual void OnEnable()
        {
            UpdateText();
            BasicLocalization.OnLanguageChanged += OnLanguageChanged;
        }

        protected virtual void OnDisable()
        {
            BasicLocalization.OnLanguageChanged -= OnLanguageChanged;
        }

#if UNITY_EDITOR

        protected virtual void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                UpdateText();
            }
        }

#endif

        void OnLanguageChanged(BasicLocalizationLanguage language)
        {
            UpdateText();
        }

        void UpdateText()
        {
            var textMeshPro = GetComponent<TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                var translation = BasicLocalization.GetTranslation(keyReference);

                if (translation != null)
                {
                    textMeshPro.text = translation;
                } else
                {
                    textMeshPro.text = fallback;
                }
            }
        }
    }
}
