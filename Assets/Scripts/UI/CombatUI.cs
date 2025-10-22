using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the combat UI, including player stats display, action buttons, and target selection.
/// </summary>
public class CombatUI : UIPanel
{
    [Header("Panels")]
    [Tooltip("The parent object containing all the player action buttons.")]
    [SerializeField] private GameObject actionButtonsPanel;

    [Header("Player Stat Displays")]
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TextMeshProUGUI playerAPText;
    [SerializeField] private TextMeshProUGUI statusText; // To show "Select a target"

    [Header("Action Buttons")]
    [SerializeField] private Button directAttackButton;
    [SerializeField] private Button areaAttackButton;
    [SerializeField] private Button specialAttackButton;
    [SerializeField] private Button meditateButton;

    [Header("Ability Assets")]
    [Tooltip("Assign the ScriptableObject for the Direct Attack here.")]
    [SerializeField] private Ability directAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Area Attack here.")]
    [SerializeField] private Ability areaAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Special Attack here.")]
    [SerializeField] private Ability specialAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Meditate Ability here.")]
    [SerializeField] private Ability meditateAbility;

    private IHealthController _playerHealth;
    private IActionPointController _playerAP;
    private ICombatService _combatService;

    private Ability _pendingAbility;
    private List<Button> _actionButtons;

    protected override void Awake()
    {
        GameLog.Log("CombatUI.Awake called.");
        base.Awake(); // Caches the CanvasGroup from UIPanel
        SetupButtonListeners();
    }

    [Inject]
    public void Construct(ICombatService combatService)
    {
        GameLog.Log("CombatUI.Construct called.");
        _combatService = combatService;
    }

    private void OnEnable()
    {
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged += HandlePhaseChanged;
        }
        else
        {
            GameLog.LogError("CombatUI has not been injected with ICombatService.", this);
            if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged -= HandlePhaseChanged;
        }
        UnsubscribeFromPlayerEvents();
    }

    private void HandlePhaseChanged(CombatPhase newPhase)
    {
        if (actionButtonsPanel == null)
        {
            GameLog.LogWarning("ActionButtonsPanel is not assigned in the inspector.", this);
            return;
        }

        bool inSelection = newPhase == CombatPhase.Selection;
        actionButtonsPanel.SetActive(inSelection);

        if (statusText != null && newPhase != CombatPhase.Targeting)
        {
            statusText.text = "";
        }

        if (inSelection)
        {
            SubscribeToPlayerEvents();
            // If we return to selection phase, cancel any pending targeting
            if (_pendingAbility != null)
            {
                CancelTargetingMode();
            }
        }
        else
        {
            UnsubscribeFromPlayerEvents();
        }
    }

    private void SubscribeToPlayerEvents()
    {
        if (_combatService == null || _playerHealth != null) return; // Already subscribed

        GameObject player = _combatService.Player;
        if (player == null)
        {
            GameLog.LogError("ICombatService returned a null player reference.", this);
            return;
        }

        var registry = player.GetComponent<IComponentRegistry>();
        if (registry == null)
        {
            GameLog.LogError("Player is missing an IComponentRegistry.", player);
            return;
        }

        _playerHealth = registry.HealthController;
        _playerAP = registry.ActionPointController;

        if (_playerHealth != null)
        {
            _playerHealth.OnValueChanged += UpdateHealthUI;
            UpdateHealthUI(_playerHealth.CurrentValue, _playerHealth.MaxValue);
        }
        if (_playerAP != null)
        {
            _playerAP.OnValueChanged += UpdateAPUI;
            UpdateAPUI(_playerAP.CurrentValue, _playerAP.MaxValue);
        }
    }

    private void UnsubscribeFromPlayerEvents()
    {
        if (_playerHealth != null) _playerHealth.OnValueChanged -= UpdateHealthUI;
        if (_playerAP != null) _playerAP.OnValueChanged -= UpdateAPUI;
        
        _playerHealth = null;
        _playerAP = null;
    }

    private void SetupButtonListeners()
    {
        if (directAttackButton == null) GameLog.LogWarning("Direct Attack Button is NULL");
        if (areaAttackButton == null) GameLog.LogWarning("Area Attack Button is NULL");
        if (specialAttackButton == null) GameLog.LogWarning("Special Attack Button is NULL");
        if (meditateButton == null) GameLog.LogWarning("Meditate Button is NULL");

        directAttackButton.onClick.AddListener(() => RequestAbility(directAttackAbility));
        areaAttackButton.onClick.AddListener(() => RequestAbility(areaAttackAbility));
        specialAttackButton.onClick.AddListener(() => RequestAbility(specialAttackAbility));
        meditateButton.onClick.AddListener(() => RequestAbility(meditateAbility));

        _actionButtons = new List<Button> { directAttackButton, areaAttackButton, specialAttackButton, meditateButton };
    }

    private void UpdateHealthUI(int current, int max)
    {
        if (playerHealthSlider != null) playerHealthSlider.value = (float)current / max;
    }

    private void UpdateAPUI(int current, int max)
    {
        if (playerAPText != null) playerAPText.text = $"PA: {current}";
    }

    private void RequestAbility(Ability ability)
    {
        GameLog.Log($"CombatUI.RequestAbility called with ability: {ability?.AbilityName ?? "NULL"}");
        if (ability == null || _combatService == null) return;

        // Only allow initiating abilities during the Selection phase
        if (_combatService.CurrentPhase != CombatPhase.Selection)
        {
            GameLog.LogWarning($"Cannot request ability '{ability.AbilityName}': combat is not in Selection phase (current: {_combatService.CurrentPhase}).");
            return;
        }

        // If we are already waiting for a target, a button click should cancel targeting.
        if (_pendingAbility != null)
        {
            CancelTargetingMode();
            return;
        }

        if (ability.Targeting.Style == TargetingStyle.SingleEnemy)
        {
            // Enter targeting mode
            _pendingAbility = ability;
            ToggleActionButtons(false);
            if (statusText != null)
            {
                statusText.text = "Select a target";
            }
            // Make the entire UI panel non-blocking for raycasts
            if (CanvasGroup != null) CanvasGroup.blocksRaycasts = false;

            // Inform the combat manager that we're entering targeting so it can switch phase/camera
            // and remember the pending ability on its side.
            _combatService.SubmitPlayerAction(ability, null);
        }
        else
        {
            // For non-targeted abilities, submit immediately.
            _combatService.SubmitPlayerAction(ability, null);
        }
    }

    public void OnTargetSelected(GameObject target)
    {
        GameLog.Log($"CombatUI.OnTargetSelected called with target: {target?.name ?? "NULL"}");
        if (_pendingAbility == null) return;
        // Ensure the combat manager is in Targeting phase before submitting the selected target
        if (_combatService == null || _combatService.CurrentPhase != CombatPhase.Targeting)
        {
            GameLog.LogError("Cannot submit target: CombatService not in Targeting phase or is null.");
            // Cancel UI-side targeting to avoid stuck state
            CancelTargetingMode();
            return;
        }

        _combatService.SubmitPlayerAction(_pendingAbility, target);
        
        // Exit targeting mode
        _pendingAbility = null;
        if (statusText != null)
        {
            statusText.text = "";
        }
        // Restore UI raycast blocking
        if (CanvasGroup != null) CanvasGroup.blocksRaycasts = true;
        // Buttons will be re-enabled by the phase manager automatically
    }

    private void CancelTargetingMode()
    {
        _pendingAbility = null;
        ToggleActionButtons(true);
        if (statusText != null)
        {
            statusText.text = "";
        }
        // Restore UI raycast blocking
        if (CanvasGroup != null) CanvasGroup.blocksRaycasts = true;
    }

    private void ToggleActionButtons(bool interactable)
    {
        foreach (var button in _actionButtons)
        {
            if (button != null) button.interactable = interactable;
        }
    }
}