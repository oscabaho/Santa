using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This component starts a turn-based combat encounter when the player enters its trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(CombatEncounter))]
public class CombatTrigger : MonoBehaviour
{
    private CombatEncounter _encounter;
    private bool _combatHasBeenTriggered = false;

    private void Awake()
    {
        _encounter = GetComponent<CombatEncounter>();
    }

    private void Start()
    {
        // Ensure the collider is set to be a trigger.
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Avoid triggering the same combat multiple times.
        if (_combatHasBeenTriggered) return;

        // Check if the object that entered the trigger is the player.
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered a combat trigger.");
            _combatHasBeenTriggered = true;

            // Get enemies for the current level and set up the encounter
            var levelService = ServiceLocator.Get<ILevelService>();
            if (levelService == null)
            {
                Debug.LogError("CombatTrigger: No ILevelService registered. Cannot start encounter.");
                return;
            }
            List<Enemy> enemies = levelService.GetEnemiesForCurrentLevel();
            _encounter.SetupEncounter(other.gameObject, enemies);

            // Use the CombatTransitionManager to start the combat via ServiceLocator.
            var combatTransition = ServiceLocator.Get<ICombatTransitionService>();
            if (combatTransition != null)
            {
                // Pass player, the assembled participants list, and the combat scene parent to avoid Core->Gameplay dependency.
                combatTransition.StartCombat(other.gameObject, _encounter.CombatParticipants, _encounter.CombatSceneParent);
            }
            else
            {
                Debug.LogError("A ICombatTransitionService is required in the scene to start combat!");
            }

            // For now, we just disable the trigger object.
            gameObject.SetActive(false);
        }
    }
}
