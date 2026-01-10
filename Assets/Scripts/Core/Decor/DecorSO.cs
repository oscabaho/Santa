using UnityEngine;

namespace Santa.Core.Decor
{
    /// <summary>
    /// ScriptableObject for dynamic decorations that can be instantiated in specific areas.
    /// </summary>
    [CreateAssetMenu(fileName = "New Decor", menuName = "Santa/Decor")]
    public class DecorSO : ScriptableObject
    {
        [Header("Decoration Data")]
        [Tooltip("The prefab to instantiate.")]
        [SerializeField] private GameObject prefab;

        [Tooltip("The name of the area parent under level visuals (e.g., 'Area_01' for Level_01).")]
        [SerializeField] private string targetAreaName = "Area_01";

        // Properties for access
        public GameObject Prefab => prefab;
        public string TargetAreaName => targetAreaName;
    }
}