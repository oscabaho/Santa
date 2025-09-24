using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A data container for a specific combat encounter.
/// This component holds all the necessary information to start a battle.
/// </summary>
public class CombatEncounter : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("The parent GameObject that contains the entire pre-staged combat scene.")]
    [SerializeField] private GameObject combatSceneParent;

    [Tooltip("The virtual camera that will be used for this combat encounter.")]
    [SerializeField] private GameObject combatCamera;

    [Header("Combatants")]
    [Tooltip("A list of transforms where enemies will be spawned.")]
    [SerializeField] private List<Transform> enemySpawnPoints;

    public GameObject CombatSceneParent => combatSceneParent;
    public GameObject CombatCamera => combatCamera;
    public List<GameObject> CombatParticipants { get; private set; }

    /// <summary>
    /// Sets up the encounter by spawning enemies and preparing the list of combatants.
    /// </summary>
    /// <param name="player">The player GameObject.</param>
    /// <param name="enemyPrefabs">A list of enemy prefabs to spawn.</param>
    public void SetupEncounter(GameObject player, List<Enemy> enemyPrefabs)
    {
        CombatParticipants = new List<GameObject>();
        CombatParticipants.Add(player);

        // TODO: Clear previously spawned enemies if any

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            if (i < enemySpawnPoints.Count)
            {
                Enemy enemyPrefab = enemyPrefabs[i];
                Transform spawnPoint = enemySpawnPoints[i];
                GameObject enemyInstance = Instantiate(enemyPrefab.gameObject, spawnPoint.position, spawnPoint.rotation, spawnPoint);
                CombatParticipants.Add(enemyInstance);
            }
            else
            {
                Debug.LogWarning($"Not enough spawn points for all enemies. Enemy {enemyPrefabs[i].name} was not spawned.");
            }
        }
    }
}