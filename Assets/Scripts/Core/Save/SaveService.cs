using System;
using Santa.Core.Security;
using UnityEngine;
using VContainer;

namespace Santa.Core.Save
{
    // Mobile-first save service storing small JSON snapshots via SecureStorage
    public class SaveService : MonoBehaviour, ISaveService
    {
        private const string SaveKey = "GameSave";

        private ICombatService _combatService;
<<<<<<< Updated upstream

        [Inject]
        public void Construct(ICombatService combatService)
        {
            _combatService = combatService;
=======
        private ISaveContributorRegistry _registry;
        private ISecureStorageService _secureStorage;
        private Santa.Core.Player.IPlayerReference _playerRef;

        [Inject]
        public void Construct(ICombatService combatService, ISecureStorageService secureStorage, ISaveContributorRegistry registry = null, Santa.Core.Player.IPlayerReference playerRef = null)
        {
            _combatService = combatService;
            _secureStorage = secureStorage;
            _registry = registry;
            _playerRef = playerRef;
>>>>>>> Stashed changes
        }

        public bool CanSaveNow()
        {
            // Allow saving only when NOT in combat.
            // Exploration is considered when there is no combat service
            // or when the combat manager reports not initialized.
            if (_combatService == null)
            {
                return true;
            }

            // If a TurnBasedCombatManager is present, rely on its initialized flag.
            // This avoids guessing phases and cleanly distinguishes exploration.
            return !TurnBasedCombatManager.CombatIsInitialized;
        }

        public void Save()
        {
            if (!CanSaveNow())
            {
                Debug.LogWarning("SaveService: Save is disabled during combat.");
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
<<<<<<< Updated upstream
            SecureStorageJson.Set(SaveKey, data);
            Debug.Log("SaveService: Game saved.");
=======

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
>>>>>>> Stashed changes
        }

        public bool TryLoad(out SaveData data)
        {
<<<<<<< Updated upstream
            var ok = SecureStorageJson.TryGet(SaveKey, out data);
            if (!ok) return false;
=======
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
>>>>>>> Stashed changes

            // Restore player position
            var playerGo = GetPlayerObject();
            if (playerGo != null)
            {
                playerGo.transform.position = data.playerPosition;
            }

            // Let contributors restore their state
            ReadContributors(in data);
            return true;
        }

        public void Delete()
        {
<<<<<<< Updated upstream
            SecureStorageJson.Delete(SaveKey);
            Debug.Log("SaveService: Save deleted.");
=======
            _secureStorage.Delete(SaveKey);
            DeleteAllBackups();
            GameLog.Log("SaveService: Save and all backups deleted.");
>>>>>>> Stashed changes
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
            // Prefer combat service player if available
            if (_combatService != null && _combatService.Player != null)
            {
                return _combatService.Player;
            }
            // Use injected player reference if available
            if (_playerRef != null && _playerRef.Player != null)
            {
                return _playerRef.Player;
            }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogError("SaveService: Player reference not available. Ensure IPlayerReference is registered and present in the base scene.");
#endif
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
<<<<<<< Updated upstream
=======

        #region Backup System

        private const string ManifestKey = "GameSave_Manifest";

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
>>>>>>> Stashed changes
    }
}
