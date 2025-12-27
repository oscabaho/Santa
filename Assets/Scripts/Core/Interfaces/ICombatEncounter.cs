using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for combat encounter data.
    /// Defines the configuration for transitioning to and loading a combat scene.
    /// </summary>
    public interface ICombatEncounter
    {
        /// <summary>
        /// Gets the Addressable key for the combat scene to load.
        /// </summary>
        string CombatSceneAddress { get; }
        
        /// <summary>
        /// Gets the pool key for this encounter type, used for object pooling.
        /// </summary>
        /// <returns>A string key uniquely identifying this encounter type.</returns>
        string GetPoolKey();
    }
}
