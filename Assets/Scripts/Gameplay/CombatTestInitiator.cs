
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// This script is for testing purposes only.
/// It finds the player and enemies in the scene at startup and initiates combat.
/// </summary>
public class CombatTestInitiator : MonoBehaviour
{
    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private string _enemyTag = "Enemy";

    void Start()
    {
        // Find the combat service
        if (!ServiceLocator.TryGet(out ICombatService combatService))
        {
            GameLog.LogError("CombatTestInitiator could not find ICombatService. Make sure TurnBasedCombatManager is in the scene.");
            return;
        }

        // Find all combatants in the scene by tag
        GameObject player = GameObject.FindGameObjectWithTag(_playerTag);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(_enemyTag);

        if (player == null)
        {
            GameLog.LogError($"CombatTestInitiator could not find a GameObject with tag '{_playerTag}'.");
            return;
        }

        if (enemies.Length == 0)
        {
            GameLog.LogError($"CombatTestInitiator could not find any GameObjects with tag '{_enemyTag}'.");
            return;
        }

        // Create a list of all participants
        List<GameObject> participants = new List<GameObject>();
        participants.Add(player);
        participants.AddRange(enemies);

        // Start the combat
        GameLog.Log("CombatTestInitiator is starting combat...");
        combatService.StartCombat(participants);
    }
}
