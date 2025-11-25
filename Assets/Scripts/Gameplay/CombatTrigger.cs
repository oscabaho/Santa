using System.Collections.Generic;
using System.Linq;
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
    private ICombatService _combatService;
    private Collider _interactionCollider;
    private bool _isListeningForCombatEnd;

    [Inject]
    public void Construct(
        IGameStateService gameStateService,
        ICombatTransitionService combatTransitionService,
        CombatScenePool combatScenePool,
        ICombatService combatService)
    {
        _gameStateService = gameStateService;
        _combatTransitionService = combatTransitionService;
        _combatScenePool = combatScenePool;
        _combatService = combatService;
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

        _interactionCollider = GetComponent<Collider>();
        if (_interactionCollider == null)
        {
            GameLog.LogError("CombatTrigger requires a Collider component to function.");
            enabled = false;
            return;
        }

        // Ensure the collider is set to be a trigger.
        _interactionCollider.isTrigger = true;
    }

    public async void StartCombatInteraction()
    {
        if (_combatHasBeenTriggered) return;
        _combatHasBeenTriggered = true;

        if (_interactionCollider != null)
        {
            _interactionCollider.enabled = false;
        }

        if (_combatTransitionService == null)
        {
            GameLog.LogError("CombatTrigger: ICombatTransitionService not found when starting combat.");
            ResetTriggerState();
            return;
        }

        if (_combatService == null)
        {
            GameLog.LogError("CombatTrigger: ICombatService not found. Cannot start combat.");
            ResetTriggerState();
            return;
        }

        GameLog.Log("Player has interacted with a combat trigger.");

        var poolKey = _encounter.GetPoolKey();
        if (_combatScenePool == null)
        {
            GameLog.LogError("CombatTrigger: CombatScenePool instance not found.");
            ResetTriggerState();
            return;
        }

        try
        {
            _activeCombatInstance = await _combatScenePool.GetInstanceAsync(poolKey, _encounter);
            if (_activeCombatInstance == null)
            {
                GameLog.LogError("Failed to get instance from pool.");
                ResetTriggerState();
                return;
            }

            _activeCombatInstance.SetActive(true); // Activate the instance
            _activeCombatPoolKey = poolKey;

            var combatArena = _activeCombatInstance.GetComponentInChildren<CombatArena>(true);
            if (combatArena == null)
            {
                GameLog.LogError($"CombatTrigger: Could not find CombatArena inside '{_activeCombatInstance.name}'.");
                HandleCombatStartFailure();
                return;
            }

            List<GameObject> participants = combatArena.Combatants
                .Where(go => go != null)
                .Distinct()
                .ToList();

            if (participants.Count == 0)
            {
                GameLog.LogError("CombatTrigger: CombatArena returned zero participants.");
                HandleCombatStartFailure();
                return;
            }

            _gameStateService.OnCombatEnded += OnCombatEnded;
            _isListeningForCombatEnd = true;
            _combatTransitionService.StartCombat(_activeCombatInstance);
            _combatService.StartCombat(participants);
            _gameStateService.StartCombat();
        }
        catch (System.Exception ex)
        {
            GameLog.LogError("Failed to start combat interaction.");
            GameLog.LogException(ex);
            HandleCombatStartFailure();
        }
    }

    private void OnCombatEnded(bool playerWon)
    {
        // Release the instance back to the pool
        ReleaseActiveInstance();
        CleanupActiveCombatInstance();

        // Unsubscribe
        if (_gameStateService != null && _isListeningForCombatEnd)
        {
            _gameStateService.OnCombatEnded -= OnCombatEnded;
            _isListeningForCombatEnd = false;
        }

        if (playerWon)
        {
            GameLog.Log($"Player won combat initiated by '{gameObject.name}'. Destroying enemy.");

            // Disable collider before destroying to trigger OnTriggerExit on PlayerInteraction
            // This ensures the interaction button is hidden
            if (_interactionCollider != null)
            {
                _interactionCollider.enabled = false;
            }

            Destroy(gameObject);
        }
        else
        {
            GameLog.Log($"Player lost combat initiated by '{gameObject.name}'. Resetting trigger.");
            ResetTriggerState();
        }
    }

    private void HandleCombatStartFailure()
    {
        if (_gameStateService != null && _isListeningForCombatEnd)
        {
            _gameStateService.OnCombatEnded -= OnCombatEnded;
            _isListeningForCombatEnd = false;
        }

        ReleaseActiveInstance();
        CleanupActiveCombatInstance();
        ResetTriggerState();
    }

    private void ReleaseActiveInstance()
    {
        if (_combatScenePool == null) return;
        if (_activeCombatInstance != null && !string.IsNullOrEmpty(_activeCombatPoolKey))
        {
            bool releaseAddressables = _encounter != null && _encounter.ReleaseAddressablesInstances;
            _combatScenePool.ReleaseInstance(_activeCombatPoolKey, _activeCombatInstance, releaseAddressables);
        }
    }

    private void CleanupActiveCombatInstance()
    {
        _activeCombatInstance = null;
        _activeCombatPoolKey = null;
    }

    private void ResetTriggerState()
    {
        _combatHasBeenTriggered = false;
        if (_interactionCollider != null)
        {
            _interactionCollider.enabled = true;
        }
    }
}