using UnityEngine;

/// <summary>
/// A data container for a specific combat encounter.
/// This component holds references to the scene and camera for a combat encounter.
/// </summary>
public class CombatEncounter : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("The parent GameObject that contains the entire pre-staged combat scene. This should have a CombatArena component.")]
    [SerializeField] private GameObject combatSceneParent;

    [Tooltip("The virtual camera that will be used for this combat encounter.")]
    [SerializeField] private GameObject combatCamera;

    public GameObject CombatSceneParent => combatSceneParent;
    public GameObject CombatCamera => combatCamera;
}