using UnityEngine;

// It is expected that this component is attached to the same GameObject as the Movement script.
// It requires an InputReader to be assigned in the inspector.
public class PlayerInteraction : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The InputReader ScriptableObject that provides player input events.")]
    [SerializeField] private InputReader inputReader;

    private CombatTrigger _currentCombatTrigger;

    private void Awake()
    {
        // The GameLog class is not defined in the provided context.
        // Assuming it's a custom static class for logging.
        // If not, this line will cause a compilation error.
        if (inputReader == null)
        {
            Debug.LogError($"InputReader is not assigned in the inspector on {gameObject.name}!", this);
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
            _currentCombatTrigger = combatTrigger;
            // Assuming ServiceLocator and IGameplayUIService are defined and accessible.
            ServiceLocator.Get<IGameplayUIService>()?.ShowActionButton(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<CombatTrigger>(out var combatTrigger) && combatTrigger == _currentCombatTrigger)
        {
            _currentCombatTrigger = null;
            ServiceLocator.Get<IGameplayUIService>()?.ShowActionButton(false);
        }
    }
}
