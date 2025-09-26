using UnityEngine;

/// <summary>
/// Holds the specific combat data for an enemy type.
/// Attach this component to each enemy prefab.
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("Enemy Info")]
    public string EnemyName = "Enemy";

    [Header("Combat Stats")]
    [Tooltip("Damage dealt by the enemy's single-target attack.")]
    public int DirectAttackDamage = 10;

    [Tooltip("Damage dealt by the enemy's area-of-effect attack.")]
    public int AreaAttackDamage = 5;

    [Header("Action Points")]
    [Tooltip("AP cost for the enemy's single-target attack.")]
    public int DirectAttackAPCost = 2;

    [Tooltip("AP cost for the enemy's area-of-effect attack.")]
    public int AreaAttackAPCost = 3;
}
