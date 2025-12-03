using UnityEngine;

namespace Santa.UI
{
    /// <summary>
    /// Component to show or hide UI elements based on the current platform.
    /// Useful for displaying platform-specific controls or instructions.
    /// </summary>
    public class PlatformSpecificUI : MonoBehaviour
    {
        [SerializeField] private bool showOnMobile = true;
        [SerializeField] private bool showOnStandalone = true;
        [SerializeField] private bool showOnEditor = true;

        private void Start()
        {
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            bool shouldShow = false;

#if UNITY_EDITOR
            shouldShow = showOnEditor;
#elif UNITY_ANDROID || UNITY_IOS
            shouldShow = showOnMobile;
#elif UNITY_STANDALONE
            shouldShow = showOnStandalone;
#endif

            gameObject.SetActive(shouldShow);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateVisibility();
            }
        }
#endif
    }
}
