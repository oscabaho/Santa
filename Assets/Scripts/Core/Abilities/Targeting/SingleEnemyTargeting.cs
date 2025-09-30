using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SingleEnemyTargeting", menuName = "Santa/Abilities/Targeting/Single Enemy")]
public class SingleEnemyTargeting : TargetingStrategy
{
    public override bool RequiresTarget => true;

    public override void FindTargets(PendingAction action, List<GameObject> allies, List<GameObject> enemies, List<GameObject> finalTargets)
    {
        if (action.PrimaryTarget != null && action.PrimaryTarget.activeInHierarchy)
        {
            finalTargets.Add(action.PrimaryTarget);
        }
    }
}
