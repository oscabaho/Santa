using System;
using UnityEngine;

/// <summary>
/// Componente de kriptonita: debilita al enemigo si el jugador tiene el objeto indicado.
/// Aplica un multiplicador al daño recibido.
/// </summary>
public class KryptoniteDebuff : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string kryptoniteItemId = "cebolla";
    [SerializeField] private float damageMultiplier = 2f; // El daño recibido se multiplica por este valor
    
    [Header("Dependencies")]
    [Tooltip("Controlador de salud del enemigo al que se aplicará el debuff.")]
    [SerializeField] private EnemyHealthController healthController;

    private bool isDebuffed = false;

    private void Awake()
    {
        // Intentar obtener la dependencia si no está asignada en el Inspector.
        if (healthController == null)
        {
            healthController = GetComponent<EnemyHealthController>();
        }

        // Ahora validar que la dependencia exista, ya sea por Inspector o por GetComponent.
        if (healthController == null)
        {
            Debug.LogError("KryptoniteDebuff: EnemyHealthController no está asignado y no se pudo encontrar en el GameObject.", this);
            return; // Detener la ejecución si la dependencia no está presente.
        }
        
        healthController.OnPreTakeDamage += OnPreTakeDamageHandler;
    }

    public void CheckKryptonite(GameObject player)
    {
        var inventory = player.GetComponent<IInventory>();
        if (inventory != null && inventory.HasItem(kryptoniteItemId))
        {
            ApplyDebuff();
        }
        else
        {
            RemoveDebuff();
        }
    }

    private void ApplyDebuff()
    {
        if (isDebuffed) return;
        isDebuffed = true;
        Debug.Log($"{gameObject.name} recibirá daño multiplicado por {damageMultiplier} debido a la kriptonita: {kryptoniteItemId}");
    }

    private void RemoveDebuff()
    {
        if (!isDebuffed) return;
        isDebuffed = false;
        Debug.Log($"{gameObject.name} restaurado a daño normal");
    }

    private int OnPreTakeDamageHandler(int baseDamage)
    {
        if (isDebuffed)
            return Mathf.RoundToInt(baseDamage * damageMultiplier);
        return baseDamage;
    }

    private void OnDestroy()
    {
        if (healthController != null)
            healthController.OnPreTakeDamage -= OnPreTakeDamageHandler;
    }
}
