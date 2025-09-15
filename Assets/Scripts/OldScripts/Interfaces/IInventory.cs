using ProyectSecret.Inventory.Items;

namespace ProyectSecret.Interfaces
{
    /// <summary>
    /// Interfaz para inventarios que permite consultar si un objeto está presente.
    /// </summary>
    public interface IInventory
    {
        bool HasItem(string itemId);
        bool AddItem(MysteryItem item);
    }
}
