using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RockFallAttack", menuName = "ProyectSecret/Enemy Attacks/Rock Fall Attack")]
public class RockFallAttack : AttackStrategy
{
    [Header("Configuración del Patrón")]
    [SerializeField] private RockFallAttackConfig config;

    public override Coroutine Execute(EnemyAttackController controller, Transform player)
    {
        // El controlador inicia la corrutina, pasándose a sí mismo como contexto.
        return controller.StartCoroutine(AttackRoutine(controller, player));
    }

    private IEnumerator AttackRoutine(EnemyAttackController controller, Transform player)
    {
        if (config == null)
        {
            Debug.LogError("RockFallAttack: La configuración (RockFallAttackConfig) no está asignada.", this);
            yield break;
        }

        if (controller.RockPool == null)
        {
            Debug.LogError("RockFallAttack: RockPool no está inicializado en EnemyAttackController.", controller);
            yield break;
        }

        for (int g = 0; g < config.numberOfGroups; g++)
        {
            for (int r = 0; r < config.rocksPerGroup; r++)
            {
                Vector3 spawnCenter = player.position;
                Vector3 spawnPos = spawnCenter + Vector3.up * config.rockSpawnHeight + Random.insideUnitSphere * config.rockSpawnRadius;

                GameObject shadowInstance = null;
                if (controller.ShadowPool != null && Physics.Raycast(spawnPos, Vector3.down, out RaycastHit hit, 100f, config.groundLayer) && controller.ShadowPool.Get() is var shadowController && shadowController != null)
                {
                    shadowInstance = shadowController.gameObject;
                    shadowInstance.transform.position = hit.point + new Vector3(0, 0.01f, 0);
                    shadowInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                }

                var rock = controller.RockPool.Get();
                if (rock == null) continue;
                var rockGO = rock.gameObject;

                rockGO.transform.position = spawnPos;
                rockGO.transform.rotation = Quaternion.identity;
                rockGO.GetComponent<RockController>()?.Initialize(controller.RockPool, shadowInstance);
                rockGO.SetActive(true);

                if (config.spawnDelayWithinGroup > 0)
                    yield return new WaitForSeconds(config.spawnDelayWithinGroup);
            }
            if (g < config.numberOfGroups - 1)
                yield return new WaitForSeconds(config.intervalBetweenGroups);
        }
    }
}
