using System.Collections.Generic;
using UnityEngine;

namespace Santa.Core.Save
{
    // Tracks defeated enemies in the scene by unique identifiers (name or explicit ID component)
    public class DefeatedEnemiesTracker : MonoBehaviour, ISaveContributor
    {
        private readonly HashSet<string> _defeated = new HashSet<string>();
        private IEventBus _eventBus;

        [VContainer.Inject]
        public void Construct(IEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.Subscribe<CharacterDeathEvent>(OnCharacterDeath);
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
            if (evt?.Character != null)
            {
                MarkDefeated(evt.Character);
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
            // Use FindObjectsByType for better performance (no sorting needed)
            var allIdentifiables = FindObjectsByType<Santa.Core.Save.UniqueIdProvider>(FindObjectsSortMode.None);
            var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var mb in allObjects)
            {
                if (mb is IUniqueIdProvider provider)
                {
                    if (!mb.gameObject.scene.IsValid()) continue; // Skip prefabs

                    var id = provider.UniqueId;
                    if (string.IsNullOrEmpty(id)) continue;

                    if (defeatedIds.Contains(id))
                    {
                        _defeated.Add(id);
                        ApplyDefeatedVisual(mb.gameObject);
                    }
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
