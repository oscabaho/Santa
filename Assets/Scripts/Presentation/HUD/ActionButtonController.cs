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
public class ActionButtonController : MonoBehaviour
{
    [Tooltip("Shared InputReader asset used across gameplay. Injected if available; can be assigned in Inspector as fallback.")]
    [SerializeField] private InputReader inputReader;

    private IGameplayUIService _gameplayUIService;
    private Button _button;
    private bool _registered;

    [Inject]
    public void Construct(IGameplayUIService gameplayUIService, InputReader injectedInputReader)
    {
        _gameplayUIService = gameplayUIService;
        if (injectedInputReader != null)
        {
            inputReader = injectedInputReader;
        }

        // If DI completes after OnEnable, try to register immediately
        if (!_registered && isActiveAndEnabled && _gameplayUIService != null)
        {
            _gameplayUIService.RegisterActionButton(gameObject);
            _registered = true;
        }
    }

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

        // Editor convenience / safety: auto-acquire InputReader if DI has not yet injected it.
#if UNITY_EDITOR
        if (inputReader == null)
        {
            var readers = Resources.FindObjectsOfTypeAll<InputReader>();
            if (readers != null && readers.Length > 0)
            {
                inputReader = readers[0];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("ActionButtonController: Auto-acquired InputReader in Editor (fallback).", this);
#endif
            }
        }
#endif

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

        if (_gameplayUIService != null && !_registered)
        {
            _gameplayUIService.RegisterActionButton(gameObject);
            _registered = true;
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
        if (inputReader == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("ActionButtonController: InputReader is not assigned. Cannot trigger interaction.", this);
#endif
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"ActionButtonController: Click -> RaiseInteract on '{inputReader.name}'.", this);
#endif
        inputReader.RaiseInteract();
    }
}
}
