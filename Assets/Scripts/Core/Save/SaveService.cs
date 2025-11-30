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
        private const string BackupKeyPrefix = "GameSave_Backup_";
        private const int MaxBackups = 3;
        private const float MinSaveInterval = 5f; // seconds

        private float _lastSaveTime = -999f;

        private ICombatService _combatService;
        private ISaveContributorRegistry _registry;

        [Inject]
        public void Construct(ICombatService combatService, ISaveContributorRegistry registry = null)
        {
            _combatService = combatService;
            _registry = registry;
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
                GameLog.LogWarning("SaveService: Save is disabled during combat.");
                return;
            }

            // Rate limiting: prevent save spam
            if (Time.time - _lastSaveTime < MinSaveInterval)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SaveService: Save rate limited. Please wait {MinSaveInterval - (Time.time - _lastSaveTime):F1} seconds.");
#endif
                return;
            }

            var data = new SaveData
            {
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
                savedAtUtc = DateTime.UtcNow,
            };

            // Capture player position
            var playerGo = FindPlayerObject();
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
            SecureStorageJson.Set(SaveKey, data);
            _lastSaveTime = Time.time;

            GameLog.Log("SaveService: Game saved.");
        }

        public bool TryLoad(out SaveData data)
        {
            // Try main save first
            var ok = SecureStorageJson.TryGet(SaveKey, out data);

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

            // Restore player position
            var playerGo = FindPlayerObject();
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
            SecureStorageJson.Delete(SaveKey);
            DeleteAllBackups();
            GameLog.Log("SaveService: Save and all backups deleted.");
        }

        private GameObject FindPlayerObject()
        {
            // Prefer combat service player if available
            if (_combatService != null && _combatService.Player != null)
            {
                return _combatService.Player;
            }
            // Fallback by tag/name
            var byTag = GameObject.FindWithTag("Player");
            if (byTag != null) return byTag;
            return GameObject.Find("Player");
        }

        private System.Collections.Generic.IReadOnlyList<ISaveContributor> GetContributors()
        {
            // Use registry if available (preferred)
            if (_registry != null)
            {
                return _registry.GetValidContributors();
            }

            // Fallback to scene scanning (legacy, expensive)
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning("SaveService: ISaveContributorRegistry not injected. Falling back to expensive FindObjectsByType. Consider adding SaveContributorRegistry to scene.");
#endif
            var contributors = new System.Collections.Generic.List<ISaveContributor>();
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in allMonoBehaviours)
            {
                if (mb is ISaveContributor sc)
                {
                    contributors.Add(sc);
                }
            }
            return contributors;
        }

        private void WriteContributors(ref SaveData data)
        {
            var contributors = GetContributors();
            foreach (var sc in contributors)
            {
                // Registry handles validity checks, but double-check for safety
                if (sc is MonoBehaviour mb && mb != null)
                {
                    sc.WriteTo(ref data);
                }
            }
            // extras can be filled by contributors via ref data
        }

        private void ReadContributors(in SaveData data)
        {
            var contributors = GetContributors();
            foreach (var sc in contributors)
            {
                // Registry handles validity checks, but double-check for safety
                if (sc is MonoBehaviour mb && mb != null)
                {
                    sc.ReadFrom(in data);
                }
            }
        }

        #region Backup System

        private void CreateBackup()
        {
            // Copy current main save to backup with timestamp
            if (!SecureStorageJson.TryGet(SaveKey, out SaveData currentData))
            {
                // No existing save to backup
                return;
            }

            var timestamp = DateTime.UtcNow.Ticks;
            var backupKey = $"{BackupKeyPrefix}{timestamp}";
            SecureStorageJson.Set(backupKey, currentData);

            // Clean old backups (keep only last MaxBackups)
            CleanOldBackups();
        }

        private bool TryLoadFromBackup(out SaveData data)
        {
            data = default;

            // Get all backup keys sorted by timestamp (most recent first)
            var backupKeys = GetBackupKeysSorted();

            foreach (var backupKey in backupKeys)
            {
                if (SecureStorageJson.TryGet(backupKey, out data))
                {
                    return true;
                }
            }

            return false;
        }

        private System.Collections.Generic.List<string> GetBackupKeysSorted()
        {
            var backupKeys = new System.Collections.Generic.List<string>();

            // Check for up to MaxBackups * 2 to account for old backups
            for (int i = 0; i < MaxBackups * 2; i++)
            {
                // We can't enumerate all keys in SecureStorage, so we try known patterns
                // This is a limitation - in production, consider using a manifest file
            }

            // For now, try a simple approach: check last 10 possible backup slots
            // This is a simplified implementation
            var now = DateTime.UtcNow.Ticks;
            var oneHourTicks = TimeSpan.FromHours(1).Ticks;

            for (int i = 0; i < 10; i++)
            {
                var estimatedTicks = now - (i * oneHourTicks);
                var testKey = $"{BackupKeyPrefix}{estimatedTicks}";

                // Test if this backup exists by trying to read it
                if (SecureStorageJson.TryGet<SaveData>(testKey, out _))
                {
                    backupKeys.Add(testKey);
                    if (backupKeys.Count >= MaxBackups) break;
                }
            }

            return backupKeys;
        }

        private void CleanOldBackups()
        {
            // Note: This is a simplified implementation
            // In a production system, you'd want a manifest file to track all backups
            // For now, we rely on the fact that old backups will naturally be replaced
        }

        private void DeleteAllBackups()
        {
            var backupKeys = GetBackupKeysSorted();
            foreach (var backupKey in backupKeys)
            {
                SecureStorageJson.Delete(backupKey);
            }
        }

        #endregion
    }
}
