using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseActionSpeedStrategy", menuName = "Santa/Upgrades/Strategies/Increase Action Speed")]
public class IncreaseActionSpeedStrategy : UpgradeStrategySO
{
    public int SpeedBonus = 20;

    public override void Apply(IUpgradeTarget target)
    {
        target.IncreaseGlobalActionSpeed(SpeedBonus);
    }
}
