using System.Threading;
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
    private ICombatEncounterManager _encounterManager;
    private Collider _interactionCollider;
    private CancellationTokenSource _loadCancellation;

    [Inject]
    public void Construct(ICombatEncounterManager encounterManager)
    {
        _encounterManager = encounterManager;
    }

    private void Awake()
    {
        _encounter = GetComponent<CombatEncounter>();
        if (_encounter == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("CombatTrigger requires a CombatEncounter component.");
#endif
            enabled = false;
            return;
        }

        _interactionCollider = GetComponent<Collider>();
        if (_interactionCollider == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("CombatTrigger requires a Collider component to function.");
#endif
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
        _loadCancellation = new CancellationTokenSource();
        var ct = _loadCancellation.Token;

        if (_interactionCollider != null)
        {
            _interactionCollider.enabled = false;
        }

        if (_encounterManager == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("CombatTrigger: ICombatEncounterManager not found when starting combat.");
#endif
            ResetTriggerState();
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("Player has interacted with a combat trigger.");
#endif

        try
        {
            bool playerWon = await _encounterManager.StartEncounterAsync(_encounter);

            if (ct.IsCancellationRequested || this == null || gameObject == null)
            {
                return;
            }

            if (playerWon)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"Player won combat initiated by '{gameObject.name}'. Destroying enemy.");
#endif
                Destroy(gameObject);
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"Player lost combat initiated by '{gameObject.name}'. Resetting trigger.");
#endif
                ResetTriggerState();
            }
        }
        catch (System.Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("Failed to start combat interaction.");
            GameLog.LogException(ex);
#endif
            ResetTriggerState();
        }
    }

    private void ResetTriggerState()
    {
        _combatHasBeenTriggered = false;
        if (_loadCancellation != null)
        {
            _loadCancellation.Cancel();
            _loadCancellation.Dispose();
            _loadCancellation = null;
        }
        if (_interactionCollider != null)
        {
            _interactionCollider.enabled = true;
        }
    }

    private void OnDestroy()
    {
        if (_loadCancellation != null)
        {
            _loadCancellation.Cancel();
            _loadCancellation.Dispose();
            _loadCancellation = null;
        }
    }
}