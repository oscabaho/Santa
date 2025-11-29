using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// The EnemyBrain now needs an AbilityHolder to know what it can do.
[RequireComponent(typeof(ActionPointComponentBehaviour))]
[RequireComponent(typeof(AbilityHolder))]
public class EnemyBrain : MonoBehaviour, IBrain
{
    private AbilityHolder _abilityHolder;
    private ActionPointComponentBehaviour _apComponent;

    private void Awake()
    {
        _abilityHolder = GetComponent<AbilityHolder>();
        _apComponent = GetComponent<ActionPointComponentBehaviour>();
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
        // Defensive: Only target entities with the "Player" tag
        GameObject target = null;
        for (int i = 0; i < allAllies.Count; i++)
        {
            var a = allAllies[i];
            if (a != null && a.activeInHierarchy && a.CompareTag(GameConstants.Tags.Player))
            {
                target = a;
                break;
            }
        }
        if (target == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"{gameObject.name} (Enemy) could not find a valid Player target.");
#endif
            return new PendingAction(); // No target, do nothing.
        }

        // Simple AI: Find the most expensive ability it can afford and use it.
        Ability chosenAbility = null;
        int bestCost = -1;
        var abilities = _abilityHolder.Abilities;
        for (int i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];
            if (ability != null && _apComponent.ActionPoints.HasEnough(ability.ApCost))
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string targetName = target != null ? target.name : "NULL";
            GameLog.Log($"{gameObject.name} (Enemy) chose ability '{chosenAbility.AbilityName}' targeting '{targetName}'");
#endif
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
