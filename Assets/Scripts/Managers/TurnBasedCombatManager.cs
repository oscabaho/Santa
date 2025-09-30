using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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

    private WaitForSeconds _waitOneSecond;

    private readonly List<PendingAction> _pendingActions = new List<PendingAction>();
    private readonly List<GameObject> _combatants = new List<GameObject>();
    private GameObject _player;
    private readonly List<PendingAction> _sortedActions = new List<PendingAction>();
    private readonly List<GameObject> _tempEnemies = new List<GameObject>(8);
    private readonly List<GameObject> _tempAllies = new List<GameObject>(8);

    private ITargetResolver _targetResolver;

    // Caches for components to avoid repeated GetComponent calls
    private readonly Dictionary<GameObject, HealthComponentBehaviour> _healthComponents = new Dictionary<GameObject, HealthComponentBehaviour>();
    private readonly Dictionary<GameObject, ActionPointComponentBehaviour> _apComponents = new Dictionary<GameObject, ActionPointComponentBehaviour>();
    private readonly Dictionary<GameObject, IBrain> _brains = new Dictionary<GameObject, IBrain>();
    private readonly List<GameObject> _reusableTargetList = new List<GameObject>(8);

    public IReadOnlyList<GameObject> AllCombatants => _combatants;

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
        _waitOneSecond = new WaitForSeconds(1.0f);
        _targetResolver = new TargetResolver();
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

        _player = null;
        for (int i = 0; i < _combatants.Count; i++)
        {
            if (_combatants[i] != null && _combatants[i].CompareTag("Player"))
            {
                _player = _combatants[i];
                break;
            }
        }

        // Cache all components at the start of combat
        _healthComponents.Clear();
        _apComponents.Clear();
        _brains.Clear();
        foreach (var combatant in _combatants)
        {
            if (combatant != null)
            {
                var health = combatant.GetComponent<HealthComponentBehaviour>();
                if (health != null) _healthComponents[combatant] = health;

                var ap = combatant.GetComponent<ActionPointComponentBehaviour>();
                if (ap != null)
                {
                    _apComponents[combatant] = ap;
                    ap.SetValue(100); // Set initial AP to 100
                }

                var brain = combatant.GetComponent<IBrain>();
                if (brain != null) _brains[combatant] = brain;
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

        OnPlayerTurnStarted?.Invoke();
    }

    public void SubmitPlayerAction(Ability ability, GameObject primaryTarget)
    {
        if (_currentState != CombatState.Planning) return;

        if (!_apComponents.TryGetValue(_player, out var playerAP))
        {
            Debug.LogError("Player does not have an ActionPointComponentBehaviour cached!");
            OnPlayerTurnStarted?.Invoke();
            return;
        }

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
                _brains.TryGetValue(combatant, out var brain);
                _apComponents.TryGetValue(combatant, out var aiAP);

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

            _reusableTargetList.Clear();
            _targetResolver.ResolveTargets(action, _combatants, _reusableTargetList);
            
            try
            {
                action.Ability.Execute(_reusableTargetList, action.Caster);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while executing ability {action.Ability.AbilityName} from {action.Caster.name}: {ex}");
            }

            if (CheckForDefeat()) { yield break; }
            if (CheckForVictory()) { yield break; }

            yield return _waitOneSecond;
        }

        if (_currentState == CombatState.Executing)
        {
            Debug.Log("Execution phase finished.");
            StartNewTurn();
        }
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
