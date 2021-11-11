using System.Collections.Generic;
using UnityEngine;

namespace m039.BasicLocalization
{
    /// <summary>
    /// Holds all information related to a translation.
    /// </summary>
    [System.Serializable]
    public class BasicLocalizationTranslation
    {
        public static BasicLocalizationTranslation Create(string key)
        {
            return new BasicLocalizationTranslation
            {
                guid = System.Guid.NewGuid().ToString(),
                key = key
            };
        }

        /// <summary>
        /// A phrase represantes a translation entity.
        /// </summary>
        [System.Serializable]
        public struct Phrase
        {
            /// <summary>
            /// A language of the translation.
            /// </summary>
            public BasicLocalizationLanguage language;

            /// <summary>
            /// A text translation.
            /// </summary>
            public string text;
        }

        /// <summary>
        /// A key of translation.
        /// In other words, it is the secondary identifier of the translation.
        /// </summary>
        public string key;

        /// <summary>
        /// The main translation data.
        /// </summary>
        public List<Phrase> phrases;

        /// <summary>
        /// In some cases key can be modified, but the guid is always the same.
        /// It is a primary identifier of the translation.
        /// </summary>
        [SerializeField]
        internal string guid;

        /// <summary>
        /// Gets a text for the specified language.
        /// </summary>
        /// <param name="language"></param>
        /// <returns>Translation text or null if there is no one.</returns>
        public string GetTranslation(BasicLocalizationLanguage language)
        {
            if (phrases != null)
            {
                foreach (var p in phrases)
                {
                    if (p.language != null &&
                        language != null &&
                        p.language.LanguageId.Equals(language.LanguageId))
                    {
                        return p.text;
                    }
                }
            }

            return null;
        }

#if UNITY_EDITOR

        internal void PutTranslation(BasicLocalizationLanguage language, string text)
        {
            if (language == null)
                return;

            if (phrases != null)
            {
                for (int i = 0; i < phrases.Count; i++)
                {
                    var p = phrases[i];
                    if (p.language != null &&
                        language != null &&
                        p.language.LanguageId.Equals(language.LanguageId))
                    {
                        phrases[i] = new Phrase
                        {
                            language = language,
                            text = text
                        };
                        return;
                    }
                }
            }

            if (phrases == null)
            {
                phrases = new List<Phrase>();
            }

            phrases.Add(new Phrase
            {
                language = language,
                text = text
            });
        }

        internal void ReservePhrase(BasicLocalizationLanguage language)
        {
            if (phrases != null)
            {
                foreach (var p in phrases)
                {
                    if (p.language != null &&
                        language != null &&
                        p.language.LanguageId.Equals(language.LanguageId))
                        return;
                }
            }

            if (phrases == null)
            {
                phrases = new List<Phrase>();
            }

            phrases.Add(new Phrase
            {
                language = language,
                text = string.Empty
            });
        }

#endif
    }

}
