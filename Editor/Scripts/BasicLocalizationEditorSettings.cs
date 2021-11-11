using UnityEditor;

namespace m039.BasicLocalization
{
    public static class BasicLocalizationEditorSettings
    {
        public static BasicLocalizationProfile ActiveLocalizationProfile
        {
            get
            {
                return BasicLocalizationProfile.GetProfileFromConfig();
            }
            set
            {
                if (value == null)
                {
                    EditorBuildSettings.RemoveConfigObject(BasicLocalizationProfile.ConfigName);
                }
                else
                {
                    EditorBuildSettings.AddConfigObject(BasicLocalizationProfile.ConfigName, value, true);
                }
            }
        }
    }
}
