using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool; // <-- Añadir

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }
    
    [System.Serializable]
    public class VfxPoolConfig
    {
        public string Key;
        public GameObject Prefab;
        public int InitialSize = 10;
    }
    
    [Header("Configuración de Pools de VFX")]
    [SerializeField] private List<VfxPoolConfig> _vfxPoolsConfig;
    
    private Dictionary<string, IObjectPool<PooledParticleSystem>> _vfxPools; // <-- Cambiar tipo
    
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
            // Suscribirse al evento para reproducir VFX
            GameEventBus.Instance?.Subscribe<HitboxImpactEvent>(OnHitboxImpact);
            GameEventBus.Instance?.Subscribe<PlayVFXRequest>(OnPlayVFXRequest);
        }
    }

    private void InitializePools()
    {
        _vfxPools = new Dictionary<string, IObjectPool<PooledParticleSystem>>(); // <-- Cambiar tipo
        foreach (var config in _vfxPoolsConfig)
        {
            if (config.Prefab == null)
            {
                Debug.LogWarning($"VFXManager: El prefab para la clave '{config.Key}' no está asignado.");
                continue;
            }

            if (config.Prefab.GetComponent<PooledParticleSystem>() == null)
            {
                Debug.LogError($"VFXManager: El prefab para la clave '{config.Key}' no tiene el componente 'PooledParticleSystem'.");
                continue;
            }

            // Usar el ObjectPool de Unity
            var pool = new ObjectPool<PooledParticleSystem>(
                () => CreatePooledVFX(config.Prefab),
                OnGetFromPool,
                OnReleaseToPool,
                OnDestroyPooledVFX,
                true,
                config.InitialSize
            );
            _vfxPools[config.Key] = pool; // <-- Asignar el nuevo pool
        }
    }

    private void OnDestroy()
    {
        // Buena práctica: desuscribirse para evitar errores.
        if (Instance == this && GameEventBus.Instance != null)
        {
            GameEventBus.Instance.Unsubscribe<HitboxImpactEvent>(OnHitboxImpact);
            GameEventBus.Instance.Unsubscribe<PlayVFXRequest>(OnPlayVFXRequest);
        }
    }

    /// <summary>
    /// Obtiene un efecto del pool y lo activa en la posición y rotación deseadas.
    /// </summary>
    public GameObject PlayEffect(string key, Vector3 position, Quaternion? rotation = null)
    {
        if (!_vfxPools.ContainsKey(key))
        {
            Debug.LogWarning($"VFXManager: No se encontró un pool para la clave '{key}'.");
            return null;
        }
        
        var vfxInstance = _vfxPools[key].Get();
        if (vfxInstance == null) return null;
        var vfxObject = vfxInstance.gameObject;
        if (vfxObject == null) return null;

        if (vfxInstance == null)
        {
            Debug.LogError($"VFXManager: El objeto del pool para la clave '{key}' no tiene el componente 'PooledParticleSystem'. El prefab está mal configurado.");
            // Devolver el objeto al pool si es posible o simplemente desactivarlo.
            vfxObject.SetActive(false); 
            return null;
        }
        
        vfxInstance.transform.position = position;
        vfxInstance.transform.rotation = rotation ?? Quaternion.identity;
        
        return vfxObject;
    }

    #region Métodos de Gestión del Pool (Nuevos)

    private PooledParticleSystem CreatePooledVFX(GameObject prefab)
    {
        var go = Instantiate(prefab, transform);
        var pooledVfx = go.GetComponent<PooledParticleSystem>();
        // La clave del diccionario es necesaria para saber a qué pool devolverlo
        string key = _vfxPoolsConfig.Find(c => c.Prefab == prefab)?.Key;
        if (key != null)
        {
            pooledVfx.Pool = _vfxPools[key];
        }
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
        if (!string.IsNullOrEmpty(evt.WeaponData.ImpactVFXKey))
        {
            PlayEffect(evt.WeaponData.ImpactVFXKey, evt.ImpactPoint);
        }
    }

    public void PlayFadeAndDestroyEffect(GameObject targetObject, float duration)
    {
        // Esta lógica es más específica y podría ir en su propio componente,
        // pero por ahora la mantenemos aquí para simplicidad.
        StartCoroutine(FadeRoutine(targetObject, duration));
    }

    private IEnumerator FadeRoutine(GameObject targetObject, float duration)
    {
        // Aquí podrías añadir una lógica de fade más compleja si lo necesitas.
        // Por ahora, simplemente desactivamos el objeto después de la duración.
        yield return new WaitForSeconds(duration);
        
        // En lugar de destruir, lo desactivamos. Si es un objeto del pool,
        // su propio componente PooledParticleSystem lo devolverá.
        // Si no, simplemente se desactiva.
        if (targetObject != null)
        {
            GameEventBus.Instance.Publish(new VFXCompletedEvent(targetObject));
            targetObject.SetActive(false);
        }
    }
}
