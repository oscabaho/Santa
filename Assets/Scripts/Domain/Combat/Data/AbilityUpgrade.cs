using UnityEngine;



// Enum to define the nature of the upgrade.
public enum UpgradeType
{
    IncreaseDamage,
    IncreaseAPRecovery, // Amount of AP recovered by GainAP ability
    IncreaseMaxHealth,
    ReduceSpecialMissChance
}

/// <summary>
/// A ScriptableObject that defines a single, specific upgrade.
/// It uses a Strategy pattern where the actual upgrade logic is defined
/// in a separate UpgradeStrategySO scriptable object.
/// </summary>
[CreateAssetMenu(fileName = "NewAbilityUpgrade", menuName = "Santa/Ability Upgrade", order = 0)]
public class AbilityUpgrade : ScriptableObject
{
    [Header("Strategy")]
    [Tooltip("The strategy that defines what this upgrade does.")]
    [SerializeField] private UpgradeStrategySO _strategy;

    [Header("UI Information")]
    [Tooltip("The name of the upgrade to be displayed in the UI.")]
    public string UpgradeName;

    [Tooltip("The description shown to the player on the upgrade choice screen.")]
    [TextArea(3, 5)]
    public string UpgradeDescription;

    public UpgradeStrategySO Strategy => _strategy;
}

