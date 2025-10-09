using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AllEnemiesTargeting", menuName = "Santa/Abilities/Targeting/All Enemies")]
public class AllEnemiesTargeting : TargetingStrategy
{
    public override TargetingStyle Style => TargetingStyle.AllEnemies;

    public override void ResolveTargets(GameObject caster, GameObject primaryTarget, IReadOnlyList<GameObject> allCombatants, List<GameObject> results, Ability ability)
    {
        foreach (var combatant in allCombatants)
        {
            if (combatant != null && combatant.CompareTag("Enemy"))
            {
                results.Add(combatant);
            }
        }
    }
}
