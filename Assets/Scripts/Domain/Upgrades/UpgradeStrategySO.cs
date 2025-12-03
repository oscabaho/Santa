using UnityEngine;

/// <summary>
/// Base class for an upgrade strategy using the Strategy Pattern.
/// Each concrete implementation defines how an upgrade affects player stats.
/// </summary>
public abstract class UpgradeStrategySO : ScriptableObject
{
    /// <summary>
    /// Applies the upgrade logic to the specified target.
    /// </summary>
    /// <param name="target">The IUpgradeTarget instance to modify.</param>
    public abstract void Apply(IUpgradeTarget target);
}
