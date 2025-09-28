using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the player's permanent progression by applying and saving ability upgrades.
/// </summary>
public class UpgradeManager : MonoBehaviour, IUpgradeService, IUpgradeTarget
{
    private static UpgradeManager Instance { get; set; }

    [Header("Upgrade Pool")]
    [Tooltip("A list of all possible upgrades that can be offered to the player.")]
    [SerializeField] private List<AbilityUpgrade> allPossibleUpgrades;

    [Header("UI Reference")]
    [Tooltip("Assign the GameObject that has the UpgradeUI component here.")]
    [SerializeField] private MonoBehaviour upgradeUIMonoBehaviour;
    private IUpgradeUI _upgradeUI;


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
        ServiceLocator.Register<IUpgradeService>(this);
        DontDestroyOnLoad(gameObject);

        _upgradeUI = upgradeUIMonoBehaviour as IUpgradeUI;
        if (_upgradeUI == null)
        {
            Debug.LogError("A component implementing IUpgradeUI is not assigned in the UpgradeManager!");
        }

        LoadStats();
    }

    private void OnDestroy()
    {
        var registered = ServiceLocator.Get<IUpgradeService>();
        if ((UnityEngine.Object)registered == (UnityEngine.Object)this)
            ServiceLocator.Unregister<IUpgradeService>();
        if (Instance == this) Instance = null;
    }

    public void PresentUpgradeOptions()
    {
        var randomUpgrades = GetRandomUpgrades(2);
        if (randomUpgrades.Count < 2)
        {
            Debug.LogWarning("Not enough unique upgrades available to present a choice.");
            ServiceLocator.Get<ICombatTransitionService>()?.EndCombat();
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
            Debug.Log($"Applied upgrade: {upgrade.UpgradeName}. New value saved.");
            SaveStats();
        }
        else
        {
            Debug.LogWarning($"Upgrade '{upgrade.UpgradeName}' has no strategy assigned.");
        }
    }

    // --- Public Methods for Strategies ---
    public void IncreaseDirectAttackDamage(int amount) => DirectAttackDamage += amount;
    public void IncreaseAreaAttackDamage(int amount) => AreaAttackDamage += amount;
    public void IncreaseSpecialAttackDamage(int amount) => SpecialAttackDamage += amount;
    public void IncreaseEnergyGainedPerTurn(int amount) => EnergyGainedPerTurn += amount;
    public void ReduceSpecialAttackMissChance(float amount) => SpecialAttackMissChance = Mathf.Max(0, SpecialAttackMissChance - amount);

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
