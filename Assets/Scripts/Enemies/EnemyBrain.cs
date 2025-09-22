using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Enemy))]
[RequireComponent(typeof(ActionPointComponentBehaviour))]
public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private float turnDelay = 1.5f;

    private Enemy _enemyData;
    private ActionPointComponentBehaviour _actionPoints;
    private HealthComponentBehaviour _playerHealth;

    private void Awake()
    {
        _enemyData = GetComponent<Enemy>();
        _actionPoints = GetComponent<ActionPointComponentBehaviour>();
    }

    public void TakeTurn(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("EnemyBrain: Player GameObject is null.", this);
            EndTurn();
            return;
        }
        _playerHealth = player.GetComponent<HealthComponentBehaviour>();
        StartCoroutine(TakeTurnCoroutine());
    }

    private IEnumerator TakeTurnCoroutine()
    {
        Debug.Log($"-- {_enemyData.EnemyName}'s turn. --");
        yield return new WaitForSeconds(turnDelay);

        if (_playerHealth == null)
        {
            Debug.LogError("EnemyBrain: Player HealthComponent not found!", this);
            EndTurn();
            yield break;
        }

        bool performedAction = false;
        // 50/50 chance to try area attack first
        if (Random.value > 0.5f)
        {
            if (_actionPoints.ActionPoints.HasEnough(_enemyData.AreaAttackAPCost))
            {
                Debug.Log($"{_enemyData.EnemyName} uses its Area Attack!");
                _actionPoints.ActionPoints.SpendActionPoints(_enemyData.AreaAttackAPCost);
                _playerHealth.AffectValue(-_enemyData.AreaAttackDamage);
                performedAction = true;
            }
        }

        // If no action was performed yet, try direct attack
        if (!performedAction && _actionPoints.ActionPoints.HasEnough(_enemyData.DirectAttackAPCost))
        {
            Debug.Log($"{_enemyData.EnemyName} uses its Direct Attack!");
            _actionPoints.ActionPoints.SpendActionPoints(_enemyData.DirectAttackAPCost);
            _playerHealth.AffectValue(-_enemyData.DirectAttackDamage);
            performedAction = true;
        }

        if (!performedAction)
        {
            Debug.Log($"{_enemyData.EnemyName} does not have enough AP to act.");
        }

        Debug.Log($"Player health is now: {_playerHealth.CurrentValue}");

        EndTurn();
    }

    private void EndTurn()
    {
        // Notify the TurnBasedCombatManager that the turn is over.
        TurnBasedCombatManager.Instance.EndEnemyTurn();
    }
}
