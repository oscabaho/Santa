using UnityEngine;

[CreateAssetMenu(fileName = "HealthPotion", menuName = "Inventory/HealthPotion")]
public class HealthPotion : MysteryItem, IUsableItem
{
    [SerializeField] private int healthToRestore = 25;

    public bool IsConsumable => true;

    public void Use(GameObject user)
    {
        var healthController = user.GetComponent<PlayerHealthController>();
        if (healthController != null)
        {
            healthController.Health.AffectValue(healthToRestore);
            Debug.Log($"Used {DisplayName}, restored {healthToRestore} health.");
        }
    }
}
