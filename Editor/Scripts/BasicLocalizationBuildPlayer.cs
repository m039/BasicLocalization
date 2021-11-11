using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace m039.BasicLocalization
{

    public class BasicLocalizationBuildPlayer : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        public int callbackOrder => 1;

        BasicLocalizationProfile _profile;

        bool _removeFromPreloadedAssets;

        public void OnPreprocessBuild(BuildReport report)
        {
            _removeFromPreloadedAssets = false;
            _profile = BasicLocalizationEditorSettings.ActiveLocalizationProfile;
            if (_profile == null)
                return;

            _profile.Editor.SetMainAssetFlag();

            // Add the localization settings to the preloaded assets.
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            bool wasDirty = IsPlayerSettingsDirty();

            if (!preloadedAssets.Contains(_profile))
            {
                ArrayUtility.Add(ref preloadedAssets, _profile);
                PlayerSettings.SetPreloadedAssets(preloadedAssets);

                // If we have to add the settings then we should also remove them.
                _removeFromPreloadedAssets = true;

                // Clear the dirty flag so we dont flush the modified file (case 1254502)
                if (!wasDirty)
                    ClearPlayerSettingsDirtyFlag();
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            if (_profile == null || !_removeFromPreloadedAssets)
                return;

            _profile.Editor.ClearMainAssetFlag();

            bool wasDirty = IsPlayerSettingsDirty();

            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            ArrayUtility.Remove(ref preloadedAssets, _profile);
            PlayerSettings.SetPreloadedAssets(preloadedAssets);

            _profile = null;

            // Clear the dirty flag so we dont flush the modified file (case 1254502)
            if (!wasDirty)
                ClearPlayerSettingsDirtyFlag();
        }

        static bool IsPlayerSettingsDirty()
        {
            var settings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            if (settings != null && settings.Length > 0)
                return EditorUtility.IsDirty(settings[0]);
            return false;
        }

        static void ClearPlayerSettingsDirtyFlag()
        {
            var settings = Resources.FindObjectsOfTypeAll<PlayerSettings>();
            if (settings != null && settings.Length > 0)
                EditorUtility.ClearDirty(settings[0]);
        }
    }

}
