using Santa.Core;
using Santa.Infrastructure.Input;
using Santa.Presentation.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using VContainer;

namespace Santa.Presentation.HUD
{

    public class VirtualGamepadUI : MonoBehaviour
    {
        [Header("Child UI Elements")]
        [Tooltip("Drag the Action Button GameObject that is a child of this prefab here (optional if ActionButtonController present).")]
        [SerializeField] private GameObject actionButton;
        [Header("Input Source")]
        [Tooltip("Reference to the shared InputReader asset used by gameplay. This is required for the on-screen action button to invoke interactions.")]
        [SerializeField] private InputReader inputReader;

        private IGameplayUIService _gameplayUIService;

        // [Inject] removed to avoid VContainer resolution errors on dynamic instantiation


        private bool _registered;
        private Button _actionBtn; // legacy wiring fallback
        private ActionButtonController _externalController;

        void OnEnable()
        {
            // If an external ActionButtonController exists among children, defer all button logic to it.
            _externalController = GetComponentInChildren<ActionButtonController>(true);
            if (_externalController != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("VirtualGamepadUI: External ActionButtonController found; skipping internal wiring.", this);
#endif
            }
            else
            {
                if (actionButton == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogError("The 'actionButton' is not assigned in the VirtualGamepadUI script.", this);
#endif
                    return;
                }

                // Wire up the UI button click to trigger the same interaction flow (fallback path)
                if (!actionButton.TryGetComponent<Button>(out _actionBtn))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogError("VirtualGamepadUI: The actionButton GameObject has no Button component.", this);
#endif
                }
                else
                {
                    _actionBtn.onClick.AddListener(OnActionButtonPressed);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose($"VirtualGamepadUI: Button listener added. Button interactable={_actionBtn.interactable}", this);
#endif
                }
            }

            // Try to auto-acquire InputReader if not assigned
            if (inputReader == null)
            {
                inputReader = Resources.Load<InputReader>("InputReader");
                if (inputReader == null)
                {
                    var readers = Resources.FindObjectsOfTypeAll<InputReader>();
                    if (readers != null && readers.Length > 0)
                    {
                        inputReader = readers[0];
                    }
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (inputReader != null)
                    GameLog.LogVerbose("VirtualGamepadUI: Auto-acquired InputReader from Resources.", this);
#endif
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"VirtualGamepadUI: OnEnable complete. InputReader={(inputReader != null ? inputReader.name : "NULL")}, GameplayUIService={(_gameplayUIService != null ? "SET" : "NULL")}", this);
#endif

            // EventSystem diagnostics

            var es = EventSystem.current;
            if (es == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError("VirtualGamepadUI: No EventSystem present in scene. UI clicks will not work.", this);
#endif
            }
            else
            {
#if ENABLE_INPUT_SYSTEM
                var hasInputSystemModule = es.GetComponent<InputSystemUIInputModule>() != null;
#else
            var hasInputSystemModule = false;
#endif
                var hasStandaloneModule = es.GetComponent<StandaloneInputModule>() != null;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"VirtualGamepadUI: EventSystem modules -> InputSystemUIInputModule={hasInputSystemModule}, StandaloneInputModule={hasStandaloneModule}", this);
#endif
            }

            if (_externalController == null)
            {
                if (_gameplayUIService == null)
                {
                    var ui = FindFirstObjectByType<GameplayUIManager>();
                    if (ui != null) _gameplayUIService = ui;
                }

                if (_gameplayUIService != null && !_registered)
                {
                    _gameplayUIService.RegisterActionButton(actionButton);
                    _registered = true;
                }
            }
        }

        void OnDisable()
        {
            if (_externalController == null)
            {
                if (_actionBtn != null)
                {
                    _actionBtn.onClick.RemoveListener(OnActionButtonPressed);
                }
            }
        }

        private void OnDestroy()
        {
            if (_externalController == null && _registered && _gameplayUIService != null && actionButton != null)
            {
                _gameplayUIService.UnregisterActionButton(actionButton);
                _registered = false;
            }
        }

        private void OnActionButtonPressed()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string inputReaderName = inputReader != null ? inputReader.name : "NULL";
            string btnName = _actionBtn != null ? _actionBtn.name : "NULL";
            bool btnActive = actionButton != null && actionButton.activeInHierarchy;
            GameLog.LogVerbose($"VirtualGamepadUI: OnActionButtonPressed called. InputReader={inputReaderName}, Button={btnName}, ButtonActive={btnActive}", this);
#endif


            if (inputReader == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError("VirtualGamepadUI: InputReader is not assigned. Cannot trigger interaction.", this);
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"VirtualGamepadUI: Calling RaiseInteract() on InputReader '{inputReader.name}'.", this);
#endif
            inputReader.RaiseInteract();
        }
    }
}
