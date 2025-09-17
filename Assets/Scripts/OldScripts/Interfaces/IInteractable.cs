using UnityEngine; // Añadir esta línea

/// <summary>
/// Interfaz para objetos con los que el jugador puede interactuar.
/// </summary>
public interface IInteractable
{
    void Interact(GameObject interactor);
}
