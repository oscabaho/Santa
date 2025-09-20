using UnityEngine;

/// <summary>
/// Componente MonoBehaviour para exponer y gestionar EnergyComponent en un GameObject.
/// </summary>
[DisallowMultipleComponent]
public class EnergyComponentBehaviour : MonoBehaviour, IStatController
{
    [SerializeField]
    private EnergyComponent energy = new EnergyComponent();
    public EnergyComponent Energy { get { return energy; } }

    public int CurrentValue => energy.CurrentValue;
    public int MaxValue => energy.MaxValue;
    public void AffectValue(int value) => energy.AffectValue(value);

    /// <summary>
    /// Establece la energía al máximo.
    /// </summary>
    public void SetToMax()
    {
        energy.SetToMax();
    }

    private void Awake()
    {
        // Inicialización si es necesario
    }
}
