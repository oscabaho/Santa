using UnityEngine;

namespace Santa.Core.Save
{
    // Provides a stable unique ID for scene objects (enemies, decor, etc.)
    public class UniqueIdProvider : MonoBehaviour, IUniqueIdProvider
    {
        [SerializeField] private string uniqueId;

        public string UniqueId => uniqueId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(uniqueId))
            {
                uniqueId = System.Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif
    }
}
