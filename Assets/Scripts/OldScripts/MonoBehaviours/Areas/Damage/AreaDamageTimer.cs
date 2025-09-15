using UnityEngine;
using ProyectSecret.Components;

namespace ProyectSecret.Areas.Damage
{
    public class AreaDamageTimer : MonoBehaviour
    {
        private int damage;
        private float damageInterval;
        private float timer = 0f;
        private HealthComponentBehaviour healthBehaviour;

        // Inicialización desde AreaDamage
        public void Init(int damage, float interval)
        {
            this.damage = damage;
            this.damageInterval = interval;
        }

        private void Awake()
        {
            healthBehaviour = GetComponent<HealthComponentBehaviour>();
            if (healthBehaviour == null || healthBehaviour.Health == null)
            {
                Debug.LogWarning("AreaDamageTimer: No se encontró HealthComponentBehaviour en el objeto.");
                enabled = false;
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer >= damageInterval)
            {
                ApplyDamage();
                timer = 0f;
            }
        }

        private void ApplyDamage()
        {
            if (healthBehaviour != null && healthBehaviour.Health != null)
            {
                healthBehaviour.Health.AffectValue(-damage);
                Debug.Log($"AreaDamageTimer: {gameObject.name} recibe {damage} de daño continuo por área.");
            }
        }
    }
}
