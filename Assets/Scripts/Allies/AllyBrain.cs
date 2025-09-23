using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AbilityHolder))]
public class AllyBrain : MonoBehaviour, IBrain
{
    private AbilityHolder _abilityHolder;

    private void Awake()
    {
        _abilityHolder = GetComponent<AbilityHolder>();
    }

    public PendingAction ChooseAction(
        PendingAction? playerAction,
        List<GameObject> allEnemies,
        List<GameObject> allAllies)
    {
        // 1. Find Target(s) based on user's rules
        GameObject primaryTarget = FindBestTarget(playerAction, allEnemies);

        if (primaryTarget == null)
        {
            Debug.Log($"{gameObject.name} could not find a valid target.");
            return new PendingAction();
        }

        // 2. Choose an Ability to use on that target
        // (Simple AI: use the most expensive affordable ability)
        var ap = GetComponent<ActionPointComponentBehaviour>();
        Ability chosenAbility = _abilityHolder.Abilities
            .Where(a => ap.ActionPoints.HasEnough(a.ApCost))
            .OrderByDescending(a => a.ApCost)
            .FirstOrDefault();

        if (chosenAbility != null)
        {
            Debug.Log($"{gameObject.name} decides to use {chosenAbility.AbilityName} on {primaryTarget.name}.");
            return new PendingAction
            {
                Ability = chosenAbility,
                Caster = gameObject,
                PrimaryTarget = primaryTarget
            };
        }

        Debug.Log($"{gameObject.name} cannot afford any abilities.");
    return new PendingAction();
    }

    private GameObject FindBestTarget(
        PendingAction? playerAction,
        List<GameObject> allEnemies)
    {
        var activeEnemies = allEnemies.Where(e => e.activeInHierarchy).ToList();
        if (!activeEnemies.Any())
        {
            return null;
        }

        // Find the minimum health among all active enemies
        int minHealth = activeEnemies.Min(e => e.GetComponent<HealthComponentBehaviour>().CurrentValue);

        // Get all enemies that are tied for the lowest health
        var lowestHealthEnemies = activeEnemies
            .Where(e => e.GetComponent<HealthComponentBehaviour>().CurrentValue == minHealth)
            .ToList();

        // If there's only one, that's our target
        if (lowestHealthEnemies.Count == 1)
        {
            return lowestHealthEnemies[0];
        }

        // Tie-breaker logic
        if (lowestHealthEnemies.Count > 1)
        {
            // Rule 2a: Check if the player targeted one of the tied enemies
            if (playerAction.HasValue && playerAction.Value.PrimaryTarget != null)
            {
                GameObject playerTarget = playerAction.Value.PrimaryTarget;
                if (lowestHealthEnemies.Contains(playerTarget))
                {
                    Debug.Log($"{gameObject.name} will focus fire on player's target: {playerTarget.name}");
                    return playerTarget; // Focus fire!
                }
            }

            // Rule 2b: Player did not target a tied enemy, or hasn't acted. Choose randomly.
            int randomIndex = Random.Range(0, lowestHealthEnemies.Count);
            Debug.Log($"{gameObject.name} chooses a random target among tied-health enemies.");
            return lowestHealthEnemies[randomIndex];
        }

        // Fallback, should not be reached if there are any active enemies
        return activeEnemies.FirstOrDefault();
    }
}
