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
    private IObjectResolver _resolver;
    private bool? _desiredActionVisible; // null = no preference yet
    private bool _readyHooked;
    private float _lastNoTriggerLogTime;
    private const float NoTriggerLogCooldown = 2f; // seconds between warning logs

    [Inject]
    public void Construct(IGameplayUIService gameplayUIService, IObjectResolver resolver, InputReader injectedInputReader)
    {
        _gameplayUIService = gameplayUIService;
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
            GameLog.LogError($"InputReader is not assigned in the inspector on {gameObject.name}!", this);
        }
    }

    private void OnEnable()
    {
        GameLog.Log($"PlayerInteraction: OnEnable. InputReader={inputReader?.name ?? "NULL"}", this);
        
        if (inputReader != null)
        {
            inputReader.InteractEvent += OnInteract;
            GameLog.Log($"PlayerInteraction: Subscribed to InteractEvent on InputReader '{inputReader.name}'.", this);
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
    }

    private void OnInteract()
    {
        GameLog.Log($"PlayerInteraction: OnInteract called. CurrentTrigger={_currentCombatTrigger?.name ?? "NULL"}", this);
        
        if (_currentCombatTrigger != null)
        {
            GameLog.Log($"PlayerInteraction: Starting combat interaction on trigger '{_currentCombatTrigger.name}'.", this);
            _currentCombatTrigger.StartCombatInteraction();
        }
        else
        {
            // Throttle warning spam if player repeatedly interacts outside a trigger zone.
            float t = Time.unscaledTime;
            if (t - _lastNoTriggerLogTime > NoTriggerLogCooldown)
            {
                _lastNoTriggerLogTime = t;
                GameLog.LogWarning("PlayerInteraction: OnInteract called but no CombatTrigger is set. Player might not be inside a trigger zone.", this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger))
        {
            _resolver.Inject(combatTrigger);
            _currentCombatTrigger = combatTrigger;
            GameLog.Log($"PlayerInteraction: Entered trigger zone '{combatTrigger.name}'. Setting as current trigger.", this);
            
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