// This helper is editor-only to avoid shipping auto-combat in builds.
#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using VContainer;

/// <summary>
/// This script is for testing purposes only.
/// It finds the player and enemies in the scene at startup and initiates combat.
/// </summary>
public class CombatTestInitiator : MonoBehaviour
{
    [Header("Testing Options")]
    [Tooltip("If enabled, combat will auto-start on Play using scene objects tagged as Player/Enemy. Leave OFF when testing Exploration.")]
    [SerializeField] private bool _autoStartOnPlay = false;

    [SerializeField] private string _playerTag = "Player";
    [SerializeField] private string _enemyTag = "Enemy";

    private ICombatService _combatService;

    [Inject]
    public void Construct(ICombatService combatService)
    {
        _combatService = combatService;
    }

    void Start()
    {
        // Allow using TestScene for Exploration: do nothing unless explicitly enabled
        if (!_autoStartOnPlay)
        {
            return;
        }

        if (_combatService == null)
        {
            GameLog.LogError("CombatTestInitiator could not find ICombatService. Make sure it is registered in a LifetimeScope.");
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
        _combatService.StartCombat(participants);
    }
}
#endif
