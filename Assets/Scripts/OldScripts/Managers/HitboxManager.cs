using UnityEngine;
using UnityEngine.Pool;
using ProyectSecret.Combat.Behaviours;
using System.Collections.Generic;

namespace ProyectSecret.Managers
{
    public class HitboxManager : MonoBehaviour // TODO: Convertir a un sistema de pooling unificado como VFXManager
    {
        public static HitboxManager Instance { get; private set; }

        // Usamos el prefab del GameObject como clave para el pool.
        private readonly Dictionary<GameObject, IObjectPool<WeaponHitbox>> _hitboxPools = new Dictionary<GameObject, IObjectPool<WeaponHitbox>>();

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
            }
        }

        /// <summary>
        /// Obtiene una instancia de WeaponHitbox desde el pool correspondiente a un prefab.
        /// El objeto devuelto estará inactivo. Quien lo solicita es responsable de activarlo.
        /// </summary>
        /// <param name="hitboxPrefab">El prefab de la hitbox que se quiere obtener.</param>
        /// <returns>Un componente WeaponHitbox o null si el pool está agotado o el prefab es inválido.</returns>
        public WeaponHitbox Get(GameObject hitboxPrefab)
        {
            if (hitboxPrefab == null)
            {
                Debug.LogError("HitboxManager: Se intentó obtener una hitbox con un prefab nulo.");
                return null;
            }

            // Usamos TryGetValue para ser más eficientes y seguros.
            if (!_hitboxPools.TryGetValue(hitboxPrefab, out var pool))
            {
                // Si no existe un pool para este prefab, lo creamos sobre la marcha.
                // Validamos que el prefab tenga el componente necesario.
                if (hitboxPrefab.GetComponent<WeaponHitbox>() == null)
                {
                    Debug.LogError($"HitboxManager: El prefab '{hitboxPrefab.name}' no tiene el componente 'WeaponHitbox'. No se puede crear un pool.");
                    return null;
                }
                pool = new ObjectPool<WeaponHitbox>(
                    () => CreateHitbox(hitboxPrefab),
                    OnGetFromPool,
                    OnReleaseToPool,
                    OnDestroyPooledObject,
                    true, 5
                );
                _hitboxPools[hitboxPrefab] = pool;
            }

            var hitbox = pool.Get();
            if (hitbox == null)
            {
                // El warning de pool agotado ya se muestra en la clase ObjectPool.
                return null;
            }
            
            // Asignamos el pool al que pertenece para que pueda devolverse a sí mismo.
            hitbox.Pool = pool;
            
            return hitbox;
        }

        #region Pool Management Methods
        private WeaponHitbox CreateHitbox(GameObject prefab)
        {
            return Instantiate(prefab, transform).GetComponent<WeaponHitbox>();
        }
        private void OnGetFromPool(WeaponHitbox hitbox)
        {
            hitbox.gameObject.SetActive(true);
        }
        private void OnReleaseToPool(WeaponHitbox hitbox)
        {
            hitbox.gameObject.SetActive(false);
        }
        private void OnDestroyPooledObject(WeaponHitbox hitbox)
        {
            Destroy(hitbox.gameObject);
        }
        #endregion
    }
}
