using UnityEngine;

namespace Santa.Domain.Upgrades
{
    [CreateAssetMenu(fileName = "ReduceAPCostStrategy", menuName = "Santa/Upgrades/Strategies/Reduce AP Cost")]
    public class ReduceAPCostStrategy : UpgradeStrategySO
    {
        public int CostReduction = 1;

        public override void Apply(IUpgradeTarget target)
        {
            target.IncreaseGlobalAPCostReduction(CostReduction);
        }
    }
}
