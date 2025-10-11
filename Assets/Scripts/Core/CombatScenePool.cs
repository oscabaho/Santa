using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

/// <summary>
/// Simple pool for combat scene prefabs. Tries to use Addressables when available; falls back to Resources/prefab.
/// </summary>
public class CombatScenePool : MonoBehaviour
{
    private static CombatScenePool _instance;
    public static CombatScenePool Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("__CombatScenePool");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<CombatScenePool>();
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, Task<GameObject>> _pendingInstantiations = new Dictionary<string, Task<GameObject>>();

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
            var handle = Addressables.InstantiateAsync(encounter.CombatSceneAddress, new InstantiationParameters(transform, false));
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
#endif

        if (encounter != null)
        {
            var inst = await encounter.InstantiateCombatSceneFallbackAsync();
            if (inst != null)
            {
                inst.SetActive(false);
                return inst;
            }
        }

        GameLog.LogWarning($"Could not create instance for key '{key}'");
        return null;
    }


    /// <summary>
    /// Release an instance back to the pool (deactivate and store). If Addressables was used to instantiate,
    /// caller is responsible for Addressables.ReleaseInstance if desired (we keep the instance alive for pooling).
    /// </summary>
    public void ReleaseInstance(string key, GameObject instance)
    {
        if (instance == null || string.IsNullOrEmpty(key)) return;
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
