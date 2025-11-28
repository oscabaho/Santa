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

    public override void Execute(List<GameObject> targets, GameObject caster, IUpgradeService upgradeService, IReadOnlyList<GameObject> allCombatants)
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

        // Area Attack Splash Damage Logic
        if (abilityType == AbilityType.AreaAttack && targets.Count > 0 && allCombatants != null)
        {
            GameObject primaryTarget = targets[0];

            // Build list of other active enemies
            var otherEnemies = new List<GameObject>(3);
            for (int i = 0; i < allCombatants.Count; i++)
            {
                var combatant = allCombatants[i];
                if (combatant != null
                    && combatant.activeInHierarchy
                    && combatant.CompareTag(GameConstants.Tags.Enemy)
                    && combatant != primaryTarget)
                {
                    // Check if still alive
                    if (combatant.TryGetComponent<HealthComponentBehaviour>(out var health) && health.CurrentValue > 0)
                    {
                        otherEnemies.Add(combatant);
                    }
                }
            }

            // If there are other enemies, deal splash damage to one random enemy
            if (otherEnemies.Count > 0)
            {
                int randomIndex = Random.Range(0, otherEnemies.Count);
                GameObject splashTarget = otherEnemies[randomIndex];

                if (splashTarget.TryGetComponent<HealthComponentBehaviour>(out var splashHealth))
                {
                    // Splash damage is 50% of base damage (no critical on splash)
                    int splashDamage = damage / 2;
                    splashHealth.AffectValue(-splashDamage);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log($"[SPLASH] {splashTarget.name} takes {splashDamage} area damage!");
#endif
                }
            }
        }
    }


}
