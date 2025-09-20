using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the player's permanent progression by applying and saving ability upgrades.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Upgrade Pool")]
    [Tooltip("A list of all possible upgrades that can be offered to the player.")]
    [SerializeField] private List<AbilityUpgrade> allPossibleUpgrades;

    [Header("UI Reference")]
    [SerializeField] private UpgradeUI upgradeUI;

    // Player Stats - These will be modified by upgrades.
    // We are centralizing them here for now.
    public int DirectAttackDamage { get; private set; } = 25;
    public int AreaAttackDamage { get; private set; } = 10;
    public int SpecialAttackDamage { get; private set; } = 75;
    public float SpecialAttackMissChance { get; private set; } = 0.2f;
    public int EnergyGainedPerTurn { get; private set; } = 34;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (upgradeUI == null)
        {
            Debug.LogError("UpgradeUI is not assigned in the UpgradeManager!");
        }

        LoadStats();
    }

    /// <summary>
    /// Called at the end of a victorious battle to start the upgrade process.
    /// </summary>
    public void PresentUpgradeOptions()
    {
        var randomUpgrades = GetRandomUpgrades(2);
        if (randomUpgrades.Count < 2)
        {
            Debug.LogWarning("Not enough unique upgrades available to present a choice.");
            // If we can't offer a choice, just end combat directly.
            CombatTransitionManager.Instance?.EndCombat();
            return;
        }
        upgradeUI.ShowUpgrades(randomUpgrades[0], randomUpgrades[1]);
    }

    private List<AbilityUpgrade> GetRandomUpgrades(int count)
    {
        if (allPossibleUpgrades == null || allPossibleUpgrades.Count < count)
        {
            return new List<AbilityUpgrade>();
        }

        var shuffledUpgrades = allPossibleUpgrades.OrderBy(a => Random.value).ToList();
        return shuffledUpgrades.Take(count).ToList();
    }

    public void ApplyUpgrade(AbilityUpgrade upgrade)
    {
        switch (upgrade.StatToUpgrade)
        {
            case UpgradeType.IncreaseDamage:
                if (upgrade.TargetAbility == AbilityType.DirectAttack)
                    DirectAttackDamage += (int)upgrade.UpgradeValue;
                else if (upgrade.TargetAbility == AbilityType.AreaAttack)
                    AreaAttackDamage += (int)upgrade.UpgradeValue;
                else if (upgrade.TargetAbility == AbilityType.SpecialAttack)
                    SpecialAttackDamage += (int)upgrade.UpgradeValue;
                break;

            case UpgradeType.IncreaseEnergyGain:
                EnergyGainedPerTurn += (int)upgrade.UpgradeValue;
                break;

            case UpgradeType.ReduceSpecialMissChance:
                SpecialAttackMissChance = Mathf.Max(0, SpecialAttackMissChance - upgrade.UpgradeValue);
                break;
        }

        Debug.Log($"Applied upgrade: {upgrade.UpgradeName}. New value saved.");
        SaveStats();
    }

    private void SaveStats()
    {
        PlayerPrefs.SetInt(nameof(DirectAttackDamage), DirectAttackDamage);
        PlayerPrefs.SetInt(nameof(AreaAttackDamage), AreaAttackDamage);
        PlayerPrefs.SetInt(nameof(SpecialAttackDamage), SpecialAttackDamage);
        PlayerPrefs.SetFloat(nameof(SpecialAttackMissChance), SpecialAttackMissChance);
        PlayerPrefs.SetInt(nameof(EnergyGainedPerTurn), EnergyGainedPerTurn);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        DirectAttackDamage = PlayerPrefs.GetInt(nameof(DirectAttackDamage), DirectAttackDamage);
        AreaAttackDamage = PlayerPrefs.GetInt(nameof(AreaAttackDamage), AreaAttackDamage);
        SpecialAttackDamage = PlayerPrefs.GetInt(nameof(SpecialAttackDamage), SpecialAttackDamage);
        SpecialAttackMissChance = PlayerPrefs.GetFloat(nameof(SpecialAttackMissChance), SpecialAttackMissChance);
        EnergyGainedPerTurn = PlayerPrefs.GetInt(nameof(EnergyGainedPerTurn), EnergyGainedPerTurn);
    }
}
