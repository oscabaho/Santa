using UnityEngine;

/// <summary>
/// Componente MonoBehaviour para exponer y gestionar HealthComponent en un GameObject.
/// </summary>
[DisallowMultipleComponent]
public class HealthComponentBehaviour : MonoBehaviour, IHealthController
{
    [SerializeField]
    private HealthComponent health = new HealthComponent();
    public HealthComponent Health { get { return health; } }

    public event System.Action<int, int> OnValueChanged { add => health.OnValueChanged += value; remove => health.OnValueChanged -= value; }

    public int CurrentValue => health.CurrentValue;
    public int MaxValue => health.MaxValue;
    public void AffectValue(int value) => health.AffectValue(value);
    public void SetValue(int value) => health.SetValue(value);

    /// <summary>
    /// Establece la vida al máximo.
    /// </summary>
    public void SetToMax()
    {
        health.SetToMax();
    }

    private void Awake()
    {
        if (health != null)
        {
            // Ensure health starts at a valid value; default serialized value is zero.
            health.SetToMax();
        }
    }
}
