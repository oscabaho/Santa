using System.Collections.Generic;
using UnityEngine;
using Santa.Core.Decor;

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

    [Header("Transition Settings")]
    [Tooltip("The world position where the liberation wave originates and the camera focuses.")]
    public Vector3 transitionCenter;
    [Tooltip("The radius of the area to be liberated (how far the wave goes).")]
    public float transitionRadius = 50f;

    [Header("Visual State GameObjects")]
    [Tooltip("All visuals for this level, including static and dynamic decorations.")]
    public List<DecorSO> visuals;
}