using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the contract for a service that executes a single combat action.
/// </summary>
public interface IActionExecutor
{
    /// <summary>
    /// Executes a given pending action.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="allCombatants">A read-only list of all combatants.</param>
    /// <param name="healthCache">A cache of health components for performance.</param>
    /// <param name="upgradeService">Service providing player stats from upgrades.</param>
    void Execute(PendingAction action, IReadOnlyList<GameObject> allCombatants, IReadOnlyDictionary<GameObject, IHealthController> healthCache, IUpgradeService upgradeService);
}

/// <summary>
/// Handles the logic for executing a single combat action.
/// </summary>
public class ActionExecutor : MonoBehaviour, IActionExecutor
{
    public void Execute(PendingAction action, IReadOnlyList<GameObject> allCombatants, IReadOnlyDictionary<GameObject, IHealthController> healthCache, IUpgradeService upgradeService)
    {
        if (action.Caster == null)
        {
            GameLog.LogWarning("Skipping action: caster is null.");
            return;
        }

        if (action.Ability == null)
        {
            GameLog.LogWarning($"Skipping action from {action.Caster.name}: Ability is null.");
            return;
        }

        if (!healthCache.TryGetValue(action.Caster, out var casterHealth))
        {
            GameLog.LogWarning($"{action.Caster.name} has no cached IHealthController; skipping action {action.Ability.AbilityName}.");
            return;
        }

        if (casterHealth.CurrentValue <= 0)
        {
            GameLog.Log($"{action.Caster.name} is defeated and cannot perform {action.Ability.AbilityName}.");
            return;
        }

        // Validate target and retarget if dead (for SingleEnemy targeting)
        GameObject validatedTarget = ValidateAndRetarget(action, allCombatants, healthCache);

        // Create updated action with validated target
        var validatedAction = new PendingAction
        {
            Ability = action.Ability,
            Caster = action.Caster,
            PrimaryTarget = validatedTarget
        };

        // Use local list to avoid any potential shared state issues with async execution
        var targetList = new List<GameObject>(8);
        if (validatedAction.Ability.Targeting != null)
        {
            validatedAction.Ability.Targeting.ResolveTargets(validatedAction.Caster, validatedAction.PrimaryTarget, allCombatants, targetList, validatedAction.Ability);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            string targetNames = targetList.Count > 0
                ? string.Join(", ", System.Linq.Enumerable.Select(targetList, t => t != null ? t.name : "NULL"))
                : "NONE";
            GameLog.Log($"[ActionExecutor] {validatedAction.Caster.name} resolved {targetList.Count} target(s): {targetNames}");
#endif
        }
        else
        {
            GameLog.LogWarning($"Ability {validatedAction.Ability.AbilityName} has no Targeting strategy assigned!");
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"[ActionExecutor] About to execute {validatedAction.Ability.AbilityName} with {targetList.Count} target(s)");
#endif

        try
        {
            validatedAction.Ability.Execute(targetList, validatedAction.Caster, upgradeService, allCombatants);
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"Exception while executing ability {validatedAction.Ability.AbilityName} from {validatedAction.Caster.name}: {ex}");
        }
    }

    /// <summary>
    /// Validates if the primary target is alive, and finds a replacement if dead.
    /// Only applies to SingleEnemy targeting style.
    /// </summary>
    private GameObject ValidateAndRetarget(PendingAction action, IReadOnlyList<GameObject> allCombatants, IReadOnlyDictionary<GameObject, IHealthController> healthCache)
    {
        GameObject originalTarget = action.PrimaryTarget;

        // If no primary target or not SingleEnemy targeting, return as-is
        if (originalTarget == null || action.Ability.Targeting == null ||
            action.Ability.Targeting.Style != TargetingStyle.SingleEnemy)
        {
            return originalTarget;
        }

        // Check if original target is still alive
        if (healthCache.TryGetValue(originalTarget, out var targetHealth) && targetHealth.CurrentValue > 0)
        {
            return originalTarget; // Target is alive, use it
        }

        // Target is dead, find replacement with highest HP
        GameObject newTarget = FindHighestHPEnemy(action.Caster, allCombatants, healthCache);

        if (newTarget != null && newTarget != originalTarget)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"[ActionExecutor] {action.Caster.name}'s original target {originalTarget.name} is dead. Re-targeting to {newTarget.name}.");
#endif

            // Show feedback to player
            if (action.Caster.CompareTag(GameConstants.Tags.Player))
            {
                GameLog.Log($"Target {originalTarget.name} defeated! Auto-targeting {newTarget.name} (highest HP).");
            }
        }
        else if (newTarget == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"[ActionExecutor] {action.Caster.name}'s target {originalTarget.name} is dead and no valid replacement found.");
#endif
        }

        return newTarget;
    }

    /// <summary>
    /// Finds the enemy with the highest HP from the opposite side of the caster.
    /// </summary>
    private GameObject FindHighestHPEnemy(GameObject caster, IReadOnlyList<GameObject> allCombatants, IReadOnlyDictionary<GameObject, IHealthController> healthCache)
    {
        string enemyTag = caster.CompareTag(GameConstants.Tags.Player) ? GameConstants.Tags.Enemy : GameConstants.Tags.Player;

        GameObject highestHPEnemy = null;
        int maxHP = 0;

        for (int i = 0; i < allCombatants.Count; i++)
        {
            var combatant = allCombatants[i];
            if (combatant != null && combatant.activeInHierarchy && combatant.CompareTag(enemyTag))
            {
                if (healthCache.TryGetValue(combatant, out var health) && health.CurrentValue > 0)
                {
                    if (health.CurrentValue > maxHP)
                    {
                        maxHP = health.CurrentValue;
                        highestHPEnemy = combatant;
                    }
                }
            }
        }

        return highestHPEnemy;
    }
}
