using System.Collections.Generic;

public interface IUpgradeService
{
    void PresentUpgradeOptions();
    void ApplyUpgrade(AbilityUpgrade upgrade);
    int MaxActionPoints { get; }
}
