using System;

/// <summary>
/// Componente concreto para los Puntos de Acción (PA), hereda de StatComponent.
/// </summary>
[Serializable]
public class ActionPointComponent : StatComponent
{
    /// <summary>
    /// Valor actual de Puntos de Acción (alias de CurrentValue).
    /// </summary>
    public int CurrentActionPoints => CurrentValue;

    /// <summary>
    /// Gasta Puntos de Acción llamando a AffectValue con valor negativo.
    /// </summary>
    public void SpendActionPoints(int amount)
    {
        AffectValue(-amount);
    }

    /// <summary>
    /// Comprueba si hay suficientes Puntos de Acción para una acción.
    /// </summary>
    public bool HasEnough(int amount)
    {
        return CurrentValue >= amount;
    }

    /// <summary>
    /// Rellena los Puntos de Acción al máximo. Alias de SetToMax().
    /// </summary>
    public void Refill()
    {
        SetToMax();
    }
}
