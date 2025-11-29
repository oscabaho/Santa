using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.UI;
using VContainer;

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

    [Header("Enemy Stat Displays")]
    [SerializeField] private Slider rightEnemyHealthSlider;
    [SerializeField] private Slider leftEnemyHealthSlider;
    [SerializeField] private Slider centralEnemyHealthSlider;

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

    // Callbacks for enemy health updates with cached position data
    private struct EnemyHealthData
    {
        public IHealthController Health;
        public System.Action<int, int> Callback;
        public CombatPosition Position; // Cached to avoid GetComponent
    }
    private readonly Dictionary<GameObject, EnemyHealthData> _enemyHealthData = new Dictionary<GameObject, EnemyHealthData>();

    private Ability _pendingAbility;
    private List<Button> _actionButtons;

    protected override void Awake()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("CombatUI.Awake called.");
#endif
        base.Awake(); // Caches the CanvasGroup from UIPanel
        _ = LoadAbilitiesAsync();
    }

    private async Task LoadAbilitiesAsync()
    {
        try
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("CombatUI: Loading abilities via Addressables...");
#endif

            var directTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.Direct).Task;
            var areaTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.Area).Task;
            var specialTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.Special).Task;
            var meditateTask = Addressables.LoadAssetAsync<Ability>(Santa.Core.Addressables.AddressableKeys.Abilities.GainAP).Task;

            await Task.WhenAll(directTask, areaTask, specialTask, meditateTask);

            directAttackAbility = directTask.Result;
            areaAttackAbility = areaTask.Result;
            specialAttackAbility = specialTask.Result;
            meditateAbility = meditateTask.Result;

            _abilitiesLoaded = true;
            SetupButtonListeners();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("CombatUI: Abilities loaded successfully.");
#endif
            // Now that abilities are loaded, handle current phase if service is ready
            if (_combatService != null && isActiveAndEnabled)
            {
                HandlePhaseChanged(_combatService.CurrentPhase);
            }
        }
        catch (OperationException ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"CombatUI: Failed to load abilities via Addressables. Operation failed: {ex.Message}");
#endif
        }
        catch (System.Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"CombatUI: Unexpected error while loading abilities. {ex.Message}");
#endif
        }
    }

    [Inject]
    public void Construct(ICombatService combatService)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("CombatUI.Construct called.");
#endif
        _combatService = combatService;
    }

    private void OnEnable()
    {
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged += HandlePhaseChanged;
            // Only handle phase immediately if abilities have been loaded to avoid race
            if (_abilitiesLoaded)
            {
                HandlePhaseChanged(_combatService.CurrentPhase);
            }
        }
        else
        {
            // Late injection: UIManager injects after instantiating the prefab.
            // Avoid noisy errors; keep the panel hidden until injection arrives.
            if (actionButtonsPanel != null) actionButtonsPanel.SetActive(false);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("CombatUI: Waiting for DI to complete (ICombatService not yet injected).", this);
#endif
        }
    }

    private void OnDisable()
    {
        if (_combatService != null)
        {
            _combatService.OnPhaseChanged -= HandlePhaseChanged;
        }
        UnsubscribeFromPlayerEvents();
        UnsubscribeFromEnemyEvents();
    }

    private void OnDestroy()
    {
        // Release Addressables assets

        _playerHealth = null;
        _playerAP = null;
    }

    private void HandlePhaseChanged(CombatPhase newPhase)
    {
        if (actionButtonsPanel == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("ActionButtonsPanel is not assigned in the inspector.", this);
#endif
            return;
        }

        bool inSelection = newPhase == CombatPhase.Selection;
        actionButtonsPanel.SetActive(inSelection);

        if (statusText != null && newPhase != CombatPhase.Targeting)
        {
            statusText.text = "";
        }

        // Always try to subscribe to enemies if not already subscribed
        SubscribeToEnemyEvents();

        if (inSelection)
        {
            // Avoid subscription if player is not assigned yet (e.g., before combat starts)
            if (_combatService == null || _combatService.Player == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose("CombatUI: Player is not available yet; will subscribe when combat initializes.", this);
#endif
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
        else if (newPhase == CombatPhase.Victory || newPhase == CombatPhase.Defeat)
        {
            // Combat ended; clean up subscriptions to break potential reference cycles
            // and ensure we don't hold onto destroyed enemies.
            UnsubscribeFromPlayerEvents();
            UnsubscribeFromEnemyEvents();
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
            // Combat has not initialized yet; wait for next phase change.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("CombatUI: Service returned null player; deferring player subscription.", this);
#endif
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

    private void SubscribeToEnemyEvents()
    {
        if (_combatService == null || _enemyHealthData.Count > 0) return; // Already subscribed or no service

        var enemies = _combatService.Enemies;
        if (enemies == null || enemies.Count == 0) return;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var registry = enemy.GetComponent<IComponentRegistry>();
            if (registry != null && registry.HealthController != null)
            {
                var health = registry.HealthController;
                System.Action<int, int> callback = null;
                CombatPosition position = CombatPosition.Center; // Default

                // Cache position from component (called once per enemy)
                var posId = enemy.GetComponent<CombatPositionIdentifier>();
                if (posId != null)
                {
                    position = posId.Position;
                    switch (position)
                    {
                        case CombatPosition.Right:
                            callback = (curr, max) => UpdateSlider(rightEnemyHealthSlider, curr, max);
                            break;
                        case CombatPosition.Left:
                            callback = (curr, max) => UpdateSlider(leftEnemyHealthSlider, curr, max);
                            break;
                        case CombatPosition.Center:
                            callback = (curr, max) => UpdateSlider(centralEnemyHealthSlider, curr, max);
                            break;
                    }
                }
                else
                {
                    // Fallback to name-based identification (Legacy)
                    if (enemy.name.Contains("Right", System.StringComparison.OrdinalIgnoreCase))
                    {
                        position = CombatPosition.Right;
                        callback = (curr, max) => UpdateSlider(rightEnemyHealthSlider, curr, max);
                    }
                    else if (enemy.name.Contains("Left", System.StringComparison.OrdinalIgnoreCase))
                    {
                        position = CombatPosition.Left;
                        callback = (curr, max) => UpdateSlider(leftEnemyHealthSlider, curr, max);
                    }
                    else if (enemy.name.Contains("Central", System.StringComparison.OrdinalIgnoreCase))
                    {
                        position = CombatPosition.Center;
                        callback = (curr, max) => UpdateSlider(centralEnemyHealthSlider, curr, max);
                    }
                }

                if (callback != null)
                {
                    health.OnValueChanged += callback;

                    // Cache all data together
                    _enemyHealthData[enemy] = new EnemyHealthData
                    {
                        Health = health,
                        Callback = callback,
                        Position = position
                    };

                    // Initial update
                    callback(health.CurrentValue, health.MaxValue);
                }
            }
        }
    }

    private void UnsubscribeFromEnemyEvents()
    {
        foreach (var kvp in _enemyHealthData)
        {
            if (kvp.Value.Health != null)
            {
                kvp.Value.Health.OnValueChanged -= kvp.Value.Callback;
            }
        }
        _enemyHealthData.Clear();
    }

    private void UpdateSlider(Slider slider, int current, int max)
    {
        if (slider != null && max > 0)
        {
            slider.value = (float)current / max;
        }
    }

    private void SetupButtonListeners()
    {
        if (!_abilitiesLoaded)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("CombatUI: Abilities not loaded yet, deferring button setup.");
#endif
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
        if (playerAPText != null) playerAPText.text = string.Format(Santa.Core.Config.UIStrings.ActionPointsLabelFormat, current);
        RefreshButtonInteractability();
    }

    private void RequestAbility(Ability ability)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"CombatUI.RequestAbility called with ability: {ability?.AbilityName ?? "NULL"}");
#endif
        if (ability == null || _combatService == null) return;

        // Only allow initiating abilities during the Selection phase
        if (_combatService.CurrentPhase != CombatPhase.Selection)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"Cannot request ability '{ability.AbilityName}': combat is not in Selection phase (current: {_combatService.CurrentPhase}).");
#endif
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
                statusText.text = Santa.Core.Config.UIStrings.SelectTarget;
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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"CombatUI.OnTargetSelected called with target: {target?.name ?? "NULL"}");
#endif
        if (_pendingAbility == null) return;
        // Ensure the combat manager is in Targeting phase before submitting the selected target
        if (_combatService == null || _combatService.CurrentPhase != CombatPhase.Targeting)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("Cannot submit target: CombatService not in Targeting phase or is null.");
#endif
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

    private void RefreshButtonInteractability()
    {
        if (_playerAP == null || !_abilitiesLoaded) return;
        int currentAP = _playerAP.CurrentValue;

        void SetButtonState(Button button, Ability ability)
        {
            if (button != null && ability != null)
                button.interactable = currentAP >= ability.ApCost;
        }

        SetButtonState(directAttackButton, directAttackAbility);
        SetButtonState(areaAttackButton, areaAttackAbility);
        SetButtonState(specialAttackButton, specialAttackAbility);
        SetButtonState(meditateButton, meditateAbility);
    }
}