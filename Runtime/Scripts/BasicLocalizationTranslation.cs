using System.Collections.Generic;

namespace m039.BasicLocalization
{
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

        [System.Serializable]
        public struct Phrase
        {
            public BasicLocalizationLanguage language;
            public string text;
        }

        public string key;

        public List<Phrase> phrases;

        public string guid;

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
