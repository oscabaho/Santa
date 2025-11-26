using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseMaxHealthStrategy", menuName = "Santa/Upgrades/Strategies/Increase Max Health")]
public class IncreaseMaxHealthStrategy : UpgradeStrategySO
{
    public int HealthIncrease = 50;

    public override void Apply(IUpgradeTarget target)
    {
        target.IncreaseMaxHealth(HealthIncrease);
    }
}
