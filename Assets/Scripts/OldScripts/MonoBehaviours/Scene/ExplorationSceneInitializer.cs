using UnityEngine;
using ProyectSecret.Combat.SceneManagement;
using ProyectSecret.Inventory; // Necesario para ItemDatabase
using ProyectSecret.MonoBehaviours.Player; // Para PaperMarioPlayerMovement
using ProyectSecret.Managers; // Para el AudioManager
using ProyectSecret.Events;   // Necesario para el GameEventBus

/// <summary>
/// Inicializa la escena de exploración y posiciona al jugador en un punto fijo (estatua) si viene de una derrota en combate.
/// </summary>
public class ExplorationSceneInitializer : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform statueSpawnPoint;
    [SerializeField] private PlayerPersistentData playerPersistentData;
    [SerializeField] private ItemDatabase itemDatabase; // Añadir referencia a la base de datos de ítems
    [Header("Cancion de fondo")]
    [SerializeField] private AudioClip backgroundMusic; // Canción de fondo para la escena

    private void Start()
    {
        if (!ValidateReferences()) return;

        Vector3 spawnPosition = DetermineSpawnPosition();
        var player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        InitializePlayerState(player);
        InitializeSceneAudio();

        // Publicar el evento al final, después de que todo esté listo.
        GameEventBus.Instance.Publish(new PlayerSpawnedEvent(player));
    }

    private bool ValidateReferences()
    {
        if (playerPrefab != null && statueSpawnPoint != null && playerPersistentData != null && itemDatabase != null)
            return true;
        
        #if UNITY_EDITOR
        Debug.LogError("ExplorationSceneInitializer: Faltan referencias esenciales. Asigna PlayerPrefab, StatueSpawnPoint, PlayerPersistentData e ItemDatabase en el Inspector.");
        #endif
        return false;
    }

    private Vector3 DetermineSpawnPosition()
    {
        if (playerPersistentData.CameFromDefeat)
        {
            return statueSpawnPoint.position;
        }
        
        if (playerPersistentData.HasSavedPosition)
        {
            return playerPersistentData.LastPosition;
        }
        
        return statueSpawnPoint.position; // Posición por defecto
    }

    private void InitializePlayerState(GameObject player)
    {
        if (playerPersistentData.CameFromDefeat)
        {
            playerPersistentData.ApplyToPlayer(player, itemDatabase);
            playerPersistentData.CameFromDefeat = false; // Resetear el flag
        }
        else if (string.IsNullOrEmpty(playerPersistentData.equippedWeaponId))
        {
            // Lógica de primera partida
            var health = player.GetComponent<ProyectSecret.Components.HealthComponentBehaviour>();
            if (health != null) health.SetToMax();
            
            var stamina = player.GetComponent<ProyectSecret.Components.StaminaComponentBehaviour>();
            if (stamina != null) stamina.SetToMax();
        }
        else
        {
            // Carga normal
            playerPersistentData.ApplyToPlayer(player, itemDatabase);
        }
        
        // Invalidar la posición guardada después de usarla, en todos los casos.
        playerPersistentData.HasSavedPosition = false;
    }
    private void InitializeSceneAudio()
    {
        if (AudioManager.Instance != null)
        {
            if (backgroundMusic != null)
                AudioManager.Instance.PlayMusic(backgroundMusic);
        }
        else
        {
            #if UNITY_EDITOR
            Debug.LogWarning("ExplorationSceneInitializer: Instancia de AudioManager no encontrada. La música de fondo no se reproducirá.");
            #endif
        }
    }
}
