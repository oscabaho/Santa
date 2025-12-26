using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject that defines the data for a specific level or area.
/// It holds references to visual elements for different states.
/// </summary>
[CreateAssetMenu(fileName = "New LevelData", menuName = "Santa/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Information")]
    [Tooltip("The name of the level or area.")]
    public string levelName;

    [Header("Visual State GameObjects")]
    [Tooltip("Visuals to be active when the area is in its initial, 'gentrified' state (e.g., North American Christmas).")]
    public List<GameObject> gentrifiedVisuals;

    [Tooltip("Visuals to be active when the area is 'liberated' (e.g., Colombian Christmas).")]
    public List<GameObject> liberatedVisuals;
}