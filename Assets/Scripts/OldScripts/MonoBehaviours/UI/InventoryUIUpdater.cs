using UnityEngine;

/// <summary>
/// Ejemplo de suscriptor al evento InventoryChangedEvent para actualizar la UI.
/// </summary>
public class InventoryUIUpdater : MonoBehaviour
{
    private void OnEnable()
    {
        GameEventBus.Instance.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    private void OnDisable()
    {
        GameEventBus.Instance.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    private void OnInventoryChanged(InventoryChangedEvent evt)
    {
        // Aquí actualizas la UI según el nuevo estado del inventario
        #if UNITY_EDITOR
        Debug.Log($"Inventario actualizado. Total de ítems: {evt.Inventory.GetItems().Count}");
        #endif
        // Ejemplo: Redibujar slots, actualizar iconos, etc.
    }
}
