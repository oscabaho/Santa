using UnityEngine;

/// <summary>
/// Desactiva el GameObject cuando la vida llega a 0 y notifica al EventBus.
/// </summary>
[RequireComponent(typeof(HealthComponentBehaviour))]
public class DieOnZeroHealth : MonoBehaviour
{
    private HealthComponentBehaviour _health;

    private void Awake()
    {
        _health = GetComponent<HealthComponentBehaviour>();
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
            // Publicar evento global
            ServiceLocator.Get<IEventBus>()?.Publish(new CharacterDeathEvent(gameObject));

            // Desactivar el objeto para que TurnBasedCombatManager lo considere como "muerto"
            gameObject.SetActive(false);
        }
    }
}
