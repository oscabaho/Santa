using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

/// <summary>
/// Manages the combat UI, including player stats display and action buttons.
/// </summary>
public class CombatUI : UIPanel
{
    [Header("Panels")]
    [Tooltip("The parent object containing all the player action buttons.")]
    [SerializeField] private GameObject _actionButtonsPanel;

    [Header("Player Stat Displays")]
    [SerializeField] private Slider _playerHealthSlider;
    [SerializeField] private TextMeshProUGUI _playerAPText;

    [Header("Action Buttons")]
    [SerializeField] private Button _directAttackButton;
    [SerializeField] private Button _areaAttackButton;
    [SerializeField] private Button _specialAttackButton;
    [SerializeField] private Button _meditateButton; // New button for gaining AP

    [Header("Ability Assets")]
    [Tooltip("Assign the ScriptableObject for the Direct Attack here.")]
    [SerializeField] private Ability _directAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Area Attack here.")]
    [SerializeField] private Ability _areaAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Special Attack here.")]
    [SerializeField] private Ability _specialAttackAbility;
    [Tooltip("Assign the ScriptableObject for the Meditate Ability here.")]
    [SerializeField] private Ability _meditateAbility;

    private IHealthController _playerHealth;
    private IActionPointController _playerAP;
    private ICombatService _combatService;

    protected override void Awake()
    {
        base.Awake(); // Caches the CanvasGroup from UIPanel
        SetupButtonListeners();
    }

    private void OnEnable()
    {
        if (ServiceLocator.TryGet(out _combatService))
        {
            // Subscribe to the main phase change event
            _combatService.OnPhaseChanged += HandlePhaseChanged;
            // Handle the initial state when the UI is enabled
            HandlePhaseChanged(_combatService.CurrentPhase);
        }
        else
        {
            GameLog.LogError("CombatUI could not find ICombatService on enable.", this);
            // Hide panels if the service isn't ready
            _actionButtonsPanel.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged -= HandlePhaseChanged;
        }
        // Always unsubscribe from player stats to prevent memory leaks
        UnsubscribeFromPlayerEvents();
        _combatService = null;
    }

    /// <summary>
    /// The core logic that reacts to changes in the combat flow.
    /// </summary>
private void HandlePhaseChanged(CombatPhase newPhase)
{
    // Ensure the action panel is valid before trying to use it
    if (_actionButtonsPanel == null)
    {
        GameLog.LogWarning("ActionButtonsPanel is not assigned in the inspector.", this);
        return;
    }

    switch (newPhase)
    {
        case CombatPhase.Selection:
            // Player's turn to choose an action
            _actionButtonsPanel.SetActive(true);
            SubscribeToPlayerEvents(); // Subscribe to stats only when relevant
            break;

        case CombatPhase.Execution:
            // Actions are being performed, hide buttons
            _actionButtonsPanel.SetActive(false);
            UnsubscribeFromPlayerEvents(); // Unsubscribe to save performance
            break;

        case CombatPhase.Victory:
        case CombatPhase.Defeat:
            // End of combat, hide buttons
            _actionButtonsPanel.SetActive(false);
            UnsubscribeFromPlayerEvents();
            break;
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
        // Assigns the RequestAbility method to each button's click event.
        _directAttackButton.onClick.AddListener(() => RequestAbility(_directAttackAbility));
        _areaAttackButton.onClick.AddListener(() => RequestAbility(_areaAttackAbility));
        _specialAttackButton.onClick.AddListener(() => RequestAbility(_specialAttackAbility));
        _meditateButton.onClick.AddListener(() => RequestAbility(_meditateAbility));
    }

    private void UpdateHealthUI(int current, int max)
    {
        if (_playerHealthSlider != null) _playerHealthSlider.value = (float)current / max;
    }

    private void UpdateAPUI(int current, int max)
    {
        if (_playerAPText != null) _playerAPText.text = $"PA: {current}";
    }

    /// <summary>
    /// Called when an action button is pressed. It submits the ability request to the combat service.
    /// </summary>
    private void RequestAbility(Ability ability)
    {
        if (ability == null || _combatService == null)
        {
            GameLog.LogWarning("Ability or Combat Service is not available.", this);
            return;
        }

        // The UI's only job is to signal the intent. The service handles the rest.
        _combatService.SubmitPlayerAction(ability);
    }
}
