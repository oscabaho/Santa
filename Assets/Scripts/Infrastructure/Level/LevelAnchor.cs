using UnityEngine;

namespace Santa.Infrastructure.Level
{
    /// <summary>
    /// Component to mark the anchor point for a level's visuals in the scene hierarchy.
    /// LevelManager will use this transform as the parent for instantiating visuals.
    /// </summary>
    public class LevelAnchor : MonoBehaviour
    {
        [Tooltip("The Level Data asset this anchor corresponds to.")]
        public Santa.Core.LevelData levelData;
    }
}