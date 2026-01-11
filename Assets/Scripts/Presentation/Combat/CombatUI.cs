using Santa.Core;
using Santa.Core.Config;
using Santa.Domain.Combat;
using Santa.Presentation.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Santa.Presentation.Combat
{

    /// <summary>
    /// Orchestrates the combat UI, coordinating between stats display and action buttons.
    /// Handles combat phase changes and targeting mode coordination.
    /// Refactored to delegate responsibilities to specialized components.
    /// </summary>
    public class CombatUI : UIPanel
    {
        [Header("Panels")]
        [Tooltip("The parent object containing all the player action buttons.")]
        [SerializeField] private GameObject actionButtonsPanel;

        [Header("Player Stat Displays")]
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private TextMeshProUGUI playerAPText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Enemy Stat Displays")]
        [SerializeField] private Slider rightEnemyHealthSlider;
        [SerializeField] private Slider leftEnemyHealthSlider;
        [SerializeField] private Slider centralEnemyHealthSlider;

        [Header("Action Buttons")]
        [SerializeField] private Button directAttackButton;
        [SerializeField] private Button areaAttackButton;
        [SerializeField] private Button specialAttackButton;
        [SerializeField] private Button meditateButton;

        // Specialized components
        private CombatUIStatsDisplay _statsDisplay;
        private CombatUIActionButtons _actionButtons;

        // Dependencies
        private ICombatService _combatService;

        // Targeting state
        private Ability _pendingAbility;

        protected override void Awake()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose(Santa.Core.Config.LogMessages.CombatUI.AwakeCalled);
#endif
            base.Awake();

            // Initialize specialized components
            InitializeComponents();
        }

        [Inject]
        public void Construct(ICombatService combatService)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose(Santa.Core.Config.LogMessages.CombatUI.ConstructCalled);
#endif
            InitializeServiceConnection(combatService);
        }

        private void InitializeServiceConnection(ICombatService combatService)
        {
            _combatService = combatService;

            // If we are enabled and have the service, subscribe now
            if (isActiveAndEnabled && _combatService != null)
            {
                _combatService.OnPhaseChanged += HandlePhaseChanged;

                // If already in a phase, update immediately
                HandlePhaseChanged(_combatService.CurrentPhase);
            }
        }

        private void InitializeComponents()
        {
            // Create or get stats display component
            _statsDisplay = gameObject.AddComponent<CombatUIStatsDisplay>();
            _statsDisplay.Initialize(
                playerHealthSlider,
                playerAPText,
                leftEnemyHealthSlider,
                centralEnemyHealthSlider,
                rightEnemyHealthSlider
            );

            // Create or get action buttons component
            _actionButtons = gameObject.AddComponent<CombatUIActionButtons>();
            _actionButtons.Initialize(
                directAttackButton,
                areaAttackButton,
                specialAttackButton,
                meditateButton
            );

            // Subscribe to ability requests from buttons
            _actionButtons.OnAbilityRequested += HandleAbilityRequested;
        }

        private void OnEnable()
        {
            if (_combatService != null)
            {
                _combatService.OnPhaseChanged += HandlePhaseChanged;

                // If already in a phase, update immediately
                HandlePhaseChanged(_combatService.CurrentPhase);
            }
        }



        private void OnDisable()
        {
            if (_combatService != null)
            {
                _combatService.OnPhaseChanged -= HandlePhaseChanged;
            }

            if (_statsDisplay != null)
            {
                _statsDisplay.UnsubscribeFromPlayer();
                _statsDisplay.UnsubscribeFromEnemies();
            }
        }

        private void OnDestroy()
        {
            if (_actionButtons != null)
            {
                _actionButtons.OnAbilityRequested -= HandleAbilityRequested;
            }
        }

        private void HandlePhaseChanged(CombatPhase newPhase)
        {
            if (actionButtonsPanel == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning(Santa.Core.Config.LogMessages.CombatUI.ActionButtonsNotAssigned, this);
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
            if (_statsDisplay != null && _combatService != null)
            {
                _statsDisplay.SubscribeToEnemies(_combatService.Enemies);
            }

            if (inSelection)
            {
                // Subscribe to player stats
                if (_combatService == null || _combatService.Player == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogVerbose(Santa.Core.Config.LogMessages.CombatUI.PlayerNotAvailable, this);
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
                // Combat ended; clean up subscriptions
                if (_statsDisplay != null)
                {
                    _statsDisplay.UnsubscribeFromPlayer();
                    _statsDisplay.UnsubscribeFromEnemies();
                }
            }
            else
            {
                if (_statsDisplay != null)
                {
                    _statsDisplay.UnsubscribeFromPlayer();
                }
            }
        }

        private void SubscribeToPlayerEvents()
        {
            if (_combatService == null || _statsDisplay == null) return;

            GameObject player = _combatService.Player;
            if (player == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose(Santa.Core.Config.LogMessages.CombatUI.PlayerNull, this);
#endif
                return;
            }

            var registry = player.GetComponent<IComponentRegistry>();
            if (registry == null)
            {
                GameLog.LogError(Santa.Core.Config.LogMessages.CombatUI.PlayerMissingRegistry, player);
                return;
            }

            _statsDisplay.SubscribeToPlayer(registry.HealthController, registry.ActionPointController);

            // Refresh button interactability after subscribing
            if (_actionButtons != null)
            {
                _actionButtons.RefreshButtonInteractability(_statsDisplay.CurrentPlayerAP);
            }
        }

        private void HandleAbilityRequested(Ability ability, GameObject primaryTarget)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"CombatUI: Ability requested - {(ability != null ? ability.AbilityName : "NULL")}");
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

            // If we are already waiting for a target, clicking a button should cancel targeting
            if (_pendingAbility != null)
            {
                CancelTargetingMode();
                return;
            }

            if (ability.Targeting.Style == TargetingStyle.SingleEnemy)
            {
                // Enter targeting mode
                _pendingAbility = ability;

                if (_actionButtons != null)
                {
                    _actionButtons.SetButtonsInteractable(false);
                }

                if (statusText != null)
                {
                    statusText.text = Santa.Core.Config.UIStrings.SelectTarget;
                }

                // Make the entire UI panel non-blocking for raycasts
                if (CanvasGroup != null) CanvasGroup.blocksRaycasts = false;

                // Inform the combat manager that we're entering targeting
                _combatService.SubmitPlayerAction(ability, null);
            }
            else
            {
                // For non-targeted abilities, submit immediately
                _combatService.SubmitPlayerAction(ability, null);
            }
        }

        /// <summary>
        /// Called by EnemyTarget when a target is selected during targeting mode.
        /// </summary>
        public void OnTargetSelected(GameObject target)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"CombatUI: Target selected - {(target != null ? target.name : "NULL")}");
#endif

            if (_pendingAbility == null) return;

            // Ensure the combat manager is in Targeting phase
            if (_combatService == null || _combatService.CurrentPhase != CombatPhase.Targeting)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError(Santa.Core.Config.LogMessages.CombatUI.TargetSubmitFailed);
#endif
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
        }

        private void CancelTargetingMode()
        {
            bool hadPendingAbility = _pendingAbility != null;
            _pendingAbility = null;

            if (_actionButtons != null)
            {
                _actionButtons.SetButtonsInteractable(true);
                _actionButtons.RefreshButtonInteractability(_statsDisplay != null ? _statsDisplay.CurrentPlayerAP : 0);
            }

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
    }
}