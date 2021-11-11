using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace m039.BasicLocalization
{
    [CustomEditor(typeof(BasicLocalizationProfile))]
    public class BasicLocalizationProfileEditor : Editor
    {
        BasicLocalizationProfileEditorController _editorController;

        BasicLocalizationProfileEditorController GetEditorController()
        {
            var profile = target as BasicLocalizationProfile;
            if (profile == null) {
                _editorController = null;
            } else
            {
                if (_editorController == null || _editorController.profile != profile)
                {
                    _editorController = new BasicLocalizationProfileEditorController(profile, true);
                }
            }

            return _editorController;
        }

        public override void OnInspectorGUI()
        {
            // Load the real class values into the serialized copy
            serializedObject.Update();

            var editorController = GetEditorController();
            if (editorController != null)
            {
                editorController.Draw();
            }
                    
            serializedObject.ApplyModifiedProperties();
        }
    }
}
