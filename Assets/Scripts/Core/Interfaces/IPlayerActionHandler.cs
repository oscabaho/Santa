using System;
using UnityEngine;

/// <summary>
/// Interface for handling player actions in combat including validation, targeting, and submission.
/// </summary>
public interface IPlayerActionHandler
{
    /// <summary>
    /// Fired when the player enters targeting mode for single-target abilities.
    /// </summary>
    event Action<Ability> OnTargetingStarted;

    /// <summary>
    /// Fired when targeting is cancelled.
    /// </summary>
    event Action OnTargetingCancelled;

    /// <summary>
    /// Fired when a player action is successfully validated and submitted.
    /// </summary>
    event Action<PendingAction> OnActionSubmitted;

    /// <summary>
    /// Whether the player is currently in targeting mode.
    /// </summary>
    bool IsInTargetingMode { get; }

    /// <summary>
    /// Attempts to submit a player action, handling validation, AP costs, and targeting flow.
    /// </summary>
    /// <param name="ability">The ability to use</param>
    /// <param name="primaryTarget">The target (null if self/area or entering targeting)</param>
    /// <param name="currentPhase">Current combat phase</param>
    /// <param name="stateManager">Combat state manager for accessing player and components</param>
    /// <param name="upgradeService">Optional upgrade service for AP cost reduction</param>
    /// <returns>True if action was submitted or targeting started, false if validation failed</returns>
    bool TrySubmitAction(
        Ability ability,
        GameObject primaryTarget,
        CombatPhase currentPhase,
        ICombatStateManager stateManager,
        IUpgradeService upgradeService);

    /// <summary>
    /// Cancels the current targeting mode if active.
    /// </summary>
    void CancelTargeting();
}
