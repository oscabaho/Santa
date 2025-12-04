namespace Santa.Domain.Upgrades
{
    /// <summary>
    /// Defines the contract for any object that can have upgrades applied to it.
    /// This interface is used by UpgradeStrategySO to apply upgrades without
    /// depending directly on the UpgradeManager.
    /// </summary>
    public interface IUpgradeTarget
    {
        void IncreaseDirectAttackDamage(int amount);
        void IncreaseAreaAttackDamage(int amount);
        void IncreaseSpecialAttackDamage(int amount);
        void IncreaseAPRecoveryAmount(int amount);
        void ReduceSpecialAttackMissChance(float amount);
        void IncreaseMaxActionPoints(int amount);
        void IncreaseMaxHealth(int amount);
        void IncreaseGlobalAPCostReduction(int amount);
        void IncreaseGlobalActionSpeed(int amount);
        void IncreaseCriticalHitChance(float amount);
    }
}
