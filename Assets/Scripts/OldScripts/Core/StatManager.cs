using UnityEngine;

public enum StatType
{
    Health,
    Stamina
}

/// <summary>
/// Centraliza el acceso a todos los componentes de estadísticas del jugador.
/// </summary>
public class StatManager : MonoBehaviour
{
    // Asigna estos componentes en el Inspector.
    [SerializeField] private PlayerHealthController healthController;
    [SerializeField] private PlayerStaminaController staminaController;

    /// <summary>
    /// Obtiene el controlador de una estadística específica.
    /// </summary>
    public IStatController GetStat(StatType type)
    {
        switch (type)
        {
            case StatType.Health: return healthController?.Health;
            case StatType.Stamina: return staminaController?.Stamina;
            default: return null;
        }
    }
}
