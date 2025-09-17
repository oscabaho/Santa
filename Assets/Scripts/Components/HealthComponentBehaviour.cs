using UnityEngine;

/// <summary>
/// Componente MonoBehaviour para exponer y gestionar HealthComponent en un GameObject.
/// </summary>
[DisallowMultipleComponent]
public class HealthComponentBehaviour : MonoBehaviour, IStatController
{
    [SerializeField]
    private HealthComponent health = new HealthComponent();
    public HealthComponent Health { get { return health; } }

    public int CurrentValue => health.CurrentValue;
    public int MaxValue => health.MaxValue;
    public void AffectValue(int value) => health.AffectValue(value);

    /// <summary>
    /// Establece la vida al máximo.
    /// </summary>
    public void SetToMax()
    {
        health.SetToMax();
    }

    private void Awake()
    {
        // Inicialización si es necesario
    }
}
