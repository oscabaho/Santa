using System;
using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for controlling combat phases, managing turn flow, and enemy target states.
    /// </summary>
    public interface ICombatPhaseController
{
    /// <summary>
    /// The current phase of combat.
    /// </summary>
    CombatPhase CurrentPhase { get; }

    /// <summary>
    /// Fired when the combat phase changes.
    /// </summary>
    event Action<CombatPhase> OnPhaseChanged;

    /// <summary>
    /// Fired when the player's turn starts (Selection phase begins).
    /// </summary>
    event Action OnPlayerTurnStarted;

    /// <summary>
    /// Fired when the player's turn ends (action submitted).
    /// </summary>
    event Action OnPlayerTurnEnded;

    /// <summary>
    /// Starts a new turn, entering the Selection phase.
    /// </summary>
    void StartTurn();

    /// <summary>
    /// Enters the Targeting phase for single-target ability selection.
    /// </summary>
    void EnterTargetingPhase();

    /// <summary>
    /// Cancels targeting and returns to Selection phase.
    /// </summary>
    void CancelTargeting();

    /// <summary>
    /// Notifies that the player has submitted an action and their turn ended.
    /// </summary>
    void NotifyPlayerActionSubmitted();

    /// <summary>
    /// Enters the Execution phase to begin processing actions.
    /// </summary>
    /// <param name="pendingActions">Actions queued for execution</param>
    void StartExecutionPhase(System.Collections.Generic.List<PendingAction> pendingActions);

    /// <summary>
    /// Ends combat with the given result.
    /// </summary>
    /// <param name="playerWon">True if player won, false if defeated</param>
    void EndCombat(bool playerWon);

    /// <summary>
    /// Enables or disables enemy target colliders for targeting.
    /// </summary>
    /// <param name="active">True to enable, false to disable</param>
    void SetEnemyTargetsActive(bool active);
    }
}
