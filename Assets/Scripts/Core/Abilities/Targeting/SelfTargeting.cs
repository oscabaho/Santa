using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SelfTargeting", menuName = "Santa/Abilities/Targeting/Self")]
public class SelfTargeting : TargetingStrategy
{
    public override TargetingStyle Style => TargetingStyle.Self;

    public override void ResolveTargets(GameObject caster, GameObject primaryTarget, IReadOnlyList<GameObject> allCombatants, List<GameObject> results, Ability ability)
    {
        if (caster != null)
        {
            results.Add(caster);
        }
    }
}
