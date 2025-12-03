using System.Collections.Generic;
using UnityEngine;
using Santa.Core;
using Santa.Domain.Combat;

namespace Santa.Domain.Entities
{
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
        GameObject target = null;
        for (int i = 0; i < allAllies.Count; i++)
        {
            var a = allAllies[i];
            if (a != null && a.activeInHierarchy)
            {
                target = a;
                break;
            }
        }
        if (target == null)
        {
            return new PendingAction(); // No target, do nothing.
        }

        // Simple AI: Find the most expensive ability it can afford and use it.
        var ap = GetComponent<ActionPointComponentBehaviour>();
        Ability chosenAbility = null;
        int bestCost = -1;
        var abilities = _abilityHolder.Abilities;
        for (int i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];
            if (ability != null && ap.ActionPoints.HasEnough(ability.ApCost))
            {
                if (ability.ApCost > bestCost)
                {
                    bestCost = ability.ApCost;
                    chosenAbility = ability;
                }
            }
        }

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
}
