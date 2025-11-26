using UnityEngine;

/// <summary>
/// Configuration ScriptableObject that holds all base player stats.
/// This allows easy balancing through the Unity Inspector without code changes.
/// </summary>
[CreateAssetMenu(fileName = "PlayerStatsConfig", menuName = "Santa/Player Stats Config")]
public class PlayerStatsConfig : ScriptableObject
{
    [Header("Combat Stats")]
    [Tooltip("Base damage for direct single-target attacks")]
    public int DirectAttackDamage = 25;

    [Tooltip("Base damage for area attacks that hit all enemies")]
    public int AreaAttackDamage = 10;

    [Tooltip("Base damage for high-risk special attacks")]
    public int SpecialAttackDamage = 75;

    [Range(0f, 1f)]
    [Tooltip("Chance for special attacks to miss (0.2 = 20%)")]
    public float SpecialAttackMissChance = 0.2f;

    [Header("Resources")]
    [Tooltip("Amount of AP recovered by the Gain AP ability")]
    public int APRecoveryAmount = 34;

    [Tooltip("Maximum action points the player can accumulate")]
    public int MaxActionPoints = 100;

    [Tooltip("Maximum health points")]
    public int MaxHealth = 100;

    [Header("Advanced Stats")]
    [Range(0f, 1f)]
    [Tooltip("Base chance to deal critical damage (0.1 = 10%)")]
    public float BaseCriticalHitChance = 0.1f;

    [Tooltip("Global reduction to AP costs of all abilities (default 0)")]
    public int GlobalAPCostReduction = 0;

    [Tooltip("Global bonus to action speed for turn order (default 0)")]
    public int GlobalActionSpeedBonus = 0;
}
