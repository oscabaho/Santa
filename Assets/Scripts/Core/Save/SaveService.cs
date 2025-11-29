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
            SecureStorageJson.Set(SaveKey, data);
            GameLog.Log("SaveService: Game saved.");
        }

        public bool TryLoad(out SaveData data)
        {
            var ok = SecureStorageJson.TryGet(SaveKey, out data);
            if (!ok) return false;

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
            GameLog.Log("SaveService: Save deleted.");
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
    }
}
