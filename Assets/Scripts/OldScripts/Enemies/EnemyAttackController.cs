using System.Collections;
using UnityEngine;
using ProyectSecret.VFX;
using UnityEngine.Pool;
using ProyectSecret.Enemies.Strategies;
using ProyectSecret.Characters.Enemies;

namespace ProyectSecret.Enemies
{
    /// <summary>
    /// Gestiona los ataques de un enemigo utilizando el patrón Strategy.
    /// Delega la lógica de cada fase de ataque a un ScriptableObject de tipo AttackStrategy.
    /// También gestiona los recursos necesarios para los ataques (prefabs, pools).
    /// </summary>
    [RequireComponent(typeof(EnemyHealthController))]
    public class EnemyAttackController : MonoBehaviour
    {
        public enum AttackPhase { Phase1, Phase2, Phase3 }

        [Header("Estrategias de Ataque (ScriptableObjects)")]
        [SerializeField] private AttackStrategy _phase1Strategy;
        [SerializeField] private AttackStrategy _phase2Strategy;
        [SerializeField] private AttackStrategy _phase3Strategy;

        [Header("Recursos para los Ataques")]
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private GameObject shadowPrefab;
        [SerializeField] private GameObject vulnerablePartPrefab;
        [SerializeField] private Transform vulnerableSpawnPoint;

        [Header("Control del Ciclo de Ataque")]
        [Tooltip("Tiempo de espera en segundos entre el final de un ataque y el inicio del siguiente.")]
        [SerializeField] private float _timeBetweenPhases = 2.5f;

        [Header("Configuración de Pools")]
        [SerializeField] private int rockPoolSize = 20;
        [SerializeField] private int shadowPoolSize = 20;
        [SerializeField] private int vulnerablePartPoolSize = 5;

        // Propiedades públicas para que las estrategias accedan a los recursos.
        public IObjectPool<RockController> RockPool { get; private set; }
        public IObjectPool<ShadowController> ShadowPool { get; private set; }
        public EnemyHealthController HealthController { get; private set; }
        public IObjectPool<VulnerablePartController> VulnerablePartPool { get; private set; }
        public Transform VulnerableSpawnPoint => vulnerableSpawnPoint;

        private AttackStrategy _currentStrategy;
        private AttackPhase _currentPhase;
        private Coroutine _currentAttackCoroutine;

        private void Awake()
        {
            HealthController = GetComponent<EnemyHealthController>();

            // Inicializamos los pools de objetos que gestionará este controlador.
            if (rockPrefab != null) {
                RockPool = new ObjectPool<RockController>(
                    () => Instantiate(rockPrefab, transform).GetComponent<RockController>(),
                    OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, true, rockPoolSize
                );
            }

            if (shadowPrefab != null) {
                ShadowPool = new ObjectPool<ShadowController>(
                    () => Instantiate(shadowPrefab, transform).GetComponent<ShadowController>(),
                    OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, true, shadowPoolSize
                );
            }

            if (vulnerablePartPrefab != null) {
                VulnerablePartPool = new ObjectPool<VulnerablePartController>(
                    () => Instantiate(vulnerablePartPrefab, transform).GetComponent<VulnerablePartController>(),
                    OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject, true, vulnerablePartPoolSize
                );
            }


            // Establecemos la estrategia inicial.
            SetPhase(AttackPhase.Phase1);
        }

        private void Start()
        {
            // Al iniciar, comenzamos el ciclo de ataque automático.
            StartCoroutine(AttackCycleRoutine());
        }

        /// <summary>
        /// Cambia la estrategia de ataque actual del enemigo.
        /// </summary>
        public void SetPhase(AttackPhase phase)
        {
            _currentPhase = phase; // Guardamos la fase actual para el ciclo.

            switch (phase)
            {
                case AttackPhase.Phase1:
                    _currentStrategy = _phase1Strategy;
                    break;
                case AttackPhase.Phase2:
                    _currentStrategy = _phase2Strategy;
                    break;
                case AttackPhase.Phase3:
                    _currentStrategy = _phase3Strategy;
                    break;
            }
        }

        /// <summary>
        /// Inicia el ataque usando la estrategia actual.
        /// </summary>
        public void StartAttack(Transform player)
        {
            if (_currentStrategy == null)
            {
                Debug.LogError("No se ha asignado una estrategia de ataque.", this);
                return;
            }

            if (_currentAttackCoroutine != null)
            {
                StopCoroutine(_currentAttackCoroutine);
            }

            _currentAttackCoroutine = _currentStrategy.Execute(this, player);
        }

        /// <summary>
        /// Obtiene una parte vulnerable del pool y la posiciona.
        /// </summary>
        public GameObject GetVulnerablePartFromPool()
        {
            if (VulnerablePartPool == null || vulnerableSpawnPoint == null)
            {
                Debug.LogWarning("VulnerablePartPool o VulnerableSpawnPoint no están configurados en EnemyAttackController.", this);
                return null;
            }

            var partInstance = VulnerablePartPool.Get(); // Esto ahora devuelve el componente
            partInstance.transform.position = vulnerableSpawnPoint.position;
            partInstance.transform.rotation = vulnerableSpawnPoint.rotation;
            return partInstance.gameObject; // El pool se encarga de activarlo
        }

        /// <summary>
        /// Corrutina que gestiona el ciclo de ataque del enemigo.
        /// </summary>
        private IEnumerator AttackCycleRoutine()
        {
            // Esperamos un frame para asegurar que todo esté inicializado.
            yield return null;

            // Buscamos al jugador una sola vez al inicio del combate.
            var player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("Player no encontrado. El ciclo de ataque no puede comenzar.", this);
                yield break;
            }

            // El ciclo se repite mientras el enemigo esté vivo.
            while (HealthController.Health.CurrentValue > 0)
            {
                // Ejecutamos el ataque de la estrategia actual.
                StartAttack(player);

                // Esperamos a que la corrutina del ataque actual termine.
                if (_currentAttackCoroutine != null)
                    yield return _currentAttackCoroutine;

                // Esperamos un tiempo antes de pasar a la siguiente fase.
                yield return new WaitForSeconds(_timeBetweenPhases);

                // Pasamos a la siguiente fase en el ciclo.
                SetPhase((AttackPhase)(((int)_currentPhase + 1) % System.Enum.GetValues(typeof(AttackPhase)).Length));
            }
        }
        
        #region Pool Management Methods
        private void OnGetFromPool<T>(T obj) where T : MonoBehaviour
        {
            obj.gameObject.SetActive(true);
        }
        private void OnReleaseToPool<T>(T obj) where T : MonoBehaviour
        {
            obj.gameObject.SetActive(false);
        }
        private void OnDestroyPooledObject<T>(T obj) where T : MonoBehaviour
        {
            Destroy(obj.gameObject);
        }
        #endregion
    }
}
