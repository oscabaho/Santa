using System;
using UnityEngine;

/// <summary>
/// Concrete component for Health, inherits from StatComponent.
/// </summary>
[Serializable]
public class HealthComponent : StatComponent
{
    /// <summary>
    /// Event fired when health reaches 0. Passes the GameObject that died.
    /// </summary>
    public event Action<GameObject> OnDeath;

    private GameObject owner;

    public void SetOwner(GameObject ownerObject)
    {
        owner = ownerObject;
    }

    public override void SetValue(int newValue)
    {
        int previousValue = currentValue;
        base.SetValue(newValue);

        // Trigger death event when going from alive to dead
        if (previousValue > 0 && currentValue <= 0)
        {
            OnDeath?.Invoke(owner);
        }
    }
}
