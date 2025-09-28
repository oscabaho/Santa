using UnityEngine;

[CreateAssetMenu(fileName = "ReduceSpecialMissChanceStrategy", menuName = "Santa/Upgrades/Strategies/Reduce Special Miss Chance")]
public class ReduceSpecialMissChanceStrategy : UpgradeStrategySO
{
    [Range(0f, 1f)]
    public float MissChanceReduction;

    public override void Apply(IUpgradeTarget target)
    {
        target.ReduceSpecialAttackMissChance(MissChanceReduction);
    }
}
