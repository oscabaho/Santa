using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SingleEnemyTargeting", menuName = "Santa/Abilities/Targeting/Single Enemy")]
public class SingleEnemyTargeting : TargetingStrategy
{
    public override TargetingStyle Style => TargetingStyle.SingleEnemy;

    public override void ResolveTargets(GameObject caster, GameObject primaryTarget, IReadOnlyList<GameObject> allCombatants, List<GameObject> results, Ability ability)
    {
        if (primaryTarget != null && primaryTarget.activeInHierarchy)
        {
            results.Add(primaryTarget);
        }
    }
}
