using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseCriticalHitChanceStrategy", menuName = "Santa/Upgrades/Strategies/Increase Critical Hit Chance")]
public class IncreaseCriticalHitChanceStrategy : UpgradeStrategySO
{
    [Range(0f, 1f)]
    public float CritChanceIncrease = 0.1f;

    public override void Apply(IUpgradeTarget target)
    {
        target.IncreaseCriticalHitChance(CritChanceIncrease);
    }
}
