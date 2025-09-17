using UnityEngine;

/// <summary>
/// Clase base abstracta para todas las estrategias de ataque de un enemigo.
/// Permite encapsular diferentes comportamientos de ataque en ScriptableObjects.
/// </summary>
public abstract class AttackStrategy : ScriptableObject
{
    public abstract Coroutine Execute(EnemyAttackController controller, Transform player);
}
