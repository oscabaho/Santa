using System.Collections.Generic;

/// <summary>
/// Modelo de inventario que gestiona la lógica de almacenamiento y manipulación de ítems.
/// </summary>
public class InventoryModel
{
    private readonly int maxSlots;
    private readonly List<MysteryItem> items;

    public InventoryModel(int maxSlots)
    {
        this.maxSlots = maxSlots;
        items = new List<MysteryItem>(maxSlots);
    }

    public bool AddItem(MysteryItem item)
    {
        if (items.Count >= maxSlots || item == null)
            return false;
        items.Add(item);
        return true;
    }

    public bool RemoveItem(string itemId)
    {
        var item = FindItem(itemId);
        if (item != null)
        {
            return RemoveItem(item);
        }
        return false;
    }

    public bool RemoveItem(MysteryItem item)
    {
        if (item == null) return false;
        return items.Remove(item);
    }

    public MysteryItem FindItem(string itemId)
    {
        return items.Find(i => i != null && i.Id == itemId);
    }
    public void Clear()
    {
        items.Clear();
    }

    public bool HasItem(string itemId)
    {
        return items.Exists(i => i != null && i.Id == itemId);
    }

    public IReadOnlyList<MysteryItem> GetItems()
    {
        return items.AsReadOnly();
    }
}
