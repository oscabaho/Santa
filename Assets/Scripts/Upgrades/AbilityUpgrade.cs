using UnityEngine;

// Enum to define which player ability an upgrade affects.
public enum AbilityType
{
    DirectAttack,
    AreaAttack,
    SpecialAttack
}

// Enum to define the nature of the upgrade.
public enum UpgradeType
{
    IncreaseDamage,
    IncreaseEnergyGain, // For all attacks
    ReduceSpecialMissChance
}

/// <summary>
/// A ScriptableObject that defines a single, specific upgrade for a player ability.
/// </summary>
[CreateAssetMenu(fileName = "NewAbilityUpgrade", menuName = "Santa/Ability Upgrade", order = 0)]
public class AbilityUpgrade : ScriptableObject
{
    [Header("Upgrade Details")]
    [Tooltip("The ability this upgrade applies to.")]
    public AbilityType TargetAbility;

    [Tooltip("The type of bonus this upgrade provides.")]
    public UpgradeType StatToUpgrade;

    [Tooltip("The value of the upgrade (e.g., 5 for +5 damage).")]
    public float UpgradeValue;

    [Header("UI Information")]
    [Tooltip("The name of the upgrade to be displayed in the UI.")]
    public string UpgradeName;

    [Tooltip("The description shown to the player on the upgrade choice screen.")]
    [TextArea(3, 5)]
    public string UpgradeDescription;
}
