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
    }

    [Serializable]
    public struct SerializableKV
    {
        public string key;
        public string value;
    }
}
