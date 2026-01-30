using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Santa.Core;
using Santa.Infrastructure.Input;

namespace Santa.Infrastructure.Bootstrap
{
    /// <summary>
    /// Ensures critical systems initialize in the correct order for mobile compatibility:
    /// 1. EventSystem (UISystemConfigurator)
    /// 2. InputReader
    /// 3. Other gameplay systems
    /// 
    /// Place this script on a GameObject that exists in the first scene (before gameplay).
    /// This guarantees that mobile input will work correctly.
    /// </summary>
    [DefaultExecutionOrder(-1000)] // Run VERY early, before other systems
    public class BootstrapInitializer : MonoBehaviour
    {
        private static bool _initialized = false;

        private void Awake()
        {
            // Only initialize once globally
            if (_initialized)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("BootstrapInitializer: Already initialized. Skipping.", this);
#endif
                return;
            }

            _initialized = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("BootstrapInitializer: Starting critical system initialization...", this);
#else
            GameLog.Log("BootstrapInitializer: Initializing mobile input systems");
#endif

            InitializeEventSystem();
            InitializeInputReader();
            ValidateCriticalSystems();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("BootstrapInitializer: Bootstrap complete. All systems ready.", this);
#else
            GameLog.Log("BootstrapInitializer: Bootstrap complete");
#endif
        }

        /// <summary>
        /// Ensures EventSystem is configured with proper input module for New Input System.
        /// </summary>
        private void InitializeEventSystem()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("BootstrapInitializer: Initializing EventSystem...", this);
#endif

            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameLog.LogError("BootstrapInitializer: EventSystem NOT FOUND! Creating one now.", this);
                var eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            }

            // Check if EventSystemConfigurator exists
            var configurator = FindFirstObjectByType<Santa.Infrastructure.UI.EventSystemConfigurator>();
            if (configurator == null)
            {
                GameLog.LogError("BootstrapInitializer: EventSystemConfigurator NOT FOUND! Creating one now.", this);
                var configuratorGO = new GameObject("EventSystemConfigurator");
                configuratorGO.AddComponent<Santa.Infrastructure.UI.EventSystemConfigurator>();
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("BootstrapInitializer: EventSystem initialization complete.", this);
#endif
        }

        /// <summary>
        /// Ensures InputReader is properly loaded and both action maps are enabled.
        /// </summary>
        private void InitializeInputReader()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("BootstrapInitializer: Initializing InputReader...", this);
#endif

            var readers = Resources.FindObjectsOfTypeAll<InputReader>();
            if (readers == null || readers.Length == 0)
            {
                GameLog.LogError("BootstrapInitializer: CRITICAL - InputReader NOT FOUND in Resources! Mobile combat WILL NOT WORK.", this);
                return;
            }

            var inputReader = readers[0];
            if (inputReader == null)
            {
                GameLog.LogError("BootstrapInitializer: InputReader is NULL!", this);
                return;
            }

            // Force enable to ensure action maps are initialized
            inputReader.EnableGameplayInput();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"BootstrapInitializer: InputReader '{inputReader.name}' initialized and enabled.", this);
#else
            GameLog.Log($"BootstrapInitializer: InputReader initialized - mobile input ready");
#endif
        }

        /// <summary>
        /// Validates that all critical systems are in place.
        /// </summary>
        private void ValidateCriticalSystems()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("BootstrapInitializer: Validating critical systems...", this);

            var errors = 0;

            // Check EventSystem
            var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameLog.LogError("BootstrapInitializer: Validation FAILED - EventSystem missing!", this);
                errors++;
            }
            else
            {
                GameLog.LogVerbose("BootstrapInitializer: ✓ EventSystem found", this);
            }

            // Check InputReader
            var inputReaders = Resources.FindObjectsOfTypeAll<InputReader>();
            if (inputReaders == null || inputReaders.Length == 0)
            {
                GameLog.LogError("BootstrapInitializer: Validation FAILED - InputReader missing!", this);
                errors++;
            }
            else
            {
                GameLog.LogVerbose("BootstrapInitializer: ✓ InputReader found", this);
            }

            // Check Canvas
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameLog.LogWarning("BootstrapInitializer: No Canvas found. UI will not render.", this);
            }
            else
            {
                GameLog.LogVerbose("BootstrapInitializer: ✓ Canvas found", this);

                // Check GraphicRaycaster
                var raycaster = canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>();
                if (raycaster == null)
                {
                    GameLog.LogWarning("BootstrapInitializer: Canvas missing GraphicRaycaster. Adding one.", this);
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
                else
                {
                    GameLog.LogVerbose("BootstrapInitializer: ✓ GraphicRaycaster found", this);
                }
            }

            // Check Camera.main for mobile
            if (UnityEngine.Camera.main == null)
            {
                GameLog.LogWarning("BootstrapInitializer: Camera.main is NULL! Ensure your main camera is tagged as 'MainCamera'.", this);
            }
            else
            {
                GameLog.LogVerbose("BootstrapInitializer: ✓ Camera.main exists", this);
            }

            if (errors > 0)
            {
                GameLog.LogError($"BootstrapInitializer: Validation complete with {errors} CRITICAL ERRORS!", this);
            }
            else
            {
                GameLog.Log("BootstrapInitializer: Validation complete - all systems ready!", this);
            }
#endif
        }
    }
}
