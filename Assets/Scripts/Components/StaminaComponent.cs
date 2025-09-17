using System;
using UnityEngine;

/// <summary>
/// Componente concreto para la stamina, hereda de StatComponent.
/// </summary>
[Serializable]
public class StaminaComponent : StatComponent
{
    [Tooltip("Tasa de regeneración por segundo en modo Exploración.")]
    [SerializeField] private float regenRate = 15f;

    private float _regenAccumulator = 0f;

    /// <summary>
    /// Valor actual de stamina (alias de CurrentValue).
    /// </summary>
    public int CurrentStamina { get { return CurrentValue; } }

    /// <summary>
    /// Consume stamina llamando a AffectValue con valor negativo.
    /// </summary>
    public void UseStamina(int amount)
    {
        AffectValue(-amount);
        // Any time we use stamina, reset the regen accumulator to add a slight delay before regen starts.
        _regenAccumulator = 0f;
    }

    /// <summary>
    /// Comprueba si hay suficiente stamina para una acción.
    /// </summary>
    public bool HasEnough(int amount)
    {
        return CurrentValue >= amount;
    }

    /// <summary>
    /// Regenera stamina a lo largo del tiempo, solo en modo Exploración.
    /// </summary>
    public void Regenerate(float deltaTime)
    {
        if (GameStateManager.CurrentState != GameStateManager.GameState.Exploration)
        {
            _regenAccumulator = 0;
            return;
        }

        if (CurrentValue < MaxValue)
        {
            _regenAccumulator += regenRate * deltaTime;
            if (_regenAccumulator >= 1f)
            {
                int amountToRegen = Mathf.FloorToInt(_regenAccumulator);
                AffectValue(amountToRegen);
                _regenAccumulator -= amountToRegen;
            }
        }
        else
        {
            _regenAccumulator = 0;
        }
    }
}
