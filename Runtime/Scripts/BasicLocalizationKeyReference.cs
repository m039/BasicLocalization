using UnityEngine;

namespace m039.BasicLocalization
{
    /// <summary>
    /// This class has all values needed to identify a translation.
    /// 
    /// Use it in your custom localized <see cref="MonoBehaviour"/> as in <see cref="LocalizedTextMeshPro"/>.
    /// </summary>
    [System.Serializable]
    public struct BasicLocalizationKeyReference
    {
        /// <summary>
        /// The value of this variable is used to find a translation.
        /// </summary>
        public string key;

        /// <summary>
        /// All translation has a guied along to the key. This variable helps to find a
        /// translation when the key is modified.
        /// </summary>
        [SerializeField]
        internal string guid;
    }
}
