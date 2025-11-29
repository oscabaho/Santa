using System.Collections.Generic;
using UnityEngine;

namespace Santa.Core.Save
{
    /// <summary>
    /// Centralized registry for ISaveContributor components.
    /// Replaces expensive FindObjectsByType calls with O(1) registration.
    /// </summary>
    public class SaveContributorRegistry : MonoBehaviour, ISaveContributorRegistry
    {
        private readonly HashSet<ISaveContributor> _contributors = new();
        private readonly List<ISaveContributor> _validContributors = new();
        private bool _isDirty = true;

        /// <summary>
        /// Registers a save contributor. Should be called in OnEnable.
        /// </summary>
        public void Register(ISaveContributor contributor)
        {
            if (contributor == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("Attempted to register null ISaveContributor.");
#endif
                return;
            }

            if (_contributors.Add(contributor))
            {
                _isDirty = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"SaveContributorRegistry: Registered {contributor.GetType().Name}");
#endif
            }
        }

        /// <summary>
        /// Unregisters a save contributor. Should be called in OnDisable.
        /// </summary>
        public void Unregister(ISaveContributor contributor)
        {
            if (contributor == null) return;

            if (_contributors.Remove(contributor))
            {
                _isDirty = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogVerbose($"SaveContributorRegistry: Unregistered {contributor.GetType().Name}");
#endif
            }
        }

        /// <summary>
        /// Gets all valid (non-destroyed) contributors.
        /// Results are cached until next Register/Unregister.
        /// </summary>
        public IReadOnlyList<ISaveContributor> GetValidContributors()
        {
            if (_isDirty)
            {
                RefreshValidContributors();
                _isDirty = false;
            }

            return _validContributors;
        }

        private void RefreshValidContributors()
        {
            _validContributors.Clear();

            // Remove destroyed contributors and build valid list
            _contributors.RemoveWhere(c =>
            {
                if (c is MonoBehaviour mb)
                {
                    return mb == null; // Unity null check for destroyed objects
                }
                return c == null;
            });

            _validContributors.AddRange(_contributors);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.LogVerbose($"SaveContributorRegistry: Refreshed. {_validContributors.Count} valid contributors.");
#endif
        }

        private void OnDestroy()
        {
            _contributors.Clear();
            _validContributors.Clear();
        }
    }
}
