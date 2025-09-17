using UnityEngine;

public class AreaDamage : MonoBehaviour
{
    [Tooltip("Daño infligido al entrar en el área")]
    [SerializeField] private int damage = 10;
    [Tooltip("Intervalo en segundos para aplicar daño continuo")]
    [SerializeField] private float damageInterval = 1f;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"AreaDamage: OnTriggerEnter con {other.gameObject.name}");
        var healthBehaviour = other.GetComponent<HealthComponentBehaviour>();
        if (healthBehaviour != null && healthBehaviour.Health != null)
        {
            healthBehaviour.Health.AffectValue(-damage); // Daño inmediato al entrar
            Debug.Log($"AreaDamage: {other.gameObject.name} recibió {damage} de daño al entrar en el área.");

            AreaDamageTimer timer = other.gameObject.AddComponent<AreaDamageTimer>();
            timer.Init(damage, damageInterval); // Inicializa con los valores del área
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var timer = other.GetComponent<AreaDamageTimer>();
        if (timer != null)
            Destroy(timer);
    }
}
