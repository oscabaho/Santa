using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerEquipmentController))]
public class PlayerInventory : MonoBehaviour, IInventory, IPersistentData
{
    private PlayerEquipmentController equipmentController;
    [Header("Configuración")]
    [SerializeField] private InventoryConfig inventoryConfig;
    [SerializeField] private List<MysteryItem> initialItems = new List<MysteryItem>();
    private InventoryModel inventoryModel;

    private void Awake()
    {
        if (inventoryConfig == null)
        {
            Debug.LogError("InventoryConfig no está asignado. El inventario no funcionará correctamente. Por favor, asigna un InventoryConfig en el Inspector.", this);
            enabled = false; // Desactiva el componente si la configuración crítica falta.
            return;
        }

        inventoryModel = new InventoryModel(inventoryConfig.maxSlots);
        equipmentController = GetComponent<PlayerEquipmentController>();
        foreach (var item in initialItems)
        {
            if (item != null)
                AddItem(item); // Usar AddItem para publicar el evento
        }
    }

#if UNITY_EDITOR
    // Esta comprobación se ejecuta en el editor para advertir al desarrollador con antelación.
    private void OnValidate()
    {
        if (inventoryConfig == null)
        {
            Debug.LogWarning("PlayerInventory: El campo InventoryConfig no está asignado. El componente no funcionará en tiempo de ejecución.", this);
        }
    }
#endif

    public bool HasItem(string itemId)
    {
        return inventoryModel.HasItem(itemId);
    }

    public bool AddItem(MysteryItem item)
    {
        bool success = inventoryModel.AddItem(item);
        if (success)
            GameEventBus.Instance.Publish(new InventoryChangedEvent(this));
        return success;
    }

    public bool RemoveItem(string itemId)
    {
        bool success = inventoryModel.RemoveItem(itemId);
        if (success)
            GameEventBus.Instance.Publish(new InventoryChangedEvent(this));
        return success;
    }

    public IReadOnlyList<MysteryItem> GetItems() => inventoryModel.GetItems();

    public bool UseItem(string itemId, GameObject user)
    {
        var item = inventoryModel.FindItem(itemId);
        if (item == null) return false;

        // Prioriza equipar sobre el uso genérico para ítems que se pueden equipar.
        if (item is IEquipable)
        {
            // Podemos llamar directamente a EquipItem, que ya maneja la eliminación del inventario.
            return EquipItem(item, user);
        }

        // Si no es equipable, se recurre al uso genérico.
        if (item is IUsableItem usable)
        {
            usable.Use(user);
            if (usable.IsConsumable)
            {
                inventoryModel.RemoveItem(item); // Eliminar solo si es consumible
                GameEventBus.Instance.Publish(new InventoryChangedEvent(this));
            }
            return true;
        }

        return false;
    }
    
    public bool EquipItem(string itemId, GameObject user)
    {
        // Este método ahora solo busca el ítem y llama a la sobrecarga.
        var itemToEquip = inventoryModel.FindItem(itemId);
        if (itemToEquip != null)
        {
            return EquipItem(itemToEquip, user);
        }
        return false;
    }

    // Sobrecarga privada para EquipItem que acepta el objeto directamente, evitando otra búsqueda.
    private bool EquipItem(MysteryItem item, GameObject user)
    {
        if (item is IEquipable equipable && equipmentController != null)
        {
            if (equipable.GetSlotType() == EquipmentSlotType.Weapon && item is WeaponItem weaponItem)
            {
                equipmentController.EquipWeapon(weaponItem);
                inventoryModel.RemoveItem(item); // Eliminar después de equipar
                GameEventBus.Instance.Publish(new InventoryChangedEvent(this));
                return true;
            }
        }
        return false;
    }

    public SerializableInventoryData ExportInventoryData()
    {
        var data = new SerializableInventoryData();
        foreach (var item in inventoryModel.GetItems())
        {
            if (item != null)
            {
                data.itemIds.Add(item.Id);
            }
        }
        return data;
    }

    public void ImportInventoryData(SerializableInventoryData data, ItemDatabase itemDatabase)
    {
        if (data == null || itemDatabase == null) return;

        inventoryModel.Clear();

        foreach (var id in data.itemIds)
        {
            var item = itemDatabase.GetItem(id);
            if (item != null)
                AddItem(item);
        }
    }

    #region IPersistentData Implementation

    public void SaveData(PlayerPersistentData data)
    {
        data.inventoryData = ExportInventoryData();
    }

    public void LoadData(PlayerPersistentData data, ItemDatabase itemDatabase)
    {
        ImportInventoryData(data.inventoryData, itemDatabase);
    }

    #endregion
}
