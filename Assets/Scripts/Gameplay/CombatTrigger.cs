using UnityEngine;

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

            // Use the CombatTransitionManager to start the combat.
            if (CombatTransitionManager.Instance != null)
            {
                CombatTransitionManager.Instance.StartCombat(_encounter);
            }
            else
            {
                Debug.LogError("A CombatTransitionManager is required in the scene to start combat!");
            }

            // For now, we just disable the trigger object.
            gameObject.SetActive(false);
        }
    }
}
