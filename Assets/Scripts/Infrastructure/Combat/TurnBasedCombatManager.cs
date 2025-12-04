using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Santa.Core;
using Santa.Core.Config;
using Santa.Domain.Combat;
using Santa.Presentation.Combat;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Manages the turn-based combat flow, delegating state storage to a CombatState object.
    /// This class is responsible for the COMBAT FLOW (Planning -> Executing) and coordinating other services.
    /// </summary>
    public class TurnBasedCombatManager : MonoBehaviour, ICombatService
    {
        public event Action<CombatPhase> OnPhaseChanged;
        public event Action OnPlayerTurnStarted;
        public event Action OnPlayerTurnEnded;

        public CombatPhase CurrentPhase { get; private set; }

        private readonly CombatState _combatState = new();
        private readonly IWinConditionChecker _winConditionChecker = new DefaultWinConditionChecker();
    private readonly List<EnemyTarget> _enemyTargets = new();

    // This list is for sorting and is ephemeral to the execution phase, so it stays here.
    private readonly List<PendingAction> _sortedActions = new();

    private Ability _abilityPendingTarget; // Stores the ability waiting for a target

    public IReadOnlyList<GameObject> AllCombatants => _combatState.AllCombatants;
    public IReadOnlyList<GameObject> Enemies => _combatState.Enemies;
    public GameObject Player => _combatState.Player;

    [Header("Configuration")]
    [SerializeField] private float _delayBetweenActions = 1.0f;

    private IActionExecutor _actionExecutor;
    private IAIManager _aiManager;
    private IUpgradeService _upgradeService;
    private ICombatTransitionService _combatTransitionService;

    public static bool CombatIsInitialized { get; private set; } = false;

    [Inject]
    public void Construct(IUpgradeService upgradeService = null, ICombatTransitionService combatTransitionService = null)
    {
        _upgradeService = upgradeService;
        _combatTransitionService = combatTransitionService;
    }

    private void Awake()
    {
        _actionExecutor = GetComponentInChildren<IActionExecutor>();
        _aiManager = GetComponentInChildren<IAIManager>();

        if (_actionExecutor == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"Could not find a component implementing IActionExecutor in children of {gameObject.name}.", this);
#endif
            enabled = false;
        }

        if (_aiManager == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"Could not find a component implementing IAIManager in children of {gameObject.name}.", this);
#endif
            enabled = false;
        }
    }

    public void StartCombat(List<GameObject> participants)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("--- COMBAT STARTED ---");
#endif
        _combatState.Initialize(participants);
        CombatIsInitialized = true; // Set the flag

        // Cache all EnemyTarget components
        _enemyTargets.Clear();
        foreach (var enemy in _combatState.Enemies)
        {
            if (enemy.TryGetComponent<EnemyTarget>(out var target))
            {
                _enemyTargets.Add(target);
            }
        }

        if (_combatState.Player == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("Combat cannot start without a player!");
#endif
            return;
        }

        // Sync Player AP with UpgradeService
        if (_upgradeService != null)
        {
            if (_combatState.APComponents.TryGetValue(_combatState.Player, out var playerAP))
            {
                int maxAP = _upgradeService.MaxActionPoints;
                playerAP.SetMaxValue(maxAP);
                playerAP.SetValue(maxAP); // Set to max AP (base value)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"TurnBasedCombatManager: Synced Player AP to UpgradeService. MaxAP set to {maxAP}.");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("TurnBasedCombatManager: Player has no ActionPointComponent to sync upgrades to.");
#endif
            }
        }

        // Sync Player Health with UpgradeService
        if (_upgradeService != null)
        {
            if (_combatState.HealthComponents.TryGetValue(_combatState.Player, out var playerHealth))
            {
                int maxHealth = _upgradeService.MaxHealth;
                playerHealth.SetMaxValue(maxHealth);
                playerHealth.SetValue(maxHealth); // Heal to full at start of combat
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"TurnBasedCombatManager: Synced Player Health to UpgradeService. MaxHealth set to {maxHealth}.");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("TurnBasedCombatManager: Player has no HealthComponent to sync upgrades to.");
#endif
            }
        }

        gameObject.SetActive(true);
        StartNewTurn();
        // Log all received participants and their tags
        if (participants == null || participants.Count == 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("StartCombat called with null or empty participants list!");
#endif
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"StartCombat received {participants.Count} participants:");
#endif
            for (int i = 0; i < participants.Count; i++)
            {
                var obj = participants[i];
                if (obj == null)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogWarning($"Participant {i}: NULL");
#endif
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log($"Participant {i}: name={obj.name}, tag={obj.tag}");
#endif
                }
            }
        }

        // Log result of player detection
        if (_combatState.Player == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"CombatState.Player is null after initialization! No participant with tag '{GameConstants.Tags.Player}' was found.");
            GameLog.LogError("Combat cannot start without a player!");
#endif
            return;
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"CombatState.Player assigned: name={_combatState.Player.name}, tag={_combatState.Player.tag}");
#endif
        }
    }

    private void StartNewTurn()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("--- SELECTION PHASE ---: Starting new turn.");
#endif
        CurrentPhase = CombatPhase.Selection;
        OnPhaseChanged?.Invoke(CurrentPhase);
        _combatState.PendingActions.Clear();
        _abilityPendingTarget = null;

        OnPlayerTurnStarted?.Invoke();
    }

    public void SubmitPlayerAction(Ability ability, GameObject primaryTarget = null)
    {
        // If we are in the targeting phase, this call is providing the target.
        if (CurrentPhase == CombatPhase.Targeting)
        {
            if (_abilityPendingTarget == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogError("Received a target submission but no ability was pending.");
#endif
                StartNewTurn(); // Reset the turn state
                return;
            }

            // Use the pending ability with the newly provided target
            ProcessActionSubmission(_abilityPendingTarget, primaryTarget);
            _abilityPendingTarget = null;
            return;
        }

        // Standard check for being in the correct phase to initiate an action.
        if (CurrentPhase != CombatPhase.Selection) return;

        // If the ability requires a target but none was provided, switch to Targeting phase.
        if (ability.Targeting != null && ability.Targeting.Style == TargetingStyle.SingleEnemy && primaryTarget == null)
        {
            _abilityPendingTarget = ability;
            CurrentPhase = CombatPhase.Targeting;
            SetEnemyTargetsActive(true); // Directly enable targets
            OnPhaseChanged?.Invoke(CurrentPhase);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"Player selected ability '{ability.AbilityName}'. Waiting for target selection.");
#endif
            return;
        }

        // If targeting is not required, or was already provided, process the action immediately.
        ProcessActionSubmission(ability, primaryTarget);
    }

    public void CancelTargeting()
    {
        if (CurrentPhase != CombatPhase.Targeting)
        {
            return;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("Player cancelled targeting. Returning to selection phase.");
#endif


        _abilityPendingTarget = null;
        SetEnemyTargetsActive(false);

        CurrentPhase = CombatPhase.Selection;
        OnPhaseChanged?.Invoke(CurrentPhase);
        OnPlayerTurnStarted?.Invoke();
    }

    private void ProcessActionSubmission(Ability ability, GameObject primaryTarget)
    {
        if (!CombatIsInitialized)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("ProcessActionSubmission called but combat is not initialized!");
#endif
            return;
        }

        if (ability == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("Player tried to submit a null ability.");
#endif
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        // Defensive: ensure we have a valid player reference before using it as a dictionary key.
        if (_combatState.Player == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("Cannot process action submission: CombatState.Player is null.");
#endif
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        if (!_combatState.APComponents.TryGetValue(_combatState.Player, out var playerAP))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("Player does not have an ActionPointComponent cached!");
#endif
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        if (playerAP.CurrentValue < ability.ApCost)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"Player cannot afford action: {ability.AbilityName}. Cost: {ability.ApCost}, Has: {playerAP.CurrentValue}");
#endif
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        // Apply global AP cost reduction from upgrades (minimum cost is 1)
        int actualCost = Mathf.Max(1, ability.ApCost - (_upgradeService?.GlobalAPCostReduction ?? 0));

        // Re-validate targeting here for the final submission

        if (ability.Targeting != null && ability.Targeting.Style == TargetingStyle.SingleEnemy && primaryTarget == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"Cannot perform {ability.AbilityName}: No target specified for a single-target ability.");
#endif
            OnPlayerTurnStarted?.Invoke(); // Allow player to try again
            return;
        }

        playerAP.AffectValue(-actualCost);
        _combatState.PendingActions.Add(new PendingAction { Ability = ability, Caster = _combatState.Player, PrimaryTarget = primaryTarget });
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Player submitted action: {ability.AbilityName} targeting {(primaryTarget != null ? primaryTarget.name : "self/area")}. Cost: {actualCost} AP.");
#endif


        SetEnemyTargetsActive(false); // Disable targets after selection

        // If the action was a full submission (not just entering targeting), end the player's turn.

        OnPlayerTurnEnded?.Invoke();
        FinalizeSelectionAndExecuteTurn();
    }

    private void FinalizeSelectionAndExecuteTurn()
    {
        // Find player action without LINQ allocation
        PendingAction? playerAction = null;
        for (int i = 0; i < _combatState.PendingActions.Count; i++)
        {
            if (_combatState.PendingActions[i].Caster == _combatState.Player)
            {
                playerAction = _combatState.PendingActions[i];
                break;
            }
        }

        if (playerAction == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("Could not find player's action for AI planning context.");
#endif
        }

        _aiManager.PlanActions(
            _combatState.AllCombatants,
            _combatState.Player,
            _combatState.Brains,
            _combatState.APComponents,
            _combatState.PendingActions,
            playerAction);

        ExecuteTurnAsync().Forget();
    }

    private async UniTaskVoid ExecuteTurnAsync()
    {
        try
        {
            PrepareExecutionPhase();

            foreach (var action in _sortedActions)
            {
                CombatResult result = await ProcessActionAsync(action);
                if (result != CombatResult.Ongoing)
                {
                    HandleCombatEnd(result);
                    return; // End turn execution
                }
            }

            // If the loop completes, it means no one won or lost, so start a new turn.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("Execution phase finished.");
#endif
            StartNewTurn();
        }
        catch (System.OperationCanceledException)
        {
            // Expected during scene transitions
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("TurnBasedCombatManager: Turn execution cancelled.");
#endif
        }
        catch (System.Exception ex)
        {
            GameLog.LogError($"TurnBasedCombatManager.ExecuteTurnAsync: Exception during turn execution: {ex.Message}");
            GameLog.LogException(ex);
            // Try to end combat gracefully on error
            HandleCombatEnd(CombatResult.Defeat);
        }
    }

    private void PrepareExecutionPhase()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("--- EXECUTION PHASE ---");
#endif
        CurrentPhase = CombatPhase.Execution;
        OnPhaseChanged?.Invoke(CurrentPhase);

        _sortedActions.Clear();
        _sortedActions.AddRange(_combatState.PendingActions);

        // Apply global action speed bonus to player actions

        int speedBonus = _upgradeService?.GlobalActionSpeedBonus ?? 0;
        _sortedActions.Sort((a, b) =>
        {
            int speedA = a.Ability.ActionSpeed;
            int speedB = b.Ability.ActionSpeed;

            // Add speed bonus to player actions

            if (a.Caster == _combatState.Player) speedA += speedBonus;
            if (b.Caster == _combatState.Player) speedB += speedBonus;


            return speedB.CompareTo(speedA);
        });
    }

    private async UniTask<CombatResult> ProcessActionAsync(PendingAction action)
    {
        _actionExecutor.Execute(action, _combatState.AllCombatants, _combatState.HealthComponents, _upgradeService);

        // Wait for visual feedback using UniTask (allocation-free)
        await Santa.Core.Utils.AsyncUtils.Wait(_delayBetweenActions, this.GetCancellationTokenOnDestroy());

        return _winConditionChecker.Check(_combatState);
    }

    private void HandleCombatEnd(CombatResult result)
    {
        CurrentPhase = (result == CombatResult.Victory) ? CombatPhase.Victory : CombatPhase.Defeat;
        OnPhaseChanged?.Invoke(CurrentPhase);
        EndCombat(result == CombatResult.Victory);
    }

    private void EndCombat(bool playerWon)
    {
        _combatState.Clear();
        _enemyTargets.Clear();
        CombatIsInitialized = false; // Reset the flag

        if (playerWon)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("--- COMBAT ENDED: VICTORY ---");
#endif
            // Don't deactivate here - let UpgradeUI flow complete
            // TurnBasedCombatManager will be deactivated by CombatTransitionManager after EndCombat
            _upgradeService?.PresentUpgradeOptions();
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("--- COMBAT ENDED: DEFEAT ---");
#endif
            _combatTransitionService.EndCombat(false);
            // Only deactivate on defeat since no upgrade selection is needed
            gameObject.SetActive(false);
        }
    }

    private void SetEnemyTargetsActive(bool isActive)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"Setting EnemyTarget colliders to: {isActive}");
#endif
        foreach (var target in _enemyTargets)
        {
            target.SetColliderActive(isActive);
        }
    }

    private void OnDestroy()
    {
        OnPhaseChanged = null;
        OnPlayerTurnStarted = null;
        OnPlayerTurnEnded = null;
        CombatIsInitialized = false;
    }
    }
}