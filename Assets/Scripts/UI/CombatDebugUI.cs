using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A temporary UI for debugging combat. It provides a button to end the player's turn.
/// </summary>
public class CombatDebugUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button endTurnButton;

    private void Start()
    {
        if (endTurnButton == null)
        {
            Debug.LogError("End Turn Button is not assigned in the CombatDebugUI script.", this);
            return;
        }

        // Hook up the button's click event to our method.
        endTurnButton.onClick.AddListener(OnEndTurnButtonPressed);
    }

    private void Update()
    {
        // This logic controls the visibility of the End Turn button.
        // It should only be visible during combat and only on the player's turn.
        bool isPlayerTurn = GameStateManager.CurrentState == GameStateManager.GameState.Combat &&
                            TurnBasedCombatManager.Instance != null &&
                            TurnBasedCombatManager.Instance.CurrentCombatant != null &&
                            TurnBasedCombatManager.Instance.CurrentCombatant.CompareTag("Player");

        if (endTurnButton.gameObject.activeSelf != isPlayerTurn)
        {
            endTurnButton.gameObject.SetActive(isPlayerTurn);
        }
    }

    public void OnEndTurnButtonPressed()
    {
        if (TurnBasedCombatManager.Instance != null)
        {
            Debug.Log("End Turn button pressed. Advancing to the next turn.");
            TurnBasedCombatManager.Instance.NextTurn();
        }
    }
}
