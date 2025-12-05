using System;
using UnityEngine;
using Santa.Core.Config;

namespace Santa.Core.Save
{
    [Serializable]
    public class SaveData
    {
        // Scene and time
        public string sceneName;
        public long savedAtUtcTicks; // DateTime stored as ticks for JsonUtility compatibility

        // Helper property for easy DateTime access
        public DateTime savedAtUtc
        {
            get => savedAtUtcTicks > 0 ? new DateTime(savedAtUtcTicks, DateTimeKind.Utc) : default;
            set => savedAtUtcTicks = value.Ticks;
        }

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
            // Assuming game world is within defined bounds
            if (playerPosition.magnitude > GameConstants.PlayerStats.MaxPlayerPositionMagnitude)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SaveData.Validate: Player position out of bounds: {playerPosition}");
#endif
                return false;
            }

            // Timestamp should not be in the future
            var savedTime = savedAtUtc;
            if (savedTime > DateTime.UtcNow.AddMinutes(5))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SaveData.Validate: Save timestamp is in the future: {savedTime}");
#endif
                return false;
            }

            // Timestamp should not be from before game was created (2000 is a reasonable minimum)
            if (savedTime != default && savedTime.Year < 2000)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"SaveData.Validate: Save timestamp is unreasonably old: {savedTime}");
#endif
                return false;
            }

            // At least one upgrade should have been acquired or this is a fresh save
            // (This is not a validation failure, just informational)

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
