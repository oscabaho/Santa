using UnityEngine;
using VContainer;

/// <summary>
/// This component starts a turn-based combat encounter when the player interacts with it.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CombatEncounter))]
public class CombatTrigger : MonoBehaviour
{
    private CombatEncounter _encounter;
    private bool _combatHasBeenTriggered = false;
    private GameObject _activeCombatInstance;
    private string _activeCombatPoolKey;
    private IGameStateService _gameStateService;
    private ICombatTransitionService _combatTransitionService;
    private CombatScenePool _combatScenePool;

    [Inject]
    public void Construct(IGameStateService gameStateService, ICombatTransitionService combatTransitionService, CombatScenePool combatScenePool)
    {
        _gameStateService = gameStateService;
        _combatTransitionService = combatTransitionService;
        _combatScenePool = combatScenePool;
    }

    private void Awake()
    {
        _encounter = GetComponent<CombatEncounter>();
        if (_encounter == null)
        {
            GameLog.LogError("CombatTrigger requires a CombatEncounter component.");
            enabled = false;
            return;
        }

        // Ensure the collider is set to be a trigger.
        GetComponent<Collider>().isTrigger = true;
    }

    public async void StartCombatInteraction()
    {
        if (_combatHasBeenTriggered) return;
        _combatHasBeenTriggered = true;

        if (_combatTransitionService == null)
        {
            GameLog.LogError("CombatTrigger: ICombatTransitionService not found when starting combat.");
            _combatHasBeenTriggered = false; // Allow retry
            return;
        }
        
        GameLog.Log("Player has interacted with a combat trigger.");

        var poolKey = _encounter.GetPoolKey();
        if (_combatScenePool == null) 
        {
            GameLog.LogError("CombatTrigger: CombatScenePool instance not found.");
            _combatHasBeenTriggered = false; // Allow retry
            return; 
        }

        try
        {
            _activeCombatInstance = await _combatScenePool.GetInstanceAsync(poolKey, _encounter);
            if (_activeCombatInstance == null)
            {
                GameLog.LogError("Failed to get instance from pool.");
                _combatHasBeenTriggered = false;
                return;
            }

            _activeCombatInstance.SetActive(true); // Activate the instance
            _activeCombatPoolKey = poolKey;
            
            _gameStateService.OnCombatEnded += OnCombatEnded;
            _combatTransitionService.StartCombat(_activeCombatInstance);
            _gameStateService.StartCombat();

            gameObject.SetActive(false);
        }
        catch (System.Exception ex)
        {
            GameLog.LogError("Failed to start combat interaction.");
            GameLog.LogException(ex);
            _combatHasBeenTriggered = false; // Allow retry on failure
        }
    }

    private void OnCombatEnded()
    {
        // Release the instance back to the pool
        if (_activeCombatInstance != null && !string.IsNullOrEmpty(_activeCombatPoolKey))
        {
            _combatScenePool.ReleaseInstance(_activeCombatPoolKey, _activeCombatInstance);
        }

        // Unsubscribe
        if (_gameStateService != null)
        {
            _gameStateService.OnCombatEnded -= OnCombatEnded;
        }

        _activeCombatInstance = null;
        _activeCombatPoolKey = null;
    }
}