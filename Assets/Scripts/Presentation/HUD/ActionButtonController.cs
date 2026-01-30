using Santa.Core;
using Santa.Infrastructure.Input;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using VContainer;
#if UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace Santa.Presentation.HUD
{

    /// <summary>
    /// Dedicated controller for the on-screen Action Button.
    /// - Wires Button.onClick to InputReader.RaiseInteract()
    /// - Registers this GameObject as the gameplay action button in IGameplayUIService
    /// - Keeps logic isolated from VirtualGamepadUI (sticks, other controls)
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ActionButtonController : MonoBehaviour, IPointerDownHandler
    {
        [Tooltip("Shared InputReader asset used across gameplay. Injected if available; can be assigned in Inspector as fallback.")]
        [SerializeField] private InputReader inputReader;

        private IGameplayUIService _gameplayUIService;
        private Button _button;
        private bool _registered;
        private float _lastInteractionTime;
        private const float InteractionCooldown = 0.2f;
        private System.Action _onActionCallback;

        public void SetActionCallback(System.Action callback)
        {
            _onActionCallback = callback;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"ActionButtonController: Callback set. Target={(_onActionCallback != null ? _onActionCallback.Target : "NULL")}", this);
#endif
        }

        // [Inject] removed to support runtime instantiation flexibility

        private void Awake()
        {
            _button = GetComponent<Button>();
            if (_button == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError("ActionButtonController: Button component missing.", this);
#endif
            }
            else
            {
                // Ensure there's a Graphic to receive UI raycasts. If missing, add an invisible Image.
                var graphic = GetComponent<Graphic>();
                if (graphic == null)
                {
                    var image = gameObject.AddComponent<Image>();
                    image.color = new Color(1f, 1f, 1f, 0f); // invisible
                    image.raycastTarget = true;
                    if (_button.targetGraphic == null)
                    {
                        _button.targetGraphic = image;
                    }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("ActionButtonController: Added invisible Image to enable UI raycasts.", this);
#endif
                }
            }

            // Default to hidden handled by GameplayUIManager upon registration. 
            // We MUST remain active here so OnEnable runs and registers us.
        }


        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"ActionButtonController: Listener added. Interactable={_button.interactable}", this);
#else
            // CRITICAL: Log button state even in Release for mobile debugging
            GameLog.Log($"ActionButtonController: Button listener added. Interactable={_button.interactable}");
#endif
            }
            else
            {
                // CRITICAL: This is a hard blocker - button must exist
                GameLog.LogError("ActionButtonController: Button component is NULL! This should never happen.", this);
            }

            // Auto-acquire InputReader if not assigned (Runtime & Editor)
            if (inputReader == null)
            {
                // Fallback: Try to load from Resources by name
                inputReader = Resources.Load<InputReader>("InputReader");
                if (inputReader != null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("ActionButtonController: Loaded 'InputReader' from Resources.", this);
#else
                GameLog.Log($"ActionButtonController: Loaded InputReader from Resources");
#endif
                }
                else
                {
                    // Last ditch: Find any loaded instance
                    var readers = Resources.FindObjectsOfTypeAll<InputReader>();
                    if (readers != null && readers.Length > 0)
                    {
                        inputReader = readers[0];
                        GameLog.Log($"ActionButtonController: Auto-acquired InputReader '{readers[0].name}' via FindObjectsOfTypeAll.");
                    }
                    else
                    {
                        GameLog.LogError("ActionButtonController: InputReader NOT FOUND in Resources! Button will not trigger combats.", this);
                    }
                }
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            else
            {
                GameLog.LogVerbose($"ActionButtonController: InputReader already assigned: '{inputReader.name}'", this);
            }
#endif

            // Validate EventSystem & input module; auto-fix common misconfiguration at runtime
            var es = EventSystem.current;
            if (es == null)
            {
                // CRITICAL: No EventSystem means no UI input processing
                GameLog.LogError("ActionButtonController: CRITICAL - No EventSystem found in scene. UI clicks will NOT work. Please add EventSystem to scene.", this);
            }
            else
            {
                var module = es.currentInputModule;
                string moduleName = module != null ? module.GetType().Name : "<none>";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"ActionButtonController: EventSystem present. currentInputModule={moduleName}", this);
#else
            // Log module type even in Release - critical for mobile input debugging
            if (module == null)
            {
                GameLog.LogError($"ActionButtonController: EventSystem exists but currentInputModule is NULL!");
            }
            else
            {
                GameLog.Log($"ActionButtonController: EventSystem using {moduleName}");
            }
#endif
            }

            // Check parent Canvas configuration - CRITICAL for mobile touch input
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameLog.LogError("ActionButtonController: CRITICAL - No parent Canvas found! Button cannot be rendered or interacted with.", this);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"ActionButtonController: Canvas found: {canvas.gameObject.name}, RenderMode={canvas.renderMode}", this);
#endif

                // Validate Camera for ScreenSpaceCamera mode
                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    if (canvas.worldCamera == null)
                    {
                        var mainCam = Camera.main;
                        if (mainCam != null)
                        {
                            canvas.worldCamera = mainCam;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            GameLog.LogVerbose("ActionButtonController: Assigned Camera.main to Canvas.worldCamera.", this);
#endif
                        }
                        else
                        {
                            // CRITICAL: Camera.main not found - raycasts will fail
                            GameLog.LogError("ActionButtonController: CRITICAL - Canvas is ScreenSpaceCamera but Camera.main is NULL! Tag your main camera with 'MainCamera'.", this);
                        }
                    }
                }

                // Ensure GraphicRaycaster exists
                var raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("ActionButtonController: Added GraphicRaycaster to parent Canvas.", this);
#else
                GameLog.Log("ActionButtonController: Added GraphicRaycaster to Canvas for mobile input.");
#endif
                }
                else if (!raycaster.enabled)
                {
                    // CRITICAL: Raycaster disabled means no raycasts work
                    GameLog.LogError("ActionButtonController: CRITICAL - GraphicRaycaster is DISABLED on Canvas! Enabling it now.", this);
                    raycaster.enabled = true;
                }
            }

            // Check parent CanvasGroups - they can block raycasts
            var groups = GetComponentsInParent<CanvasGroup>(includeInactive: true);
            foreach (var cg in groups)
            {
                bool isBlocking = false;
                string blockReason = "";

                if (!cg.interactable)
                {
                    isBlocking = true;
                    blockReason += "interactable=false ";
                }
                if (!cg.blocksRaycasts)
                {
                    isBlocking = true;
                    blockReason += "blocksRaycasts=false";
                }

                if (isBlocking)
                {
                    GameLog.LogError($"ActionButtonController: CRITICAL - Parent CanvasGroup '{cg.gameObject.name}' is BLOCKING INPUT: {blockReason}. Fixing now.", this);
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }
            }

            // Find and register with GameplayUIService
            if (_gameplayUIService == null)
            {
                var ui = FindFirstObjectByType<Santa.Presentation.UI.GameplayUIManager>();
                if (ui != null)
                {
                    _gameplayUIService = ui;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose("ActionButtonController: Found GameplayUIManager.", this);
#endif
                }
                else
                {
                    GameLog.LogError("ActionButtonController: GameplayUIManager not found! Button visibility won't be managed.", this);
                }
            }

            if (_gameplayUIService != null && !_registered)
            {
                _gameplayUIService.RegisterActionButton(gameObject);
                _registered = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("ActionButtonController: Registered with GameplayUIManager.", this);
#else
            GameLog.Log("ActionButtonController: Registered action button with UI service.");
#endif
            }
            else if (_gameplayUIService == null)
            {
                GameLog.LogError("ActionButtonController: Cannot register - GameplayUIService is NULL!", this);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"ActionButtonController: OnEnable complete. Status: Button={(_button != null ? "OK" : "FAIL")}, InputReader={(inputReader != null ? "OK" : "FAIL")}, UIService={(_gameplayUIService != null ? "OK" : "FAIL")}, Registered={_registered}", this);
#else
        // CRITICAL: Final status log even in Release for debugging
        GameLog.Log($"ActionButtonController: OnEnable complete. Button={(inputReader != null ? "Ready" : "ERROR: No InputReader")}");
#endif
        }

        private void Update()
        {
            // Retry registration if failed in OnEnable (e.g., manager loaded late)
            if (!_registered)
            {
                if (_gameplayUIService == null)
                {
                    _gameplayUIService = FindFirstObjectByType<Santa.Presentation.UI.GameplayUIManager>();
                }

                if (_gameplayUIService != null)
                {
                    _gameplayUIService.RegisterActionButton(gameObject);
                    _registered = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log("ActionButtonController: Late registration effective.", this);
#else
                GameLog.Log("ActionButtonController: Late registration with UI service.");
#endif
                }
            }

            // Additional validation every frame in development
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            // Check if InputReader disappeared at runtime (unlikely but possible)
            if (inputReader == null && _registered)
            {
                var readers = Resources.FindObjectsOfTypeAll<InputReader>();
                if (readers != null && readers.Length > 0)
                {
                    inputReader = readers[0];
                    GameLog.LogWarning("ActionButtonController: InputReader was null but reacquired. This should not happen.", this);
                }
            }
#endif
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(OnButtonClicked);
            }
        }

        private void OnDestroy()
        {
            if (_registered && _gameplayUIService != null)
            {
                _gameplayUIService.UnregisterActionButton(gameObject);
                _registered = false;
            }
        }

        private void OnButtonClicked()
        {
            // Handled by OnPointerDown for better responsiveness, but kept as fallback
            TriggerInteraction();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_button != null && !_button.interactable) return;
            TriggerInteraction();
        }

        private void TriggerInteraction()
        {
            // Check cooldown
            if (Time.unscaledTime - _lastInteractionTime < InteractionCooldown)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"ActionButtonController: TriggerInteraction blocked by cooldown. Remaining={InteractionCooldown - (Time.unscaledTime - _lastInteractionTime):F3}s", this);
#endif
                return;
            }

            _lastInteractionTime = Time.unscaledTime;

            // CRITICAL: Check if Button exists and is interactable
            if (_button == null)
            {
                GameLog.LogError("ActionButtonController: CRITICAL - Button is NULL in TriggerInteraction! This should never happen.", this);
                return;
            }

            if (!_button.interactable)
            {
                GameLog.LogWarning("ActionButtonController: Button is not interactable. Interaction blocked.", this);
                return;
            }

            // PRIORITY: Execute direct callback (Command Pattern)
            if (_onActionCallback != null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("ActionButtonController: Executing direct Action callback.", this);
#endif
                _onActionCallback.Invoke();
                return;
            }

            // CRITICAL: Check if InputReader is available
            if (inputReader == null)
            {
                GameLog.LogError("ActionButtonController: CRITICAL - InputReader is NULL! Cannot trigger interaction. This is why combat doesn't start.", this);
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"ActionButtonController: Input Triggered -> RaiseInteract on '{inputReader.name}'.", this);
#else
        // CRITICAL: Log even in Release builds for mobile debugging
        GameLog.Log($"ActionButtonController: TriggerInteraction called -> firing InputReader.RaiseInteract()");
#endif

            inputReader.RaiseInteract();
        }
    }
}
