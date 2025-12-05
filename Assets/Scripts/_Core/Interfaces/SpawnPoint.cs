using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Marker interface for identifying the player respawn point in a scene.
    /// Attach this component to the GameObject that serves as the spawn location.
    /// </summary>
    public interface ISpawnPoint
    {
        Transform GetSpawnTransform();
    }

    /// <summary>
    /// Simple implementation of ISpawnPoint for marking respawn locations.
    /// </summary>
    public class SpawnPoint : MonoBehaviour, ISpawnPoint
    {
        public Transform GetSpawnTransform() => transform;
    }
}
