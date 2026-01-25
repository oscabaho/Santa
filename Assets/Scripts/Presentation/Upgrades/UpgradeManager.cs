using System.Collections.Generic;
using Santa.Core;
using Santa.Core.Save;
using Santa.Presentation.Upgrades;
using Santa.Domain.Upgrades;
using AbilityUpgrade = Santa.Domain.Combat.AbilityUpgrade;
using Santa.Domain.Combat;
using UnityEngine;
using VContainer;

namespace Santa.Presentation.Upgrades
{

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

    private readonly Santa.Presentation.Upgrades.UpgradeStatsContainer _stats = new Santa.Presentation.Upgrades.UpgradeStatsContainer();

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
        _stats.InitializeFromConfig(baseStatsConfig);
    }

    // [Inject] removed to avoid VContainer reliability issues
    // public void Construct(...) ...

    private void EnsureDependencies()
    {
        if (_upgradeUI == null)
        {
            var ui = FindFirstObjectByType<Santa.Presentation.Upgrades.UpgradeUI>();
            if (ui != null) _upgradeUI = ui;
        }
        if (_combatTransitionService == null)
        {
            var transition = FindFirstObjectByType<Santa.Infrastructure.Combat.CombatTransitionManager>();
            if (transition != null) _combatTransitionService = transition;
        }
    }

    private void Start()
    {
        LoadStats();
    }

    public async void PresentUpgradeOptions()
    {
        EnsureDependencies();

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

        if (availableUpgrades.Count < 2)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"Not enough unique upgrades to offer a choice. Only {availableUpgrades.Count} available.");
#endif
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

        try
        {
            // Dynamic check: If UI is not found, try to load it via UIManager
            if (_upgradeUI == null)
            {
                var uiManager = FindFirstObjectByType<Santa.Presentation.UI.UIManager>();
                if (uiManager != null)
                {
                    // Request UIManager to load/show the panel. This is async but returns void-like UniTask.
                    await uiManager.ShowPanel(Santa.Core.Addressables.AddressableKeys.UIPanels.UpgradeUI);
                    
                    // After loading, the panel should be in the scene. Find it.
                    // We search even inactive ones just in case ShowPanel didn't activate it for some reason (though it should).
                    var ui = FindFirstObjectByType<Santa.Presentation.Upgrades.UpgradeUI>(FindObjectsInactive.Include);
                    if (ui != null)
                    {
                        _upgradeUI = ui;
                    }
                }
            }

            if (_upgradeUI != null)
            {
                _upgradeUI.ShowUpgrades(option1, option2);
            }
            else
            {
                GameLog.LogError("UpgradeManager: UpgradeUI could not be loaded or found after request. Skipping to EndCombat.");
                _combatTransitionService?.EndCombat(true);
            }
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"UpgradeManager: Exception showing upgrades: {ex.Message}. Skipping to EndCombat(Victory).");
            GameLog.LogException(ex);
            _combatTransitionService?.EndCombat(true);
        }
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

    // --- Persistence (Mobile-friendly secure storage) ---

    private void SaveStats()
    {
        Santa.Core.Save.SecureStorage.SetString(Santa.Core.Config.GameKeys.LastUpgrade, _lastSelectedUpgrade);
    }

    private void LoadStats()
    {
        if (Santa.Core.Save.SecureStorage.TryGetString(Santa.Core.Config.GameKeys.LastUpgrade, out var stored))
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
        Santa.Core.Save.SecureStorage.Delete(Santa.Core.Config.GameKeys.LastUpgrade);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("Reset last selected upgrade.");
#endif
    }

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
        {
            foreach (var name in data.acquiredUpgrades)
            {
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
                }
            }
        }
        _lastSelectedUpgrade = data.lastUpgrade;
    }
}
}
