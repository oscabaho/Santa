using UnityEngine;
using ProyectSecret.Interfaces;
using ProyectSecret.Events;

namespace ProyectSecret.MonoBehaviours.Player
{
    /// <summary>
    /// Gestiona la interacción del jugador con el mundo (NPCs, objetos, etc.).
    /// </summary>
    [RequireComponent(typeof(PlayerInputController))]
    public class PlayerInteractionController : MonoBehaviour
    {
        [Header("Dependencias")]
        [Tooltip("Referencia al componente InteractionDetector que está en un objeto hijo.")]
        [SerializeField] private InteractionDetector interactionDetector;

        private IInteractable currentInteractable;
        private IInteractable lastInteractable;
        private PlayerInputController inputController;

        private void Awake()
        {
            inputController = GetComponent<PlayerInputController>();

            if (interactionDetector == null)
            {
                Debug.LogError("PlayerInteractionController: No se ha asignado un InteractionDetector en el Inspector. Este componente es necesario para detectar objetos interactuables.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (inputController != null)
                inputController.OnInteractPressed += PerformInteraction;
        }

        private void OnDisable()
        {
            if (inputController != null)
                inputController.OnInteractPressed -= PerformInteraction;
        }

        private void Update()
        {
            // Constantemente revisa si hay algo interactuable en frente.
            if (interactionDetector != null)
            {
                lastInteractable = currentInteractable;
                // El detector nos da el interactuable más cercano en el trigger.
                // Podrías añadir lógica adicional para ver si está en el campo de visión.
                currentInteractable = interactionDetector.GetClosestInteractable(transform);

                // Si el interactuable ha cambiado, publicamos eventos.
                if (currentInteractable != lastInteractable)
                {
                    if (lastInteractable != null)
                        GameEventBus.Instance.Publish(new InteractableOutOfRangeEvent(lastInteractable));
                    if (currentInteractable != null)
                        GameEventBus.Instance.Publish(new InteractableInRangeEvent(currentInteractable));
                }
            }
        }

        // Este método solo se llama cuando se presiona el botón de interactuar.
        private void PerformInteraction()
        {
            // Si tenemos un interactuable a la vista, llamamos a su método Interact.
            if (currentInteractable != null)
            {
                currentInteractable.Interact(gameObject);
            }
        }
    }
}
