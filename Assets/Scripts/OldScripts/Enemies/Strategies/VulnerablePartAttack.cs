using System.Collections;
using UnityEngine;

namespace ProyectSecret.Enemies.Strategies
{
    [CreateAssetMenu(fileName = "VulnerablePartAttack", menuName = "ProyectSecret/Enemy Attacks/Vulnerable Part Attack")]
    public class VulnerablePartAttack : AttackStrategy
    {
        // Esta estrategia no necesita parámetros configurables,
        // ya que la lógica es simplemente "instanciar la parte vulnerable".
        // El prefab y el punto de spawn los gestionará el EnemyAttackController.

        public override Coroutine Execute(EnemyAttackController controller, Transform player)
        {
            return controller.StartCoroutine(AttackRoutine(controller));
        }

        private IEnumerator AttackRoutine(EnemyAttackController controller)
        {
            if (controller.VulnerableSpawnPoint != null)
            {
                // La estrategia le pide al controlador una parte vulnerable del pool.
                // El controlador gestiona el pool y la posiciona.
                var partObject = controller.GetVulnerablePartFromPool();
                if (partObject != null && partObject.TryGetComponent<VulnerablePartController>(out var vulnerableController))
                {
                    vulnerableController.Initialize(controller.HealthController, controller.VulnerablePartPool);
                }
            }
            yield break; // La corrutina termina inmediatamente después de instanciar.
        }
    }
}