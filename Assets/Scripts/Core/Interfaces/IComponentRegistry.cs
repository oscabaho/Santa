/// <summary>
/// Defines a registry for commonly accessed components on an entity.
/// </summary>
public interface IComponentRegistry
{
    IHealthController HealthController { get; }
    IActionPointController ActionPointController { get; }
}
