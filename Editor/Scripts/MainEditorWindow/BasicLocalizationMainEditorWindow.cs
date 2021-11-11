using UnityEngine;
using UnityEditor;

using static m039.BasicLocalization.BasicLocalizationUIUtils;

namespace m039.BasicLocalization
{
    public class BasicLocalizationMainEditorWindow : EditorWindow
    {
        BasicLocalizationProfileEditorController _editorController;

        [System.NonSerialized]
        EmptyProfileScreen _emptyProfileScreen = new EmptyProfileScreen();

        [System.NonSerialized]
        Rect _emptyProfileScreenRect;

        [MenuItem("Window/Basic Localization")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(BasicLocalizationMainEditorWindow), false, "Basic Localization");
        }

        void OnGUI()
        {
            var profile = BasicLocalizationEditorSettings.ActiveLocalizationProfile;

            if (profile == null)
            {
                DrawEmptyScreen();
            } else {
                DrawProfilePicker();

                if (_editorController == null || _editorController.profile != profile)
                {
                    _editorController = new BasicLocalizationProfileEditorController(profile, false);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(PaddingSmall);

                _editorController.Draw();

                GUILayout.Space(PaddingSmall);
                GUILayout.EndHorizontal();
            }
        }

        public static void ForceRepaint() {
            if (HasOpenInstances<BasicLocalizationMainEditorWindow>())
            {
                var window = GetWindow<BasicLocalizationMainEditorWindow>(false, null, false);
                window.Repaint();
            }
        }

        void DrawEmptyScreen()
        {
            // Draw an empty screen in the center.

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            var size = _emptyProfileScreen.GetSize();
            var minSize = _emptyProfileScreen.GetMinSize();

            var rect = GUILayoutUtility.GetRect(
                0,
                0,
                GUILayout.MaxWidth(size.x),
                GUILayout.MaxHeight(size.y),
                GUILayout.MinWidth(minSize.x),
                GUILayout.MinHeight(minSize.y)
                );
            if (Event.current.type == EventType.Repaint)
            {
                _emptyProfileScreenRect = rect;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            _emptyProfileScreen.OnGUI(_emptyProfileScreenRect);
        }

        void DrawProfilePicker()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(PaddingSmall);
            GUILayout.BeginHorizontal();
            GUILayout.Space(PaddingSmall);

            BasicLocalizationEditorSettings.ActiveLocalizationProfile = (BasicLocalizationProfile)EditorGUILayout.ObjectField(
                "Default Profile",
                BasicLocalizationEditorSettings.ActiveLocalizationProfile,
                typeof(BasicLocalizationProfile),
                false,
                GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Create", GUILayout.ExpandWidth(false)))
            {
                var ls = CreateLocalizationSettingsAsset();
                if (ls != null)
                {
                    BasicLocalizationEditorSettings.ActiveLocalizationProfile = ls;
                }
            }

            GUILayout.Space(PaddingSmall);
            GUILayout.EndHorizontal();
            GUILayout.Space(PaddingBig);
            GUILayout.EndVertical();
        }

        public static BasicLocalizationProfile CreateLocalizationSettingsAsset()
        {
            var path = EditorUtility.SaveFilePanelInProject("Save Localization Profile", "BasicLocalizationProfile", "asset", "Please enter a filename to save the project's localization profile to.");

            if (string.IsNullOrEmpty(path))
                return null;

            var settings = BasicLocalizationProfile.Create("Default Localization Settings");         

            AssetDatabase.CreateAsset(settings, path);
            AssetDatabase.SaveAssets();

            return settings;
        }

        public class EmptyProfileScreen
        {
            public Vector2 GetMinSize()
            {
                var size = GetSize();
                size.x = 240;
                return size;
            }

            public Vector2 GetSize()
            {
                return new Vector2(320,
                    EditorGUIUtility.singleLineHeight * 3 + // HelpBox
                    EditorGUIUtility.singleLineHeight + // ObjectField
                    EditorGUIUtility.singleLineHeight + // Create Button
                    PaddingBig * 3 +
                    PaddingSmall
                    );
            }

            public void OnGUI(Rect rect)
            {
                rect.x += PaddingBig;
                rect.width -= PaddingBig * 2;
                rect.y += PaddingBig;

                var buttonWidth = rect.width - PaddingBig * 2;

                var helpBoxRect = new Rect(
                    rect.x,
                    rect.y,
                    rect.width,
                    EditorGUIUtility.singleLineHeight * 3
                    );
                var objectFieldRect = new Rect(
                    rect.x + rect.width / 2 - buttonWidth / 2,
                    rect.y + helpBoxRect.height + PaddingBig,
                    buttonWidth,
                    EditorGUIUtility.singleLineHeight
                    );
                var createButtonRect = new Rect(
                    rect.x + rect.width / 2 - buttonWidth / 2,
                    objectFieldRect.y + objectFieldRect.height + PaddingSmall,
                    buttonWidth,
                    EditorGUIUtility.singleLineHeight
                    );

                // Draw a helpbox.

                BasicLocalizationUIUtils.DrawHelpBox(
                    helpBoxRect,
                    "Select a localization profile\nor\ncreate a new one.",
                    MessageType.Info,
                    TextAnchor.MiddleCenter
                    );

                // Draw a object field.

                BasicLocalizationEditorSettings.ActiveLocalizationProfile = (BasicLocalizationProfile) EditorGUI.ObjectField(
                    objectFieldRect,
                    GUIContent.none,
                    BasicLocalizationEditorSettings.ActiveLocalizationProfile,
                    typeof(BasicLocalizationProfile),
                    false
                    );

                // Draw a create button.

                if (GUI.Button(createButtonRect, "Create"))
                {
                    var ls = CreateLocalizationSettingsAsset();
                    if (ls != null)
                    {
                        BasicLocalizationEditorSettings.ActiveLocalizationProfile = ls;
                    }
                }
            }
        }
    }
}
