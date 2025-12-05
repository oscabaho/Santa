using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Santa.Core;
using Santa.Core.Save;

namespace Santa.Infrastructure.Save
{
    /// <summary>
    /// Example implementation of ISaveContributor showing best practices.
    /// This component demonstrates how to save and restore any game state.
    /// Use this as a template for creating new save-enabled components.
    /// </summary>
    public class SaveContributorTemplate : MonoBehaviour, ISaveContributor
    {
        // === DATA TO PERSIST ===
        [Header("State to Save")]
        [SerializeField] private bool isAreaLiberated = false;
        [SerializeField] private int visitCount = 0;
        private readonly HashSet<string> _visitedLocations = new HashSet<string>();

        // === DEPENDENCIES ===
        private ISaveContributorRegistry _registry;

        // === INITIALIZATION ===
        [VContainer.Inject]
        public void Construct(ISaveContributorRegistry registry = null)
        {
            _registry = registry;
        }

        private void OnEnable()
        {
            // Register with save system so SaveService finds this component
            _registry?.Register(this);
        }

        private void OnDisable()
        {
            // Unregister when disabled to avoid stale references
            _registry?.Unregister(this);
        }

        // === PUBLIC INTERFACE ===
        public void MarkLocationVisited(string locationId)
        {
            if (!string.IsNullOrEmpty(locationId))
            {
                _visitedLocations.Add(locationId);
                visitCount++;
            }
        }

        public bool HasVisited(string locationId) => _visitedLocations.Contains(locationId);

        public void LiberateArea()
        {
            isAreaLiberated = true;
        }

        // === SAVE/LOAD IMPLEMENTATION ===

        /// <summary>
        /// Called by SaveService when saving the game.
        /// Write ALL persistent data to the SaveData structure.
        /// This method receives a reference, so modify it directly.
        /// </summary>
        public void WriteTo(ref SaveData data)
        {
            // Example 1: Store simple values in extras
            var extras = new List<SerializableKV>(data.extras ?? System.Array.Empty<SerializableKV>());

            extras.Add(new SerializableKV { key = "SaveContributorTemplate_IsAreaLiberated", value = isAreaLiberated.ToString() });
            extras.Add(new SerializableKV { key = "SaveContributorTemplate_VisitCount", value = visitCount.ToString() });

            // Example 2: Store collections as comma-separated strings
            string visitedLocationsStr = string.Join(",", _visitedLocations);
            extras.Add(new SerializableKV { key = "SaveContributorTemplate_VisitedLocations", value = visitedLocationsStr });

            data.extras = extras.ToArray();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"SaveContributorTemplate: Saved {_visitedLocations.Count} visited locations, area liberated: {isAreaLiberated}");
#endif
        }

        /// <summary>
        /// Called by SaveService when loading a saved game.
        /// Restore ALL persistent data from the SaveData structure.
        /// This method receives a reference (in keyword), so you read from it.
        /// </summary>
        public void ReadFrom(in SaveData data)
        {
            // Reset to defaults before loading
            isAreaLiberated = false;
            visitCount = 0;
            _visitedLocations.Clear();

            // Extract values from extras
            if (data.extras == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("SaveContributorTemplate: No extras found in save data");
#endif
                return;
            }

            // Create a dictionary for faster lookups
            var extraDict = new System.Collections.Generic.Dictionary<string, string>();
            foreach (var kv in data.extras)
            {
                extraDict[kv.key] = kv.value;
            }

            // Example 1: Parse simple boolean
            if (extraDict.TryGetValue("SaveContributorTemplate_IsAreaLiberated", out var liberatedStr))
            {
                if (bool.TryParse(liberatedStr, out var liberated))
                {
                    isAreaLiberated = liberated;
                }
            }

            // Example 2: Parse integer
            if (extraDict.TryGetValue("SaveContributorTemplate_VisitCount", out var visitCountStr))
            {
                if (int.TryParse(visitCountStr, out var count))
                {
                    visitCount = count;
                }
            }

            // Example 3: Parse collection (comma-separated string)
            if (extraDict.TryGetValue("SaveContributorTemplate_VisitedLocations", out var visitedStr))
            {
                if (!string.IsNullOrEmpty(visitedStr))
                {
                    foreach (var locationId in visitedStr.Split(','))
                    {
                        if (!string.IsNullOrEmpty(locationId))
                        {
                            _visitedLocations.Add(locationId);
                        }
                    }
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"SaveContributorTemplate: Loaded {_visitedLocations.Count} visited locations, area liberated: {isAreaLiberated}");
#endif
        }

        // === HELPER METHODS ===

        /// <summary>
        /// Helper to find a value in extras by key.
        /// </summary>
        private bool TryGetExtra(in SaveData data, string key, out string value)
        {
            value = null;
            if (data.extras == null) return false;

            foreach (var kv in data.extras)
            {
                if (kv.key == key)
                {
                    value = kv.value;
                    return true;
                }
            }

            return false;
        }
    }
}
