using System;
using Santa.Core;
using Santa.Core.Config;
using Santa.Core.Events;
using Santa.Core.Save;
using Santa.Infrastructure.Combat;
using Cysharp.Threading.Tasks;
using System.IO;
using System.Text;
using UnityEngine;
using VContainer;

namespace Santa.Core.Save
{
    // Mobile-first save service storing small JSON snapshots via SecureStorage
    public class SaveService : MonoBehaviour, ISaveService
    {
        private const string SaveKey = "GameSave";
        private const string ManifestKey = "GameSave_Manifest";
        private const string BackupKeyPrefix = "GameSave_Backup_";
        private const int MaxBackups = 2;

        private ISaveContributorRegistry _registry;
        private ISecureStorageService _secureStorage;
        private IEventBus _eventBus;
        private float _lastSaveTime;

        [Inject]
        public void Construct(ISecureStorageService secureStorage, ISaveContributorRegistry registry = null, IEventBus eventBus = null)
        {
            _secureStorage = secureStorage;
            _registry = registry;
            _eventBus = eventBus;
        }

        public bool CanSaveNow()
        {
            // Allow saving only when NOT in combat.
            // Check for TurnBasedCombatManager logic directly
            return !TurnBasedCombatManager.CombatIsInitialized;
        }

        public void Save()
        {
            if (!CanSaveNow())
            {
                UnityEngine.Debug.LogWarning("SaveService: Save is disabled during combat.");
                return;
            }
            var data = new SaveData
            {
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                savedAtUtc = DateTime.UtcNow,
            };

            // Capture player position
            var playerGo = GetPlayerObject();
            if (playerGo != null)
            {
                data.playerPosition = playerGo.transform.position;
            }

            // Allow scene components to contribute data
            WriteContributors(ref data);

            // Validate before saving
            if (!data.Validate())
            {
                GameLog.LogError("SaveService: Save data validation failed. Not saving.");
                return;
            }

            // Create backup before overwriting main save
            CreateBackup();

            // Save main file
            _secureStorage.Save(SaveKey, data);
            _lastSaveTime = Time.time;

            GameLog.Log("SaveService: Game saved.");
        }

        public bool TryLoad(out SaveData data)
        {
            // Try main save first
            var ok = _secureStorage.TryLoad(SaveKey, out data);

            // If main save fails, try backups
            if (!ok)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("SaveService: Main save failed. Attempting to load from backups...");
#endif
                ok = TryLoadFromBackup(out data);

                if (!ok)
                {
                    GameLog.LogError("SaveService: All save files failed to load.");
                    return false;
                }

                GameLog.Log("SaveService: Successfully loaded from backup.");
            }

            // Validate loaded data
            if (!data.Validate())
            {
                GameLog.LogError("SaveService: Loaded save data failed validation.");
                data = default;
                return false;
            }

            // Restore player position to respawn point (not the saved position)
            // This ensures the player always appears at the designated spawn location
            var playerGo = GetPlayerObject();
            if (playerGo != null)
            {
                var spawnPoint = FindSpawnPoint();
                if (spawnPoint != null)
                {
                    playerGo.transform.position = spawnPoint.position;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log($"SaveService: Player positioned at respawn point: {spawnPoint.position}");
#endif
                }
                else
                {
                    // Fallback: use saved position if no respawn point found
                    playerGo.transform.position = data.playerPosition;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.LogWarning("SaveService: No respawn point found. Using saved player position instead.");
#endif
                }
            }

            // Let contributors restore their state
            ReadContributors(in data);

            // Publish game loaded event to notify all systems that save has been loaded
            _eventBus?.Publish(new GameLoadedEvent(data));

            return true;
        }

        public void Delete()
        {
            _secureStorage.Delete(SaveKey);
            DeleteAllBackups();
            GameLog.Log("SaveService: Save and all backups deleted.");
        }

        public bool TryGetLastSaveTimeUtc(out DateTime utc)
        {
            // Read current main save without mutating any state
            if (_secureStorage.TryLoad(SaveKey, out SaveData data))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"SaveService.TryGetLastSaveTimeUtc: Loaded data with timestamp {data.savedAtUtc:u} (Year={data.savedAtUtc.Year})");
#endif
                // Guard against default DateTime (no real save yet)
                if (data.savedAtUtc != default && data.savedAtUtc.Year >= 2000)
                {
                    utc = data.savedAtUtc;
                    return true;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SaveService.TryGetLastSaveTimeUtc: Timestamp validation failed (default={data.savedAtUtc == default}, year={data.savedAtUtc.Year})");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("SaveService.TryGetLastSaveTimeUtc: TryLoad returned false - no save data found");
#endif
            }
            utc = default;
            return false;
        }

        private GameObject GetPlayerObject()
        {
            // 1. Try Combat Service first (if in combat)
            // Use runtime lookup since SaveService is global and CombatService is local
            var combatService = FindFirstObjectByType<TurnBasedCombatManager>(); 
            if (combatService != null && combatService.Player != null)
            {
                return combatService.Player;
            }

            // 2. Try Exploration Player (via PlayerReference or identifier)
            // Use FindFirstObjectByType for interface implementation on MonoBehaviour
            var playerRef = FindFirstObjectByType<Santa.Core.Player.PlayerReference>();
            if (playerRef != null && playerRef.Player != null)
            {
                return playerRef.Player;
            }
            
            // Fallback to searching by identifier directly if manager missing
            var explorationId = FindFirstObjectByType<ExplorationPlayerIdentifier>();
            if (explorationId != null) return explorationId.gameObject;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("SaveService: Player reference not available.");
#endif
            return null;
        }

        private Transform FindSpawnPoint()
        {
            // First, try to find by specific SpawnPoint component type (most efficient)
            var spawnPointComponent = FindFirstObjectByType<SpawnPoint>(FindObjectsInactive.Include);
            if (spawnPointComponent != null)
            {
                var spawnTransform = spawnPointComponent.GetSpawnTransform();
                if (spawnTransform != null)
                {
                    return spawnTransform;
                }
            }
            
            // Fallback: Search for GameObject named "SpawnPoint"
            var spawnObj = GameObject.Find("SpawnPoint");
            if (spawnObj != null)
            {
                return spawnObj.transform;
            }
            
            return null;
        }

        private void WriteContributors(ref SaveData data)
        {
            var contributors = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var mb in contributors)
            {
                if (mb is ISaveContributor sc)
                {
                    sc.WriteTo(ref data);
                }
            }
            // extras can be filled by contributors via ref data
        }

        private void ReadContributors(in SaveData data)
        {
            var contributors = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in contributors)
            {
                if (mb is ISaveContributor sc)
                {
                    sc.ReadFrom(in data);
                }
            }
        }

        #region Backup System

        private SaveManifest LoadManifest()
        {
            if (_secureStorage.TryLoad(ManifestKey, out SaveManifest manifest))
            {
                return manifest;
            }
            return new SaveManifest();
        }

        private void SaveManifest(SaveManifest manifest)
        {
            _secureStorage.Save(ManifestKey, manifest);
        }

        private void CreateBackup()
        {
            // Copy current main save to backup with timestamp
            if (!_secureStorage.TryLoad(SaveKey, out SaveData currentData))
            {
                // No existing save to backup
                return;
            }

            var timestamp = DateTime.UtcNow.Ticks;
            var backupKey = $"{BackupKeyPrefix}{timestamp}";

            // Save the backup file
            _secureStorage.Save(backupKey, currentData);

            // Update manifest
            var manifest = LoadManifest();
            manifest.Backups.Add(new BackupEntry { Key = backupKey, Timestamp = timestamp });

            // Clean old backups (keep only last MaxBackups)
            CleanOldBackups(manifest);

            // Save updated manifest
            SaveManifest(manifest);
        }

        private bool TryLoadFromBackup(out SaveData data)
        {
            data = default;

            // Get all backup keys sorted by timestamp (most recent first)
            var backupKeys = GetBackupKeysSorted();

            foreach (var backupKey in backupKeys)
            {
                if (_secureStorage.TryLoad(backupKey, out data))
                {
                    return true;
                }
            }

            return false;
        }

        private System.Collections.Generic.List<string> GetBackupKeysSorted()
        {
            var manifest = LoadManifest();
            var keys = new System.Collections.Generic.List<string>();

            // Sort by timestamp descending
            manifest.Backups.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

            foreach (var entry in manifest.Backups)
            {
                keys.Add(entry.Key);
            }

            return keys;
        }

        private void CleanOldBackups(SaveManifest manifest)
        {
            // Sort by timestamp descending (newest first)
            manifest.Backups.Sort((a, b) => b.Timestamp.CompareTo(a.Timestamp));

            if (manifest.Backups.Count <= MaxBackups)
            {
                return;
            }

            // Identify backups to remove (all after MaxBackups)
            int removeCount = manifest.Backups.Count - MaxBackups;
            var toRemove = manifest.Backups.GetRange(MaxBackups, removeCount);

            // Remove files
            foreach (var entry in toRemove)
            {
                _secureStorage.Delete(entry.Key);
            }

            // Remove from manifest
            manifest.Backups.RemoveRange(MaxBackups, removeCount);
        }

        private void DeleteAllBackups()
        {
            var manifest = LoadManifest();
            foreach (var entry in manifest.Backups)
            {
                _secureStorage.Delete(entry.Key);
            }

            manifest.Backups.Clear();
            SaveManifest(manifest);
            _secureStorage.Delete(ManifestKey);
        }

        #endregion
    }
}
