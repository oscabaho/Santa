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
    private readonly List<GameObject> _reusableTargetList = new List<GameObject>(8);

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

        _reusableTargetList.Clear();
        if (action.Ability.Targeting != null)
        {
            action.Ability.Targeting.ResolveTargets(action.Caster, action.PrimaryTarget, allCombatants, _reusableTargetList, action.Ability);
        }
        else
        {
            GameLog.LogWarning($"Ability {action.Ability.AbilityName} has no Targeting strategy assigned!");
        }

        try
        {
            action.Ability.Execute(_reusableTargetList, action.Caster, upgradeService);
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"Exception while executing ability {action.Ability.AbilityName} from {action.Caster.name}: {ex}");
        }
    }
}
