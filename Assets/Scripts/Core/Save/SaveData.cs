using System;
using UnityEngine;

namespace Santa.Core.Save
{
    [Serializable]
    public class SaveData
    {
        // Scene and time
        public string sceneName;
        public DateTime savedAtUtc;

        // Player state
        public Vector3 playerPosition;

        // Progress snapshots (store identifiers)
        public string lastUpgrade;
        public string[] acquiredUpgrades; // optional identifiers
        public string[] defeatedEnemyIds; // identifiers/names
        public string[] environmentChangeIds; // decorative changes applied

        // Extensible key/value pairs for other systems
        public SerializableKV[] extras;

        /// <summary>
        /// Validates that this save data contains reasonable values.
        /// Helps prevent loading corrupted or tampered save files.
        /// </summary>
        public bool Validate()
        {
            // Scene name should not be empty
            if (string.IsNullOrEmpty(sceneName))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("SaveData.Validate: Scene name is null or empty.");
#endif
                return false;
            }

            // Player position should be within reasonable bounds
            // Assuming game world is within ±10,000 units
            if (playerPosition.magnitude > 10000f)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SaveData.Validate: Player position out of bounds: {playerPosition}");
#endif
                return false;
            }

            // Timestamp should not be in the future
            if (savedAtUtc > DateTime.UtcNow.AddMinutes(5))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SaveData.Validate: Save timestamp is in the future: {savedAtUtc}");
#endif
                return false;
            }

            // All checks passed
            return true;
        }
    }

    [Serializable]
    public struct SerializableKV
    {
        public string key;
        public string value;
    }
}
