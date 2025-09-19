using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages the logic for turn-based combat, including turn order, abilities, and combat state.
/// </summary>
public class TurnBasedCombatManager : MonoBehaviour
{
    public static TurnBasedCombatManager Instance { get; private set; }

    [Header("Player AP Costs")]
    [SerializeField] private int _directAttackAPCost = 1;
    [SerializeField] private int _areaAttackAPCost = 2;
    [SerializeField] private int _specialAttackAPCost = 3;

    [Header("Enemy Settings")]
    [SerializeField] private float _enemyTurnDelay = 1.5f;

    private Queue<GameObject> _turnOrder = new Queue<GameObject>();
    private GameObject _currentCombatant;
    private bool _isPlayerTurn = false;

    // Combatant Data
    private GameObject _player;
    private List<GameObject> _enemies = new List<GameObject>();
    private EnergyComponentBehaviour _playerEnergy;
    private ActionPointComponentBehaviour _playerAP;
    private Dictionary<GameObject, ActionPointComponentBehaviour> _enemyAP = new Dictionary<GameObject, ActionPointComponentBehaviour>();

    public GameObject CurrentCombatant => _currentCombatant;
    public bool IsPlayerTurn => _isPlayerTurn;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartCombat(List<GameObject> participants)
    {
        if (participants == null || participants.Count < 2) return;

        _turnOrder.Clear();
        _enemies.Clear();
        _enemyAP.Clear();
        _player = null;

        foreach (var p in participants)
        {
            var apComponent = p.GetComponent<ActionPointComponentBehaviour>();
            if (apComponent == null)
            {
                Debug.LogError($"{p.name} is missing an ActionPointComponentBehaviour!", p);
                continue; // Skip participants without AP
            }

            if (p.CompareTag("Player"))
            {
                _player = p;
                _playerEnergy = _player.GetComponent<EnergyComponentBehaviour>();
                _playerAP = apComponent;
            }
            else
            {
                _enemies.Add(p);
                _enemyAP[p] = apComponent;
            }
        }

        if (_player == null || _enemies.Count == 0) return;
        if (_playerEnergy == null) Debug.LogError("Player is missing EnergyComponentBehaviour!");

        _turnOrder.Enqueue(_player);
        foreach (var enemy in _enemies) _turnOrder.Enqueue(enemy);

        Debug.Log($"Combat started with {participants.Count} participants!");
        NextTurn();
    }

    #region Player Abilities

    public void PlayerAttackDirect(GameObject target)
    {
        if (!IsPlayerTurn || target == null) return;
        if (!_playerAP.ActionPoints.HasEnough(_directAttackAPCost)) 
        {
            Debug.Log("Not enough AP for Direct Attack!");
            return;
        }

        _playerAP.ActionPoints.SpendActionPoints(_directAttackAPCost);
        Debug.Log($"Player uses Direct Attack on {target.name}!");
        ApplyDamageToEnemy(target, UpgradeManager.Instance.DirectAttackDamage);
        EndPlayerTurn();
    }

    public void PlayerAttackArea()
    {
        if (!IsPlayerTurn) return;
        if (!_playerAP.ActionPoints.HasEnough(_areaAttackAPCost))
        {
            Debug.Log("Not enough AP for Area Attack!");
            return;
        }

        _playerAP.ActionPoints.SpendActionPoints(_areaAttackAPCost);
        Debug.Log("Player uses Area Attack!");
        List<GameObject> activeEnemies = new List<GameObject>(_enemies.Where(e => e.activeInHierarchy));
        foreach (var enemy in activeEnemies) ApplyDamageToEnemy(enemy, UpgradeManager.Instance.AreaAttackDamage);
        EndPlayerTurn();
    }

    public void PlayerAttackSpecial(GameObject target)
    {
        if (!IsPlayerTurn || target == null) return;
        if (!_playerAP.ActionPoints.HasEnough(_specialAttackAPCost))
        {
            Debug.Log("Not enough AP for Special Attack!");
            return;
        }
        if (!_playerEnergy.Energy.IsFull())
        {
            Debug.Log("Not enough energy for Special Attack!");
            return;
        }

        _playerAP.ActionPoints.SpendActionPoints(_specialAttackAPCost);
        _playerEnergy.Energy.UseSpecialAttack();

        if (Random.value < UpgradeManager.Instance.SpecialAttackMissChance)
        {
            Debug.Log("Player's Special Attack MISSED!");
        }
        else
        {
            Debug.Log($"Player uses Special Attack on {target.name}!");
            ApplyDamageToEnemy(target, UpgradeManager.Instance.SpecialAttackDamage);
        }
        EndPlayerTurn();
    }

    private void EndPlayerTurn()
    {
        _isPlayerTurn = false;
        if (CheckForVictory()) EndCombat();
        else NextTurn();
    }

    #endregion

    private void ApplyDamageToEnemy(GameObject enemy, int damage)
    {
        var enemyHealth = enemy.GetComponent<HealthComponentBehaviour>();
        if (enemyHealth == null) return;

        enemyHealth.AffectValue(-damage);
        Debug.Log($"{enemy.name} health: {enemyHealth.CurrentValue}");

        if (enemyHealth.CurrentValue <= 0)
        {
            Debug.Log($"{enemy.name} has been defeated!");
            enemy.SetActive(false);
            _turnOrder = new Queue<GameObject>(_turnOrder.Where(c => c != enemy));
        }
    }

    private bool CheckForVictory() => _enemies.All(e => !e.activeInHierarchy);

    public void NextTurn()
    {
        if (_turnOrder.Count == 0 || CheckForVictory()) { EndCombat(); return; }

        _currentCombatant = _turnOrder.Dequeue();
        if (!_currentCombatant.activeInHierarchy) { NextTurn(); return; }

        _turnOrder.Enqueue(_currentCombatant);
        Debug.Log($"Turn started for: {_currentCombatant.name}");

        if (_currentCombatant.CompareTag("Player"))
        {
            _isPlayerTurn = true;
            _playerAP?.Refill();
            _playerEnergy?.AffectValue(UpgradeManager.Instance.EnergyGainedPerTurn);
            Debug.Log("Player's turn. Choose an ability.");
        }
        else
        {
            _isPlayerTurn = false;
            _enemyAP[_currentCombatant]?.Refill();
            StartCoroutine(EnemyTurn(_currentCombatant));
        }
    }

    private IEnumerator EnemyTurn(GameObject enemyGO)
    {
        Debug.Log($"{enemyGO.name}'s turn.");
        yield return new WaitForSeconds(_enemyTurnDelay);

        var playerHealth = _player.GetComponent<HealthComponentBehaviour>();
        var enemyData = enemyGO.GetComponent<Enemy>();
        var enemyAP = _enemyAP[enemyGO];

        if (enemyData != null && enemyAP != null)
        {
            bool performedAction = false;
            // 50/50 chance to try area attack first
            if (Random.value > 0.5f)
            {
                if (enemyAP.ActionPoints.HasEnough(enemyData.AreaAttackAPCost))
                {
                    Debug.Log($"{enemyData.EnemyName} uses its Area Attack!");
                    enemyAP.ActionPoints.SpendActionPoints(enemyData.AreaAttackAPCost);
                    playerHealth.AffectValue(-enemyData.AreaAttackDamage);
                    performedAction = true;
                }
            }
            
            // If no action was performed yet, try direct attack
            if (!performedAction && enemyAP.ActionPoints.HasEnough(enemyData.DirectAttackAPCost))
            {
                Debug.Log($"{enemyData.EnemyName} uses its Direct Attack!");
                enemyAP.ActionPoints.SpendActionPoints(enemyData.DirectAttackAPCost);
                playerHealth.AffectValue(-enemyData.DirectAttackDamage);
                performedAction = true;
            }

            if (!performedAction)
            {
                Debug.Log($"{enemyData.EnemyName} does not have enough AP to act.");
            }
        }

        Debug.Log($"Player health: {playerHealth.CurrentValue}");

        if (playerHealth.CurrentValue <= 0) EndCombat();
        else NextTurn();
    }

    private void EndCombat()
    {
        bool playerWon = CheckForVictory();
        if (playerWon)
        {
            Debug.Log("Victory! Presenting upgrade options.");
            UpgradeManager.Instance.PresentUpgradeOptions();
            return;
        }

        Debug.Log("Defeat! Combat has ended.");
        _isPlayerTurn = false;
        if (CombatTransitionManager.Instance != null) CombatTransitionManager.Instance.EndCombat();
    }
}
