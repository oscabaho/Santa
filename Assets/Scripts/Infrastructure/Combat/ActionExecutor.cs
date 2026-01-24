using System.Collections.Generic;
using Santa.Core;
using Santa.Core.Config;
using Santa.Domain.Combat;
using UnityEngine;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Handles the logic for executing a single combat action.
    /// </summary>
    public class ActionExecutor : MonoBehaviour, IActionExecutor
    {
        // Reusable list to avoid allocations during Execute
        private readonly List<GameObject> _targetList = new(8);

        public void Execute(PendingAction action, IReadOnlyList<GameObject> allCombatants, IReadOnlyDictionary<GameObject, IHealthController> healthCache, IUpgradeService upgradeService, ICombatLogService combatLogService)
        {
            if (action.Caster == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("Skipping action: caster is null.");
#endif
                return;
            }

            if (action.Ability == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"Skipping action from {action.Caster.name}: Ability is null.");
#endif
                return;
            }

            if (!healthCache.TryGetValue(action.Caster, out var casterHealth))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"{action.Caster.name} has no cached IHealthController; skipping action {action.Ability.AbilityName}.");
#endif
                return;
            }

            if (casterHealth.CurrentValue <= 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"{action.Caster.name} is defeated and cannot perform {action.Ability.AbilityName}.");
#endif
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
            _targetList.Clear();
            if (validatedAction.Ability.Targeting != null)
            {
                validatedAction.Ability.Targeting.ResolveTargets(validatedAction.Caster, validatedAction.PrimaryTarget, allCombatants, _targetList, validatedAction.Ability);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                System.Text.StringBuilder sb = new();
                for (int i = 0; i < _targetList.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(_targetList[i] != null ? _targetList[i].name : "NULL");
                }
                string targetNames = _targetList.Count > 0 ? sb.ToString() : "NONE";
                GameLog.Log($"[ActionExecutor] {validatedAction.Caster.name} resolved {_targetList.Count} target(s): {targetNames}");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"Ability {validatedAction.Ability.AbilityName} has no Targeting strategy assigned!");
#endif
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"[ActionExecutor] About to execute {validatedAction.Ability.AbilityName} with {_targetList.Count} target(s)");
#endif

            try
            {
                // Trigger animation if the caster has a controller
                if (validatedAction.Caster.TryGetComponent<Santa.Presentation.Combat.CombatAnimationController>(out var animController))
                {
                    animController.TriggerAttack();
                }

                validatedAction.Ability.Execute(_targetList, validatedAction.Caster, upgradeService, allCombatants, combatLogService);
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError($"Exception while executing ability {validatedAction.Ability.AbilityName} from {validatedAction.Caster.name}: {ex}");
#else
                _ = ex;
#endif
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log($"Target {originalTarget.name} defeated! Auto-targeting {newTarget.name} (highest HP).");
#endif
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
}
