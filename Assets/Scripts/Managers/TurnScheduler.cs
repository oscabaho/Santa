using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Responsible for managing the flow of combat turns and phases.
/// </summary>
public class TurnScheduler
{
    public event Action<CombatPhase> OnPhaseChanged;
    public event Action OnPlayerTurnStarted;
    public event Action OnPlayerTurnEnded;

    public CombatPhase CurrentPhase { get; private set; }

    private readonly MonoBehaviour _coroutineRunner;
    private readonly float _delayBetweenActions;

    public TurnScheduler(MonoBehaviour coroutineRunner, float delayBetweenActions)
    {
        _coroutineRunner = coroutineRunner;
        _delayBetweenActions = delayBetweenActions;
    }

    public void StartTurn()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("--- SELECTION PHASE ---: Starting new turn.");
#endif
        CurrentPhase = CombatPhase.Selection;
        OnPhaseChanged?.Invoke(CurrentPhase);
        OnPlayerTurnStarted?.Invoke();
    }

    public void StartExecutionPhase(List<PendingAction> sortedActions)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("--- EXECUTION PHASE ---");
#endif
        CurrentPhase = CombatPhase.Execution;
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public void EndCombat(bool playerWon)
    {
        CurrentPhase = playerWon ? CombatPhase.Victory : CombatPhase.Defeat;
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public void SetTargetingPhase()
    {
        CurrentPhase = CombatPhase.Targeting;
        OnPhaseChanged?.Invoke(CurrentPhase);
    }

    public void CancelTargeting()
    {
        CurrentPhase = CombatPhase.Selection;
        OnPhaseChanged?.Invoke(CurrentPhase);
        OnPlayerTurnStarted?.Invoke();
    }

    public void NotifyPlayerTurnEnded()
    {
        OnPlayerTurnEnded?.Invoke();
    }
}
