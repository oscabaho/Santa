using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Santa.Core;

namespace Santa.Infrastructure.UI
{
    /// <summary>
    /// Configures the EventSystem and UIInputModule at runtime to ensure UI input works correctly
    /// on both Editor and Mobile builds. This is critical for mobile input handling.
    /// 
    /// This script:
    /// - Creates EventSystem if it doesn't exist
    /// - Replaces legacy StandaloneInputModule with InputSystemUIInputModule (New Input System)
    /// - Ensures touch input is properly configured on mobile devices
    /// - Validates that all UI components are properly set up
    /// 
    /// Should be placed on a persistent GameObject (e.g., BootstrapManager or similar).
    /// </summary>
    [RequireComponent(typeof(EventSystem))]
    public class EventSystemConfigurator : MonoBehaviour
    {
        private EventSystem _eventSystem;
        private InputSystemUIInputModule _uiInputModule;

        private void Awake()
        {
            // Ensure this only runs once globally
            if (FindObjectsByType<EventSystemConfigurator>(FindObjectsSortMode.None).Length > 1)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("EventSystemConfigurator: Multiple instances detected. Destroying duplicate.", this);
#endif
                Destroy(gameObject);
                return;
            }

            InitializeEventSystem();
            ValidateConfiguration();
        }

        /// <summary>
        /// Initializes the EventSystem with proper input module configuration.
        /// </summary>
        private void InitializeEventSystem()
        {
            _eventSystem = GetComponent<EventSystem>();

            if (_eventSystem == null)
            {
                GameLog.LogError("EventSystemConfigurator: EventSystem component missing!", this);
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("EventSystemConfigurator: EventSystem found. Configuring input module...", this);
#endif

            // Remove legacy StandaloneInputModule if present (Old Input System)
            var legacyModule = GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("EventSystemConfigurator: Removing legacy StandaloneInputModule (Old Input System).", this);
#endif
                Destroy(legacyModule);
            }

            // Ensure InputSystemUIInputModule is present (New Input System)
            _uiInputModule = GetComponent<InputSystemUIInputModule>();
            if (_uiInputModule == null)
            {
                _uiInputModule = gameObject.AddComponent<InputSystemUIInputModule>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("EventSystemConfigurator: Added InputSystemUIInputModule (New Input System).", this);
#else
                GameLog.Log("EventSystemConfigurator: Added InputSystemUIInputModule for mobile input handling.");
#endif
            }

            // Verify the input module is set as current
            if (_eventSystem.currentInputModule != _uiInputModule)
            {
                _eventSystem.SetSelectedGameObject(null); // Clear selection to force module reassignment
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("EventSystemConfigurator: Reset EventSystem selection to force input module update.", this);
#endif
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"EventSystemConfigurator: EventSystem configured successfully. InputModule={_uiInputModule.GetType().Name}", this);
#else
            GameLog.Log("EventSystemConfigurator: EventSystem configured for mobile input.");
#endif
        }

        /// <summary>
        /// Validates that UI components are properly configured for input handling.
        /// </summary>
        private void ValidateConfiguration()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Check if there's a Canvas in the scene
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameLog.LogWarning("EventSystemConfigurator: No Canvas found in scene. UI may not render or respond to input.", this);
                return;
            }

            // Verify Canvas has GraphicRaycaster
            var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
            if (raycaster == null)
            {
                GameLog.LogWarning("EventSystemConfigurator: Canvas missing GraphicRaycaster. Adding one now.", this);
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }

            // Check Camera.main for ScreenSpaceCamera Canvas
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    var mainCam = UnityEngine.Camera.main;
                    if (mainCam != null)
                    {
                        canvas.worldCamera = mainCam;
                        GameLog.LogVerbose("EventSystemConfigurator: Assigned Camera.main to Canvas.worldCamera.", this);
                    }
                    else
                    {
                        GameLog.LogWarning("EventSystemConfigurator: Canvas is ScreenSpaceCamera but Camera.main is null!", this);
                    }
                }
            }

            // Warn about blocking CanvasGroups
            var allCanvasGroups = FindObjectsByType<CanvasGroup>(FindObjectsSortMode.None);
            foreach (var cg in allCanvasGroups)
            {
                if (!cg.interactable || !cg.blocksRaycasts)
                {
                    if (!cg.interactable)
                    {
                        GameLog.LogWarning($"EventSystemConfigurator: CanvasGroup '{cg.gameObject.name}' has interactable=false. UI clicks will be blocked.", this);
                    }
                    if (!cg.blocksRaycasts)
                    {
                        GameLog.LogWarning($"EventSystemConfigurator: CanvasGroup '{cg.gameObject.name}' has blocksRaycasts=false. May prevent proper raycasting.", this);
                    }
                }
            }
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("EventSystemConfigurator: Enabled and ready for input processing.", this);
#endif
        }
    }
}
