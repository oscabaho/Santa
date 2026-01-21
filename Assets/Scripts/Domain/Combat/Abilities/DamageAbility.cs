using System.Collections.Generic;
using Santa.Core;
using UnityEngine;
using VContainer;

namespace Santa.Domain.Combat
{
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

        // Static cache to avoid allocations during splash target calculation
        private static readonly List<GameObject> _splashTargetsCache = new List<GameObject>(10);

        public override void Execute(List<GameObject> targets, GameObject caster, IUpgradeService upgradeService, IReadOnlyList<GameObject> allCombatants, ICombatLogService combatLogService)
        {
            if (targets == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"{caster.name}: Execute called with null targets list!");
#endif
                return;
            }

            if (targets.Count == 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"{caster.name}: Execute called with empty targets list (Count=0)!");
#endif
                return;
            }

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
            combatLogService?.LogMessage($"{caster.name} uses {AbilityName}!", CombatLogType.Info);

            // Use for loop instead of foreach for mobile performance
            for (int i = 0; i < targets.Count; i++)
            {
                GameObject target = targets[i];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                string targetName = target != null ? target.name : "NULL";
                GameLog.Log($"[DamageAbility] Processing target [{i}]: {targetName}");
#endif
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
                    if (isCritical)
                    {
                        combatLogService?.LogMessage($"CRITICAL HIT! {target.name} takes {finalDamage} damage!", CombatLogType.Critical);
                    }
                    else
                    {
                        combatLogService?.LogMessage($"{target.name} takes {finalDamage} damage.", CombatLogType.Damage);
                    }
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                else
                {
                    GameLog.LogWarning($"[DamageAbility] {target.name} has NO HealthComponentBehaviour!");
                }
#endif
            }

            // Area Attack Splash Damage Logic
            if (abilityType == AbilityType.AreaAttack && targets.Count > 0 && allCombatants != null)
            {
                GameObject primaryTarget = targets[0];

                // Determine the tag of the primary target to find splash targets
                string targetTag = (primaryTarget != null) ? primaryTarget.tag : null;
                if (string.IsNullOrEmpty(targetTag))
                {
                    return; // No valid target tag, skip splash
                }

                // Build list of other combatants with the SAME tag as primary target
                // Use a static reusable list to avoid allocations, but clear it first
                _splashTargetsCache.Clear();

                for (int i = 0; i < allCombatants.Count; i++)
                {
                    var combatant = allCombatants[i];
                    if (combatant != null
                        && combatant.activeInHierarchy
                        && combatant.CompareTag(targetTag) // Same tag as primary target
                        && combatant != primaryTarget
                        && combatant != caster) // Don't splash the caster
                    {
                        // Check if still alive
                        if (combatant.TryGetComponent<HealthComponentBehaviour>(out var health) && health.CurrentValue > 0)
                        {
                            _splashTargetsCache.Add(combatant);
                        }
                    }
                }

                // If there are other valid targets, deal splash damage to one random target
                if (_splashTargetsCache.Count > 0)
                {
                    int randomIndex = Random.Range(0, _splashTargetsCache.Count);
                    GameObject splashTarget = _splashTargetsCache[randomIndex];

                    if (splashTarget.TryGetComponent<HealthComponentBehaviour>(out var splashHealth))
                    {
                        // Splash damage is 50% of base damage (no critical on splash)
                        int splashDamage = damage / 2;
                        splashHealth.AffectValue(-splashDamage);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.Log($"[SPLASH] {splashTarget.name} takes {splashDamage} area damage!");
#endif
                        combatLogService?.LogMessage($"[SPLASH] {splashTarget.name} takes {splashDamage} area damage!", CombatLogType.Damage);
                    }
                }
            }
        }


    }
}
