using System.Collections.Generic;
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

    private Santa.Core.Upgrades.UpgradeStatsContainer _stats = new Santa.Core.Upgrades.UpgradeStatsContainer();

    // Player Stats - Delegated to container
    public int DirectAttackDamage => _stats.DirectAttackDamage;
    public int AreaAttackDamage => _stats.AreaAttackDamage;
    public int SpecialAttackDamage => _stats.SpecialAttackDamage;
    public float SpecialAttackMissChance => _stats.SpecialAttackMissChance;
    public int APRecoveryAmount => _stats.APRecoveryAmount;
    public int MaxActionPoints => _stats.MaxActionPoints;
    public int MaxHealth => _stats.MaxHealth;
    public int GlobalAPCostReduction => _stats.GlobalAPCostReduction;
    public int GlobalActionSpeedBonus => _stats.GlobalActionSpeedBonus;
    public float CriticalHitChance => _stats.CriticalHitChance;

    private void Awake()
    {
        // Initialize stats from config
<<<<<<< Updated upstream
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
=======
        _stats.InitializeFromConfig(baseStatsConfig);
>>>>>>> Stashed changes
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

        // Apply the strategy to THIS manager (which delegates to container via IUpgradeTarget)
        upgrade.Strategy.Apply(this);

        // Track selection
        _lastSelectedUpgrade = upgrade.UpgradeName;
        if (!_acquiredUpgrades.Contains(upgrade.UpgradeName))
        {
            _acquiredUpgrades.Add(upgrade.UpgradeName);
        }

        SaveStats();
    }

    // --- IUpgradeTarget Implementation ---

    public void IncreaseDirectAttackDamage(int amount)
    {
        DirectAttackDamage += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Direct Attack Damage increased to {DirectAttackDamage}");
#endif
    }

    public void IncreaseAreaAttackDamage(int amount)
    {
        AreaAttackDamage += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Area Attack Damage increased to {AreaAttackDamage}");
#endif
    }

    public void IncreaseSpecialAttackDamage(int amount)
    {
        SpecialAttackDamage += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Special Attack Damage increased to {SpecialAttackDamage}");
#endif
    }

    public void IncreaseAPRecoveryAmount(int amount)
    {
        APRecoveryAmount += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"AP Recovery Amount increased to {APRecoveryAmount}");
#endif
    }

    public void ReduceSpecialAttackMissChance(float amount)
    {
        SpecialAttackMissChance = Mathf.Max(0f, SpecialAttackMissChance - amount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Special Attack Miss Chance reduced to {SpecialAttackMissChance}");
#endif
    }

    public void IncreaseMaxActionPoints(int amount)
    {
        MaxActionPoints += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Max Action Points increased to {MaxActionPoints}");
#endif
    }

    public void IncreaseMaxHealth(int amount)
    {
        MaxHealth += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Max Health increased to {MaxHealth}");
#endif
    }

    public void IncreaseGlobalAPCostReduction(int amount)
    {
        GlobalAPCostReduction += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Global AP Cost Reduction increased to {GlobalAPCostReduction}");
#endif
    }

    public void IncreaseGlobalActionSpeed(int amount)
    {
        GlobalActionSpeedBonus += amount;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Global Action Speed Bonus increased to {GlobalActionSpeedBonus}");
#endif
    }

    public void IncreaseCriticalHitChance(float amount)
    {
        CriticalHitChance = Mathf.Min(1.0f, CriticalHitChance + amount);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Critical Hit Chance increased to {CriticalHitChance * 100}%");
#endif
    }

    // --- Persistence (Mobile-friendly secure storage) ---

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

<<<<<<< Updated upstream
        // --- Save/Load Contributions ---
        public void WriteTo(ref Santa.Core.Save.SaveData data)
=======
    // --- IUpgradeTarget Implementation (Delegated to Container) ---

    public void IncreaseDirectAttackDamage(int amount) => _stats.IncreaseDirectAttackDamage(amount);
    public void IncreaseAreaAttackDamage(int amount) => _stats.IncreaseAreaAttackDamage(amount);
    public void IncreaseSpecialAttackDamage(int amount) => _stats.IncreaseSpecialAttackDamage(amount);
    public void ReduceSpecialAttackMissChance(float amount) => _stats.ReduceSpecialAttackMissChance(amount);
    public void IncreaseAPRecoveryAmount(int amount) => _stats.IncreaseAPRecoveryAmount(amount);
    public void IncreaseMaxActionPoints(int amount) => _stats.IncreaseMaxActionPoints(amount);
    public void IncreaseMaxHealth(int amount) => _stats.IncreaseMaxHealth(amount);
    public void IncreaseGlobalAPCostReduction(int amount) => _stats.IncreaseGlobalAPCostReduction(amount);
    public void IncreaseGlobalActionSpeed(int amount) => _stats.IncreaseGlobalActionSpeed(amount);
    public void IncreaseCriticalHitChance(float amount) => _stats.IncreaseCriticalHitChance(amount);

    // --- Save/Load Contributions ---
    public void WriteTo(ref Santa.Core.Save.SaveData data)
    {
        data.lastUpgrade = _lastSelectedUpgrade;
        // Direct assignment to avoid ToArray allocation if list is already array-compatible
        data.acquiredUpgrades = _acquiredUpgrades.Count > 0 ? _acquiredUpgrades.ToArray() : System.Array.Empty<string>();
    }

    public void ReadFrom(in Santa.Core.Save.SaveData data)
    {
        // Restore stats from base, then re-apply upgrades
        _stats.InitializeFromConfig(baseStatsConfig);
        _acquiredUpgrades.Clear();

        if (data.acquiredUpgrades != null && allPossibleUpgrades != null)
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
                foreach (var name in data.acquiredUpgrades)
                {
                    var upgrade = allPossibleUpgrades.FirstOrDefault(u => u.UpgradeName == name);
                    if (upgrade != null)
                    {
                        upgrade.Strategy.Apply(this);
                        _acquiredUpgrades.Add(name);
                    }
=======
                // Manual search to avoid LINQ allocation (FirstOrDefault)
                AbilityUpgrade upgrade = null;
                for (int i = 0; i < allPossibleUpgrades.Count; i++)
                {
                    if (allPossibleUpgrades[i].UpgradeName == name)
                    {
                        upgrade = allPossibleUpgrades[i];
                        break;
                    }
                }
                
                if (upgrade != null)
                {
                    // Apply to THIS manager (which delegates to container)
                    upgrade.Strategy.Apply(this);
                    _acquiredUpgrades.Add(name);
>>>>>>> Stashed changes
                }
            }
            _lastSelectedUpgrade = data.lastUpgrade;
        }
<<<<<<< Updated upstream

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
=======
        _lastSelectedUpgrade = data.lastUpgrade;
    }
>>>>>>> Stashed changes
}