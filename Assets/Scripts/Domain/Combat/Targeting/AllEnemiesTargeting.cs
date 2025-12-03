using System.Collections.Generic;
using UnityEngine;
using Santa.Core;
using Santa.Core.Config;

namespace Santa.Domain.Combat
{
    [CreateAssetMenu(fileName = "AllEnemiesTargeting", menuName = "Santa/Abilities/Targeting/All Enemies")]
    public class AllEnemiesTargeting : TargetingStrategy
    {
        public override TargetingStyle Style => TargetingStyle.AllEnemies;

        public override void ResolveTargets(GameObject caster, GameObject primaryTarget, IReadOnlyList<GameObject> allCombatants, List<GameObject> results, Ability ability)
        {
            // Determine the enemy tag based on the caster's tag
            string enemyTag = caster.CompareTag(GameConstants.Tags.Player) ? GameConstants.Tags.Enemy : GameConstants.Tags.Player;

            foreach (var combatant in allCombatants)
            {
                if (combatant != null && combatant.activeInHierarchy && combatant.CompareTag(enemyTag))
                {
                    // Only add living enemies to the target list
                    if (combatant.TryGetComponent<IHealthController>(out var health) && health.CurrentValue > 0)
                    {
                        results.Add(combatant);
                    }
                }
            }
        }
    }
}
