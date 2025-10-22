using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

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

    private readonly CombatState _combatState = new CombatState();
    private readonly IWinConditionChecker _winConditionChecker = new DefaultWinConditionChecker();
    private readonly List<EnemyTarget> _enemyTargets = new List<EnemyTarget>();

    // This list is for sorting and is ephemeral to the execution phase, so it stays here.
    private readonly List<PendingAction> _sortedActions = new List<PendingAction>();

    private Ability _abilityPendingTarget; // Stores the ability waiting for a target

    public IReadOnlyList<GameObject> AllCombatants => _combatState.AllCombatants;
    public IReadOnlyList<GameObject> Enemies => _combatState.Enemies;
    public GameObject Player => _combatState.Player;

    [Header("Configuration")]
    [SerializeField] private float _delayBetweenActions = 1.0f;

    private IActionExecutor _actionExecutor;
    private IAIManager _aiManager;

    public static bool CombatIsInitialized { get; private set; } = false;

    private void Awake()
    {
        _actionExecutor = GetComponentInChildren<IActionExecutor>();
        _aiManager = GetComponentInChildren<IAIManager>();

        if (_actionExecutor == null)
        {
            GameLog.LogError($"Could not find a component implementing IActionExecutor in children of {gameObject.name}.", this);
            enabled = false;
        }

        if (_aiManager == null)
        {
            GameLog.LogError($"Could not find a component implementing IAIManager in children of {gameObject.name}.", this);
            enabled = false;
        }

        if (enabled)
        {
            ServiceLocator.Register<ICombatService>(this);
        }
    }

    private void OnDestroy()
    {
        var registered = ServiceLocator.Get<ICombatService>();
        if ((UnityEngine.Object)registered == (UnityEngine.Object)this)
            ServiceLocator.Unregister<ICombatService>();
    }

    public void StartCombat(List<GameObject> participants)
    {
        GameLog.Log("--- COMBAT STARTED ---");
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
            GameLog.LogError("Combat cannot start without a player!");
            return;
        }

        gameObject.SetActive(true);
        StartNewTurn();
            // Log all received participants and their tags
            if (participants == null || participants.Count == 0)
            {
                GameLog.LogError("StartCombat called with null or empty participants list!");
            }
            else
            {
                GameLog.Log($"StartCombat received {participants.Count} participants:");
                for (int i = 0; i < participants.Count; i++)
                {
                    var obj = participants[i];
                    if (obj == null)
                    {
                        GameLog.LogWarning($"Participant {i}: NULL");
                    }
                    else
                    {
                        GameLog.Log($"Participant {i}: name={obj.name}, tag={obj.tag}");
                    }
                }
            }

            // Log result of player detection
            if (_combatState.Player == null)
            {
                GameLog.LogError("CombatState.Player is null after initialization! No participant with tag 'Player' was found.");
                GameLog.LogError("Combat cannot start without a player!");
                return;
            }
            else
            {
                GameLog.Log($"CombatState.Player assigned: name={_combatState.Player.name}, tag={_combatState.Player.tag}");
            }
    }

    private void StartNewTurn()
    {
        GameLog.Log("--- SELECTION PHASE ---: Starting new turn.");
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
                GameLog.LogError("Received a target submission but no ability was pending.");
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
        if (ability.Targeting.Style == TargetingStyle.SingleEnemy && primaryTarget == null)
        {
            _abilityPendingTarget = ability;
            CurrentPhase = CombatPhase.Targeting;
            SetEnemyTargetsActive(true); // Directly enable targets
            OnPhaseChanged?.Invoke(CurrentPhase);
            GameLog.Log($"Player selected ability '{ability.AbilityName}'. Waiting for target selection.");
            return;
        }

        // If targeting is not required, or was already provided, process the action immediately.
        ProcessActionSubmission(ability, primaryTarget);
    }

    private void ProcessActionSubmission(Ability ability, GameObject primaryTarget)
    {
        if (!CombatIsInitialized)
        {
            GameLog.LogError("ProcessActionSubmission called but combat is not initialized!");
            return;
        }

        if (ability == null)
        {
            GameLog.LogWarning("Player tried to submit a null ability.");
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        // Defensive: ensure we have a valid player reference before using it as a dictionary key.
        if (_combatState.Player == null)
        {
            GameLog.LogError("Cannot process action submission: CombatState.Player is null.");
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        if (!_combatState.APComponents.TryGetValue(_combatState.Player, out var playerAP))
        {
            GameLog.LogError("Player does not have an ActionPointComponent cached!");
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        if (playerAP.CurrentValue < ability.ApCost)
        {
            GameLog.LogWarning($"Player cannot afford action: {ability.AbilityName}. Cost: {ability.ApCost}, Has: {playerAP.CurrentValue}");
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        // Re-validate targeting here for the final submission
        if (ability.Targeting.Style == TargetingStyle.SingleEnemy && primaryTarget == null)
        {
            GameLog.LogWarning($"Cannot perform {ability.AbilityName}: No target specified for a single-target ability.");
            OnPlayerTurnStarted?.Invoke(); // Allow player to try again
            return;
        }

        playerAP.AffectValue(-ability.ApCost);
        _combatState.PendingActions.Add(new PendingAction { Ability = ability, Caster = _combatState.Player, PrimaryTarget = primaryTarget });
        GameLog.Log($"Player submitted action: {ability.AbilityName} targeting {primaryTarget?.name ?? "self/area"}.");

        SetEnemyTargetsActive(false); // Disable targets after selection

        // If the action was a full submission (not just entering targeting), end the player's turn.
        
        OnPlayerTurnEnded?.Invoke();
        FinalizeSelectionAndExecuteTurn();
    }

    private void FinalizeSelectionAndExecuteTurn()
    {
        PendingAction? playerAction = _combatState.PendingActions.FirstOrDefault(action => action.Caster == _combatState.Player);

        if (playerAction == null)
        {
            GameLog.LogWarning("Could not find player's action for AI planning context.");
        }

        _aiManager.PlanActions(
            _combatState.AllCombatants,
            _combatState.Player,
            _combatState.Brains,
            _combatState.APComponents,
            _combatState.PendingActions,
            playerAction);

        _ = ExecuteTurnAsync();
    }

    private async Task ExecuteTurnAsync()
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
        GameLog.Log("Execution phase finished.");
        StartNewTurn();
    }

    private void PrepareExecutionPhase()
    {
        GameLog.Log("--- EXECUTION PHASE ---");
        CurrentPhase = CombatPhase.Execution;
        OnPhaseChanged?.Invoke(CurrentPhase);

        _sortedActions.Clear();
        _sortedActions.AddRange(_combatState.PendingActions);
        _sortedActions.Sort((a, b) => b.Ability.ActionSpeed.CompareTo(a.Ability.ActionSpeed));
    }

    private async Task<CombatResult> ProcessActionAsync(PendingAction action)
    {
        _actionExecutor.Execute(action, _combatState.AllCombatants, _combatState.HealthComponents);
        
        // Wait for a moment to let players see the action
        await Task.Delay(TimeSpan.FromSeconds(_delayBetweenActions));

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
            GameLog.Log("--- COMBAT ENDED: VICTORY ---");
            ServiceLocator.Get<IUpgradeService>()?.PresentUpgradeOptions();
        }
        else
        {
            GameLog.Log("--- COMBAT ENDED: DEFEAT ---");
            var combatTransition = ServiceLocator.Get<ICombatTransitionService>();
            if (combatTransition != null) combatTransition.EndCombat();
        }
        gameObject.SetActive(false);
    }

    private void SetEnemyTargetsActive(bool isActive)
    {
        GameLog.Log($"Setting EnemyTarget colliders to: {isActive}");
        foreach (var target in _enemyTargets)
        {
            target.SetColliderActive(isActive);
        }
    }
}