using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllEnemiesTargeting", menuName = "Santa/Abilities/Targeting/All Enemies")]
public class AllEnemiesTargeting : TargetingStrategy
{
    public override bool RequiresTarget => false;

    public override void FindTargets(PendingAction action, List<GameObject> allies, List<GameObject> enemies, List<GameObject> finalTargets)
    {
        bool isPlayerSide = action.Caster != null && (action.Caster.CompareTag("Player") || action.Caster.CompareTag("Ally"));
        List<GameObject> potentialTargets = isPlayerSide ? enemies : allies;

        finalTargets.AddRange(potentialTargets);
    }
}
