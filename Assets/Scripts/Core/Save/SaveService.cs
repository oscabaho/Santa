using System;
using UnityEngine;
using VContainer;
using Santa.Core.Security;

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
            var playerGo = FindPlayerObject();
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
            Debug.Log("SaveService: Save deleted.");
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

        private void WriteContributors(ref SaveData data)
        {
            var contributors = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            var list = new System.Collections.Generic.List<SerializableKV>();
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
            var contributors = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
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
