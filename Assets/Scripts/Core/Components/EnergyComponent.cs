using System;
using UnityEngine;

/// <summary>
/// Componente concreto para la energía del ataque especial.
/// </summary>
[Serializable]
public class EnergyComponent : StatComponent
{
    /// <summary>
    /// Comprueba si la energía está al máximo.
    /// </summary>
    public bool IsFull()
    {
        return CurrentValue >= MaxValue;
    }

    /// <summary>
    /// Gasta toda la energía.
    /// </summary>
    public void UseSpecialAttack()
    {
        SetValue(0);
    }
}
