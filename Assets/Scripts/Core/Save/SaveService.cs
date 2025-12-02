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

        [Inject]
        public void Construct(ICombatService combatService)
        {
            _combatService = combatService;
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
            SecureStorageJson.Set(SaveKey, data);
            Debug.Log("SaveService: Game saved.");
        }

        public bool TryLoad(out SaveData data)
        {
            var ok = SecureStorageJson.TryGet(SaveKey, out data);
            if (!ok) return false;

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
            SecureStorageJson.Delete(SaveKey);
            Debug.Log("SaveService: Save deleted.");
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
    }
}
