using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using System;

using static m039.BasicLocalization.BasicLocalizationUIUtils;
using System.Collections;

namespace m039.BasicLocalization
{
    [System.Serializable]
    public class BasicLocalizationProfileEditorController
    {
        const int TabLanguagesIndex = 0;

        const int TabTranslationsIndex = 1;

        const int TabExports = 2;

#if BASIC_LOCALIZATION_DEBUG
        const int TabDebug = 3;
#endif

        static string[] Tabs = new string[] {
            "Languages",
            "Translations",
            "Export / Import",
#if BASIC_LOCALIZATION_DEBUG
            "Debug"
#endif
        };

        public BasicLocalizationProfile profile;

        int _selectedTab;

        LanguageSelectorsDrawHelper _selectorsDrawHelper;

        AvailableLanguagesDrawHelper _languagesDrawHelper;

        TranslationsDrawHelper _translationsDrawHelper;

        bool _useMinHeight;

        Vector2 _scrollViewPosition;

        public BasicLocalizationProfileEditorController(BasicLocalizationProfile profile, bool usedInInspector)
        {
            this.profile = profile;
            _languagesDrawHelper = new AvailableLanguagesDrawHelper();
            _translationsDrawHelper = new TranslationsDrawHelper();
            _selectorsDrawHelper = new LanguageSelectorsDrawHelper();
            _useMinHeight = usedInInspector;
        }

        public void Draw()
        {
            GUILayout.BeginVertical();

            _selectedTab = GUILayout.Toolbar(_selectedTab, Tabs);

            EditorGUILayout.Space(PaddingSmall);

            if (_selectedTab == TabLanguagesIndex)
            {
                _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);

                DrawSelectDefaultLanguage(profile);

                EditorGUILayout.Space(PaddingBig);

                _selectorsDrawHelper.Draw(profile);

                EditorGUILayout.Space(PaddingSmall);

                _languagesDrawHelper.Draw(profile);

                EditorGUILayout.EndScrollView();
            }
            else if (_selectedTab == TabTranslationsIndex)
            {
                _translationsDrawHelper.Draw(profile, _useMinHeight);
            }
            else if (_selectedTab == TabExports)
            {
                DrawExportAndImportControls(profile);
            }
#if BASIC_LOCALIZATION_DEBUG
            else if (_selectedTab == TabDebug)
            {
                EditorGUILayout.Space();

                DrawDebugControls(profile);
            }
#endif

            GUILayout.EndVertical();
        }

#if BASIC_LOCALIZATION_DEBUG

        string _debugNumberOfTranslation;

        void DrawDebugControls(BasicLocalizationProfile localizationProfile)
        {
            GUILayout.BeginHorizontal();

            _debugNumberOfTranslation = EditorGUILayout.TextField("Number of Translations", _debugNumberOfTranslation);

            if (GUILayout.Button("Fill With Test Data"))
            {
                if (int.TryParse(_debugNumberOfTranslation, out int result) && result > 0 && result < 10000)
                {
                    var profile = localizationProfile;
                    profile.translations.Clear();
                    var english = profile.GetLanguage("English");
                    var russian = profile.GetLanguage("Russian");
                    var testLanguages = new BasicLocalizationLanguage[10];

                    for (int i = 0; i < testLanguages.Length; i++)
                    {
                        testLanguages[i] = profile.GetLanguage($"Test {i + 1}");
                    }

                    for (int i = 0; i < result; i++)
                    {
                        var key = $"Test {i}";
                        var translation = profile.GetTranslationRawByKey(key);
                        if (translation == null)
                        {
                            translation = profile.Editor.AddTranslation(key);
                        }

                        translation.PutTranslation(english, $"Test {i}");
                        translation.PutTranslation(russian, $"Тест {i}");

                        for (int k = 0; k < testLanguages.Length; k++)
                        {
                            translation.PutTranslation(testLanguages[k], $"Test[{k}] => {i}");
                        }
                    }

                    EditorUtility.SetDirty(profile);
                }
                else
                {
                    Debug.LogWarning("Can't fill the profile with test data.");
                }
            }

            GUILayout.EndHorizontal();
        }
#endif

        static void DrawExportAndImportControls(BasicLocalizationProfile localizationProfile)
        {
            // Export

            if (GUILayout.Button("Export As CSV File"))
            {
                var path = EditorUtility.SaveFilePanel("Export Translations", string.Empty, "BasicLocalizationTranslations", "csv");
                BasicLocalizationCSVConverter.Export(localizationProfile, path);
            }

            // Import

            if (GUILayout.Button("Import From CSV File"))
            {
                var path = EditorUtility.OpenFilePanel("Import Translations", string.Empty, "csv");
                BasicLocalizationCSVConverter.Import(localizationProfile, path);
            }
        }

        static void DrawSelectDefaultLanguage(BasicLocalizationProfile profile)
        {
            var languageNames = new string[profile.languages.Count + 1];

            languageNames[0] = "Unknown";

            for (int i = 1; i < languageNames.Length; i++)
            {
                languageNames[i] = profile.languages[i - 1].LanguageId;
            }

            EditorGUI.BeginChangeCheck();

            var index = EditorGUILayout.Popup("Default Language", profile.languages.IndexOf(profile.defaultLanguage) + 1, languageNames);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, "Selected Default Language");
                profile.defaultLanguage = index == 0 || index == -1? null : profile.languages[index - 1];
                EditorUtility.SetDirty(profile);
            }
        }

        class TranslationsSearchField : SearchField
        {
            static GUILayoutOption[] _sLayoutOptions;

            public string search;

            public GUILayoutOption[] GetLayoutOptions()
            {
                if (_sLayoutOptions == null)
                {
                    _sLayoutOptions = new GUILayoutOption[]
                    {
                        GUILayout.ExpandWidth(true),
                        GUILayout.Height(EditorGUIUtility.singleLineHeight + PaddingSmall * 2)
                    };
                }

                return _sLayoutOptions;
            }

            public void Draw(Rect position)
            {
                position.y += PaddingSmall;
                position.height -= PaddingSmall * 2;
                position.x += PaddingSmall;
                position.width -= PaddingSmall * 2;

                search = OnGUI(position, search);
            }
        }

        class TranslationsTreeView : TreeView
        {
            static readonly GUIStyle BoxStyle1;

            static readonly GUIStyle BoxStyle2;

            static readonly GUIStyle PlaceholderLabelStyle;

            static TranslationsTreeView()
            {
                BoxStyle1 = new GUIStyle();
                BoxStyle2 = new GUIStyle();

                var texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, Color.Lerp(GUI.skin.label.normal.textColor, InspectorBackgroundColor, 0.95f));
                texture.Apply();
                BoxStyle1.normal.background = texture;

                PlaceholderLabelStyle = new GUIStyle("label");
                PlaceholderLabelStyle.fontStyle = FontStyle.Italic;
            }

            BasicLocalizationProfile _profile;

            int _languagesCount = -1;

            int _translationCount = -1;

            LanguageVisibilityMask _languageVisibilityMask;

            public TranslationsTreeView(TreeViewState state) : base(state)
            {
                _languageVisibilityMask = new LanguageVisibilityMask();

                Reload();
            }

            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem { id = -1, depth = -1 };
            }

            public void Draw(BasicLocalizationProfile profile, Rect position)
            {
                bool reload = false;

                if (_profile == null || profile != _profile)
                {
                    _profile = profile;
                    reload = true;
                }

                if (_profile != null && _translationCount != _profile.translations.Count)
                {
                    _translationCount = _profile.translations.Count;
                    reload = true;
                }

                if (_profile != null && _languagesCount != _profile.languages.Count)
                {
                    _languagesCount = _profile.languages.Count;
                    _languageVisibilityMask.Recreate(_profile.languages);
                    UpdateRowHeight();
                }

                if (reload) {
                    Reload();
                }

                OnGUI(position);
            }

            TreeViewItem[] _cache;
            
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                if (_profile == null || _profile.translations.Count <= 0)
                {
                    return new TreeViewItem[0];
                }
                else
                {
                    var cacheIndex = (_cache == null)? -1 : _cache.Length - 1;
                    var items = new List<TreeViewItem>();
                    for (int i = 0; i < _profile.translations.Count; i++)
                    {
                        var add = true;
                        var searchString = state.searchString?.Trim();

                        if (!string.IsNullOrEmpty(searchString) && _profile.translations[i]?.key != null)
                        {
                            add = _profile.translations[i].key.IndexOf(
                                searchString,
                                System.StringComparison.CurrentCultureIgnoreCase
                                ) != -1;
                        }

                        if (add)
                        {
                            TreeViewItem item;

                            if (_cache != null && cacheIndex >= 0)
                            {
                                item = _cache[cacheIndex--];
                                item.id = i;
                            } else
                            {
                                item = new TreeViewItem
                                {
                                    id = i,
                                    depth = 0
                                };
                            }

                            items.Add(item);
                        }
                    }
                    return _cache = items.ToArray();
                }
            }

            void UpdateRowHeight()
            {
                int languageCounts = _languageVisibilityMask.GetVisibileCount();

                if (languageCounts < 0)
                {
                    languageCounts = _profile.languages.Count;
                    languageCounts = Mathf.Max(languageCounts, 1);
                }

                rowHeight = EditorGUIUtility.singleLineHeight * (1 + languageCounts * 2) +
                    PaddingSmall * languageCounts +
                    PaddingBig;

                BasicLocalizationMainEditorWindow.ForceRepaint();
            }

            static readonly List<int> _sAncestors = new List<int>
            {
                -1
            };

            protected override IList<int> GetAncestors(int id)
            {
                return _sAncestors;
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                if (_profile != null)
                {
                    var rect = GetRowRect(args.row);
                    var position = new Rect(
                        rect.position.x + PaddingBig,
                        rect.position.y + PaddingSmall,
                        rect.width - 2 * PaddingBig,
                        EditorGUIUtility.singleLineHeight
                        );

                    // Draw stripes.

                    if (Event.current.type == EventType.Repaint && !args.selected)
                    {
                        var style = args.row % 2 == 0 ? BoxStyle1 : BoxStyle2;

                        style.Draw(new Rect(
                                position.x - PaddingSmall,
                                position.y - PaddingSmall,
                                position.width + PaddingSmall * 2,
                                rect.height
                                ),
                                false, false, false, false);
                    }

                    // Draw a key field.

                    var translation = _profile.translations[args.item.id];

                    EditorGUI.BeginChangeCheck();

                    var key = EditorGUI.TextField(position, translation.key);

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_profile, "Changed a translation key");
                        translation.key = key;
                        EditorUtility.SetDirty(_profile);
                    }

                    // A placeholder for the key field.

                    if (string.IsNullOrEmpty(key))
                    {
                        var oldColor = GUI.color;
                        GUI.color = Color.gray;

                        EditorGUI.LabelField(position, "Key", PlaceholderLabelStyle);

                        GUI.color = oldColor;
                    }

                    if (_profile.languages.Count > 0)
                    {
                        // Draw language fields.

                        var languageRect = new Rect(
                            position.x,
                            position.y + EditorGUIUtility.singleLineHeight + PaddingSmall,
                            position.width,
                            EditorGUIUtility.singleLineHeight * 2
                            );

                        for (int i = 0; i < _profile.languages.Count; i++)
                        {
                            var language = _profile.languages[i];
                            if (_languageVisibilityMask.IsVisible(language))
                            {
                                EditorGUI.BeginChangeCheck();

                                translation.ReservePhrase(language);

                                EditorGUI.indentLevel++;

                                var areaRect = EditorGUI.PrefixLabel(languageRect, new GUIContent(language.languageId));

                                EditorGUI.indentLevel--;

                                var text = EditorGUI.TextArea(areaRect, translation.GetTranslation(language));

                                if (EditorGUI.EndChangeCheck())
                                {
                                    Undo.RecordObject(_profile, "Changed phrases");
                                    translation.PutTranslation(language, text);
                                    EditorUtility.SetDirty(_profile);
                                }

                                languageRect.y += EditorGUIUtility.singleLineHeight * 2 + PaddingSmall;
                            }
                        }
                    } else
                    {
                        // Draw a helpbox if there are no languages.

                        var helpBoxRect = new Rect(
                            position.x,
                            position.y + EditorGUIUtility.singleLineHeight + PaddingSmall,
                            position.width,
                            EditorGUIUtility.singleLineHeight * 2
                            );

                        BasicLocalizationUIUtils.DrawHelpBox(
                            helpBoxRect,
                            "There are no available languages. Create a new one.",
                            MessageType.Info,
                            TextAnchor.MiddleLeft
                            );
                    }
                }
            }

            protected override void ContextClickedItem(int id)
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Delete"), false, OnItemDeleted);

                menu.ShowAsContext();
            }

            void OnItemDeleted()
            {
                if (_profile != null)
                {
                    _profile.Editor.RemoveTranslationAt(GetSelection());
                    Reload();
                    SetSelection(new List<int>());
                }
            }

            public void SetLanguageVisibility(BasicLocalizationLanguage language, bool value)
            {
                _languageVisibilityMask.SetVisibility(language, value);
                UpdateRowHeight();
            }

            public bool IsLanguageVisibile(BasicLocalizationLanguage language)
            {
                return _languageVisibilityMask.IsVisible(language);
            }

            public void ShowAllLanguages()
            {
                _languageVisibilityMask.ShowAll();
                UpdateRowHeight();
            }

            public void HideAllLanguages()
            {
                _languageVisibilityMask.HideAll();
                UpdateRowHeight();
            }

            class LanguageVisibilityMask
            {
                Dictionary<BasicLocalizationLanguage, bool> _mask;

                public int GetVisibileCount()
                {
                    if (_mask == null)
                        return -1;

                    var count = 0;

                    foreach (var pair in _mask)
                    {
                        if (pair.Value)
                            count++;
                    }

                    return count;
                }

                public void SetVisibility(BasicLocalizationLanguage language, bool value)
                {
                    if (_mask != null && _mask.ContainsKey(language))
                    {
                        _mask[language] = value;
                    }
                }

                public bool IsVisible(BasicLocalizationLanguage language)
                {
                    return _mask == null || (_mask.TryGetValue(language, out bool value) && value);
                }

                public void Recreate(List<BasicLocalizationLanguage> languages)
                {
                    if (languages == null || languages.Count <= 0)
                    {
                        _mask = null;
                        return;
                    }

                    var oldMask = _mask;
                    _mask = new Dictionary<BasicLocalizationLanguage, bool>();

                    foreach (var language in languages)
                    {
                        if (oldMask != null)
                        {
                            if (oldMask.TryGetValue(language, out bool value))
                            {
                                _mask.Add(language, value);
                            } else
                            {
                                _mask.Add(language, true);
                            }
                        } else
                        {
                            _mask.Add(language, true);
                        }
                    }
                }

                public void ShowAll()
                {
                    if (_mask == null)
                        return;

                    foreach (var key in new List<BasicLocalizationLanguage>(_mask.Keys))
                    {
                        _mask[key] = true;
                    }
                }

                public void HideAll()
                {
                    if (_mask == null)
                        return;

                    foreach (var key in new List<BasicLocalizationLanguage>(_mask.Keys))
                    {
                        _mask[key] = false;
                    }
                }
            }
        }

        [System.Serializable]
        class TranslationsDrawHelper
        {
            static GUIContent _sVisibilityIcon;

            TreeViewState _treeViewState;

            TranslationsTreeView _treeView;

            BasicLocalizationProfile _localizationProfile;

            TranslationsSearchField _searchField;

            Rect _rect1;

            Rect _rect2;

            Rect _languageVisibilityRect;

            public void Draw(BasicLocalizationProfile profile, bool useMinHeight)
            {
                if (_sVisibilityIcon == null)
                {
                    _sVisibilityIcon = EditorGUIUtility.IconContent("animationvisibilitytoggleon");
                }

                if (_searchField == null)
                {
                    _searchField = new TranslationsSearchField();
                }

                // Make a layout.

                EditorGUILayout.BeginVertical(
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true)
                );

                EditorGUILayout.BeginHorizontal();

                var rect1 = GUILayoutUtility.GetRect(
                        GUIContent.none,
                        GUIStyle.none,
                        _searchField.GetLayoutOptions()
                    );

                // Draw a language visibility field.

                var oldEnabled = GUI.enabled;
                GUI.enabled = _localizationProfile != null &&
                    _localizationProfile.translations != null &&
                    _localizationProfile.languages.Count > 0;

                if (EditorGUILayout.DropdownButton(_sVisibilityIcon, FocusType.Passive, GUILayout.ExpandWidth(false)))
                {
                    PopupWindow.Show(_languageVisibilityRect, new LanguageMaskPopupWindowContent(this));
                }

                GUI.enabled = oldEnabled;

                if (Event.current.type == EventType.Repaint)
                {
                    _languageVisibilityRect = GUILayoutUtility.GetLastRect();
                }

                EditorGUILayout.EndHorizontal();

                var rect2 = GUILayoutUtility.GetRect(
                        GUIContent.none,
                        GUIStyle.none,
                        GUILayout.MinHeight(useMinHeight ? 300 : 0),
                        GUILayout.ExpandHeight(true),
                        GUILayout.ExpandWidth(true)
                    );

                GUILayout.Space(PaddingSmall);

                if (GUILayout.Button("Add Translation"))
                {
                    AddNewTranslation();
                }

                GUILayout.Space(PaddingSmall);

                EditorGUILayout.EndVertical();

                // Draw a list of translations.

                if (Event.current.type == EventType.Repaint)
                {
                    _rect1 = rect1;
                    _rect2 = rect2;
                }

                if (_treeViewState == null)
                {
                    _treeViewState = new TreeViewState();
                }

                if (_localizationProfile == null ||
                    (_localizationProfile != null && !_localizationProfile.Equals(profile)) ||
                    (profile != null && _treeView == null))
                {
                    _localizationProfile = profile;
                    _treeView = new TranslationsTreeView(_treeViewState);
                }

                _searchField.Draw(_rect1);

                if (_treeView != null)
                {
                    _treeView.searchString = _searchField.search;
                    _treeView.Draw(profile, _rect2);
                }
            }

            void AddNewTranslation()
            {
                if (_localizationProfile != null) {
                    _localizationProfile.Editor.AddTranslation(string.Empty);
                    _treeView.Reload();

                    if (_localizationProfile.translations.Count > 0)
                    {
                        var id = _localizationProfile.translations.Count - 1;
                        _treeView.FrameItem(id);
                        _treeView.SetSelection(new List<int>
                        {
                            id
                        });
                    }
                }
            }

            class LanguageMaskPopupWindowContent : PopupWindowContent
            {
                TranslationsDrawHelper _parent;

                public LanguageMaskPopupWindowContent(TranslationsDrawHelper parent)
                {
                    _parent = parent;
                }

                public override Vector2 GetWindowSize()
                {
                    return new Vector2(
                        WindowWidth + PaddingBig * 2,
                        _parent._localizationProfile.languages.Count * EditorGUIUtility.singleLineHeight +
                        EditorGUIUtility.singleLineHeight * 2 +
                        PaddingBig * 2 +
                        PaddingSmall
                        );
                }

                const float WindowWidth = 180;

                const float ToggleWidth = 16;

                public override void OnGUI(Rect position)
                {
                    var rect = new Rect(
                        position.x + PaddingBig,
                        position.y + PaddingBig,
                        position.width - PaddingBig * 2,
                        EditorGUIUtility.singleLineHeight
                        );

                    for (int i = 0; i < _parent._localizationProfile.languages.Count; i++)
                    {
                        var language = _parent._localizationProfile.languages[i];
                        EditorGUI.LabelField(rect, language.LanguageId);

                        EditorGUI.BeginChangeCheck();

                        var toggleValue = EditorGUI.Toggle(new Rect(
                            rect.x + rect.width - ToggleWidth,
                            rect.y,
                            ToggleWidth,
                            EditorGUIUtility.singleLineHeight
                            ),
                            _parent._treeView.IsLanguageVisibile(language));

                        if (EditorGUI.EndChangeCheck())
                        {
                            _parent._treeView.SetLanguageVisibility(language, toggleValue);
                        }

                        rect.y += EditorGUIUtility.singleLineHeight;
                    }

                    rect.y += PaddingSmall;

                    if (GUI.Button(rect, "Show All"))
                    {
                        _parent._treeView.ShowAllLanguages();
                    }

                    rect.y += EditorGUIUtility.singleLineHeight;

                    if (GUI.Button(rect, "Hide All"))
                    {
                        _parent._treeView.HideAllLanguages();
                    }
                }
            }
        }

        [System.Serializable]
        class LanguageSelectorsDrawHelper
        {
            ReorderableList _list;

            BasicLocalizationProfile _profile;

            SerializedObject _serializedObject;

            public void Draw(BasicLocalizationProfile profile)
            {
                _profile = profile;

                if (_list == null)
                {
                    _list = new ReorderableList(null, typeof(BaseLanguageSelector), true, true, true, true);
                    _list.drawHeaderCallback = DrawHeader;
                    _list.onRemoveCallback = OnRemove;
                    _list.onAddCallback = OnAdd;
                    _list.drawElementCallback = DrawElement;
                    _list.elementHeightCallback = ElementHeight;
                    _list.onReorderCallback = OnReorder;
                }

                if (_serializedObject == null || _serializedObject.targetObject != _profile)
                {
                    _serializedObject = new SerializedObject(_profile);
                    _list.list = _profile.languageSelectors;
                }
                
                _list.DoLayoutList();
            }

            void OnReorder(ReorderableList list)
            {
                EditorUtility.SetDirty(_profile);
            }

            float ElementHeight(int index)
            {
                var element = _list.list[index] as IReordableListElement;
                if (element != null)
                {
                    element.CurrentProfile = _profile;
                    var height = element.GetElementHeight();
                    element.CurrentProfile = null;
                    return height;
                }

                return 0;
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                var element = _list.list[index] as IReordableListElement;
                if (element != null)
                {
                    element.CurrentProfile = _profile;
                    element.DrawElement(rect, isActive, isFocused);
                    element.CurrentProfile = null;
                }
            }

            void OnAdd(ReorderableList list)
            {
                Undo.RegisterCompleteObjectUndo(_profile, "Add Element In Array");

                var menu = new GenericMenu();
                var index = list.index;

                if (index == -1)
                {
                    index = list.list.Count;
                }

                Type last = null;
                var foundTypes = TypeCache.GetTypesDerivedFrom(typeof(BaseLanguageSelector));
                for (int i = 0; i < foundTypes.Count; ++i)
                {
                    var type = foundTypes[i];

                    if (type.IsAbstract || type.IsGenericType)
                        continue;

                    // Ignore Unity types as they can not be managed references.
                    if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                        continue;

                    last = type;

                    var name = ManagedReferenceUtils.GetDisplayName(type);
                    menu.AddItem(name, false, () =>
                    {
                        AddManagedItem(type, index);
                    });
                }

                if (menu.GetItemCount() == 1)
                {
                    AddManagedItem(last, index);
                }
                else
                {
                    menu.ShowAsContext();
                }
            }

            protected void AddManagedItem(Type type, int index)
            {
                using (var serializableObject = new SerializedObject(_profile))
                {
                    var listProperty = serializableObject.FindProperty("languageSelectors");
                    var instance = Activator.CreateInstance(type);
                    listProperty.InsertArrayElementAtIndex(index);
                    var element = listProperty.GetArrayElementAtIndex(index);

                    element.managedReferenceValue = instance;
                    listProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            void OnRemove(ReorderableList list)
            {
                using (var serializableObject = new SerializedObject(_profile))
                {
                    var listProperty = serializableObject.FindProperty("languageSelectors");

                    Undo.RegisterCompleteObjectUndo(_profile, "Remove Element From Array");

                    listProperty.DeleteArrayElementAtIndex(list.index);
                    listProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            void DrawHeader(Rect rect)
            {
                EditorGUI.LabelField(rect, "Language Selectors");
            }
        }

        [System.Serializable]
        class AvailableLanguagesDrawHelper
        {
            ReorderableList _list;

            BasicLocalizationProfile _profile;

            public void Draw(BasicLocalizationProfile profile)
            {
                _profile = profile;
                var languages = _profile.languages;

                if (_list == null)
                {
                    _list = new ReorderableList(languages, typeof(BasicLocalizationLanguage), true, true, true, true);

                    _list.drawHeaderCallback = DrawHeader;
                    _list.onRemoveCallback = OnRemove;
                    _list.onAddCallback = OnAdd;
                    _list.drawElementCallback = DrawElement;
                    _list.onReorderCallback = OnReorder;
                }

                _list.list = languages;
                _list.DoLayoutList();
            }

            void OnReorder(ReorderableList list)
            {
                EditorUtility.SetDirty(_profile);
            }

            void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
            {
                var language = _profile.languages[index];

                EditorGUI.BeginChangeCheck();

                var newLanguage = EditorGUI.TextField(new Rect(
                    rect.x,
                    rect.y,
                    EditorGUIUtility.labelWidth,
                    rect.height
                    ),
                    language.languageId
                    );

                var newLocale = (BasicLocalizationLocale)EditorGUI.ObjectField(
                    new Rect(
                        rect.x + EditorGUIUtility.labelWidth,
                        rect.y,
                        rect.width - EditorGUIUtility.labelWidth,
                        rect.height
                        ),
                    GUIContent.none,
                    language.locale,
                    typeof(BasicLocalizationLocale),
                    false
                    );

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(_profile, "Changed Languages Info");

                    language.languageId = newLanguage;
                    language.locale = newLocale;

                    EditorUtility.SetDirty(_profile);
                }
            }

            void OnAdd(ReorderableList list)
            {               
                _profile.Editor.AddLanguage("New Language");
            }

            void OnRemove(ReorderableList rl)
            {
                _profile.Editor.RemoveLanguageAt(rl.index);
            }

            void DrawHeader(Rect rect)
            {
                EditorGUI.LabelField(rect, "Available Languages");
            }
        }

    }
}
