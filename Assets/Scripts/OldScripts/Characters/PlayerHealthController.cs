using UnityEngine;

/// <summary>
/// Controlador de salud y muerte para el jugador. Hereda de HealthControllerBase.
/// Su única responsabilidad es gestionar la salud y la muerte del jugador.
/// </summary>
[RequireComponent(typeof(HealthComponentBehaviour))]
// Ya no requiere StaminaComponentBehaviour, esa es responsabilidad de PlayerStaminaController
public class PlayerHealthController : HealthControllerBase
{
    // --- Toda la lógica de Stamina ha sido movida a PlayerStaminaController ---

    private void OnEnable()
    {
        GameEventBus.Instance.Subscribe<PlayerStateRestoreRequestEvent>(OnRestoreStateRequested);
    }

    private void OnDisable()
    {
        // Es buena práctica comprobar si la instancia del bus aún existe,
        // especialmente al cerrar la aplicación.
        if (GameEventBus.Instance != null)
            GameEventBus.Instance.Unsubscribe<PlayerStateRestoreRequestEvent>(OnRestoreStateRequested);
    }
    protected override void Death()
    {
        // La clase base ya se encarga de reproducir el sonido de muerte.
        // Aquí solo nos ocupamos de la lógica específica del jugador.
        
        // Notificar a otros sistemas que el jugador ha muerto, en lugar de destruirlo aquí.
        // El CombatSceneController se encargará de la lógica de derrota y de la destrucción del objeto.
        GameEventBus.Instance.Publish(new PlayerDiedEvent(gameObject));
        
        // Desactivamos el controlador para que no pueda recibir más daño.
        // La destrucción del objeto la manejará otro sistema.
        this.enabled = false;
    }
    private void OnRestoreStateRequested(PlayerStateRestoreRequestEvent evt)
    {
        // Asegurarse de que este evento es para esta instancia del jugador
        if (evt.PlayerObject != this.gameObject) return;

        // Restaurar la salud desde los datos persistentes
        // El método 'SetCurrentValue' no existe. Usaremos un método más genérico 'SetValue'.
        Health?.SetValue(evt.Data.playerHealth);
        // También podrías restaurar la salud máxima, etc.
    }
}
