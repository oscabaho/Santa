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
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"{gameObject.name} could not find a valid target.");
            #endif
            return new PendingAction();
        }

        // 2. Choose an Ability to use on that target
        // (Simple AI: use the most expensive affordable ability)
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
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"{gameObject.name} decides to use {chosenAbility.AbilityName} on {primaryTarget.name}.");
            #endif
            return new PendingAction
            {
                Ability = chosenAbility,
                Caster = gameObject,
                PrimaryTarget = primaryTarget
            };
        }

    #if UNITY_EDITOR || DEVELOPMENT_BUILD
    GameLog.Log($"{gameObject.name} cannot afford any abilities.");
    #endif
    return new PendingAction();
    }

    private GameObject FindBestTarget(
        PendingAction? playerAction,
        List<GameObject> allEnemies)
    {
        var activeEnemiesWithHealth = new List<(GameObject Enemy, HealthComponentBehaviour Health)>();
        for (int i = 0; i < allEnemies.Count; i++)
        {
            var e = allEnemies[i];
            if (e != null && e.activeInHierarchy)
            {
                var health = e.GetComponent<HealthComponentBehaviour>();
                if (health != null)
                {
                    activeEnemiesWithHealth.Add((e, health));
                }
            }
        }

        if (activeEnemiesWithHealth.Count == 0)
        {
            return null;
        }

        // Find the minimum health among all active enemies
        int minHealth = int.MaxValue;
        for (int i = 0; i < activeEnemiesWithHealth.Count; i++)
        {
            int h = activeEnemiesWithHealth[i].Health.CurrentValue;
            if (h < minHealth) minHealth = h;
        }

        // Get all enemies that are tied for the lowest health
        var lowestHealthEnemies = new List<GameObject>();
        for (int i = 0; i < activeEnemiesWithHealth.Count; i++)
        {
            if (activeEnemiesWithHealth[i].Health.CurrentValue == minHealth)
            {
                lowestHealthEnemies.Add(activeEnemiesWithHealth[i].Enemy);
            }
        }

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
                for (int i = 0; i < lowestHealthEnemies.Count; i++)
                {
                    if (lowestHealthEnemies[i] == playerTarget)
                    {
                        #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.Log($"{gameObject.name} will focus fire on player's target: {playerTarget.name}");
                        #endif
                        return playerTarget; // Focus fire!
                    }
                }
            }

            // Rule 2b: Player did not target a tied enemy, or hasn't acted. Choose randomly.
            int randomIndex = Random.Range(0, lowestHealthEnemies.Count);
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"{gameObject.name} chooses a random target among tied-health enemies.");
            #endif
            return lowestHealthEnemies[randomIndex];
        }

        // Fallback, should not be reached if there are any active enemies
        return activeEnemiesWithHealth.Count > 0 ? activeEnemiesWithHealth[0].Enemy : null;
    }
}
