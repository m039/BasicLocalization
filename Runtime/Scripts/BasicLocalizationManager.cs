using UnityEngine;

namespace m039.BasicLocalization
{
    public class BasicLocalizationManager : MonoBehaviour
    {
        #region Inspector

        [Tooltip("Select a localization profile specific for the current scene")]
        [SerializeField]
        BasicLocalizationProfile _OverrideProfile;

        #endregion

        public BasicLocalizationProfile Profile => _OverrideProfile;

        void Awake()
        {
            BasicLocalization.OverrideCurrentProfile(_OverrideProfile);
        }

        void OnDestroy()
        {
            BasicLocalization.OverrideCurrentProfile(null);
        }
    }
}
