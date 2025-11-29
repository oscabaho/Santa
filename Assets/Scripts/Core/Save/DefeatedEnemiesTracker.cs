using System.Collections.Generic;
using UnityEngine;

namespace Santa.Core.Save
{
    // Tracks defeated enemies in the scene by unique identifiers (name or explicit ID component)
    public class DefeatedEnemiesTracker : MonoBehaviour, ISaveContributor
    {
        private readonly HashSet<string> _defeated = new HashSet<string>();
        private IEventBus _eventBus;
        private ISaveContributorRegistry _registry;

        [VContainer.Inject]
        public void Construct(IEventBus eventBus, ISaveContributorRegistry registry = null)
        {
            _eventBus = eventBus;
            _registry = registry;
            _eventBus.Subscribe<CharacterDeathEvent>(OnCharacterDeath);
        }

        private void OnEnable()
        {
            // Register with the save system
            _registry?.Register(this);
        }

        private void OnDisable()
        {
            // Unregister when disabled
            _registry?.Unregister(this);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<CharacterDeathEvent>(OnCharacterDeath);
        }

        // Call this when an enemy is defeated
        public void MarkDefeated(GameObject enemy)
        {
            var id = GetId(enemy);
            if (!string.IsNullOrEmpty(id))
            {
                _defeated.Add(id);
                ApplyDefeatedVisual(enemy);
            }
        }

        private void OnCharacterDeath(CharacterDeathEvent evt)
        {
            if (evt?.Entity != null)
            {
                MarkDefeated(evt.Entity);
            }
        }

        public void WriteTo(ref SaveData data)
        {
            data.defeatedEnemyIds = new List<string>(_defeated).ToArray();
        }

        public void ReadFrom(in SaveData data)
        {
            _defeated.Clear();
            if (data.defeatedEnemyIds == null || data.defeatedEnemyIds.Length == 0) return;

            var defeatedIds = new HashSet<string>(data.defeatedEnemyIds);
            // Use FindObjectsByType<Transform> to iterate unique GameObjects (one Transform per GameObject)
            var allTransforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var transform in allTransforms)
            {
                var go = transform.gameObject;
                if (!go.scene.IsValid()) continue; // Skip prefabs
                if (!IsEnemy(go)) continue; // Ensure we only process enemies

                var id = GetId(go); // Use the shared GetId helper for consistent ID retrieval
                if (string.IsNullOrEmpty(id)) continue;

                if (defeatedIds.Contains(id))
                {
                    _defeated.Add(id);
                    ApplyDefeatedVisual(go);
                }
            }
        }

        private static bool IsEnemy(GameObject go)
        {
            // Heuristic: tag or component present
            return go.CompareTag("Enemy") || go.GetComponent<IEnemyMarker>() != null;
        }

        private static string GetId(GameObject go)
        {
            var marker = go.GetComponent<IUniqueIdProvider>();
            if (marker != null && !string.IsNullOrEmpty(marker.UniqueId)) return marker.UniqueId;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"DefeatedEnemiesTracker: Using GameObject name '{go.name}' as ID for enemy. This may cause collisions. Consider adding an IUniqueIdProvider.", go);
#endif
            return go.name; // fallback
        }

        private static void ApplyDefeatedVisual(GameObject enemy)
        {
            // Disable enemy gameobject for simplicity; projects may prefer pooling or state flags
            enemy.SetActive(false);
        }
    }

    // Optional marker interface to enable stronger enemy detection
    public interface IEnemyMarker { }
}
