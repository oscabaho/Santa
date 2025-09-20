using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// A temporary UI for debugging combat. It provides buttons to trigger player abilities.
/// </summary>
public class CombatDebugUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject playerTurnPanel; // A parent object for the buttons
    [SerializeField] private Button directAttackButton;
    [SerializeField] private Button areaAttackButton;
    [SerializeField] private Button specialAttackButton;

    private void Start()
    {
        if (playerTurnPanel == null || directAttackButton == null || areaAttackButton == null || specialAttackButton == null)
        {
            Debug.LogError("A UI element is not assigned in the CombatDebugUI script.", this);
            return;
        }

        directAttackButton.onClick.AddListener(OnDirectAttackPressed);
        areaAttackButton.onClick.AddListener(OnAreaAttackPressed);
        specialAttackButton.onClick.AddListener(OnSpecialAttackPressed);
    }

    private void Update()
    {
        // This logic controls the visibility of the player's action buttons.
        // It should only be visible during combat and only on the player's turn.
        bool isPlayerTurn = GameStateManager.CurrentState == GameStateManager.GameState.Combat &&
                            TurnBasedCombatManager.Instance != null &&
                            TurnBasedCombatManager.Instance.IsPlayerTurn;

        if (playerTurnPanel.activeSelf != isPlayerTurn)
        {
            playerTurnPanel.SetActive(isPlayerTurn);
        }
    }

    public void OnDirectAttackPressed()
    {
        var combatManager = TurnBasedCombatManager.Instance;
        if (combatManager == null) return;

        // For this debug UI, we just attack the first available enemy.
        var firstEnemy = FindFirstActiveEnemy();
        if (firstEnemy != null)
        {
            combatManager.PlayerAttackDirect(firstEnemy);
        }
    }

    public void OnAreaAttackPressed()
    {
        TurnBasedCombatManager.Instance?.PlayerAttackArea();
    }

    public void OnSpecialAttackPressed()
    {
        var combatManager = TurnBasedCombatManager.Instance;
        if (combatManager == null) return;

        // For this debug UI, we just attack the first available enemy.
        var firstEnemy = FindFirstActiveEnemy();
        if (firstEnemy != null)
        {
            combatManager.PlayerAttackSpecial(firstEnemy);
        }
    }

    private GameObject FindFirstActiveEnemy()
    {
        // This is a helper method to find a valid target for single-target attacks.
        // It relies on finding GameObjects with the "Enemy" tag.
        return GameObject.FindGameObjectsWithTag("Enemy").FirstOrDefault(e => e.activeInHierarchy);
    }
}