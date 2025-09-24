using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Manages the turn-based combat flow following a Planning/Execution model.
/// 1. Planning Phase: All combatants choose their actions.
/// 2. Execution Phase: All chosen actions are sorted by speed and executed in order.
/// </summary>
public class TurnBasedCombatManager : MonoBehaviour, ICombatService
{
    // Reduce visibility to internal to discourage external concrete usage.
    internal static TurnBasedCombatManager Instance { get; private set; }
    public event Action OnPlayerTurnStarted; // Fired when the player needs to choose an action
    public event Action OnPlayerTurnEnded;   // Fired when the player has chosen their action

    // Internal State
    private enum CombatState { Planning, Executing }
    private CombatState _currentState;

    // Data Structures
    private readonly List<PendingAction> _pendingActions = new List<PendingAction>();
    private readonly List<GameObject> _combatants = new List<GameObject>();
    private GameObject _player;
    // Reusable lists to avoid allocations in hot paths
    private readonly List<PendingAction> _sortedActions = new List<PendingAction>();
    private readonly List<GameObject> _tempEnemies = new List<GameObject>(8);
    private readonly List<GameObject> _tempAllies = new List<GameObject>(8);
    private static readonly System.Random _rng = new System.Random();

    // Public Properties
    // Public Properties
    // Returns a reusable list (IReadOnlyList) of enemies to avoid allocations. Callers should not modify the list.
    public IReadOnlyList<GameObject> Enemies
    {
        get
        {
            _tempEnemies.Clear();
            for (int i = 0; i < _combatants.Count; i++)
            {
                var c = _combatants[i];
                if (c != null && c.CompareTag("Enemy")) _tempEnemies.Add(c);
            }
            return _tempEnemies;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
        Debug.Log("--- COMBAT STARTED ---");
        _combatants.Clear();
        _combatants.AddRange(participants);
        _player = _combatants.FirstOrDefault(c => c.CompareTag("Player"));

        if (_player == null)
        {
            Debug.LogError("Combat cannot start without a player!");
            return;
        }

        gameObject.SetActive(true);
        StartNewTurn();
    }

    private void StartNewTurn()
    {
        Debug.Log("--- PLANNING PHASE ---: Starting new turn.");
        _currentState = CombatState.Planning;
        _pendingActions.Clear();

        // Refill AP for all combatants, etc.
        foreach (var combatant in _combatants)
        {
            if(combatant.activeInHierarchy)
                combatant.GetComponent<ActionPointComponentBehaviour>()?.Refill();
        }

        // Signal UI for player to choose their action
        OnPlayerTurnStarted?.Invoke();
    }

    /// <summary>
    /// Called by the Player (via UI) to submit their chosen action for the turn.
    /// </summary>
    public void SubmitPlayerAction(Ability ability, GameObject primaryTarget)
    {
        if (_currentState != CombatState.Planning) return;

        var playerAP = _player.GetComponent<ActionPointComponentBehaviour>();
        if (ability == null || !playerAP.ActionPoints.HasEnough(ability.ApCost))
        {
            Debug.LogWarning("Player tried to submit an invalid or unaffordable action.");
            OnPlayerTurnStarted?.Invoke(); // Re-enable player turn UI
            return;
        }

        playerAP.ActionPoints.SpendActionPoints(ability.ApCost);
        _pendingActions.Add(new PendingAction { Ability = ability, Caster = _player, PrimaryTarget = primaryTarget });
        Debug.Log($"Player submitted action: {ability.AbilityName}");

        OnPlayerTurnEnded?.Invoke();
        TriggerAIPlanning();
    }

    private void TriggerAIPlanning()
    {
        // Prepare context for the AIs
    PendingAction? playerAction = null;
        for (int i = 0; i < _pendingActions.Count; i++)
        {
            if (_pendingActions[i].Caster != null && _pendingActions[i].Caster.CompareTag("Player"))
            {
                playerAction = _pendingActions[i];
                break;
            }
        }

        // Build reusable lists for enemies and allies
        _tempEnemies.Clear();
        _tempAllies.Clear();
        for (int i = 0; i < _combatants.Count; i++)
        {
            var c = _combatants[i];
            if (c == null) continue;
            if (c.CompareTag("Enemy")) _tempEnemies.Add(c);
            else _tempAllies.Add(c);
        }

        // Have all AI combatants choose and submit their actions
        for (int i = 0; i < _combatants.Count; i++)
        {
            var combatant = _combatants[i];
            if (combatant == null) continue;
            if (combatant != _player && combatant.activeInHierarchy)
            {
                var brain = combatant.GetComponent<IBrain>(); // Get the brain via interface
                var aiAP = combatant.GetComponent<ActionPointComponentBehaviour>();

                if (brain != null && aiAP != null)
                {
                    PendingAction aiAction = brain.ChooseAction(playerAction, _tempEnemies, _tempAllies);

                    if (aiAction.Ability != null && aiAP.ActionPoints.HasEnough(aiAction.Ability.ApCost))
                    {
                        aiAP.ActionPoints.SpendActionPoints(aiAction.Ability.ApCost);
                        _pendingActions.Add(aiAction);
                        Debug.Log($"{combatant.name} submitted action: {aiAction.Ability.AbilityName}");
                    }
                }
            }
        }

        // All actions are now submitted, begin execution phase
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        Debug.Log("--- EXECUTION PHASE ---");
        _currentState = CombatState.Executing;

    // Sort actions by speed (high to low) using a reusable list to avoid allocations
    _sortedActions.Clear();
    _sortedActions.AddRange(_pendingActions);
    _sortedActions.Sort((a, b) => b.Ability.ActionSpeed.CompareTo(a.Ability.ActionSpeed));

    foreach (var action in _sortedActions)
        {
            // Defensive: skip any malformed pending actions
            if (action.Caster == null)
            {
                Debug.LogWarning("Skipping action: caster is null.");
                continue;
            }

            if (action.Ability == null)
            {
                Debug.LogWarning($"Skipping action from {action.Caster.name}: Ability is null.");
                continue;
            }

            // Check if caster is still alive before performing the action
            var casterHealth = action.Caster.GetComponent<HealthComponentBehaviour>();
            if (casterHealth == null)
            {
                Debug.LogWarning($"{action.Caster.name} has no HealthComponentBehaviour; skipping action {action.Ability.AbilityName}.");
                continue;
            }

            if (casterHealth.CurrentValue <= 0)
            {
                Debug.Log($"{action.Caster.name} is defeated and cannot perform {action.Ability.AbilityName}.");
                continue;
            }

            List<GameObject> finalTargets = ResolveTargets(action);
            try
            {
                action.Ability.Execute(finalTargets, action.Caster);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while executing ability {action.Ability.AbilityName} from {action.Caster.name}: {ex}");
            }

            if (CheckForDefeat()) { yield break; }
            if (CheckForVictory()) { yield break; }

            yield return new WaitForSeconds(1.0f); // Wait time between actions
        }

        if (_currentState == CombatState.Executing) // Check if combat didn't already end
        {
            Debug.Log("Execution phase finished.");
            StartNewTurn();
        }
    }

    private List<GameObject> ResolveTargets(PendingAction action)
    {
        List<GameObject> finalTargets = new List<GameObject>(4);

        // Build filtered lists without LINQ
        _tempEnemies.Clear();
        _tempAllies.Clear();
        for (int i = 0; i < _combatants.Count; i++)
        {
            var c = _combatants[i];
            if (c == null || !c.activeInHierarchy) continue;
            if (c.CompareTag("Enemy")) _tempEnemies.Add(c);
            else _tempAllies.Add(c);
        }

        List<GameObject> potentialTargets = (action.Caster != null && (action.Caster.CompareTag("Player") || action.Caster.CompareTag("Ally"))) ? _tempEnemies : _tempAllies;

        switch (action.Ability.Targeting)
        {
            case TargetingStyle.SingleEnemy:
                if (action.PrimaryTarget != null && action.PrimaryTarget.activeInHierarchy)
                    finalTargets.Add(action.PrimaryTarget);
                break;

            case TargetingStyle.AllEnemies:
                finalTargets.AddRange(potentialTargets);
                break;

            case TargetingStyle.RandomEnemies:
                if (action.PrimaryTarget != null && action.PrimaryTarget.activeInHierarchy)
                {
                    finalTargets.Add(action.PrimaryTarget);
                    // remove primary from potentialTargets by creating a temp list
                    var temp = new List<GameObject>(potentialTargets.Count);
                    for (int i = 0; i < potentialTargets.Count; i++) if (potentialTargets[i] != action.PrimaryTarget) temp.Add(potentialTargets[i]);
                    potentialTargets = temp;
                }

                int totalToHit = Mathf.CeilToInt(_tempEnemies.Count * action.Ability.TargetPercentage);
                int additionalTargetsToHit = totalToHit - finalTargets.Count;

                if (additionalTargetsToHit > 0 && potentialTargets.Count > 0)
                {
                    // Shuffle potentialTargets in-place using Fisher-Yates with static RNG
                    for (int i = potentialTargets.Count - 1; i > 0; i--)
                    {
                        int j = _rng.Next(i + 1);
                        var tmp = potentialTargets[i];
                        potentialTargets[i] = potentialTargets[j];
                        potentialTargets[j] = tmp;
                    }
                    for (int k = 0; k < additionalTargetsToHit && k < potentialTargets.Count; k++)
                        finalTargets.Add(potentialTargets[k]);
                }
                break;
        }
        return finalTargets;
    }

    private bool CheckForVictory()
    {
        // Check manually to avoid LINQ allocations
        for (int i = 0; i < _combatants.Count; i++)
        {
            var c = _combatants[i];
            if (c != null && c.CompareTag("Enemy") && c.activeInHierarchy) return false;
        }
        EndCombat(true);
        return true;
    }

    private bool CheckForDefeat()
    {
        var playerHealth = _player.GetComponent<HealthComponentBehaviour>();
        if (playerHealth != null && playerHealth.CurrentValue <= 0)
        {
            EndCombat(false);
            return true;
        }
        return false;
    }

    private void EndCombat(bool playerWon)
    {
        _currentState = CombatState.Planning; // Reset state
        if (playerWon)
        {
            Debug.Log("--- COMBAT ENDED: VICTORY ---");
            ServiceLocator.Get<IUpgradeService>()?.PresentUpgradeOptions();
        }
        else
        {
            Debug.Log("--- COMBAT ENDED: DEFEAT ---");
            var combatTransition = ServiceLocator.Get<ICombatTransitionService>();
            if (combatTransition != null) combatTransition.EndCombat();
        }
        gameObject.SetActive(false);
    }
}

