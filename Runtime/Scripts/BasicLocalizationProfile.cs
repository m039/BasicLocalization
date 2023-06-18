using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace m039.BasicLocalization
{
    /// <summary>
    /// The main class that stores all localization information, as translaiton, languages and selectors.
    ///
    /// It is primarily a data class that has few public functions for working with it. But there is a huge
    /// invisible work that is done in the <see cref="EditorExtensions"/> class.
    /// </summary>
    public class BasicLocalizationProfile : ScriptableObject
    {
#if UNITY_EDITOR
        internal const string ConfigName = "com.m039.localization.settings";
#endif

        internal static BasicLocalizationProfile Create(string name)
        {
            var settings = CreateInstance<BasicLocalizationProfile>();

            settings.name = name;
            settings.translations = new List<BasicLocalizationTranslation>();
            settings.languages = new List<BasicLocalizationLanguage>();

            return settings;
        }

        static BasicLocalizationProfile _sInstance;

        /// <summary>
        /// An instance of the main localization profile that is single for the whole application.
        /// </summary>
        public static BasicLocalizationProfile Instance => _sInstance;

        #region Inspector

        /// <summary>
        /// A default language that will be used for most operations
        /// </summary>
        public BasicLocalizationLanguage defaultLanguage;

        /// <summary>
        /// A list of all available languages for the current profile.
        /// </summary>
        public List<BasicLocalizationLanguage> languages;

        /// <summary>
        /// A list of selectors which help of picking the right language for a translation.
        /// </summary>
        [SerializeReference]
        public List<BaseLanguageSelector> languageSelectors = new List<BaseLanguageSelector>
        {
            new CommandLineLanguageSelector(),
            new PlayerPrefLanguageSelector(),
            new SystemLanguageSelector(),
            new DefaultLanguageSelector()
        };

        /// <summary>
        /// The main data of the profile, its translations.
        /// </summary>
        public List<BasicLocalizationTranslation> translations;

#pragma warning disable 414

        [SerializeField]
        bool mainAsset = false;

#pragma warning restore 414

#if UNITY_EDITOR
        public EditorData editorData = new EditorData();

        [System.Serializable]
        public class EditorData {
            public bool translateOnlyEmptyFields = true;
        }

#endif

#endregion

        Dictionary<string, BasicLocalizationTranslation> _translationsByKey;

        Dictionary<string, BasicLocalizationTranslation> _translationByGuid;

        /// <summary>
        /// Finds and returns a language for the specified language id.
        /// </summary>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public BasicLocalizationLanguage GetLanguage(string languageId)
        {
            return GetLanguageByLanguageId(languageId);
        }

        /// <summary>
        /// Gets a text translation for the specified language and key.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetTranslation(BasicLocalizationLanguage language, string key)
        {
            var translation = GetTranslationRawByKey(key);
            if (translation != null)
            {
                return translation.GetTranslation(language);
            }

            return null;
        }

        /// <summary>
        /// Gets a text translation for the specified language and key reference.
        /// </summary>
        /// <param name="language"></param>
        /// <param name="keyReference"></param>
        /// <returns></returns>
        public string GetTranslation(BasicLocalizationLanguage language, BasicLocalizationKeyReference keyReference)
        {
            if (language == null)
                return null;

            BasicLocalizationTranslation translation;

            if (!string.IsNullOrEmpty(keyReference.guid))
            {
                translation = GetTranslationRawByGuid(keyReference.guid);
                if (translation == null)
                {
                    translation = GetTranslationRawByKey(keyReference.key); // Fallback.
                }
            }
            else
            {
                translation = GetTranslationRawByKey(keyReference.key);
            }

            if (translation != null)
            {
                return translation.GetTranslation(language);
            }

            return null;
        }

        internal BasicLocalizationTranslation GetTranslationRawByGuid(string guid)
        {
            CacheTranslations();
            if (!string.IsNullOrEmpty(guid) &&
                _translationByGuid.TryGetValue(guid, out BasicLocalizationTranslation translation))
            {
                return translation;
            }

            return null;
        }

        internal BasicLocalizationTranslation GetTranslationRawByKey(string key)
        {
            CacheTranslations();
            if (!string.IsNullOrEmpty(key) &&
                _translationsByKey.TryGetValue(key, out BasicLocalizationTranslation translation))
            {
                return translation;
            }

            return null;
        }

        void CacheTranslations()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= ClearTranslationCache;
            EditorApplication.delayCall += ClearTranslationCache;
#endif

            if (_translationsByKey == null || _translationByGuid == null)
            {
                _translationsByKey = new Dictionary<string, BasicLocalizationTranslation>();
                _translationByGuid = new Dictionary<string, BasicLocalizationTranslation>();

                if (translations != null)
                {
                    foreach (var translation in translations)
                    {
                        if (!_translationsByKey.ContainsKey(translation.key))
                        {
                            _translationsByKey.Add(translation.key, translation);
                        }

                        if (!_translationByGuid.ContainsKey(translation.guid))
                        {
                            _translationByGuid.Add(translation.guid, translation);
                        }
                    }
                }
            }
        }

#if UNITY_EDITOR

        void ClearTranslationCache()
        {
            _translationsByKey = null;
            _translationByGuid = null;
        }

#endif

#if UNITY_EDITOR

        /// <summary>
        /// Represents all a profile's data in a unified way.
        /// </summary>
        public class SnapshotData
        {
            readonly public List<string> languages = new List<string>();
            readonly public List<string> keys = new List<string>();
            readonly public List<List<string>> translations = new List<List<string>>();
        }

        EditorExtensions _editor;

        internal EditorExtensions Editor => _editor ?? (_editor = new EditorExtensions(this));

        /// <summary>
        /// The main class for modifying the profile that is working in Editor.
        /// </summary>
        internal class EditorExtensions
        {
            BasicLocalizationProfile _profile;

            public EditorExtensions(BasicLocalizationProfile profile)
            {
                _profile = profile;
            }

            public void SetMainAssetFlag()
            {
                _profile.mainAsset = true;
            }

            public void ClearMainAssetFlag()
            {
                _profile.mainAsset = false;
            }

            // Adding and removing a language.

            public void AddLanguage(string languageName, bool forceSave = true)
            {
                Undo.SetCurrentGroupName("Add Language");
                var groupIndex = Undo.GetCurrentGroup();

                // Create a new language.
                var language = BasicLocalizationLanguage.Create(languageName);

                // Save the profile.
                Undo.RegisterCompleteObjectUndo(_profile, "Add a Language");

                // Add language.
                _profile.languages.Add(language);

                ReserverPhrases(language);

                AssetDatabase.AddObjectToAsset(language, _profile);

                // Save the created language.
                Undo.RegisterCreatedObjectUndo(language, "Create Language");

                Undo.CollapseUndoOperations(groupIndex);
                EditorUtility.SetDirty(_profile);

                if (forceSave)
                {
                    AssetDatabase.SaveAssets();
                }
            }

            void ReserverPhrases(BasicLocalizationLanguage language)
            {
                if (_profile.translations != null)
                {
                    foreach (var translation in _profile.translations)
                    {
                        translation.ReservePhrase(language);
                    }
                }
            }

            public void RemoveLanguageAt(int index, bool forceSave = true)
            {
                Undo.SetCurrentGroupName("Remove Language");
                var groupIndex = Undo.GetCurrentGroup();

                // Save the profile.
                Undo.RegisterCompleteObjectUndo(_profile, "Remove a Language");

                var language = _profile.languages[index];
                _profile.languages.RemoveAt(index);

                if (language == _profile.defaultLanguage)
                {
                    _profile.defaultLanguage = null;
                }

                // Destory and save a child asset.
                Undo.DestroyObjectImmediate(language);

                RemoveUnrelevantTranslations();

                Undo.CollapseUndoOperations(groupIndex);

                EditorUtility.SetDirty(_profile);

                if (forceSave)
                {
                    AssetDatabase.SaveAssets();
                }
            }

            void RemoveUnrelevantTranslations()
            {
                if (_profile.translations != null)
                {
                    for (int i = _profile.translations.Count - 1; i >= 0; i--)
                    {
                        var translation = _profile.translations[i];
                        if (translation == null)
                        {
                            _profile.translations.RemoveAt(i);
                        }
                        else
                        {
                            if (translation.phrases != null)
                            {
                                for (int k = translation.phrases.Count - 1; k >= 0; k--)
                                {
                                    var language = translation.phrases[k].language;
                                    if (language == null)
                                    {
                                        translation.phrases.RemoveAt(k);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public BasicLocalizationTranslation AddTranslation(string key)
            {
                var translation = BasicLocalizationTranslation.Create(key);
                Undo.RegisterCompleteObjectUndo(_profile, "Add a New Translation");
                _profile.translations.Add(translation);
                EditorUtility.SetDirty(_profile);
                return translation;
            }

            public void RemoveTranslationAt(IList<int> indeces)
            {
                if (_profile.translations == null || indeces == null || indeces.Count <= 0)
                    return;

                Undo.RegisterCompleteObjectUndo(_profile, "Remove Translations");

                var sortedList = new List<int>(indeces);

                sortedList.Sort();

                for (int i = sortedList.Count - 1; i >= 0; i--)
                {
                    var index = sortedList[i];
                    if (_profile.translations.Count > index)
                    {
                        _profile.translations.RemoveAt(index);
                    }
                }

                EditorUtility.SetDirty(_profile);
            }

            public void RemoveTranslationAt(int index)
            {
                if (_profile.translations == null || _profile.translations.Count <= index)
                {
                    return;
                }

                Undo.RegisterCompleteObjectUndo(_profile, "Remove a Translation");
                _profile.translations.RemoveAt(index);
                EditorUtility.SetDirty(_profile);
            }

            // Export or import translations.

            public SnapshotData GetSnapshotData()
            {
                var exportData = new SnapshotData();

                // Fill languages.

                if (_profile.languages != null && _profile.languages.Count > 0)
                {
                    foreach (var language in _profile.languages)
                    {
                        exportData.languages.Add(language.LanguageId);
                    }
                }

                // Fill translations and keys.

                if (_profile.translations != null && _profile.translations.Count > 0)
                {
                    foreach (var translation in _profile.translations)
                    {
                        exportData.keys.Add(translation.key);

                        var snTranslation = new List<string>();

                        foreach (var language in _profile.languages)
                        {
                            snTranslation.Add(translation.GetTranslation(language));
                        }

                        exportData.translations.Add(snTranslation);
                    }
                }

                return exportData;
            }

            public void ImportSnapshotData(SnapshotData data)
            {
                Undo.SetCurrentGroupName("Import Profile Data");
                var groupIndex = Undo.GetCurrentGroup();

                Undo.RegisterCompleteObjectUndo(_profile, "Imported Snapshot Data");

                // Add missed languages.

                foreach (var snLanguage in data.languages)
                {
                    var language = _profile.GetLanguageByLanguageId(snLanguage);
                    if (language == null)
                    {
                        AddLanguage(snLanguage, forceSave: false);
                    }
                }

                // Prepare a language look up table.

                var languageLookUp = new List<BasicLocalizationLanguage>();

                foreach (var snLanguage in data.languages)
                {
                    languageLookUp.Add(_profile.GetLanguageByLanguageId(snLanguage));
                }

                // Add missed translations.

                _profile.ClearTranslationCache();

                for (int i = 0; i < data.keys.Count; i++)
                {
                    var snKey = data.keys[i];
                    var translation = _profile.GetTranslationRawByKey(snKey);
                    if (translation == null)
                    {
                        AddTranslation(snKey);
                    }
                }

                _profile.ClearTranslationCache();

                // Travers translations the last time.

                for (int i = 0; i < data.keys.Count; i++)
                {
                    var snKey = data.keys[i];
                    var translation = _profile.GetTranslationRawByKey(snKey);
                    if (translation != null)
                    {
                        for (int k = 0; k < data.languages.Count; k++)
                        {
                            var language = languageLookUp[k];
                            var snTranslation = data.translations[i];

                            if (k < snTranslation.Count)
                            {
                                translation.PutTranslation(language, data.translations[i][k]);
                            }
                        }
                    }
                }

                Undo.CollapseUndoOperations(groupIndex);

                EditorUtility.SetDirty(_profile);
                AssetDatabase.SaveAssets();
            }
        }
#endif

        BasicLocalizationLanguage GetLanguageByLanguageId(string languageId)
        {
            if (languages != null && !string.IsNullOrEmpty(languageId))
            {
                foreach (var language in languages)
                {
                    if (language != null)
                    {
                        if (languageId.Equals(language.LanguageId))
                        {
                            return language;
                        }
                    }
                }
            }

            return null;
        }

        void OnEnable()
        {
#if !UNITY_EDITOR
            if (_sInstance == null || mainAsset)
            {
                _sInstance = this;
            }
#endif
        }

#if UNITY_EDITOR
        static internal BasicLocalizationProfile GetProfileFromConfig()
        {
            UnityEditor.EditorBuildSettings.TryGetConfigObject(ConfigName, out BasicLocalizationProfile profile);
            return profile;
        }
#endif
    }

}
