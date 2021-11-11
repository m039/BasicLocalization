using UnityEngine;
using UnityEngine.Serialization;

namespace m039.BasicLocalization
{
    /// <summary>
    /// Identifies to which a translation will be related to.
    ///
    /// So, it is used like a key that also holds necessary information to properly find it in the system (like the <see cref="locale"/>).
    /// </summary>
    public class BasicLocalizationLanguage : ScriptableObject
    {
        public static BasicLocalizationLanguage Create(string languageId)
        {
            var result = CreateInstance<BasicLocalizationLanguage>();

            result.languageId = languageId;
            result.name = result.LanguageId;

            return result;
        }

        #region Inspector

        /// <summary>
        /// Identifies this language. It is used on saving or loading from Player Prefs and also in some comparisons.
        /// </summary>
        [FormerlySerializedAs("language")]
        public string languageId;

        /// <summary>
        /// Use this variable to set to which language this asset is belong to.
        ///
        /// Otherwise, it is hard to say which system language is related to this asset.
        /// </summary>
        public BasicLocalizationLocale locale;

        #endregion

        /// <summary>
        /// Returns the value that identifies this asset. <see cref="languageId"/>
        /// </summary>
        public string LanguageId => languageId;
    }
}
