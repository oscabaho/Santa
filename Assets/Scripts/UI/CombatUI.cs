using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System.Collections.Generic;
using VContainer;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using System.Threading.Tasks;

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

    // Abilities loaded dynamically via Addressables
    private Ability directAttackAbility;
    private Ability areaAttackAbility;
    private Ability specialAttackAbility;
    private Ability meditateAbility;
    private bool _abilitiesLoaded = false;

    private IHealthController _playerHealth;
    private IActionPointController _playerAP;
    private ICombatService _combatService;

    private Ability _pendingAbility;
    private List<Button> _actionButtons;

    protected override void Awake()
    {
        GameLog.Log("CombatUI.Awake called.");
        base.Awake(); // Caches the CanvasGroup from UIPanel
        _ = LoadAbilitiesAsync();
    }

    private async Task LoadAbilitiesAsync()
    {
        try
        {
            GameLog.Log("CombatUI: Loading abilities via Addressables...");

            var directTask = Addressables.LoadAssetAsync<Ability>(AbilityAddresses.Direct).Task;
            var areaTask = Addressables.LoadAssetAsync<Ability>(AbilityAddresses.Area).Task;
            var specialTask = Addressables.LoadAssetAsync<Ability>(AbilityAddresses.Special).Task;
            var meditateTask = Addressables.LoadAssetAsync<Ability>(AbilityAddresses.GainAP).Task;

            await Task.WhenAll(directTask, areaTask, specialTask, meditateTask);

            directAttackAbility = directTask.Result;
            areaAttackAbility = areaTask.Result;
            specialAttackAbility = specialTask.Result;
            meditateAbility = meditateTask.Result;

            _abilitiesLoaded = true;
            SetupButtonListeners();
            GameLog.Log("CombatUI: Abilities loaded successfully.");
        }
        catch (OperationException ex)
        {
            GameLog.LogError($"CombatUI: Failed to load abilities via Addressables. Operation failed: {ex.Message}");
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"CombatUI: Unexpected error while loading abilities. {ex.Message}");
        }
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
            HandlePhaseChanged(_combatService.CurrentPhase);
        }
        else
        {
            // Inyección tardía: UIManager inyecta después de instanciar el prefab.
            // Evitar error ruidoso; dejar el panel oculto hasta que llegue la inyección.
            if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);
            GameLog.Log("CombatUI: Waiting for DI to complete (ICombatService not yet injected).", this);
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

    private void OnDestroy()
    {
        // Release Addressables assets
        if (directAttackAbility != null) Addressables.Release(directAttackAbility);
        if (areaAttackAbility != null) Addressables.Release(areaAttackAbility);
        if (specialAttackAbility != null) Addressables.Release(specialAttackAbility);
        if (meditateAbility != null) Addressables.Release(meditateAbility);
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
            // Evitar suscripción si aún no hay jugador asignado (p.ej., antes de que empiece el combate)
            if (_combatService == null || _combatService.Player == null)
            {
                GameLog.Log("CombatUI: Player is not available yet; will subscribe when combat initializes.", this);
                if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);
                return;
            }
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
        if (_combatService == null || _playerHealth != null) return; // Already subscribed or no service yet

        GameObject player = _combatService.Player;
        if (player == null)
        {
            // Aún no se inicializa el combate; esperar a próximo cambio de fase.
            GameLog.Log("CombatUI: Service returned null player; deferring player subscription.", this);
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
        if (!_abilitiesLoaded)
        {
            GameLog.LogWarning("CombatUI: Abilities not loaded yet, deferring button setup.");
            return;
        }

        if (directAttackButton == null) GameLog.LogWarning("Direct Attack Button is NULL");
        if (areaAttackButton == null) GameLog.LogWarning("Area Attack Button is NULL");
        if (specialAttackButton == null) GameLog.LogWarning("Special Attack Button is NULL");
        if (meditateButton == null) GameLog.LogWarning("Meditate Button is NULL");

        _actionButtons = new List<Button>();

        // Local helper to reduce duplication
        void AddButtonListener(Button button, Ability ability)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => RequestAbility(ability));
                _actionButtons.Add(button);
            }
        }

        AddButtonListener(directAttackButton, directAttackAbility);
        AddButtonListener(areaAttackButton, areaAttackAbility);
        AddButtonListener(specialAttackButton, specialAttackAbility);
        AddButtonListener(meditateButton, meditateAbility);
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
        bool hadPendingAbility = _pendingAbility != null;
        _pendingAbility = null;
        ToggleActionButtons(true);
        if (statusText != null)
        {
            statusText.text = "";
        }
        // Restore UI raycast blocking
        if (CanvasGroup != null) CanvasGroup.blocksRaycasts = true;

        if (hadPendingAbility && _combatService != null && _combatService.CurrentPhase == CombatPhase.Targeting)
        {
            _combatService.CancelTargeting();
        }
    }

    private void ToggleActionButtons(bool interactable)
    {
        if (_actionButtons == null)
        {
            return;
        }

        foreach (var button in _actionButtons)
        {
            if (button != null) button.interactable = interactable;
        }
    }
}