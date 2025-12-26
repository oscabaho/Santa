using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Santa.Core.Pooling
{
    /// <summary>
    /// Lightweight pooling service for GameObjects. Mobile-friendly: no LINQ, low-GC.
    /// </summary>
    public interface IPoolService
    {
        /// <summary>
        /// Prewarms a pool for a prefab with a given key.
        /// </summary>
        /// <param name="key">Logical key (e.g., address or prefab name)</param>
        /// <param name="prefab">Prefab to instantiate when pool is empty</param>
        /// <param name="count">Number of instances to create</param>
        UniTask PrewarmAsync(string key, GameObject prefab, int count);

        /// <summary>
        /// Gets an instance from the pool or instantiates a new one if empty.
        /// </summary>
        /// <param name="key">Key used at prewarm/request</param>
        /// <param name="prefab">Fallback prefab if none prewarmed</param>
        /// <param name="position">Spawn position</param>
        /// <param name="rotation">Spawn rotation</param>
        /// <param name="parent">Optional parent; if null, uses pool root</param>
        GameObject Get(string key, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null);

        /// <summary>
        /// Returns an instance back to the pool. The object will be deactivated and reparented under the pool root.
        /// </summary>
        /// <param name="key">Key used at get</param>
        /// <param name="instance">Instance to return</param>
        void Return(string key, GameObject instance);
    }
}
