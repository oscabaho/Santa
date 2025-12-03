using UnityEngine; // Required for GameObject reference

namespace Santa.Core
{
    /// <summary>
    /// Interface for objects the player can interact with.
    /// </summary>
    public interface IInteractable
    {
        void Interact(GameObject interactor);
    }
}
