using Santa.Core;
using Santa.Core.Config;
using UnityEngine;

namespace Santa.Domain.Dialogue
{

/// <summary>
/// Component added to NPCs or objects that can start a conversation.
/// Implements IInteractable to be detected by the PlayerInteractionController.
/// </summary>
[RequireComponent(typeof(Collider))]
public class Conversation : MonoBehaviour, IInteractable
{
    [Header("Dialogue References")]
    [Tooltip("Drag here the UI panel that contains the DialogueScript.")]
    [SerializeField] private DialogueScript dialogueUI;

    [Header("Dialogue Content")]
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogueLines;

    private void Awake()
    {
        // If the UI is not assigned in the inspector, attempt to find it in the scene.
        // This is useful, but manual assignment is preferred to avoid errors.
        if (dialogueUI == null)
        {
            // Use modern method to find the object, including inactive ones.
            dialogueUI = FindAnyObjectByType<DialogueScript>(FindObjectsInactive.Include);
            if (dialogueUI == null)
            {
                GameLog.LogError("Conversation: Could not find DialogueScript in the scene.", this);
            }
        }
    }

    /// <summary>
    /// Called when the player presses the interaction button.
    /// </summary>
    public void Interact(GameObject interactor)
    {
        if (dialogueUI != null && interactor.CompareTag(GameConstants.Tags.Player))
        {
            // Pass dialogue lines to the UI controller.
            dialogueUI.dialogueLines = this.dialogueLines;

            // Activate the UI panel and start the dialogue.
            dialogueUI.gameObject.SetActive(true);
            dialogueUI.StartDialogue();
        }
    }
}
}
