using System.Collections.Generic;
using UnityEngine;
using Santa.Core;
using VContainer;

namespace Santa.Domain.Combat
{
    [CreateAssetMenu(fileName = "New Gain AP Ability", menuName = "Santa/Abilities/Gain AP Ability")]
    public class GainAPAbility : Ability
{
    private static ICombatLogService _combatLog;

    [Inject]
    public void Construct(ICombatLogService combatLogService)
    {
        _combatLog = combatLogService;
    }
    public override void Execute(List<GameObject> targets, GameObject caster, IUpgradeService upgradeService, IReadOnlyList<GameObject> allCombatants)
    {
        // Get energy gained from UpgradeService
        int amountToGain = upgradeService?.APRecoveryAmount ?? 34;

        // Use for loop instead of foreach for mobile performance
        for (int i = 0; i < targets.Count; i++)
        {
            GameObject target = targets[i];
            if (target == null) continue;

            if (target.TryGetComponent<ActionPointComponentBehaviour>(out var apComponent))
            {
                apComponent.AffectValue(amountToGain);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"{target.name} gained {amountToGain} AP.");
#endif
                _combatLog?.LogMessage($"{target.name} gained {amountToGain} AP.", CombatLogType.ActionPoints);
            }
        }
    }
    }
}
