using UnityEngine;

[CreateAssetMenu(fileName = "IncreaseEnergyGainStrategy", menuName = "Santa/Upgrades/Strategies/Increase Energy Gain")]
public class IncreaseEnergyGainStrategy : UpgradeStrategySO
{
    public int EnergyIncrease;

    public override void Apply(IUpgradeTarget target)
    {
        target.IncreaseEnergyGainedPerTurn(EnergyIncrease);
    }
}
