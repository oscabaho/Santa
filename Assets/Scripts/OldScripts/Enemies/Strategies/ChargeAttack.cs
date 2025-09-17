using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ChargeAttack", menuName = "ProyectSecret/Enemy Attacks/Charge Attack")]
public class ChargeAttack : AttackStrategy
{
    [Header("Configuración del Ataque")]
    [SerializeField] private float chargeTime = 2f;
    [SerializeField] private float attackSpeed = 10f;
    [SerializeField] private float attackDuration = 1.5f;
    [SerializeField] private int damage = 20;

    public override Coroutine Execute(EnemyAttackController controller, Transform player)
    {
        return controller.StartCoroutine(AttackRoutine(controller, player));
    }

    private IEnumerator AttackRoutine(EnemyAttackController controller, Transform player)
    {
        Transform self = controller.transform;
        float timer = 0f;
        Vector3 attackDirection = Vector3.zero;
        var damager = controller.GetComponent<ChargeAttackDamager>();

        // Fase de carga: el enemigo apunta al jugador.
        while (timer < chargeTime)
        {
            if (player != null)
            {
                Vector3 targetPosition = player.position;
                attackDirection = (targetPosition - self.position).normalized;
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // Activar el trigger de daño
        damager?.StartCharge(damage);

        // Fase de ataque: el enemigo se mueve en la dirección fijada.
        float attackTimer = 0f;
        while (attackTimer < attackDuration)
        {
            self.position += attackDirection * attackSpeed * Time.deltaTime;
            attackTimer += Time.deltaTime;
            yield return null;
        }

        // Desactivar el trigger de daño
        damager?.EndCharge();
    }
}
