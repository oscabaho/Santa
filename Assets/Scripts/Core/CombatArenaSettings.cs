using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Component to be placed on the root of a Combat Arena prefab.
/// Holds references to the specific cameras used in this arena.
/// </summary>
public class CombatArenaSettings : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    [Tooltip("The main camera used during the Selection and Execution phases.")]
    [SerializeField] private CinemachineCamera mainCombatCamera;

    [Tooltip("The camera used during the Targeting phase.")]
    [SerializeField] private CinemachineCamera targetSelectionCamera;

    public CinemachineCamera MainCombatCamera => mainCombatCamera;
    public CinemachineCamera TargetSelectionCamera => targetSelectionCamera;

    private void OnValidate()
    {
<<<<<<< Updated upstream
        if (mainCombatCamera == null) Debug.LogWarning($"{name}: MainCombatCamera is not assigned.", this);
        if (targetSelectionCamera == null) Debug.LogWarning($"{name}: TargetSelectionCamera is not assigned.", this);
=======
        if (mainCombatCamera == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"{name}: MainCombatCamera is not assigned.", this);
#endif
        }
        if (targetSelectionCamera == null)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogWarning($"{name}: TargetSelectionCamera is not assigned.", this);
#endif
        }
>>>>>>> Stashed changes
    }
}
