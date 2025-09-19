using UnityEngine;

/// <summary>
/// Componente MonoBehaviour para exponer y gestionar ActionPointComponent en un GameObject.
/// </summary>
[DisallowMultipleComponent]
public class ActionPointComponentBehaviour : MonoBehaviour, IStatController
{
    [SerializeField]
    private ActionPointComponent actionPoints = new ActionPointComponent();
    public ActionPointComponent ActionPoints { get { return actionPoints; } }

    public int CurrentValue => actionPoints.CurrentValue;
    public int MaxValue => actionPoints.MaxValue;
    public void AffectValue(int value) => actionPoints.AffectValue(value);

    /// <summary>
    /// Rellena los Puntos de Acción al máximo.
    /// </summary>
    public void Refill()
    {
        actionPoints.Refill();
    }
}
