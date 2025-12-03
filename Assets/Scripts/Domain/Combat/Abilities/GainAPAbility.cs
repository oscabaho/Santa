using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gain AP Ability", menuName = "Santa/Abilities/Gain AP Ability")]
public class GainAPAbility : Ability
{
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
            }
        }
    }
}
