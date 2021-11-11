using UnityEditor;

using UnityEngine;

namespace m039.BasicLocalization
{
    [CustomPropertyDrawer(typeof(BasicLocalizationKeyReference))]
    public class BasicLocalizationKeyReferencePropertyDrawer : PropertyDrawer
    {
        const float Padding = 5f;

        const int MaxSuggestCount = 100;

        const float SuggestButtonWidth = 44f;

        static readonly Color RedColor = new Color32(0xEB, 0x5A, 0x46, 0xff);

        static string[] _sLanguages;

        bool _loseFocusFromTextField;

        int _selectedLanguage = -1;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight + (property.isExpanded? GetHelpboxHeight() : 0);
        }

        static float GetHelpboxHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2 + Padding * 2;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_loseFocusFromTextField)
            {
                EditorGUI.FocusTextInControl(null);
                _loseFocusFromTextField = false;
            }

            EditorGUI.BeginProperty(position, label, property);

            var guidProperty = property.FindPropertyRelative("guid");
            var keyProperty = property.FindPropertyRelative("key");

            var rect1 = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var rect2 = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, GetHelpboxHeight());

            var profile = BasicLocalization.GetCurrentProfile();
            BasicLocalizationTranslation translation;
            var dontChangeValues = false;

            if (!string.IsNullOrEmpty(guidProperty.stringValue))
            {
                translation = profile?.GetTranslationRawByGuid(guidProperty.stringValue);
                if (translation == null)
                {
                    translation = profile?.GetTranslationRawByKey(keyProperty.stringValue);
                    if (translation == null)
                    {
                        dontChangeValues = true; // The profile has not a guid or a specified key.
                    } else
                    {
                        guidProperty.stringValue = keyProperty.stringValue;
                    }
                } else
                {
                    keyProperty.stringValue = translation.key;
                }
            }
            else
            {
                translation = profile?.GetTranslationRawByKey(keyProperty.stringValue);
                guidProperty.stringValue = translation?.guid;
            }

            var showPreview = true;

            if (translation == null)
            {
                if (!dontChangeValues)
                {
                    guidProperty.stringValue = null;
                }
                showPreview = false;
            }

            // Draw a foldout.

            var foldoutRect = new Rect(rect1.x, rect1.y, EditorGUIUtility.labelWidth, rect1.height);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            // Draw a text field for the key.

            var oldColor = GUI.color;
            if (translation == null)
            {
                GUI.color = RedColor;
            }

            EditorGUI.BeginChangeCheck();

            var editRect = new Rect(
                rect1.x + EditorGUIUtility.labelWidth,
                rect1.y,
                rect1.width - EditorGUIUtility.labelWidth - SuggestButtonWidth,
                rect1.height
                );

            keyProperty.stringValue = EditorGUI.TextField(editRect, GUIContent.none, keyProperty.stringValue);

            // Reset a guid if there was a change.
            if (EditorGUI.EndChangeCheck())
            {
                guidProperty.stringValue = null;
            }

            GUI.color = oldColor;

            DrawSuggest(new Rect(
                rect1.x + rect1.width - SuggestButtonWidth,
                rect1.y,
                SuggestButtonWidth,
                rect1.height
                ),
                profile,
                keyProperty,
                guidProperty);

            // Draw inner items.

            if (property.isExpanded)
            {
                // Draw the preview.
                if (showPreview)
                {
                    DrawTranslation(rect2, profile, translation);
                }
                else
                {
                    string msg;

                    if (profile == null)
                    {
                        msg = "Can't find a valid localization profile. Select or create one.";
                    } else
                    {
                        msg = "Can't find a translation for the key.";
                    }

                    BasicLocalizationUIUtils.DrawHelpBox(rect2, msg, MessageType.Warning, TextAnchor.MiddleLeft);
                }
            }

            EditorGUI.EndProperty();
        }

        void DrawSuggest(Rect position, BasicLocalizationProfile profile, SerializedProperty keyProperty, SerializedProperty guidProperty)
        {
            if (GUI.Button(position, "List..."))
            {
                var menu = new GenericMenu();

                if (profile == null)
                {
                    menu.AddDisabledItem(new GUIContent("There is no active language profile."));
                } else
                {
                    if (profile.translations != null)
                    {
                        var keyValue = keyProperty.stringValue;
                        var count = 0;

                        foreach (var translation in profile.translations)
                        {
                            var add = true;

                            if (!string.IsNullOrEmpty(keyValue) && translation.key != null)
                            {
                                add = translation.key.IndexOf(keyValue, System.StringComparison.CurrentCultureIgnoreCase) != -1;
                            }

                            if (add)
                            {
                                menu.AddItem(new GUIContent(translation.key), translation.key == keyValue, () =>
                                {
                                    keyProperty.stringValue = translation.key;
                                    guidProperty.stringValue = null; // Just in case.
                                    keyProperty.serializedObject.ApplyModifiedProperties();
                                    _loseFocusFromTextField = true; // Otherwise, it won't change UI immediately.
                                });

                                count++;
                                if (count >= MaxSuggestCount)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                menu.ShowAsContext();
            }
        }

        void DrawTranslation(Rect position, BasicLocalizationProfile profile, BasicLocalizationTranslation translation)
        {
            if (translation == null ||
                translation.phrases == null ||
                translation.phrases.Count <= 0 ||
                profile == null)
                return;

            // Cache languages.

            if (_sLanguages == null || _sLanguages.Length != translation.phrases.Count)
            {
                _sLanguages = new string[translation.phrases.Count];
            }

            for (int i = 0; i < translation.phrases.Count; i++)
            {
                _sLanguages[i] = translation.phrases[i].language?.LanguageId;
            }

            // Select default language.

            if (_selectedLanguage == -1 || _selectedLanguage >= translation.phrases.Count)
            {
                if (profile.defaultLanguage == null)
                {
                    _selectedLanguage = 0;
                }
                else
                {
                    _selectedLanguage = System.Array.IndexOf(_sLanguages, profile.defaultLanguage.LanguageId);

                    if (_selectedLanguage == -1)
                    {
                        _selectedLanguage = 0;
                    }
                }
            }

            // Draw controls.

            GUI.Box(position, GUIContent.none);

            var popupRect = new Rect(
                position.x + Padding,
                position.y + Padding,
                EditorGUIUtility.labelWidth - Padding * 2,
                EditorGUIUtility.singleLineHeight
                );
            var editRect = new Rect(
                Mathf.Ceil(position.x + EditorGUIUtility.labelWidth),
                position.y + Padding,
                position.width - EditorGUIUtility.labelWidth - Padding,
                position.height - Padding * 2
                );

            // Draw a popup.

            _selectedLanguage = EditorGUI.Popup(popupRect, _selectedLanguage, _sLanguages);

            // Draw an empty frame for the translation.

            var oldEnabled = GUI.enabled;
            GUI.enabled = false;

            EditorGUI.TextArea(editRect, string.Empty);

            GUI.enabled = oldEnabled;

            // Draw translation text.

            var style = EditorStyles.label;
            var oldAlignment = style.alignment;
            style.alignment = TextAnchor.UpperLeft;

            EditorGUI.SelectableLabel(editRect, translation.phrases[_selectedLanguage].text);

            style.alignment = oldAlignment;
        }
    }
}
