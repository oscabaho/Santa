using UnityEngine;
using ProyectSecret.Interfaces;
using ProyectSecret.UI.Dialogue;

/// <summary>
/// Componente que se añade a los NPCs o objetos que pueden iniciar una conversación.
/// Implementa IInteractable para ser detectado por el PlayerInteractionController.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Conversation : MonoBehaviour, IInteractable
{
    [Header("Referencias de Diálogo")]
    [Tooltip("Arrastra aquí el panel de la UI que contiene el DialogueScript.")]
    [SerializeField] private DialogueScript dialogueUI;
    
    [Header("Contenido del Diálogo")]
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogueLines;

    private void Awake()
    {
        // Si no se asigna la UI en el inspector, intenta encontrarla en la escena.
        // Esto es útil, pero es mejor asignarla manualmente para evitar errores.
        if (dialogueUI == null)
        {
            // Usamos el método más nuevo y eficiente para encontrar el objeto, incluyendo los inactivos.
            dialogueUI = FindAnyObjectByType<DialogueScript>(FindObjectsInactive.Include);
            if (dialogueUI == null)
            {
                Debug.LogError("Conversation: No se pudo encontrar el DialogueScript en la escena.", this);
            }
        }
    }

    /// <summary>
    /// Este método se llama cuando el jugador presiona el botón de interacción.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (dialogueUI != null && interactor.CompareTag("Player"))
        {
            // Le pasamos nuestras líneas de diálogo al controlador de la UI.
            dialogueUI.dialogueLines = this.dialogueLines;
            
            // Activamos el panel de la UI y comenzamos el diálogo.
            dialogueUI.gameObject.SetActive(true);
            dialogueUI.StartDialogue();
        }
    }
}
