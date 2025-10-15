using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

/// <summary>
/// Manages the turn-based combat flow, delegating state storage to a CombatState object.
/// This class is responsible for the COMBAT FLOW (Planning -> Executing) and coordinating other services.
/// </summary>
public class TurnBasedCombatManager : MonoBehaviour, ICombatService
{
    private static TurnBasedCombatManager Instance { get; set; }
    public event Action OnPlayerTurnStarted;
    public event Action OnPlayerTurnEnded;

    private enum CombatPhase { Planning, Executing }
    private CombatPhase _currentPhase;

    private readonly CombatState _combatState = new CombatState();
    private readonly IWinConditionChecker _winConditionChecker = new DefaultWinConditionChecker();

    // This list is for sorting and is ephemeral to the execution phase, so it stays here.
    private readonly List<PendingAction> _sortedActions = new List<PendingAction>();

    public IReadOnlyList<GameObject> AllCombatants => _combatState.AllCombatants;
    public IReadOnlyList<GameObject> Enemies => _combatState.Enemies;

    [Header("Dependencies")]
    [Tooltip("Assign the ActionExecutor component here.")]
    [SerializeField] private ActionExecutor _actionExecutor;
    [Tooltip("Assign the AIManager component here.")]
    [SerializeField] private AIManager _aiManager;

    [Header("Configuration")]
    [SerializeField] private float _delayBetweenActions = 1.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_actionExecutor == null || _aiManager == null)
        {
            GameLog.LogError("Dependencies not assigned in TurnBasedCombatManager! Please assign them in the Inspector.", this);
            enabled = false;
            return;
        }

        ServiceLocator.Register<ICombatService>(this);
    }

    private void OnDestroy()
    {
        var registered = ServiceLocator.Get<ICombatService>();
        if ((UnityEngine.Object)registered == (UnityEngine.Object)this)
            ServiceLocator.Unregister<ICombatService>();
        if (Instance == this) Instance = null;
    }

    public void StartCombat(List<GameObject> participants)
    {
        GameLog.Log("--- COMBAT STARTED ---");
        _combatState.Initialize(participants);

        if (_combatState.Player == null)
        {
            GameLog.LogError("Combat cannot start without a player!");
            return;
        }

        gameObject.SetActive(true);
        StartNewTurn();
    }

    private void StartNewTurn()
    {
        GameLog.Log("--- PLANNING PHASE ---: Starting new turn.");
        _currentPhase = CombatPhase.Planning;
        _combatState.PendingActions.Clear();

        OnPlayerTurnStarted?.Invoke();
    }

    public void SubmitPlayerAction(Ability ability, GameObject primaryTarget)
    {
        if (_currentPhase != CombatPhase.Planning) return;

        if (!_combatState.APComponents.TryGetValue(_combatState.Player, out var playerAP))
        {
            GameLog.LogError("Player does not have an ActionPointComponent cached!");
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        if (ability == null || playerAP.CurrentValue < ability.ApCost)
        {
            GameLog.LogWarning("Player tried to submit an invalid or unaffordable action.");
            OnPlayerTurnStarted?.Invoke();
            return;
        }

        playerAP.AffectValue(-ability.ApCost);
        _combatState.PendingActions.Add(new PendingAction { Ability = ability, Caster = _combatState.Player, PrimaryTarget = primaryTarget });
        GameLog.Log($"Player submitted action: {ability.AbilityName}");

        OnPlayerTurnEnded?.Invoke();
        TriggerAIPlanning();
    }

    private void TriggerAIPlanning()
    {
        PendingAction? playerAction = null;
        foreach(var action in _combatState.PendingActions)
        {
            if (action.Caster != null && action.Caster.CompareTag("Player"))
            {
                playerAction = action;
                break;
            }
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
        GameLog.Log("--- EXECUTION PHASE ---");
        _currentPhase = CombatPhase.Executing;

        _sortedActions.Clear();
        _sortedActions.AddRange(_combatState.PendingActions);
        _sortedActions.Sort((a, b) => b.Ability.ActionSpeed.CompareTo(a.Ability.ActionSpeed));

        foreach (var action in _sortedActions)
        {
            _actionExecutor.Execute(action, _combatState.AllCombatants, _combatState.HealthComponents);

            CombatResult result = _winConditionChecker.Check(_combatState);
            if (result != CombatResult.Ongoing)
            {
                EndCombat(result == CombatResult.Victory);
                return; // End the execution
            }

            await Task.Delay(TimeSpan.FromSeconds(_delayBetweenActions)); // TODO: Make this delay configurable or animation-driven.
        }

        if (_currentPhase == CombatPhase.Executing)
        {
            GameLog.Log("Execution phase finished.");
            StartNewTurn();
        }
    }

    private void EndCombat(bool playerWon)
    {
        _currentPhase = CombatPhase.Planning; // Reset for next combat
        _combatState.Clear();

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
}