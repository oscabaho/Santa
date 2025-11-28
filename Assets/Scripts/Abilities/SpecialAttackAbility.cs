using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A special, powerful attack that costs energy and has a chance to miss.
/// Damage and miss chance are determined by the UpgradeService.
/// </summary>
[CreateAssetMenu(fileName = "New Special Attack", menuName = "Santa/Abilities/Special Attack Ability", order = 53)]
public class SpecialAttackAbility : Ability
{
    public override void Execute(List<GameObject> targets, GameObject caster, IUpgradeService upgradeService, IReadOnlyList<GameObject> allCombatants)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"{caster.name} attempts a Special Attack: {AbilityName}!");
#endif

        // Get miss chance and damage from UpgradeService
        float missChance = upgradeService?.SpecialAttackMissChance ?? 0.2f;
        int baseDamage = upgradeService?.SpecialAttackDamage ?? 75;

        if (Random.value < missChance)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("...but it MISSED!");
#endif
            return;
        }

        if (targets == null) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("It's a direct hit!");
#endif

        // Use for loop instead of foreach for mobile performance
        for (int i = 0; i < targets.Count; i++)
        {
            GameObject target = targets[i];
            if (target == null) continue;

            if (target.TryGetComponent<HealthComponentBehaviour>(out var healthComponent))
            {
                // Check for critical hit
                bool isCritical = RollCriticalHit(upgradeService);
                int finalDamage = isCritical ? baseDamage * 2 : baseDamage;

                healthComponent.AffectValue(-finalDamage);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (isCritical)
                {
                    GameLog.Log($"DEVASTATING CRITICAL! {target.name} takes {finalDamage} damage!");
                }
                else
                {
                    GameLog.Log($"{target.name} takes a massive {finalDamage} damage!");
                }
#endif
            }
        }
    }


}
