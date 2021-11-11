using UnityEditor;
using UnityEngine;

namespace m039.BasicLocalization
{
    public class BasicLocalizationAddLanguageEditorWindow : EditorWindow
    {
        public static void ShowWindow(BasicLocalizationProfile profile)
        {
            var window = EditorWindow.GetWindow<BasicLocalizationAddLanguageEditorWindow>(true, "Add Language");
            window._profile = profile;
            window.minSize = new Vector2(350, 100);
            window.maxSize = window.minSize;
        }

        string _language;

        BasicLocalizationProfile _profile;

        void OnEnable()
        {
            _language = string.Empty;
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();

            if (_profile == null)
            {
                EditorGUILayout.LabelField("No settings are selected");
            } else {

                _language = EditorGUILayout.TextField("Language", _language);

                EditorGUILayout.Space();

                if (GUILayout.Button("Add Language"))
                {
                    _profile.Editor.AddLanguage(_language);
                    
                    BasicLocalizationMainEditorWindow.ForceRepaint();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
        }
    }
}
