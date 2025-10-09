using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomEnemiesTargeting", menuName = "Santa/Abilities/Targeting/Random Enemies")]
public class RandomEnemiesTargeting : TargetingStrategy
{
    private readonly System.Random _rng = new System.Random();
    private readonly List<GameObject> _enemyPool = new List<GameObject>(8);

    public override TargetingStyle Style => TargetingStyle.SingleEnemy; // Or a new style like MultiEnemy if appropriate

    public override void ResolveTargets(GameObject caster, GameObject primaryTarget, IReadOnlyList<GameObject> allCombatants, List<GameObject> results, Ability ability)
    {
        _enemyPool.Clear();
        foreach (var combatant in allCombatants)
        {
            if (combatant != null && combatant.CompareTag("Enemy"))
            {
                _enemyPool.Add(combatant);
            }
        }

        if (primaryTarget != null && primaryTarget.activeInHierarchy && _enemyPool.Contains(primaryTarget))
        {
            results.Add(primaryTarget);
            _enemyPool.Remove(primaryTarget);
        }

        if (ability.TargetPercentage <= 0) return;

        // Calculate how many targets to hit in total, based on the original number of enemies
        int totalToHit = Mathf.CeilToInt((_enemyPool.Count + results.Count) * ability.TargetPercentage);
        int additionalTargetsToHit = totalToHit - results.Count;

        if (additionalTargetsToHit > 0 && _enemyPool.Count > 0)
        {
            // Shuffle the remaining enemy pool
            for (int i = _enemyPool.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var temp = _enemyPool[i];
                _enemyPool[i] = _enemyPool[j];
                _enemyPool[j] = temp;
            }

            // Add the required number of additional targets
            for (int k = 0; k < additionalTargetsToHit && k < _enemyPool.Count; k++)
            {
                results.Add(_enemyPool[k]);
            }
        }
    }
}
