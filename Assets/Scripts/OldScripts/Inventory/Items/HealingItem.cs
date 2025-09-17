using UnityEngine;

[CreateAssetMenu(fileName = "HealingItem", menuName = "Inventory/HealingItem")]
public class HealingItem : MysteryItem, IUsableItem
{
    // Los campos id, displayName, description e icon ahora se heredan de MysteryItem.
    // Ya no es necesario declararlos aquí.
    
    [Header("Healing Properties")]
    [SerializeField] private int healAmount;

    public int HealAmount => healAmount;

    public bool IsConsumable => true;

    public void Use(GameObject user)
    {
        var health = user.GetComponent<PlayerHealthController>();
        if (health != null && health.Health != null)
        {
            health.Health.AffectValue(healAmount);
            #if UNITY_EDITOR
            Debug.Log($"Curado {healAmount} puntos de vida.");
            #endif
        }
        else
        {
            #if UNITY_EDITOR
            Debug.LogWarning("No se pudo curar: PlayerHealthController o HealthComponent no encontrado.");
            #endif
        }
    }
}
