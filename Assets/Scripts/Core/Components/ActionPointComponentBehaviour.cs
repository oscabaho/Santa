using UnityEngine;

/// <summary>
/// MonoBehaviour component exposing and managing an ActionPointComponent on a GameObject.
/// </summary>
[DisallowMultipleComponent]
public class ActionPointComponentBehaviour : MonoBehaviour, IActionPointController
{
    [SerializeField]
    private ActionPointComponent actionPoints = new ActionPointComponent();
    public ActionPointComponent ActionPoints { get { return actionPoints; } }

    public event System.Action<int, int> OnValueChanged { add => actionPoints.OnValueChanged += value; remove => actionPoints.OnValueChanged -= value; }

    public int CurrentValue => actionPoints.CurrentValue;
    public int MaxValue => actionPoints.MaxValue;
    public void AffectValue(int value) => actionPoints.AffectValue(value);
    public void SetValue(int value) => actionPoints.SetValue(value);
    public void SetMaxValue(int value) => actionPoints.SetMaxValue(value);

    /// <summary>
    /// Refills Action Points to the maximum.
    /// </summary>
    public void Refill()
    {
        actionPoints.Refill();
    }

    private void Awake()
    {
        if (actionPoints != null)
        {
            // Ensure combatants start with full action points unless explicitly configured otherwise.
            actionPoints.SetToMax();
        }
    }
}
