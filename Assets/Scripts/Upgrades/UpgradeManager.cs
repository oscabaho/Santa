using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VContainer;

/// <summary>
/// Manages the player's permanent progression by applying and saving ability upgrades.
/// </summary>
public class UpgradeManager : MonoBehaviour, IUpgradeService, IUpgradeTarget, Santa.Core.Save.ISaveContributor
{
    [Header("Upgrade Pool")]
    [Tooltip("A list of all possible upgrades that can be offered to the player.")]
    [SerializeField] private List<AbilityUpgrade> allPossibleUpgrades;

    [Header("Base Stats Configuration")]
    [Tooltip("ScriptableObject containing all base stat values for easy balancing.")]
    [SerializeField] private PlayerStatsConfig baseStatsConfig;

    private IUpgradeUI _upgradeUI;
    private ICombatTransitionService _combatTransitionService;

    // Tracking only the LAST selected upgrade to exclude it from next selection
    private string _lastSelectedUpgrade = null;
    // Track all acquired upgrades for persistence
    private readonly List<string> _acquiredUpgrades = new List<string>();

    // Player Stats - These will be modified by upgrades.
    // Initialized from baseStatsConfig.
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

    private void Awake()
    {
        // Initialize stats from config
        if (baseStatsConfig != null)
        {
            DirectAttackDamage = baseStatsConfig.DirectAttackDamage;
            AreaAttackDamage = baseStatsConfig.AreaAttackDamage;
            SpecialAttackDamage = baseStatsConfig.SpecialAttackDamage;
            SpecialAttackMissChance = baseStatsConfig.SpecialAttackMissChance;
            APRecoveryAmount = baseStatsConfig.APRecoveryAmount;
            MaxActionPoints = baseStatsConfig.MaxActionPoints;
            MaxHealth = baseStatsConfig.MaxHealth;
            GlobalAPCostReduction = baseStatsConfig.GlobalAPCostReduction;
            GlobalActionSpeedBonus = baseStatsConfig.GlobalActionSpeedBonus;
            CriticalHitChance = baseStatsConfig.BaseCriticalHitChance;
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("UpgradeManager: PlayerStatsConfig is not assigned! Using hardcoded defaults.");
#endif
            // Fallback to hardcoded values
            DirectAttackDamage = 25;
            AreaAttackDamage = 10;
            SpecialAttackDamage = 75;
            SpecialAttackMissChance = 0.2f;
            APRecoveryAmount = 34;
            MaxActionPoints = 100;
            MaxHealth = 100;
            GlobalAPCostReduction = 0;
            GlobalActionSpeedBonus = 0;
            CriticalHitChance = 0.1f;
        }
    }

    [Inject]
    public void Construct(IUpgradeUI upgradeUI, ICombatTransitionService combatTransitionService)
    {
        _upgradeUI = upgradeUI;
        _combatTransitionService = combatTransitionService;
    }

    private void Start()
    {
        LoadStats();
    }

    public void PresentUpgradeOptions()
    {
        if (allPossibleUpgrades == null || allPossibleUpgrades.Count < 2)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("Not enough upgrades defined in UpgradeManager to offer a choice.");
#endif
            return;
        }

        // Filter out the last selected upgrade to encourage variety
        var availableUpgrades = new List<AbilityUpgrade>(allPossibleUpgrades);
        if (!string.IsNullOrEmpty(_lastSelectedUpgrade))
        {
            availableUpgrades.RemoveAll(u => u.UpgradeName == _lastSelectedUpgrade);
        }

        // Ensure there are at least two upgrades to choose from to prevent an infinite loop.
        if (availableUpgrades.Count < 2)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"Not enough unique upgrades to offer a choice. Only {availableUpgrades.Count} available.");
#endif
            // If there's at least one, we could offer it, but for now, we just end combat gracefully.
            _combatTransitionService?.EndCombat(true);
            return;
        }

        // Pick 2 distinct random upgrades
        int index1 = Random.Range(0, availableUpgrades.Count);
        int index2;
        do
        {
            index2 = Random.Range(0, availableUpgrades.Count);
        } while (index2 == index1);

        AbilityUpgrade option1 = availableUpgrades[index1];
        AbilityUpgrade option2 = availableUpgrades[index2];

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Offering upgrades: {option1.UpgradeName} vs {option2.UpgradeName}");
#endif
        _upgradeUI.ShowUpgrades(option1, option2);
    }

    public void ApplyUpgrade(AbilityUpgrade upgrade)
    {
        if (upgrade == null) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Applying upgrade: {upgrade.UpgradeName}");
#endif

        // Apply the strategy to THIS manager (which holds the stats)
        upgrade.Strategy.Apply(this);

        // Track selection
        _lastSelectedUpgrade = upgrade.UpgradeName;
        if (!_acquiredUpgrades.Contains(upgrade.UpgradeName))
        {
            _acquiredUpgrades.Add(upgrade.UpgradeName);
        }

        SaveStats();
    }

    private void SaveStats()
    {
        Santa.Core.Security.SecureStorage.SetString(Santa.Core.Config.GameKeys.LastUpgrade, _lastSelectedUpgrade);
    }

    private void LoadStats()
    {
        if (Santa.Core.Security.SecureStorage.TryGetString(Santa.Core.Config.GameKeys.LastUpgrade, out var stored))
        {
            _lastSelectedUpgrade = stored;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"Last selected upgrade loaded: {_lastSelectedUpgrade}");
#endif
        }
    }

    // Helper method for testing/debugging
    public void ResetLastSelectedUpgrade()
    {
        _lastSelectedUpgrade = null;
        Santa.Core.Security.SecureStorage.Delete(Santa.Core.Config.GameKeys.LastUpgrade);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("Reset last selected upgrade.");
#endif
    }

    // --- IUpgradeTarget Implementation ---

    public void IncreaseDirectAttackDamage(int amount)
    {
        DirectAttackDamage += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"DirectAttackDamage increased by {amount}. New Value: {DirectAttackDamage}");
#endif
    }

    public void IncreaseAreaAttackDamage(int amount)
    {
        AreaAttackDamage += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"AreaAttackDamage increased by {amount}. New Value: {AreaAttackDamage}");
#endif
    }

    public void IncreaseSpecialAttackDamage(int amount)
    {
        SpecialAttackDamage += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"SpecialAttackDamage increased by {amount}. New Value: {SpecialAttackDamage}");
#endif
    }

    public void ReduceSpecialAttackMissChance(float amount)
    {
        SpecialAttackMissChance = Mathf.Max(0f, SpecialAttackMissChance - amount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"SpecialAttackMissChance reduced by {amount}. New Value: {SpecialAttackMissChance}");
#endif
    }

    public void IncreaseAPRecoveryAmount(int amount)
    {
        APRecoveryAmount += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"APRecoveryAmount increased by {amount}. New Value: {APRecoveryAmount}");
#endif
    }

    public void IncreaseMaxActionPoints(int amount)
    {
        MaxActionPoints += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"MaxActionPoints increased by {amount}. New Value: {MaxActionPoints}");
#endif
    }

    public void IncreaseMaxHealth(int amount)
    {
        MaxHealth += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"MaxHealth increased by {amount}. New Value: {MaxHealth}");
#endif
    }

    public void IncreaseGlobalAPCostReduction(int amount)
    {
        GlobalAPCostReduction += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"GlobalAPCostReduction increased by {amount}. New Value: {GlobalAPCostReduction}");
#endif
    }

    public void IncreaseGlobalActionSpeed(int amount)
    {
        GlobalActionSpeedBonus += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"GlobalActionSpeedBonus increased by {amount}. New Value: {GlobalActionSpeedBonus}");
#endif
    }

    public void IncreaseCriticalHitChance(float amount)
    {
        CriticalHitChance += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // GameLog.LogVerbose($"CriticalHitChance increased by {amount}. New Value: {CriticalHitChance}");
#endif
    }

    // --- Save/Load Contributions ---
    public void WriteTo(ref Santa.Core.Save.SaveData data)
    {
        data.lastUpgrade = _lastSelectedUpgrade;
        data.acquiredUpgrades = _acquiredUpgrades.ToArray();
    }

    public void ReadFrom(in Santa.Core.Save.SaveData data)
    {
        // Restore stats from base, then re-apply upgrades
        RestoreBaseStats();
        _acquiredUpgrades.Clear();

        if (data.acquiredUpgrades != null && allPossibleUpgrades != null)
        {
            foreach (var name in data.acquiredUpgrades)
            {
                var upgrade = allPossibleUpgrades.FirstOrDefault(u => u.UpgradeName == name);
                if (upgrade != null)
                {
                    upgrade.Strategy.Apply(this);
                    _acquiredUpgrades.Add(name);
                }
            }
        }
        _lastSelectedUpgrade = data.lastUpgrade;
    }

    private void RestoreBaseStats()
    {
        if (baseStatsConfig != null)
        {
            DirectAttackDamage = baseStatsConfig.DirectAttackDamage;
            AreaAttackDamage = baseStatsConfig.AreaAttackDamage;
            SpecialAttackDamage = baseStatsConfig.SpecialAttackDamage;
            SpecialAttackMissChance = baseStatsConfig.SpecialAttackMissChance;
            APRecoveryAmount = baseStatsConfig.APRecoveryAmount;
            MaxActionPoints = baseStatsConfig.MaxActionPoints;
            MaxHealth = baseStatsConfig.MaxHealth;
            GlobalAPCostReduction = baseStatsConfig.GlobalAPCostReduction;
            GlobalActionSpeedBonus = baseStatsConfig.GlobalActionSpeedBonus;
            CriticalHitChance = baseStatsConfig.BaseCriticalHitChance;
        }
        else
        {
            DirectAttackDamage = 25;
            AreaAttackDamage = 10;
            SpecialAttackDamage = 75;
            SpecialAttackMissChance = 0.2f;
            APRecoveryAmount = 34;
            MaxActionPoints = 100;
            MaxHealth = 100;
            GlobalAPCostReduction = 0;
            GlobalActionSpeedBonus = 0;
            CriticalHitChance = 0.1f;
        }
    }
}