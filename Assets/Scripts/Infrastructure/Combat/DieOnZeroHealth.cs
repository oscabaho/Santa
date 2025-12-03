using UnityEngine;
using VContainer;
using Santa.Core;
using Santa.Core.Events;
using Santa.Core.Save;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Desactiva el GameObject cuando la vida llega a 0 y notifica al EventBus.
    /// </summary>
    [RequireComponent(typeof(HealthComponentBehaviour))]
    public class DieOnZeroHealth : MonoBehaviour
{
    private HealthComponentBehaviour _health;
    private IEventBus _eventBus;
    // Tracker is event-driven; no direct reference needed.

    [Inject]
    public void Construct(IEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private void Awake()
    {
        _health = GetComponent<HealthComponentBehaviour>();
        // Tracker subscribes to CharacterDeathEvent; no lookup required.
    }

    private void OnEnable()
    {
        // StatComponent expone OnValueChanged(int current, int max)
        _health.Health.OnValueChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        if (_health != null)
            _health.Health.OnValueChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (current <= 0)
        {
            // Publicar evento global usando el IEventBus inyectado
            _eventBus?.Publish(new CharacterDeathEvent(gameObject));

            // DefeatedEnemiesTracker will handle persistence via CharacterDeathEvent subscription

            // Desactivar el objeto para que TurnBasedCombatManager lo considere como "muerto"
            gameObject.SetActive(false);
        }
    }
}
}
