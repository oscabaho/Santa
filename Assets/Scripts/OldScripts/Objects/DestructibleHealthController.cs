using UnityEngine;

/// <summary>
/// Controlador de salud y muerte para objetos destruibles o NPCs simples. Hereda de HealthControllerBase.
/// </summary>
[RequireComponent(typeof(Collider))]
public class DestructibleHealthController : HealthControllerBase
{
    protected override void Death()
    {
        // Aquí puedes agregar lógica personalizada para destrucción de objetos
        Destroy(gameObject);
    }
}

