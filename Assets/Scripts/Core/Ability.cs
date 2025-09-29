using UnityEngine;
using System.Collections.Generic;

public enum TargetingStyle
{
    SingleEnemy,
    AllEnemies,
    RandomEnemies,
    Self
}

/// <summary>
/// Base class for all abilities, implemented as a ScriptableObject.
/// This allows creating abilities as data assets in the editor.
/// </summary>
public abstract class Ability : ScriptableObject
{
    [Header("Info")]
    [SerializeField] private string _abilityName;
    [SerializeField] [TextArea] private string _description;

    [Header("Properties")]
    [SerializeField] private int _apCost = 1;
    [SerializeField] private TargetingStyle _targetingStyle = TargetingStyle.SingleEnemy;
    [Tooltip("Percentage of targets to affect for RandomEnemies style (0.0 to 1.0)")]
    [SerializeField] [Range(0f, 1f)] private float _targetPercentage = 1f;
    [Tooltip("Determines turn order. Higher value = faster action (executes earlier in the turn).")]
    [SerializeField] private int _actionSpeed = 100;

    public string AbilityName => _abilityName;
    public string Description => _description;
    public int ApCost => _apCost;
    public TargetingStyle Targeting => _targetingStyle;
    public float TargetPercentage => _targetPercentage;
    public int ActionSpeed => _actionSpeed;

    /// <summary>
    /// Executes the ability's logic on the given targets.
    /// </summary>
    /// <param name="targets">The list of targets, determined by the combat manager.</param>
    /// <param name="caster">The GameObject performing the ability.</param>
    public abstract void Execute(List<GameObject> targets, GameObject caster);
}
