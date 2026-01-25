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
#endif
        }

        // Auto-acquire InputReader if not assigned (Runtime & Editor)
        if (inputReader == null)
        {
            var readers = Resources.FindObjectsOfTypeAll<InputReader>();
            if (readers != null && readers.Length > 0)
            {
                inputReader = readers[0];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("ActionButtonController: Auto-acquired InputReader from Resources (fallback).", this);
#endif
            }
        }

        // Validate EventSystem & input module; auto-fix common misconfiguration at runtime (safe in dev/testing)
        var es = EventSystem.current;
        if (es == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("ActionButtonController: No EventSystem found in scene. UI clicks will not work.", this);
#endif
        }
        else
        {
            var module = es.currentInputModule;
            string moduleName = module != null ? module.GetType().Name : "<none>";
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"ActionButtonController: EventSystem present. currentInputModule={moduleName}", this);
#endif
            // Detailed configuration now handled by UIEventSystemConfigurator (avoid duplicate logic here).
        }

        // Check parent Canvas configuration
        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("ActionButtonController: Assigned Camera.main to Canvas.worldCamera (ScreenSpaceCamera).", this);
#endif
            }

            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster == null)
            {
                canvas.gameObject.AddComponent<GraphicRaycaster>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("ActionButtonController: Added GraphicRaycaster to parent Canvas.", this);
#endif
            }
        }

        // Warn if any parent CanvasGroup blocks interaction or raycasts
        var groups = GetComponentsInParent<CanvasGroup>(includeInactive: true);
        foreach (var cg in groups)
        {
            if (!cg.interactable || !cg.blocksRaycasts)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"ActionButtonController: Parent CanvasGroup '{cg.gameObject.name}' has interactable={cg.interactable}, blocksRaycasts={cg.blocksRaycasts}. This may block clicks.", this);
#endif
            }
        }

        if (_gameplayUIService == null)
        {
            var ui = FindFirstObjectByType<Santa.Presentation.UI.GameplayUIManager>();
            if (ui != null) _gameplayUIService = ui;
        }

        if (_gameplayUIService != null && !_registered)
        {
            _gameplayUIService.RegisterActionButton(gameObject);
            _registered = true;
        }
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
#endif
            }
        }
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
        if (Time.unscaledTime - _lastInteractionTime < InteractionCooldown) return;
        _lastInteractionTime = Time.unscaledTime;

        if (inputReader == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("ActionButtonController: InputReader is not assigned. Cannot trigger interaction.", this);
#endif
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"ActionButtonController: Input Triggered -> RaiseInteract on '{inputReader.name}'.", this);
#endif
        inputReader.RaiseInteract();
    }
}
}
