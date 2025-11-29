using System;
using UnityEngine;

/// <summary>
/// Handles player action submission, validation, targeting flow, and AP cost calculation.
/// Extracted from TurnBasedCombatManager to follow Single Responsibility Principle.
/// </summary>
public class PlayerActionHandler : IPlayerActionHandler
{
    public event Action<Ability> OnTargetingStarted;
    public event Action OnTargetingCancelled;
    public event Action<PendingAction> OnActionSubmitted;

    public bool IsInTargetingMode => _pendingAbility != null;

    private Ability _pendingAbility;

    public bool TrySubmitAction(
        Ability ability,
        GameObject primaryTarget,
        CombatPhase currentPhase,
        ICombatStateManager stateManager,
        IUpgradeService upgradeService)
    {
        if (ability == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("PlayerActionHandler: Attempted to submit null ability.");
#endif
            return false;
        }

        // ══════ Targeting Phase Flow ══════
        if (currentPhase == CombatPhase.Targeting)
        {
            return HandleTargetingPhase(primaryTarget, stateManager, upgradeService);
        }

        // ══════ Selection Phase Flow ══════
        if (currentPhase != CombatPhase.Selection)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"PlayerActionHandler: Cannot submit action in phase {currentPhase}");
#endif
            return false;
        }

        // Check if targeting is needed
        if (RequiresTargeting(ability) && primaryTarget == null)
        {
            EnterTargetingMode(ability);
            return true; // Targeting started successfully
        }

        // Process action immediately
        return ProcessActionWithTarget(ability, primaryTarget, stateManager, upgradeService);
    }

    private bool HandleTargetingPhase(
        GameObject target,
        ICombatStateManager stateManager,
        IUpgradeService upgradeService)
    {
        if (_pendingAbility == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("PlayerActionHandler: Received target submission but no ability was pending.");
#endif
            return false;
        }

        // Use the pending ability with the newly provided target
        bool success = ProcessActionWithTarget(_pendingAbility, target, stateManager, upgradeService);

        if (success)
        {
            _pendingAbility = null; // Clear pending on successful submission
        }

        return success;
    }

    private bool RequiresTargeting(Ability ability)
    {
        return ability.Targeting != null &&
               ability.Targeting.Style == TargetingStyle.SingleEnemy;
    }

    private void EnterTargetingMode(Ability ability)
    {
        _pendingAbility = ability;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.LogVerbose($"PlayerActionHandler: Entering targeting mode for '{ability.AbilityName}'");
#endif

        OnTargetingStarted?.Invoke(ability);
    }

    private bool ProcessActionWithTarget(
        Ability ability,
        GameObject target,
        ICombatStateManager stateManager,
        IUpgradeService upgradeService)
    {
        // ══════ Validate State ══════
        if (!stateManager.IsInitialized)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("PlayerActionHandler: Combat not initialized!");
#endif
            return false;
        }

        if (stateManager.State.Player == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("PlayerActionHandler: Player is null.");
#endif
            return false;
        }

        // ══════ Validate AP Component ══════
        if (!stateManager.State.APComponents.TryGetValue(
            stateManager.State.Player, out var playerAP))
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("PlayerActionHandler: Player has no ActionPointComponent.");
#endif
            return false;
        }

        // ══════ Calculate AP Cost ══════
        int baseCost = ability.ApCost;
        int costReduction = upgradeService?.GlobalAPCostReduction ?? 0;
        int actualCost = Mathf.Max(1, baseCost - costReduction); // Minimum cost is 1

        // ══════ Validate AP ══════
        if (playerAP.CurrentValue < actualCost)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"PlayerActionHandler: Cannot afford '{ability.AbilityName}'. Cost: {actualCost}, Has: {playerAP.CurrentValue}");
#endif
            return false;
        }

        // ══════ Validate Targeting ══════
        if (RequiresTargeting(ability) && target == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"PlayerActionHandler: '{ability.AbilityName}' requires a target.");
#endif
            return false;
        }

        // ══════ Deduct AP ══════
        playerAP.AffectValue(-actualCost);

        // ══════ Create Pending Action ══════
        var pendingAction = new PendingAction
        {
            Ability = ability,
            Caster = stateManager.State.Player,
            PrimaryTarget = target
        };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        string targetName = target != null ? target.name : "self/area";
        GameLog.LogVerbose($"PlayerActionHandler: Submitted '{ability.AbilityName}' targeting {targetName}. Cost: {actualCost} AP.");
#endif

        // ══════ Notify Submission ══════
        OnActionSubmitted?.Invoke(pendingAction);
        return true;
    }

    public void CancelTargeting()
    {
        if (_pendingAbility != null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose("PlayerActionHandler: Targeting cancelled.");
#endif

            _pendingAbility = null;
            OnTargetingCancelled?.Invoke();
        }
    }
}
