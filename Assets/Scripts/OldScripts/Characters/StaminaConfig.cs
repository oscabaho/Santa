using UnityEngine;

[CreateAssetMenu(fileName = "StaminaConfig", menuName = "ProyectSecret/Character/Stamina Config")]
public class StaminaConfig : ScriptableObject
{
    [Header("Stamina Recovery")]
    [Tooltip("Cantidad de stamina que se recupera en cada tick.")]
    public int recoveryAmount = 5;
    [Tooltip("Intervalo en segundos entre cada tick de recuperación.")]
    public float recoveryInterval = 0.5f;
    [Tooltip("Retraso en segundos antes de que comience la recuperación después de usar stamina.")]
    public float recoveryDelay = 2f;
}
