using System;
using UnityEngine;

namespace Santa.Core.Save
{
    [Serializable]
    public struct SaveData
    {
        public string sceneName;
        public DateTime savedAtUtc;
        public Vector3 playerPosition;

        // UpgradeManager
        public string lastUpgrade;
        public string[] acquiredUpgrades;

        // DefeatedEnemiesTracker
        public string[] defeatedEnemyIds;

        // EnvironmentDecorState
        public string[] environmentChangeIds;
    }
}
