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
    private static TurnBasedCombatManager Instance { get; set; }
    public event Action OnPlayerTurnStarted;
    public event Action OnPlayerTurnEnded;

    private enum CombatState { Planning, Executing }
    private CombatState _currentState;

    private readonly List<PendingAction> _pendingActions = new List<PendingAction>();
    private readonly List<GameObject> _combatants = new List<GameObject>();
    private GameObject _player;
    private readonly List<PendingAction> _sortedActions = new List<PendingAction>();
    private readonly List<GameObject> _tempEnemies = new List<GameObject>(8);
    private readonly List<GameObject> _tempAllies = new List<GameObject>(8);
    private static readonly System.Random _rng = new System.Random();

    // Cache for health components to avoid repeated GetComponent calls
    private readonly Dictionary<GameObject, HealthComponentBehaviour> _healthComponents = new Dictionary<GameObject, HealthComponentBehaviour>();

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

        // Cache all health components at the start of combat
        _healthComponents.Clear();
        foreach (var combatant in _combatants)
        {
            if (combatant != null)
            {
                var health = combatant.GetComponent<HealthComponentBehaviour>();
                if (health != null)
                {
                    _healthComponents[combatant] = health;
                }
            }
        }

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

        foreach (var combatant in _combatants)
        {
            if(combatant.activeInHierarchy)
                combatant.GetComponent<ActionPointComponentBehaviour>()?.Refill();
        }

        OnPlayerTurnStarted?.Invoke();
    }

    public void SubmitPlayerAction(Ability ability, GameObject primaryTarget)
    {
        if (_currentState != CombatState.Planning) return;

        var playerAP = _player.GetComponent<ActionPointComponentBehaviour>();
        if (ability == null || !playerAP.ActionPoints.HasEnough(ability.ApCost))
        {
            Debug.LogWarning("Player tried to submit an invalid or unaffordable action.");
            OnPlayerTurnStarted?.Invoke();
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
    PendingAction? playerAction = null;
        for (int i = 0; i < _pendingActions.Count; i++)
        {
            if (_pendingActions[i].Caster != null && _pendingActions[i].Caster.CompareTag("Player"))
            {
                playerAction = _pendingActions[i];
                break;
            }
        }

        _tempEnemies.Clear();
        _tempAllies.Clear();
        for (int i = 0; i < _combatants.Count; i++)
        {
            var c = _combatants[i];
            if (c == null) continue;
            if (c.CompareTag("Enemy")) _tempEnemies.Add(c);
            else _tempAllies.Add(c);
        }

        for (int i = 0; i < _combatants.Count; i++)
        {
            var combatant = _combatants[i];
            if (combatant == null) continue;
            if (combatant != _player && combatant.activeInHierarchy)
            {
                var brain = combatant.GetComponent<IBrain>();
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

        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        Debug.Log("--- EXECUTION PHASE ---");
        _currentState = CombatState.Executing;

    _sortedActions.Clear();
    _sortedActions.AddRange(_pendingActions);
    _sortedActions.Sort((a, b) => b.Ability.ActionSpeed.CompareTo(a.Ability.ActionSpeed));

    foreach (var action in _sortedActions)
        {
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

            if (!_healthComponents.TryGetValue(action.Caster, out var casterHealth))
            {
                Debug.LogWarning($"{action.Caster.name} has no cached HealthComponentBehaviour; skipping action {action.Ability.AbilityName}.");
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

            yield return new WaitForSeconds(1.0f);
        }

        if (_currentState == CombatState.Executing)
        {
            Debug.Log("Execution phase finished.");
            StartNewTurn();
        }
    }

    private List<GameObject> ResolveTargets(PendingAction action)
    {
        List<GameObject> finalTargets = new List<GameObject>(4);

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
                    var temp = new List<GameObject>(potentialTargets.Count);
                    for (int i = 0; i < potentialTargets.Count; i++) if (potentialTargets[i] != action.PrimaryTarget) temp.Add(potentialTargets[i]);
                    potentialTargets = temp;
                }

                int totalToHit = Mathf.CeilToInt(_tempEnemies.Count * action.Ability.TargetPercentage);
                int additionalTargetsToHit = totalToHit - finalTargets.Count;

                if (additionalTargetsToHit > 0 && potentialTargets.Count > 0)
                {
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
        if (_healthComponents.TryGetValue(_player, out var playerHealth) && playerHealth.CurrentValue <= 0)
        {
            EndCombat(false);
            return true;
        }
        return false;
    }

    private void EndCombat(bool playerWon)
    {
        _currentState = CombatState.Planning;
        _healthComponents.Clear(); // Clear the cache
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