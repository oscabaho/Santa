using UnityEngine;

/// <summary>
/// Controlador de salud y muerte para enemigos únicos. Hereda de HealthControllerBase.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyHealthController : HealthControllerBase
{
    [Header("Fade Out Config")]
    [SerializeField] private float fadeDuration = 2f;

    protected override void Death()
    {
        // Desactivar el collider para evitar más interacciones mientras muere.
        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;

        // El evento CharacterDeathEvent ya ha sido publicado por la clase base.
        // Ahora delegamos el efecto visual de la muerte al VFXManager.
        VFXManager.Instance?.PlayFadeAndDestroyEffect(gameObject, fadeDuration);
    }
}
