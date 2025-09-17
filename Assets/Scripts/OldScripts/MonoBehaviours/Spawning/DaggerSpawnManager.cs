using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Instancia el prefab de la daga en 2 spawn points aleatorios de los asignados en el inspector.
/// </summary>
public class DaggerSpawnManager : MonoBehaviour
{
    [Header("Prefab de la daga")]
    [SerializeField] private GameObject daggerPrefab;
    [Header("Spawn Points (asignar 5 en el inspector)")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [Header("Cantidad de dagas a instanciar")]
    [SerializeField] private int daggersToSpawn = 2;

    void Start()
    {
        if (daggerPrefab == null || spawnPoints.Count < daggersToSpawn)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("Configura el prefab y al menos tantos spawn points como dagas a instanciar.");
            #endif
            return;
        }

        // Creamos una copia de la lista para poder modificarla sin afectar la original.
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < daggersToSpawn; i++)
        {
            // Si por alguna razón nos quedamos sin puntos, salimos del bucle.
            if (availablePoints.Count == 0) break;

            // Elegimos un índice aleatorio de la lista de puntos *disponibles*.
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[randomIndex];

            // Instanciamos la daga en el punto elegido.
            Instantiate(daggerPrefab, spawnPoint.position, spawnPoint.rotation);

            // Eliminamos el punto de la lista para que no se pueda volver a elegir.
            availablePoints.RemoveAt(randomIndex);
        }
    }
}

