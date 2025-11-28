using UnityEngine;

namespace Santa.Core.Identifiers
{
    // Deprecated duplicate; use Santa.Core.Save.UniqueIdProvider instead.
    public class UniqueIdProviderDeprecated : MonoBehaviour
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
