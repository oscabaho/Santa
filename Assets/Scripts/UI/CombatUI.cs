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
    public static CombatUI Instance { get; private set; }

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
        GameLog.LogWarning("CombatUI.Awake called.");
        base.Awake(); // Caches the CanvasGroup from UIPanel

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        SetupButtonListeners();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        GameLog.LogWarning("CombatUI.Start called.");
        if (ServiceLocator.TryGet(out _combatService))
        {
            _combatService.OnPhaseChanged += HandlePhaseChanged;
        }
        else
        {
            GameLog.LogError("CombatUI could not find ICombatService on enable.", this);
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
        _combatService = null;
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

        if (inSelection)
        {
            GameLog.LogWarning($"Direct Attack Button interactable: {directAttackButton.interactable}");
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
        GameLog.LogWarning($"CombatUI.RequestAbility called with ability: {ability?.AbilityName ?? "NULL"}");
        if (ability == null || _combatService == null) return;

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
            if (statusText != null) statusText.text = "Select a target";
        }
        else
        {
            // For non-targeted abilities, submit immediately.
            _combatService.SubmitPlayerAction(ability, null);
        }
    }

    public void OnTargetSelected(GameObject target)
    {
        GameLog.LogWarning($"CombatUI.OnTargetSelected called with target: {target?.name ?? "NULL"}");
        if (_pendingAbility == null) return;

        _combatService.SubmitPlayerAction(_pendingAbility, target);
        
        // Exit targeting mode
        _pendingAbility = null;
        if (statusText != null) statusText.text = "";
        // Buttons will be re-enabled by the phase manager automatically
    }

    private void CancelTargetingMode()
    {
        _pendingAbility = null;
        ToggleActionButtons(true);
        if (statusText != null) statusText.text = "";
    }

    private void ToggleActionButtons(bool interactable)
    {
        foreach (var button in _actionButtons)
        {
            if (button != null) button.interactable = interactable;
        }
    }
}