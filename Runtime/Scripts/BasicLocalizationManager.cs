using UnityEngine;

namespace m039.BasicLocalization
{
    /// <summary>
    /// Mostly, it is a demonstration of how the current localizalization profile can be overridden.
    /// </summary>
    public class BasicLocalizationManager : MonoBehaviour
    {
        #region Inspector

        [Tooltip("Selects a localization profile specific for the current scene")]
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
