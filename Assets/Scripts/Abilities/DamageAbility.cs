using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A concrete Ability that deals a specified amount of damage to its targets.
/// The damage is determined by the UpgradeService based on the ability type.
/// </summary>
[CreateAssetMenu(fileName = "New Damage Ability", menuName = "Santa/Abilities/Damage Ability", order = 52)]
public class DamageAbility : Ability
{
    [Header("Damage Settings")]
    [Tooltip("Type of attack to determine which upgrade stat to use for damage.")]
    [SerializeField] private AbilityType abilityType = AbilityType.DirectAttack;

    public override void Execute(List<GameObject> targets, GameObject caster, IUpgradeService upgradeService)
    {
        if (targets == null) return;

        // Get damage from UpgradeService based on ability type
        int damage = 10; // Fallback default
        if (upgradeService != null)
        {
            damage = abilityType switch
            {
                AbilityType.DirectAttack => upgradeService.DirectAttackDamage,
                AbilityType.AreaAttack => upgradeService.AreaAttackDamage,
                _ => 10
            };
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"{caster.name} uses {AbilityName}!");
#endif

        // Use for loop instead of foreach for mobile performance
        for (int i = 0; i < targets.Count; i++)
        {
            GameObject target = targets[i];
            if (target == null) continue;

            if (target.TryGetComponent<HealthComponentBehaviour>(out var healthComponent))
            {
                // Check for critical hit (per-target, not shared)
                bool isCritical = RollCriticalHit(upgradeService);

                // Calculate final damage without mutating the base value
                int finalDamage = isCritical ? damage * 2 : damage;

                healthComponent.AffectValue(-finalDamage);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (isCritical)
                {
                    GameLog.Log($"CRITICAL HIT! {target.name} takes {finalDamage} damage.");
                }
                else
                {
                    GameLog.Log($"{target.name} takes {finalDamage} damage.");
                }
#endif
            }
        }
    }

    /// <summary>
    /// Rolls for a critical hit based on upgrade service critical chance.
    /// </summary>
    private bool RollCriticalHit(IUpgradeService upgradeService)
    {
        return upgradeService != null
            && upgradeService.CriticalHitChance > 0f
            && Random.value < upgradeService.CriticalHitChance;
    }
}
