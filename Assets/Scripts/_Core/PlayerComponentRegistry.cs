using UnityEngine;

/// <summary>
/// Concrete implementation of the component registry for the Player.
/// Gathers component references on Awake to avoid repeated GetComponent calls.
/// </summary>
public class PlayerComponentRegistry : MonoBehaviour, IComponentRegistry
{
    public IHealthController HealthController { get; private set; }
    public IActionPointController ActionPointController { get; private set; }

    private void Awake()
    {
        // Cache all component references once.
        HealthController = GetComponent<IHealthController>();
        ActionPointController = GetComponent<IActionPointController>();
    }
}
