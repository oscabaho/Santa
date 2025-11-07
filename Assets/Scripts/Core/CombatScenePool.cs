using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

/// <summary>
/// Pool for combat scene prefabs loaded via Addressables. Reuses instances to optimize performance.
/// </summary>
public class CombatScenePool : MonoBehaviour
{

    private readonly Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, Task<GameObject>> _pendingInstantiations = new Dictionary<string, Task<GameObject>>();
    private readonly Vector3 _combatSceneOffset = new Vector3(0f, -2000f, 0f);

    /// <summary>
    /// Get an instance for the given key. If pool has available instance, returns it.
    /// Otherwise loads/instantiates asynchronously and returns the instance via Task.
    /// </summary>
    public async Task<GameObject> GetInstanceAsync(string key, ICombatEncounter encounter)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        if (_pool.TryGetValue(key, out var q) && q.Count > 0)
        {
            var instance = q.Dequeue();
            if (instance != null)
            {
                return instance;
            }
        }

        // Check for pending instantiations
        if (_pendingInstantiations.TryGetValue(key, out var pendingTask))
        {
            return await pendingTask;
        }

        // Create and store the task, then remove it upon completion
        var instantiationTask = InstantiateNewInstanceAsync(key, encounter);
        _pendingInstantiations[key] = instantiationTask;

        try
        {
            return await instantiationTask;
        }
        finally
        {
            _pendingInstantiations.Remove(key);
        }
    }

    private async Task<GameObject> InstantiateNewInstanceAsync(string key, ICombatEncounter encounter)
    {
#if UNITY_ADDRESSABLES
        if (encounter != null && !string.IsNullOrEmpty(encounter.CombatSceneAddress))
        {
            // Instantiates the prefab at the specified offset position
            var handle = Addressables.InstantiateAsync(encounter.CombatSceneAddress, _combatSceneOffset, Quaternion.identity, transform);
            var inst = await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded && inst != null)
            {
                return inst;
            }
            else
            {
                GameLog.LogError($"Addressables.InstantiateAsync failed for '{encounter.CombatSceneAddress}'");
            }
        }
        else
        {
            GameLog.LogError($"CombatScenePool: No valid CombatSceneAddress configured for key '{key}'. Combat arenas MUST be loaded via Addressables.");
        }
#else
        GameLog.LogError($"CombatScenePool: UNITY_ADDRESSABLES is not defined. Combat arenas require Addressables package.");
#endif

        return null;
    }


    /// <summary>
    /// Release an instance back to the pool (deactivate and store).
    /// If <paramref name="releaseAddressablesInstance"/> is true, the instance is released back to Addressables instead.
    /// </summary>
    public void ReleaseInstance(string key, GameObject instance, bool releaseAddressablesInstance = false)
    {
        if (instance == null || string.IsNullOrEmpty(key)) return;

#if UNITY_ADDRESSABLES
        if (releaseAddressablesInstance)
        {
            Addressables.ReleaseInstance(instance);
            return;
        }
#else
        if (releaseAddressablesInstance)
        {
            GameLog.LogWarning("CombatScenePool: Requested to release Addressables instance, but UNITY_ADDRESSABLES is not defined. Instance will remain pooled.");
        }
#endif

        instance.SetActive(false);
        if (!_pool.TryGetValue(key, out var q))
        {
            q = new Queue<GameObject>();
            _pool[key] = q;
        }
        q.Enqueue(instance);
    }

    public async Task PrewarmAsync(string key, int count, ICombatEncounter encounter)
    {
        if (string.IsNullOrEmpty(key) || encounter == null) return;

        for (int i = 0; i < count; i++)
        {
            var instance = await GetInstanceAsync(key, encounter);
            if (instance != null)
            {
                ReleaseInstance(key, instance);
            }
        }
    }
}
