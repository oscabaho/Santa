using System.Threading;
using Santa.Core;
using UnityEngine;
using VContainer;

namespace Santa.Infrastructure.Combat
{
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

        // [Inject] removed to support safe runtime discovery
        // public void Construct(ICombatEncounterManager encounterManager) ...

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

        public async Cysharp.Threading.Tasks.UniTaskVoid StartCombatInteraction()
        {
            if (_combatHasBeenTriggered) return;
            _combatHasBeenTriggered = true;

            try
            {
                _loadCancellation?.Cancel();
                _loadCancellation?.Dispose();
                _loadCancellation = new CancellationTokenSource();
                var ct = _loadCancellation.Token;

                if (_interactionCollider != null)
                {
                    _interactionCollider.enabled = false;
                }

                if (_encounterManager == null)
                {
                    // Lazy lookup
                    var manager = FindFirstObjectByType<CombatEncounterManager>();
                    if (manager != null) _encounterManager = manager;
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
            catch (System.OperationCanceledException)
            {
                // Expected during scene transitions or component destruction
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log("CombatTrigger: Combat interaction cancelled.");
#endif
                ResetTriggerState();
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"CombatTrigger.StartCombatInteraction: Failed to start combat: {ex.Message}");
                GameLog.LogException(ex);
#else
            _ = ex;
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
}