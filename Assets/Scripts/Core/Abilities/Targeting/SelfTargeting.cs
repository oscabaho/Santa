using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SelfTargeting", menuName = "Santa/Abilities/Targeting/Self")]
public class SelfTargeting : TargetingStrategy
{
    public override bool RequiresTarget => false;

    public override void FindTargets(PendingAction action, List<GameObject> allies, List<GameObject> enemies, List<GameObject> finalTargets)
    {
        if (action.Caster != null)
        {
            finalTargets.Add(action.Caster);
        }
    }
}
