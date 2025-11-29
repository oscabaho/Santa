using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

/// <summary>
/// Coordinates turn-based combat flow, delegating responsibilities to specialized components.
/// Refactored from monolithic 478-line class to ~180-line coordinator.
/// </summary>
public class TurnBasedCombatManager : MonoBehaviour, ICombatService
{
    // ══════════════════════════════════════════════════════════
    // SPECIALIZED COMPONENTS (New Architecture)
    // ══════════════════════════════════════════════════════════


    private ICombatStateManager _stateManager;
    private IPlayerActionHandler _actionHandler;
    private ICombatPhaseController _phaseController;

    // ══════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ══════════════════════════════════════════════════════════


    private IActionExecutor _actionExecutor;
    private IAIManager _aiManager;
    private IUpgradeService _upgradeService;
    private ICombatTransitionService _combatTransitionService;
    private IWinConditionChecker _winConditionChecker;

    // ══════════════════════════════════════════════════════════
    // EXECUTION STATE
    // ══════════════════════════════════════════════════════════


    private readonly List<PendingAction> _sortedActions = new List<PendingAction>(16);

    [Header("Configuration")]
    [SerializeField] private float _delayBetweenActions = 1.0f;

    public static bool CombatIsInitialized { get; private set; } = false;

    // ══════════════════════════════════════════════════════════
    // ICombatService IMPLEMENTATION
    // ══════════════════════════════════════════════════════════


    public CombatPhase CurrentPhase => _phaseController.CurrentPhase;
    public GameObject Player => _stateManager.State.Player;
    public IReadOnlyList<GameObject> AllCombatants => _stateManager.State.AllCombatants;
    public IReadOnlyList<GameObject> Enemies => _stateManager.State.Enemies;

    public event Action<CombatPhase> OnPhaseChanged
    {
        add => _phaseController.OnPhaseChanged += value;
        remove => _phaseController.OnPhaseChanged -= value;
    }

    public event Action OnPlayerTurnStarted
    {
        add => _phaseController.OnPlayerTurnStarted += value;
        remove => _phaseController.OnPlayerTurnStarted -= value;
    }

    public event Action OnPlayerTurnEnded
    {
        add => _phaseController.OnPlayerTurnEnded += value;
        remove => _phaseController.OnPlayerTurnEnded -= value;
    }

    // ══════════════════════════════════════════════════════════
    // INITIALIZATION
    // ══════════════════════════════════════════════════════════

    [Inject]
    public void Construct(IUpgradeService upgradeService = null, ICombatTransitionService combatTransitionService = null)
    {
        _upgradeService = upgradeService;
        _combatTransitionService = combatTransitionService;
    }

    private void Awake()
    {
        // Create specialized components
        _stateManager = new CombatStateManager();
        _actionHandler = new PlayerActionHandler();
        _phaseController = new CombatPhaseController(this, _delayBetweenActions, _stateManager);
        _winConditionChecker = new DefaultWinConditionChecker();

        // Wire component events
        _actionHandler.OnTargetingStarted += HandleTargetingStarted;
        _actionHandler.OnActionSubmitted += HandlePlayerActionSubmitted;

        // Get child components
        _actionExecutor = GetComponentInChildren<IActionExecutor>();
        _aiManager = GetComponentInChildren<IAIManager>();

        // Validate dependencies
        if (_actionExecutor == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"Could not find IActionExecutor in children of {gameObject.name}.", this);
#endif
            enabled = false;
        }

        if (_aiManager == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError($"Could not find IAIManager in children of {gameObject.name}.", this);
#endif
            enabled = false;
        }
    }

    // ══════════════════════════════════════════════════════════
    // PUBLIC API (ICombatService)
    // ══════════════════════════════════════════════════════════

    public void StartCombat(List<GameObject> participants)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("═══ COMBAT STARTED ═══");
#endif

        _stateManager.Initialize(participants, _upgradeService);
        CombatIsInitialized = true;

        gameObject.SetActive(true);
        _phaseController.StartTurn();
    }

    public void SubmitPlayerAction(Ability ability, GameObject primaryTarget = null)
    {
        // Delegate to PlayerActionHandler
        _actionHandler.TrySubmitAction(
            ability,
            primaryTarget,
            CurrentPhase,
            _stateManager,
            _upgradeService);
    }

    public void CancelTargeting()
    {
        _actionHandler.CancelTargeting();
        _phaseController.CancelTargeting();
    }

    // ══════════════════════════════════════════════════════════
    // EVENT HANDLERS
    // ══════════════════════════════════════════════════════════

    private void HandleTargetingStarted(Ability ability)
    {
        _phaseController.EnterTargetingPhase();
    }

    private void HandlePlayerActionSubmitted(PendingAction playerAction)
    {
        _stateManager.State.PendingActions.Add(playerAction);
        _phaseController.NotifyPlayerActionSubmitted();

        FinalizeSelectionAndExecuteTurn();
    }

    // ══════════════════════════════════════════════════════════
    // TURN EXECUTION
    // ══════════════════════════════════════════════════════════

    private void FinalizeSelectionAndExecuteTurn()
    {
        // Find player action for AI context
        PendingAction? playerAction = null;
        for (int i = 0; i < _stateManager.State.PendingActions.Count; i++)
        {
            if (_stateManager.State.PendingActions[i].Caster == Player)
            {
                playerAction = _stateManager.State.PendingActions[i];
                break;
            }
        }

        // Let AI plan actions with player context
        _aiManager.PlanActions(
            _stateManager.State.AllCombatants,
            _stateManager.State.Player,
            _stateManager.State.Brains,
            _stateManager.State.APComponents,
            _stateManager.State.PendingActions,
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
                return;
            }
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("Execution phase finished.");
#endif
        _phaseController.StartTurn();
    }

    private void PrepareExecutionPhase()
    {
        _phaseController.StartExecutionPhase(_stateManager.State.PendingActions);

        _sortedActions.Clear();
        _sortedActions.AddRange(_stateManager.State.PendingActions);
        _sortedActions.Sort(SortActionsBySpeed);
    }

    private int SortActionsBySpeed(PendingAction a, PendingAction b)
    {
        int speedBonus = _upgradeService?.GlobalActionSpeedBonus ?? 0;
        int speedA = a.Ability.ActionSpeed;
        int speedB = b.Ability.ActionSpeed;

        // Add speed bonus to player actions
        if (a.Caster == Player) speedA += speedBonus;
        if (b.Caster == Player) speedB += speedBonus;

        return speedB.CompareTo(speedA); // Higher speed goes first
    }

    private async Task<CombatResult> ProcessActionAsync(PendingAction action)
    {
        _actionExecutor.Execute(
            action,
            _stateManager.State.AllCombatants,
            _stateManager.State.HealthComponents,
            _upgradeService);

        // Wait for visual feedback
        float endTime = Time.time + _delayBetweenActions;
        while (Time.time < endTime)
        {
            if (!Application.isPlaying) return CombatResult.Ongoing;
            await Task.Yield();
        }

        return _winConditionChecker.Check(_stateManager.State);
    }

    private void HandleCombatEnd(CombatResult result)
    {
        bool playerWon = result == CombatResult.Victory;
        _phaseController.EndCombat(playerWon);
        EndCombat(playerWon);
    }

    private void EndCombat(bool playerWon)
    {
        _stateManager.Clear();
        CombatIsInitialized = false;

        if (playerWon)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("═══ COMBAT ENDED: VICTORY ═══");
#endif
            _upgradeService?.PresentUpgradeOptions();
        }
        else
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log("═══ COMBAT ENDED: DEFEAT ═══");
#endif
            _combatTransitionService?.EndCombat(false);
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Cleanup event subscriptions
        if (_actionHandler != null)
        {
            _actionHandler.OnTargetingStarted -= HandleTargetingStarted;
            _actionHandler.OnActionSubmitted -= HandlePlayerActionSubmitted;
        }

        CombatIsInitialized = false;
    }
}