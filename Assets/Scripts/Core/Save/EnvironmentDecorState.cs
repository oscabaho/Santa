using System.Collections.Generic;
using UnityEngine;

namespace Santa.Core.Save
{
    // Records environment decoration changes (e.g., liberated visuals) and reapplies them on load
    public class EnvironmentDecorState : MonoBehaviour, ISaveContributor
    {
        private readonly HashSet<string> _appliedChangeIds = new HashSet<string>();

        public void ApplyChange(string changeId)
        {
            if (string.IsNullOrEmpty(changeId)) return;
            if (_appliedChangeIds.Add(changeId))
            {
                PerformChange(changeId);
            }
        }

        public void WriteTo(ref SaveData data)
        {
            // Avoid List allocation: copy HashSet directly to array
            if (_appliedChangeIds.Count == 0)
            {
                data.environmentChangeIds = System.Array.Empty<string>();
            }
            else
            {
                data.environmentChangeIds = new string[_appliedChangeIds.Count];
                _appliedChangeIds.CopyTo(data.environmentChangeIds);
            }
        }

        public void ReadFrom(in SaveData data)
        {
            _appliedChangeIds.Clear();
            if (data.environmentChangeIds == null) return;
            foreach (var id in data.environmentChangeIds)
            {
                _appliedChangeIds.Add(id);
                PerformChange(id);
            }
        }

        private void PerformChange(string id)
        {
            // Example: find decor objects with matching ID and enable them
            var decorObjects = FindObjectsByType<DecorMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var d in decorObjects)
            {
                if (d != null && d.ChangeId == id && d.gameObject.scene.IsValid())
                {
                    d.Apply();
                }
            }
        }
    }

    // Helper component to mark decor objects with change IDs
    public class DecorMarker : MonoBehaviour
    {
        public string ChangeId;
        public void Apply()
        {
            gameObject.SetActive(true);
        }
    }
}
