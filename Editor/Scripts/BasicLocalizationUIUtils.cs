using UnityEditor;
using UnityEngine;

namespace m039.BasicLocalization
{
    public static class BasicLocalizationUIUtils
    {
        public static readonly float PaddingSmall = 4;

        public static readonly float PaddingBig = 8;

        public static Color InspectorBackgroundColor => EditorGUIUtility.isProSkin ?
                    new Color32(56, 56, 56, 255) : new Color32(194, 194, 194, 255);

        public static void DrawHelpBox(Rect position, string message, MessageType type, TextAnchor aligment = TextAnchor.MiddleLeft)
        {
            var helpboxStyle = GUI.skin.GetStyle("helpbox");
            var previousAligment = helpboxStyle.alignment;
            helpboxStyle.alignment = aligment;

            EditorGUI.HelpBox(position, message, type);

            helpboxStyle.alignment = previousAligment;
        }
    }
}
