using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using VContainer;

/// <summary>
/// Manages the player's permanent progression by applying and saving ability upgrades.
/// </summary>
public class UpgradeManager : MonoBehaviour, IUpgradeService, IUpgradeTarget
{
    [Header("Upgrade Pool")]
    [Tooltip("A list of all possible upgrades that can be offered to the player.")]
    [SerializeField] private List<AbilityUpgrade> allPossibleUpgrades;

    private IUpgradeUI _upgradeUI;
    private ICombatTransitionService _combatTransitionService;

    // Player Stats - These will be modified by upgrades.
    // We are centralizing them here for now.
    public int DirectAttackDamage { get; private set; } = 25;
    public int AreaAttackDamage { get; private set; } = 10;
    public int SpecialAttackDamage { get; private set; } = 75;
    public float SpecialAttackMissChance { get; private set; } = 0.2f;
    public int EnergyGainedPerTurn { get; private set; } = 34;
    public int MaxActionPoints { get; private set; } = 100;

    [Inject]
    public void Construct(IUpgradeUI upgradeUI, ICombatTransitionService combatTransitionService)
    {
        _upgradeUI = upgradeUI;
        _combatTransitionService = combatTransitionService;
    }

    private void Awake()
    {
        // Persist only if attached to a root object; otherwise rely on the root holder (e.g., GameLifetimeScope)
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            GameLog.Log("UpgradeManager: Running as child object; lifetime managed by root holder (no DontDestroyOnLoad needed).", this);
        }
        LoadStats();
    }

    public void PresentUpgradeOptions()
    {
        var randomUpgrades = GetRandomUpgrades(2);
        if (randomUpgrades.Count < 2)
        {
            GameLog.LogWarning("Not enough unique upgrades available to present a choice.");
            _combatTransitionService?.EndCombat();
            return;
        }
        _upgradeUI?.ShowUpgrades(randomUpgrades[0], randomUpgrades[1]);
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
        if (upgrade.Strategy != null)
        {
            upgrade.Strategy.Apply(this);
            GameLog.Log($"Applied upgrade: {upgrade.UpgradeName}. New value saved.");
            SaveStats();
        }
        else
        {
            GameLog.LogWarning($"Upgrade '{upgrade.UpgradeName}' has no strategy assigned.");
        }
    }

    // --- Public Methods for Strategies ---
    public void IncreaseDirectAttackDamage(int amount) => DirectAttackDamage += amount;
    public void IncreaseAreaAttackDamage(int amount) => AreaAttackDamage += amount;
    public void IncreaseSpecialAttackDamage(int amount) => SpecialAttackDamage += amount;
    public void IncreaseEnergyGainedPerTurn(int amount) => EnergyGainedPerTurn += amount;
    public void ReduceSpecialAttackMissChance(float amount) => SpecialAttackMissChance = Mathf.Max(0, SpecialAttackMissChance - amount);
    public void IncreaseMaxActionPoints(int amount) => MaxActionPoints += amount;

    private void SaveStats()
    {
        PlayerPrefs.SetInt(nameof(DirectAttackDamage), DirectAttackDamage);
        PlayerPrefs.SetInt(nameof(AreaAttackDamage), AreaAttackDamage);
        PlayerPrefs.SetInt(nameof(SpecialAttackDamage), SpecialAttackDamage);
        PlayerPrefs.SetFloat(nameof(SpecialAttackMissChance), SpecialAttackMissChance);
        PlayerPrefs.SetInt(nameof(EnergyGainedPerTurn), EnergyGainedPerTurn);
        PlayerPrefs.SetInt(nameof(MaxActionPoints), MaxActionPoints);
        PlayerPrefs.Save();
    }

    private void LoadStats()
    {
        DirectAttackDamage = PlayerPrefs.GetInt(nameof(DirectAttackDamage), DirectAttackDamage);
        AreaAttackDamage = PlayerPrefs.GetInt(nameof(AreaAttackDamage), AreaAttackDamage);
        SpecialAttackDamage = PlayerPrefs.GetInt(nameof(SpecialAttackDamage), SpecialAttackDamage);
        SpecialAttackMissChance = PlayerPrefs.GetFloat(nameof(SpecialAttackMissChance), SpecialAttackMissChance);
        EnergyGainedPerTurn = PlayerPrefs.GetInt(nameof(EnergyGainedPerTurn), EnergyGainedPerTurn);
        MaxActionPoints = PlayerPrefs.GetInt(nameof(MaxActionPoints), MaxActionPoints);
    }
}