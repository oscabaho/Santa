using UnityEngine;

namespace ProyectSecret.Interfaces
{
    /// <summary>
    /// Interfaz para ítems que pueden ser "usados" desde el inventario.
    /// </summary>
    public interface IUsableItem
    {
        bool IsConsumable { get; }
        void Use(GameObject user);
    }
}