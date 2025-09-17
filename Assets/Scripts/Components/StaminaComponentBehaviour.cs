using UnityEngine;

/// <summary>
/// Componente MonoBehaviour para exponer y gestionar StaminaComponent en un GameObject.
/// </summary>
[DisallowMultipleComponent]
public class StaminaComponentBehaviour : MonoBehaviour, IStatController
{
    [SerializeField]
    private StaminaComponent stamina = new StaminaComponent();
    public StaminaComponent Stamina { get { return stamina; } }

    public int CurrentValue => stamina.CurrentValue;
    public int MaxValue => stamina.MaxValue;
    public void AffectValue(int value) => stamina.AffectValue(value);

    /// <summary>
    /// Establece la stamina al máximo.
    /// </summary>
    public void SetToMax()
    {
        stamina.SetToMax();
    }

    private void Update()
    {
        // This drives the regeneration logic from StaminaComponent.
        stamina.Regenerate(Time.deltaTime);
    }
}
