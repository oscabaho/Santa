using System.Collections;
using UnityEngine;

/// <summary>
/// Gestiona la lógica de la stamina del jugador, incluyendo su recuperación.
/// Su única responsabilidad es manejar la stamina.
/// </summary>
[RequireComponent(typeof(StaminaComponentBehaviour))]
public class PlayerStaminaController : MonoBehaviour
{
    private StaminaComponentBehaviour _staminaBehaviour;
    public StaminaComponent Stamina => _staminaBehaviour != null ? _staminaBehaviour.Stamina : null;

    [Header("Configuración")]
    [SerializeField] private StaminaConfig staminaConfig;
    private Coroutine _staminaRecoveryCoroutine;

    private void Awake()
    {
        _staminaBehaviour = GetComponent<StaminaComponentBehaviour>();
        if (_staminaBehaviour == null)
            Debug.LogError("PlayerStaminaController: No se encontró StaminaComponentBehaviour.");

        if (staminaConfig == null)
        {
            Debug.LogError("PlayerStaminaController: StaminaConfig no está asignado en el Inspector.", this);
            this.enabled = false; // Desactivar si no hay config
        }
    }

    private void OnEnable()
    {
        // Suscribirse a un evento genérico de acción del jugador que consume stamina
        GameEventBus.Instance.Subscribe<PlayerActionUsedStaminaEvent>(HandlePlayerAction);
    }

    private void OnDisable()
    {
        if (GameEventBus.Instance != null)
            GameEventBus.Instance.Unsubscribe<PlayerActionUsedStaminaEvent>(HandlePlayerAction);
    }

    // Este método se llama cuando cualquier acción (ataque, esquivar, etc.) consume stamina
    private void HandlePlayerAction(PlayerActionUsedStaminaEvent evt)
    {
        if (_staminaRecoveryCoroutine != null)
        {
            StopCoroutine(_staminaRecoveryCoroutine);
        }
        _staminaRecoveryCoroutine = StartCoroutine(StaminaRecoveryRoutine());
    }

    private IEnumerator StaminaRecoveryRoutine()
    {
        yield return new WaitForSeconds(staminaConfig.recoveryDelay);
        
        while (Stamina != null && Stamina.CurrentValue < Stamina.MaxValue)
        {
            Stamina.AffectValue(staminaConfig.recoveryAmount);
            yield return new WaitForSeconds(staminaConfig.recoveryInterval);
        }
        _staminaRecoveryCoroutine = null;
    }
}
