using UnityEngine;

namespace Santa.Core.Upgrades
{
    /// <summary>
    /// Encapsulates player stats and their modification logic.
    /// Separates data management from the UpgradeManager service.
    /// </summary>
    [System.Serializable]
    public class UpgradeStatsContainer : IUpgradeTarget
    {
        // Player Stats
        public int DirectAttackDamage { get; private set; }
        public int AreaAttackDamage { get; private set; }
        public int SpecialAttackDamage { get; private set; }
        public float SpecialAttackMissChance { get; private set; }
        public int APRecoveryAmount { get; private set; }
        public int MaxActionPoints { get; private set; }
        public int MaxHealth { get; private set; }
        public int GlobalAPCostReduction { get; private set; }
        public int GlobalActionSpeedBonus { get; private set; }
        public float CriticalHitChance { get; private set; }

        public void InitializeFromConfig(PlayerStatsConfig config)
        {
            if (config != null)
            {
                DirectAttackDamage = config.DirectAttackDamage;
                AreaAttackDamage = config.AreaAttackDamage;
                SpecialAttackDamage = config.SpecialAttackDamage;
                SpecialAttackMissChance = config.SpecialAttackMissChance;
                APRecoveryAmount = config.APRecoveryAmount;
                MaxActionPoints = config.MaxActionPoints;
                MaxHealth = config.MaxHealth;
                GlobalAPCostReduction = config.GlobalAPCostReduction;
                GlobalActionSpeedBonus = config.GlobalActionSpeedBonus;
                CriticalHitChance = config.BaseCriticalHitChance;
            }
            else
            {
                InitializeDefaults();
            }
        }

        public void InitializeDefaults()
        {
            // Fallback to constants from GameConstants.PlayerStats
            DirectAttackDamage = GameConstants.PlayerStats.DefaultDirectAttackDamage;
            AreaAttackDamage = GameConstants.PlayerStats.DefaultAreaAttackDamage;
            SpecialAttackDamage = GameConstants.PlayerStats.DefaultSpecialAttackDamage;
            SpecialAttackMissChance = GameConstants.PlayerStats.DefaultSpecialAttackMissChance;
            APRecoveryAmount = GameConstants.PlayerStats.DefaultAPRecoveryAmount;
            MaxActionPoints = GameConstants.PlayerStats.DefaultMaxActionPoints;
            MaxHealth = GameConstants.PlayerStats.DefaultMaxHealth;
            GlobalAPCostReduction = GameConstants.PlayerStats.DefaultGlobalAPCostReduction;
            GlobalActionSpeedBonus = GameConstants.PlayerStats.DefaultGlobalActionSpeedBonus;
            CriticalHitChance = GameConstants.PlayerStats.DefaultBaseCriticalHitChance;
        }

        // --- IUpgradeTarget Implementation ---

        public void IncreaseDirectAttackDamage(int amount) => DirectAttackDamage += amount;
        public void IncreaseAreaAttackDamage(int amount) => AreaAttackDamage += amount;
        public void IncreaseSpecialAttackDamage(int amount) => SpecialAttackDamage += amount;
        public void ReduceSpecialAttackMissChance(float amount) => SpecialAttackMissChance = Mathf.Max(0f, SpecialAttackMissChance - amount);
        public void IncreaseAPRecoveryAmount(int amount) => APRecoveryAmount += amount;
        public void IncreaseMaxActionPoints(int amount) => MaxActionPoints += amount;
        public void IncreaseMaxHealth(int amount) => MaxHealth += amount;
        public void IncreaseGlobalAPCostReduction(int amount) => GlobalAPCostReduction += amount;
        public void IncreaseGlobalActionSpeed(int amount) => GlobalActionSpeedBonus += amount;
        public void IncreaseCriticalHitChance(float amount) => CriticalHitChance += amount;
    }
}
