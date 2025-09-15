using UnityEngine;

namespace ProyectSecret.Inventory
{
    [CreateAssetMenu(fileName = "InventoryConfig", menuName = "ProyectSecret/Inventory/Inventory Config")]
    public class InventoryConfig : ScriptableObject
    {
        [Tooltip("Número máximo de huecos en el inventario.")]
        public int maxSlots = 10;
    }
}