using UnityEngine;
using ProyectSecret.Inventory.Items;
using ProyectSecret.Inventory;
using ProyectSecret.Audio; // Añadido para usar AudioData
using ProyectSecret.Interfaces;

/// <summary>
/// Permite al jugador recoger un arma del suelo e interactuar para equiparla.
/// </summary>
[RequireComponent(typeof(Collider))]
public class WeaponPickup : MonoBehaviour, IInteractable
{
    [Header("ScriptableObject del arma")]
    [SerializeField] private WeaponItem weaponItem; // Asigna el ScriptableObject en el inspector
    [Header("Sonido de recogida")]
    [SerializeField] private AudioData pickupSoundData; // Sonido que se reproduce al recoger el arma

    private void Awake()
    {
        // Asegurarse de que el collider pueda ser detectado por el sistema de interacción.
        // Si PlayerInteractionController usa Raycast, este collider debe estar en la capa 'interactionLayer'.
        // Si usa un trigger, este collider debería ser trigger.
    }

    /// <summary>
    /// Implementación de IInteractable. Se llama cuando el jugador presiona el botón de interactuar.
    /// </summary>
    /// <param name="user">El GameObject que interactúa (el jugador).</param>
    public void Interact(GameObject user)
    {
        var inventory = user.GetComponent<IInventory>();

        // Usamos "guard clauses" para salir temprano si algo falta. Es más limpio.
        if (inventory == null || weaponItem == null)
        {
            #if UNITY_EDITOR
            if (inventory == null) Debug.LogWarning($"WeaponPickup: El objeto '{user.name}' no tiene un componente que implemente IInventory.");
            if (weaponItem == null) Debug.LogWarning($"WeaponPickup: No hay un 'WeaponItem' asignado en el Inspector de '{gameObject.name}'.");
            #endif
            return;
        }

        if (inventory.AddItem(weaponItem))
        {
            // Reproducimos el sonido a través del AudioData.
            pickupSoundData?.Play();
            Destroy(gameObject);
        }
        else
        {
            // Opcional: Mostrar un mensaje de "Inventario lleno" al jugador.
            Debug.Log("No se pudo recoger el arma, el inventario está lleno.");
        }
    }
}
