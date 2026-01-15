using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using Santa.Core;
using Santa.Core.Config;
#if UNITY_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Pool for combat scene prefabs loaded via Addressables. Reuses instances to optimize performance.
    /// Injects VContainer dependencies into all components of instantiated arenas.
    /// </summary>
    public class CombatScenePool : MonoBehaviour
{
    private IObjectResolver _resolver;

    private readonly Dictionary<string, Queue<GameObject>> _pool = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, UniTask<GameObject>> _pendingInstantiations = new Dictionary<string, UniTask<GameObject>>();
    private readonly Vector3 _combatSceneOffset = new Vector3(0f, GameConstants.CombatScene.OffsetY, 0f);

    [Inject]
    public void Construct(IObjectResolver resolver)
    {
        _resolver = resolver;
    }

    /// <summary>
    /// Get an instance for the given key. If pool has available instance, returns it.
    /// Otherwise loads/instantiates asynchronously and returns the instance via UniTask.
    /// </summary>
    public async UniTask<GameObject> GetInstanceAsync(string key, ICombatEncounter encounter)
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

    private UniTask<GameObject> InstantiateNewInstanceAsync(string key, ICombatEncounter encounter)
    {
#if UNITY_ADDRESSABLES
        if (encounter != null && !string.IsNullOrEmpty(encounter.CombatSceneAddress))
        {
            // Instantiates the prefab at the specified offset position
            var handle = Addressables.InstantiateAsync(encounter.CombatSceneAddress, _combatSceneOffset, Quaternion.identity, transform);
            return InstantiateAndInjectAsync(handle);
        }
        else
        {
            GameLog.LogError($"CombatScenePool: No valid CombatSceneAddress configured for key '{key}'. Combat arenas MUST be loaded via Addressables.");
        }
#else
        UnityEngine.Debug.LogError($"CombatScenePool: UNITY_ADDRESSABLES is not defined. Combat arenas cannot be loaded. Please ensure the Addressables package is installed and the scripting define is set.");
#endif

        return UniTask.FromResult<GameObject>(null);
    }

#if UNITY_ADDRESSABLES
    private async UniTask<GameObject> InstantiateAndInjectAsync(AsyncOperationHandle<GameObject> handle)
    {
        var inst = await handle.ToUniTask();

        if (handle.Status == AsyncOperationStatus.Succeeded && inst != null)
        {
            // Inject VContainer dependencies into all components in the arena
            InjectDependenciesRecursively(inst);
            return inst;
        }
        else
        {
            GameLog.LogError($"Addressables.InstantiateAsync failed for '{handle.DebugName}'");
            return null;
        }
    }
#endif

    /// <summary>
    /// Injects VContainer dependencies recursively into all MonoBehaviour components.
    /// This ensures dynamically instantiated objects receive their dependencies.
    /// </summary>
    private void InjectDependenciesRecursively(GameObject instance)
    {
        if (_resolver == null)
        {
            GameLog.LogWarning("CombatScenePool: IObjectResolver is null, cannot inject dependencies into combat arena.");
            return;
        }

        var components = instance.GetComponentsInChildren<MonoBehaviour>(true);
        int successCount = 0;
        int failCount = 0;

        foreach (var component in components)
        {
            try
            {
                _resolver.Inject(component);
                successCount++;
            }
            catch (System.Exception ex)
            {
                // Log warning but continue injecting other components
                // Some dependencies (like CombatUI) may not be available yet
                GameLog.LogWarning($"CombatScenePool: Failed to inject dependencies into {component.GetType().Name} on {component.gameObject.name}. Error: {ex.Message}");
                failCount++;
            }
        }

        GameLog.Log($"CombatScenePool: Injected dependencies into {successCount}/{components.Length} components in arena '{instance.name}'. {failCount} failed (may be optional dependencies).");
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

    public async UniTask PrewarmAsync(string key, int count, ICombatEncounter encounter)
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
}
