using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for items that can be used from the inventory.
    /// </summary>
    public interface IUsableItem
    {
        bool IsConsumable { get; }
        void Use(GameObject user);
    }
}
