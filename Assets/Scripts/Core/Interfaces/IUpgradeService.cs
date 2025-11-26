using System.Collections.Generic;

public interface IUpgradeService
{
    void PresentUpgradeOptions();
    void ApplyUpgrade(AbilityUpgrade upgrade);

    // Player stats exposed for abilities to use
    int DirectAttackDamage { get; }
    int AreaAttackDamage { get; }
    int SpecialAttackDamage { get; }
    float SpecialAttackMissChance { get; }
    int APRecoveryAmount { get; }
    int MaxActionPoints { get; }
    int MaxHealth { get; }
    int GlobalAPCostReduction { get; }
    int GlobalActionSpeedBonus { get; }
    float CriticalHitChance { get; }
}
