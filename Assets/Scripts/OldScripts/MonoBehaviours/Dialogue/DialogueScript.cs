using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProyectSecret.UI.Dialogue
{
    public class DialogueScript : MonoBehaviour
    {
        public TextMeshProUGUI dialogueText;

        public string[] dialogueLines;
        [SerializeField]private float textSpeed = 0.1f;
        public int index;
        private InputAction interactAction;
        private InputAction look;
        void Awake()
        {
            dialogueText.text = string.Empty;
            interactAction = InputSystem.actions.FindAction("Interact");
            look = InputSystem.actions.FindAction("Look");
            gameObject.SetActive(false);
        }

        void Update()
        {
            if (interactAction.WasPressedThisFrame())
            {
                if (dialogueText.text == dialogueLines[index])
                {
                    NextLine();
                }
                else
                {
                    StopAllCoroutines();
                    dialogueText.text = dialogueLines[index];
                }
            }
        }

        public void StartDialogue()
        {
            index = 0;
            StartCoroutine(WriteLine());
        }

        IEnumerator WriteLine()
        {
            foreach (var letter in dialogueLines[index])
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(textSpeed);
            }
        }

        private void NextLine()
        {
            if (index < dialogueLines.Length - 1)
            {
                index++;
                dialogueText.text = string.Empty;
                StartCoroutine(WriteLine());
            }
            else
            {
                dialogueText.text = string.Empty;
                gameObject.SetActive(false);
            }
        }
    }
}
