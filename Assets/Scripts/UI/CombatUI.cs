using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// A UI for triggering combat abilities. This is now connected to the Ability system.
/// </summary>
public class CombatUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject playerTurnPanel;
    [SerializeField] private Button directAttackButton;
    [SerializeField] private Button areaAttackButton;
    [SerializeField] private Button specialAttackButton;

    [Header("Ability Assets")]
    [Tooltip("Assign the ScriptableObject for the Direct Attack here.")]
    [SerializeField] private Ability _directAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Area Attack here.")]
    [SerializeField] private Ability _areaAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Special Attack here.")]
    [SerializeField] private Ability _specialAttackAbility;

    private void Start()
    {
        if (_directAttackAbility == null || _areaAttackAbility == null || _specialAttackAbility == null)
        {
            Debug.LogError("One or more Ability assets are not assigned in CombatUI.", this);
            directAttackButton.interactable = false;
            areaAttackButton.interactable = false;
            specialAttackButton.interactable = false;
            return;
        }

        directAttackButton.onClick.AddListener(() => RequestAbility(_directAttackAbility));
        areaAttackButton.onClick.AddListener(() => RequestAbility(_areaAttackAbility));
        specialAttackButton.onClick.AddListener(() => RequestAbility(_specialAttackAbility));

        playerTurnPanel.SetActive(false);
    }

    private void RequestAbility(Ability ability)
    {
        var combatService = ServiceLocator.Get<ICombatService>();
        if (ability == null || combatService == null) return;

        // For this UI, we need a primary target for some abilities.
        // We'll simply pick the first available enemy from the manager's list.
        // A real UI would have a proper target selection system (e.g., clicking on an enemy).
        GameObject primaryTarget = combatService.Enemies
            .FirstOrDefault(enemy => enemy != null && enemy.activeInHierarchy);

        if (primaryTarget == null && (ability.Targeting == TargetingStyle.SingleEnemy || ability.Targeting == TargetingStyle.RandomEnemies))
        {
            Debug.LogWarning("Could not find a valid enemy to target for this UI.");
        }

        combatService.SubmitPlayerAction(ability, primaryTarget);
    }

    private void OnEnable()
    {
        var combatService = ServiceLocator.Get<ICombatService>();
        if (combatService != null)
        {
            combatService.OnPlayerTurnStarted += ShowPlayerTurnPanel;
            combatService.OnPlayerTurnEnded += HidePlayerTurnPanel;
        }
    }

    private void OnDisable()
    {
        var combatService = ServiceLocator.Get<ICombatService>();
        if (combatService != null)
        {
            combatService.OnPlayerTurnStarted -= ShowPlayerTurnPanel;
            combatService.OnPlayerTurnEnded -= HidePlayerTurnPanel;
        }
    }

    private void ShowPlayerTurnPanel()
    {
        playerTurnPanel.SetActive(true);
    }

    private void HidePlayerTurnPanel()
    {
        playerTurnPanel.SetActive(false);
    }
}
