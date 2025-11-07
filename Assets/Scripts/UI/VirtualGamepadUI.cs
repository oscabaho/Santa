using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
using VContainer;

public class VirtualGamepadUI : MonoBehaviour
{
    [Header("Child UI Elements")]
    [Tooltip("Drag the Action Button GameObject that is a child of this prefab here (optional if ActionButtonController present).")]
    [SerializeField] private GameObject actionButton;
    [Header("Input Source")]
    [Tooltip("Reference to the shared InputReader asset used by gameplay. This is required for the on-screen action button to invoke interactions.")]
    [SerializeField] private InputReader inputReader;

    private IGameplayUIService _gameplayUIService;

    [Inject]
    public void Construct(IGameplayUIService gameplayUIService, InputReader injectedInputReader)
    {
        _gameplayUIService = gameplayUIService;
        if (injectedInputReader != null)
        {
            inputReader = injectedInputReader; // Prefer injected shared asset
        }

        // If DI completes after OnEnable already ran, try to register immediately here.
        if (!_registered && actionButton != null && isActiveAndEnabled && _gameplayUIService != null)
        {
            _gameplayUIService.RegisterActionButton(actionButton);
            _registered = true;
        }
    }

    private bool _registered;
    private Button _actionBtn; // legacy wiring fallback
    private ActionButtonController _externalController;

    void OnEnable()
    {
        // If an external ActionButtonController exists among children, defer all button logic to it.
        _externalController = GetComponentInChildren<ActionButtonController>(true);
        if (_externalController != null)
        {
            GameLog.Log("VirtualGamepadUI: External ActionButtonController found; skipping internal wiring.", this);
        }
        else
        {
            if (actionButton == null)
            {
                GameLog.LogError("The 'actionButton' is not assigned in the VirtualGamepadUI script.", this);
                return;
            }

            // Wire up the UI button click to trigger the same interaction flow (fallback path)
            _actionBtn = actionButton.GetComponent<Button>();
            if (_actionBtn == null)
            {
                GameLog.LogError("VirtualGamepadUI: The actionButton GameObject has no Button component.", this);
            }
            else
            {
                _actionBtn.onClick.AddListener(OnActionButtonPressed);
                GameLog.Log($"VirtualGamepadUI: Button listener added. Button interactable={_actionBtn.interactable}", this);
            }
        }

#if UNITY_EDITOR
        // Editor convenience: try to auto-acquire InputReader if not assigned
        if (inputReader == null)
        {
            var readers = Resources.FindObjectsOfTypeAll<InputReader>();
            if (readers != null && readers.Length > 0)
            {
                inputReader = readers[0];
                GameLog.Log("VirtualGamepadUI: Auto-acquired InputReader in Editor.", this);
            }
        }
#endif

        GameLog.Log($"VirtualGamepadUI: OnEnable complete. InputReader={inputReader?.name ?? "NULL"}, GameplayUIService={(_gameplayUIService != null ? "SET" : "NULL")}", this);

        // EventSystem diagnostics
        var es = EventSystem.current;
        if (es == null)
        {
            GameLog.LogError("VirtualGamepadUI: No EventSystem present in scene. UI clicks will not work.", this);
        }
        else
        {
#if ENABLE_INPUT_SYSTEM
            var hasInputSystemModule = es.GetComponent<InputSystemUIInputModule>() != null;
#else
            var hasInputSystemModule = false;
#endif
            var hasStandaloneModule = es.GetComponent<StandaloneInputModule>() != null;
            GameLog.Log($"VirtualGamepadUI: EventSystem modules -> InputSystemUIInputModule={hasInputSystemModule}, StandaloneInputModule={hasStandaloneModule}", this);
        }

        if (_externalController == null)
        {
            if (_gameplayUIService != null && !_registered)
            {
                _gameplayUIService.RegisterActionButton(actionButton);
                _registered = true;
            }
            else if (_gameplayUIService == null)
            {
                // Inyección tardía: UIManager inyecta luego de instanciar el prefab
                GameLog.Log("VirtualGamepadUI: Waiting for DI to complete (IGameplayUIService not yet injected).", this);
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
        GameLog.Log($"VirtualGamepadUI: OnActionButtonPressed called. InputReader={inputReader?.name ?? "NULL"}, Button={_actionBtn?.name ?? "NULL"}, ButtonActive={actionButton?.activeInHierarchy ?? false}", this);
        
        if (inputReader == null)
        {
            GameLog.LogError("VirtualGamepadUI: InputReader is not assigned. Cannot trigger interaction.", this);
            return;
        }

        GameLog.Log($"VirtualGamepadUI: Calling RaiseInteract() on InputReader '{inputReader.name}'.", this);
        inputReader.RaiseInteract();
    }

    private void Update()
    {
        // Debug temporal: detectar cuando el botón cambia de estado
        if (_externalController == null && actionButton != null && _lastButtonState != actionButton.activeInHierarchy)
        {
            _lastButtonState = actionButton.activeInHierarchy;
            GameLog.Log($"VirtualGamepadUI: Action button visibility changed to {_lastButtonState}", this);
        }
    }

    private bool _lastButtonState;
}
