using UnityEditor;

namespace m039.BasicLocalization
{
    [CustomEditor(typeof(BasicLocalizationLanguage))]
    public class BasicLocalizationLanguageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var t = target as BasicLocalizationLanguage;

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck() && t != null)
            {
                Undo.RecordObject(t, "Name Changed");
                t.name = t.LanguageId;
                AssetDatabase.SaveAssets();
            }
        }
    }
}
