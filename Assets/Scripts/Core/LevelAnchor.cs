using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Component to mark the anchor point for a level's visuals in the scene hierarchy.
    /// LevelManager will use this transform as the parent for instantiating visuals.
    /// </summary>
    public class LevelAnchor : MonoBehaviour
    {
        [Tooltip("The name of the level this anchor corresponds to.")]
        public string levelName;
    }
}