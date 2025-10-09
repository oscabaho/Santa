using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VFXManager : MonoBehaviour, IVFXService
{
    private static VFXManager Instance { get; set; }
    
    [System.Serializable]
    public class VfxPoolConfig
    {
        public string Key;
        public GameObject Prefab;
        public int InitialSize = 10;
    }
    
    [Header("VFX Pool Configuration")]
    [SerializeField] private List<VfxPoolConfig> _vfxPoolsConfig;
    
    private Dictionary<string, IObjectPool<PooledParticleSystem>> _vfxPools;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
            ServiceLocator.Register<IVFXService>(this);

            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Subscribe<HitboxImpactEvent>(OnHitboxImpact);
            eventBus?.Subscribe<PlayVFXRequest>(OnPlayVFXRequest);
        }
    }

    private void InitializePools()
    {
        _vfxPools = new Dictionary<string, IObjectPool<PooledParticleSystem>>();
        foreach (var config in _vfxPoolsConfig)
        {
            if (config.Prefab == null)
            {
                GameLog.LogWarning($"VFXManager: Prefab for key '{config.Key}' is not assigned.");
                continue;
            }

            if (config.Prefab.GetComponent<PooledParticleSystem>() == null)
            {
                GameLog.LogError($"VFXManager: Prefab for key '{config.Key}' is missing the 'PooledParticleSystem' component.");
                continue;
            }

            var pool = new ObjectPool<PooledParticleSystem>(
                () => CreatePooledVFX(config.Prefab, config.Key),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPooledVFX,
                true,
                config.InitialSize
            );
            _vfxPools[config.Key] = pool;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            var registered = ServiceLocator.Get<IVFXService>();
            if ((UnityEngine.Object)registered == (UnityEngine.Object)this)
                ServiceLocator.Unregister<IVFXService>();

            var eventBus = ServiceLocator.Get<IEventBus>();
            if (eventBus != null)
            {
                eventBus.Unsubscribe<HitboxImpactEvent>(OnHitboxImpact);
                eventBus.Unsubscribe<PlayVFXRequest>(OnPlayVFXRequest);
            }
            Instance = null;
        }
    }

    public GameObject PlayEffect(string key, Vector3 position, Quaternion? rotation = null)
    {
        if (!_vfxPools.TryGetValue(key, out var pool))
        {
            GameLog.LogWarning($"VFXManager: No pool found for key '{key}'.");
            return null;
        }
        
        var vfxInstance = pool.Get();
        if (vfxInstance == null) return null; // Should not happen with a properly configured pool

        vfxInstance.transform.position = position;
        vfxInstance.transform.rotation = rotation ?? Quaternion.identity;
        
        return vfxInstance.gameObject;
    }

    #region Pool Management Methods

    private PooledParticleSystem CreatePooledVFX(GameObject prefab, string key)
    {
        var go = Instantiate(prefab, transform);
        var pooledVfx = go.GetComponent<PooledParticleSystem>();
        pooledVfx.Pool = _vfxPools[key];
        return pooledVfx;
    }

    private void OnGetFromPool(PooledParticleSystem vfx)
    {
        vfx.gameObject.SetActive(true);
    }

    private void OnReleaseToPool(PooledParticleSystem vfx)
    {
        vfx.gameObject.SetActive(false);
    }

    private void OnDestroyPooledVFX(PooledParticleSystem vfx)
    {
        Destroy(vfx.gameObject);
    }
    #endregion

    private void OnPlayVFXRequest(PlayVFXRequest request)
    {
        PlayEffect(request.Key, request.Position, request.Rotation);
    }

    private void OnHitboxImpact(HitboxImpactEvent evt)
    {
        if (!string.IsNullOrEmpty(evt.ImpactVfxKey))
        {
            PlayEffect(evt.ImpactVfxKey, evt.ImpactPoint);
        }
    }

    public void PlayFadeAndDestroyEffect(GameObject targetObject, float duration)
    {
        StartCoroutine(FadeRoutine(targetObject, duration));
    }

    private IEnumerator FadeRoutine(GameObject targetObject, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        if (targetObject != null)
        {
            // This event could be used by other systems to know when the VFX is done.
            ServiceLocator.Get<IEventBus>()?.Publish(new VFXCompletedEvent(targetObject));
            
            // If the object has a PooledParticleSystem component, its OnEnable coroutine will handle
            // returning it to the pool. If not, we just deactivate it.
            targetObject.SetActive(false);
        }
    }
}