using System;

/// <summary>
/// Concrete component for Action Points (AP), inherits from StatComponent.
/// </summary>
[Serializable]
public class ActionPointComponent : StatComponent
{
    /// <summary>
    /// Current Action Points value (alias of CurrentValue).
    /// </summary>
    public int CurrentActionPoints => CurrentValue;

    /// <summary>
    /// Spends Action Points by calling AffectValue with a negative value.
    /// </summary>
    public void SpendActionPoints(int amount)
    {
        AffectValue(-amount);
    }

    /// <summary>
    /// Checks whether there are enough Action Points for an action.
    /// </summary>
    public bool HasEnough(int amount)
    {
        return CurrentValue >= amount;
    }

    /// <summary>
    /// Refills Action Points to maximum (alias of SetToMax()).
    /// </summary>
    public void Refill()
    {
        SetToMax();
    }

    /// <summary>
    /// Override SetValue to allow AP to exceed MaxValue (uncapped).
    /// MaxValue acts as the base starting value.
    /// </summary>
    public override void SetValue(int newValue)
    {
        // Only clamp the lower bound to 0. Upper bound is uncapped.
        currentValue = UnityEngine.Mathf.Max(0, newValue);
        TriggerValueChanged();
    }
}
