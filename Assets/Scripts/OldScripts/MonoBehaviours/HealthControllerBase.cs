using System;
using UnityEngine;

/// <summary>
/// Clase abstracta base para controladores de salud y muerte de entidades.
/// Gestiona vida, daño, debuffs y eventos de muerte.
/// </summary>
[RequireComponent(typeof(HealthComponentBehaviour))]
public abstract class HealthControllerBase : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] protected HealthComponentBehaviour healthBehaviour;
    public event Func<int, int> OnPreTakeDamage;
    public HealthComponent Health => healthBehaviour != null ? healthBehaviour.Health : null;

    protected virtual void Awake()
    {
        if (healthBehaviour == null)
            healthBehaviour = GetComponent<HealthComponentBehaviour>();
        if (healthBehaviour == null)
        {
            #if UNITY_EDITOR
            Debug.LogWarning($"{GetType().Name}: HealthComponentBehaviour no asignado.");
            #endif
        }
    }
    public virtual void TakeDamage(int amount)
    {
        if (Health == null) return;
        int finalAmount = amount;
        if (OnPreTakeDamage != null)
        {
            foreach (Func<int, int> handler in OnPreTakeDamage.GetInvocationList())
            {
                finalAmount = handler(finalAmount);
            }
        }
        Health.AffectValue(-finalAmount);
        if (Health.CurrentValue <= 0)
        {
            // Publica el evento de muerte y luego llama al comportamiento específico de la clase hija.
            GameEventBus.Instance.Publish(new CharacterDeathEvent(gameObject));
            Death();
        }
    }

    protected abstract void Death();
}
