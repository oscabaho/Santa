using UnityEngine;

/// <summary>
/// This component starts a turn-based combat encounter when the player interacts with it.
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

    public void StartCombatInteraction()
    {
        // Avoid triggering the same combat multiple times.
        if (_combatHasBeenTriggered) return;

        GameObject combatScene = _encounter.CombatSceneParent;
        if (combatScene == null)
        {
            Debug.LogError("CombatTrigger: CombatSceneParent is not assigned in the CombatEncounter component.");
            return;
        }

        Debug.Log("Player has interacted with a combat trigger.");
        _combatHasBeenTriggered = true;

        // Use the CombatTransitionManager to start the combat via ServiceLocator.
        var combatTransition = ServiceLocator.Get<ICombatTransitionService>();
        if (combatTransition != null)
        {
            combatTransition.StartCombat(combatScene);
        }
        else
        {
            Debug.LogError("A ICombatTransitionService is required in the scene to start combat!");
        }

        // For now, we just disable the trigger object.
        gameObject.SetActive(false);
    }
}