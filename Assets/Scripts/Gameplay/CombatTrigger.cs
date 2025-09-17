using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This component starts a turn-based combat encounter when the player enters its trigger.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CombatTrigger : MonoBehaviour
{
    [Header("Combat Participants")]
    [Tooltip("A list of enemy GameObjects that will participate in this battle.")]
    [SerializeField] private List<GameObject> enemiesInEncounter;

    private bool _combatHasBeenTriggered = false;

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

            // Compile the list of all combatants.
            List<GameObject> participants = new List<GameObject>();
            participants.Add(other.gameObject); // Add the player
            participants.AddRange(enemiesInEncounter); // Add the enemies

            // Find the TurnBasedCombatManager and start the combat.
            if (TurnBasedCombatManager.Instance != null)
            {
                TurnBasedCombatManager.Instance.StartCombat(participants);
            }
            else
            {
                Debug.LogError("A TurnBasedCombatManager is required in the scene to start combat!");
            }

            // For now, we just disable the trigger object. In a real game, you might destroy it
            // or play an animation before transitioning to a combat scene.
            gameObject.SetActive(false);
        }
    }
}
