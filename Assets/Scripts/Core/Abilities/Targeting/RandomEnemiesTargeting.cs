using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomEnemiesTargeting", menuName = "Santa/Abilities/Targeting/Random Enemies")]
public class RandomEnemiesTargeting : TargetingStrategy
{
    private readonly System.Random _rng = new System.Random();
    private readonly List<GameObject> _randomTargetPool = new List<GameObject>(8);

    public override bool RequiresTarget => true;

    public override void FindTargets(PendingAction action, List<GameObject> allies, List<GameObject> enemies, List<GameObject> finalTargets)
    {
        bool isPlayerSide = action.Caster != null && (action.Caster.CompareTag("Player") || action.Caster.CompareTag("Ally"));
        List<GameObject> potentialTargets = isPlayerSide ? enemies : allies;

        if (action.PrimaryTarget != null && action.PrimaryTarget.activeInHierarchy)
        {
            finalTargets.Add(action.PrimaryTarget);
            
            _randomTargetPool.Clear();
            for (int i = 0; i < potentialTargets.Count; i++)
            {
                if (potentialTargets[i] != action.PrimaryTarget)
                {
                    _randomTargetPool.Add(potentialTargets[i]);
                }
            }
            potentialTargets = _randomTargetPool;
        }

        int totalToHit = Mathf.CeilToInt(potentialTargets.Count * action.Ability.TargetPercentage);
        int additionalTargetsToHit = totalToHit - finalTargets.Count;

        if (additionalTargetsToHit > 0 && potentialTargets.Count > 0)
        {
            for (int i = potentialTargets.Count - 1; i > 0; i--)
            {
                int j = _rng.Next(i + 1);
                var tmp = potentialTargets[i];
                potentialTargets[i] = potentialTargets[j];
                potentialTargets[j] = tmp;
            }
            for (int k = 0; k < additionalTargetsToHit && k < potentialTargets.Count; k++)
            {
                finalTargets.Add(potentialTargets[k]);
            }
        }
    }
}
