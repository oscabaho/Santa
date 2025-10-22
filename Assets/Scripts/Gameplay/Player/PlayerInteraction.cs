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

    [Inject]
    public void Construct(IGameplayUIService gameplayUIService, IObjectResolver resolver)
    {
        _gameplayUIService = gameplayUIService;
        _resolver = resolver;
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
        if (inputReader != null)
        {
            inputReader.InteractEvent += OnInteract;
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
        if (_currentCombatTrigger != null)
        {
            _currentCombatTrigger.StartCombatInteraction();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger))
        {
            _resolver.Inject(combatTrigger);
            _currentCombatTrigger = combatTrigger;
            _gameplayUIService?.ShowActionButton(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger) && combatTrigger == _currentCombatTrigger)
        {
            _currentCombatTrigger = null;
            _gameplayUIService?.ShowActionButton(false);
        }
    }
}