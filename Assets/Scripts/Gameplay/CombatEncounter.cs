using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A data container for a specific combat encounter.
/// This component holds all the necessary information to start a battle.
/// </summary>
public class CombatEncounter : MonoBehaviour
{
    [Header("Scene Setup")]
    [Tooltip("The parent GameObject that contains the entire pre-staged combat scene.")]
    [SerializeField] private GameObject combatSceneParent;

    [Tooltip("The camera that will be used for this combat encounter.")]
    [SerializeField] private Camera combatCamera;

    [Header("Combatants")]
    [Tooltip("The list of all participants in this battle (player and enemies) that exist within the combat scene.")]
    [SerializeField] private List<GameObject> combatParticipants;

    public GameObject CombatSceneParent => combatSceneParent;
    public Camera CombatCamera => combatCamera;
    public List<GameObject> CombatParticipants => combatParticipants;
}
