using UnityEngine;
using System.Collections;
using UnityEngine.Pool;

public class VulnerablePartController : MonoBehaviour, IDamageable
{
    [SerializeField] private float vulnerableTime = 3f;

    private bool isVulnerable = true;
    private EnemyHealthController enemyHealth;
    private IObjectPool<VulnerablePartController> pool;

    /// <summary>
    /// Inicializa la parte vulnerable con una referencia a la salud del enemigo.
    /// Debe ser llamado por quien lo instancia.
    /// </summary>
    public void Initialize(EnemyHealthController healthController, IObjectPool<VulnerablePartController> objectPool)
    {
        enemyHealth = healthController;
        pool = objectPool;
    }

    private void Start()
    {
        // Usamos una corutina para gestionar el ciclo de vida.
        StartCoroutine(LifecycleRoutine());
    }

    private IEnumerator LifecycleRoutine()
    {
        // Esperar el tiempo de vulnerabilidad.
        yield return new WaitForSeconds(vulnerableTime);

        // Desactivar la vulnerabilidad y destruir el objeto.
        isVulnerable = false;
        // En lugar de destruir, lo devolvemos al pool.
        pool?.Release(this);
    }

    public void TakeDamage(int amount)
    {
        if (isVulnerable && enemyHealth != null)
        {
            enemyHealth.TakeDamage(amount);
        }
    }
}
