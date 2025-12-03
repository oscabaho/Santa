using System;
using System.Collections.Generic;
using UnityEngine;
using Santa.Core;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Controls combat phases, manages turn flow, and handles enemy target activation.
    /// Delegates to TurnScheduler for phase timing and coordinates with CombatStateManager.
    /// </summary>
    public class CombatPhaseController : ICombatPhaseController
{
    public CombatPhase CurrentPhase => _turnScheduler.CurrentPhase;

    public event Action<CombatPhase> OnPhaseChanged
    {
        add => _turnScheduler.OnPhaseChanged += value;
        remove => _turnScheduler.OnPhaseChanged -= value;
    }

    public event Action OnPlayerTurnStarted
    {
        add => _turnScheduler.OnPlayerTurnStarted += value;
        remove => _turnScheduler.OnPlayerTurnStarted -= value;
    }

    public event Action OnPlayerTurnEnded
    {
        add => _turnScheduler.OnPlayerTurnEnded += value;
        remove => _turnScheduler.OnPlayerTurnEnded -= value;
    }

    private readonly TurnScheduler _turnScheduler;
    private readonly ICombatStateManager _stateManager;

    public CombatPhaseController(ICombatStateManager stateManager)
    {
        _turnScheduler = new TurnScheduler();
        _stateManager = stateManager;
    }

    public void StartTurn()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("CombatPhaseController: Starting new turn.");
#endif

        _turnScheduler.StartTurn();
    }

    public void EnterTargetingPhase()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("CombatPhaseController: Entering Targeting phase.");
#endif

        _turnScheduler.SetTargetingPhase();
        SetEnemyTargetsActive(true);
    }

    public void CancelTargeting()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("CombatPhaseController: Targeting cancelled, returning to Selection.");
#endif

        SetEnemyTargetsActive(false);
        _turnScheduler.CancelTargeting();
    }

    public void NotifyPlayerActionSubmitted()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose("CombatPhaseController: Player action submitted, turn ended.");
#endif

        SetEnemyTargetsActive(false);
        _turnScheduler.NotifyPlayerTurnEnded();
    }

    public void StartExecutionPhase(List<PendingAction> pendingActions)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"CombatPhaseController: Starting Execution phase with {pendingActions.Count} actions.");
#endif

        _turnScheduler.StartExecutionPhase(pendingActions);
    }

    public void EndCombat(bool playerWon)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log($"CombatPhaseController: Combat ended. Player won: {playerWon}");
#endif

        _turnScheduler.EndCombat(playerWon);
    }

    public void SetEnemyTargetsActive(bool active)
    {
        var targets = _stateManager.GetEnemyTargets();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"CombatPhaseController: Setting {targets.Count} enemy targets to {(active ? "active" : "inactive")}");
#endif

        foreach (var target in targets)
        {
            if (target != null)
            {
                target.SetColliderActive(active);
            }
        }
    }
}
}
