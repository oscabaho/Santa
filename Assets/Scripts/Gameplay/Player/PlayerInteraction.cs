using UnityEngine;
using VContainer;

// It is expected that this component is attached to the same GameObject as the Movement script.
// It requires an InputReader to be assigned in the inspector.
public class PlayerInteraction : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The InputReader ScriptableObject that provides player input events.")]
    [SerializeField] private InputReader inputReader;

    private CombatTrigger _currentCombatTrigger;
    private IGameplayUIService _gameplayUIService;
    private IGameStateService _gameStateService;
    private IObjectResolver _resolver;
    private bool? _desiredActionVisible; // null = no preference yet
    private bool _readyHooked;
    private float _lastNoTriggerLogTime;
    private const float NoTriggerLogCooldown = 2f; // seconds between warning logs

    [Inject]
    public void Construct(IGameplayUIService gameplayUIService, IGameStateService gameStateService, IObjectResolver resolver, InputReader injectedInputReader)
    {
        _gameplayUIService = gameplayUIService;
        _gameStateService = gameStateService;
        _resolver = resolver;
        if (injectedInputReader != null)
        {
            inputReader = injectedInputReader; // Ensure shared InputReader instance
        }
    }

    private void Awake()
    {
        if (inputReader == null)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"InputReader is not assigned in the inspector on {gameObject.name}!", this);
            #endif
        }
    }

    private void OnEnable()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"PlayerInteraction: OnEnable. InputReader={inputReader?.name ?? "NULL"}", this);
        #endif

        if (inputReader != null)
        {
            inputReader.InteractEvent += OnInteract;
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"PlayerInteraction: Subscribed to InteractEvent on InputReader '{inputReader.name}'.", this);
            #endif
        }

        // Subscribe to combat end event to clear interaction state
        if (_gameStateService != null)
        {
            _gameStateService.OnCombatEnded += OnCombatEnded;
        }

        // Hook a single ready handler to apply latest desired state when UI becomes ready
        if (_gameplayUIService != null && !_readyHooked)
        {
            _readyHooked = true;
            _gameplayUIService.WhenReady(this, () =>
            {
                if (_desiredActionVisible.HasValue)
                {
                    _gameplayUIService.ShowActionButton(_desiredActionVisible.Value);
                }
            });
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.InteractEvent -= OnInteract;
        }

        // Unsubscribe from combat end event
        if (_gameStateService != null)
        {
            _gameStateService.OnCombatEnded -= OnCombatEnded;
        }
    }

    private void OnCombatEnded(bool playerWon)
    {
        // Clear interaction state when combat ends
        // This ensures the button is hidden when returning to exploration
        _currentCombatTrigger = null;
        _desiredActionVisible = false;

        if (_gameplayUIService != null && _gameplayUIService.IsReady)
        {
            _gameplayUIService.ShowActionButton(false);
        }

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("PlayerInteraction: Combat ended, cleared interaction state and hid action button.", this);
        #endif
    }

    private void OnInteract()
    {
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"PlayerInteraction: OnInteract called. CurrentTrigger={_currentCombatTrigger?.name ?? "NULL"}", this);
        #endif

        if (_currentCombatTrigger != null)
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"PlayerInteraction: Starting combat interaction on trigger '{_currentCombatTrigger.name}'.", this);
            #endif
            _currentCombatTrigger.StartCombatInteraction();
        }
        else
        {
            // Throttle warning spam if player repeatedly interacts outside a trigger zone.
            float t = Time.unscaledTime;
            if (t - _lastNoTriggerLogTime > NoTriggerLogCooldown)
            {
                _lastNoTriggerLogTime = t;
                #if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("PlayerInteraction: OnInteract called but no CombatTrigger is set. Player might not be inside a trigger zone.", this);
                #endif
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger))
        {
            _resolver.Inject(combatTrigger);
            _currentCombatTrigger = combatTrigger;
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"PlayerInteraction: Entered trigger zone '{combatTrigger.name}'. Setting as current trigger.", this);
            #endif

            _desiredActionVisible = true;
            if (_gameplayUIService != null && _gameplayUIService.IsReady)
            {
                _gameplayUIService.ShowActionButton(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger) && combatTrigger == _currentCombatTrigger)
        {
            _currentCombatTrigger = null;
            _desiredActionVisible = false;
            if (_gameplayUIService != null && _gameplayUIService.IsReady)
            {
                _gameplayUIService.ShowActionButton(false);
            }
        }
    }
}