using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// The EnemyBrain now needs an AbilityHolder to know what it can do.
[RequireComponent(typeof(ActionPointComponentBehaviour))]
[RequireComponent(typeof(AbilityHolder))]
public class EnemyBrain : MonoBehaviour, IBrain
{
    private AbilityHolder _abilityHolder;

    private void Awake()
    {
        _abilityHolder = GetComponent<AbilityHolder>();
    }

    /// <summary>
    /// This method is called by the TurnBasedCombatManager during the PLANNING phase.
    /// The AI decides what action to take and returns its choice without executing it.
    /// </summary>
    public PendingAction ChooseAction(
        PendingAction? playerAction,
        List<GameObject> allEnemies,
        List<GameObject> allAllies)
    {
        // Simple AI: Find a target from the allies list (usually just the player).
        GameObject target = allAllies.FirstOrDefault(a => a.activeInHierarchy);
        if (target == null)
        {
            return new PendingAction(); // No target, do nothing.
        }

        // Simple AI: Find the most expensive ability it can afford and use it.
        var ap = GetComponent<ActionPointComponentBehaviour>();
        Ability chosenAbility = _abilityHolder.Abilities
            .Where(a => ap.ActionPoints.HasEnough(a.ApCost)) // Find all affordable abilities
            .OrderByDescending(a => a.ApCost) // Order by most expensive
            .FirstOrDefault(); // Take the best one, or null if none are affordable

        if (chosenAbility != null)
        {
            // Package the decision into a PendingAction and return it.
            return new PendingAction
            {
                Ability = chosenAbility,
                Caster = gameObject,
                PrimaryTarget = target
            };
        }
        else
        {
            // Return an empty action if no ability can be used.
            return new PendingAction();
        }
    }
}
