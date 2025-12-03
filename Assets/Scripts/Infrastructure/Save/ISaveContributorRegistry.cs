using System.Collections.Generic;

namespace Santa.Core.Save
{
    /// <summary>
    /// Interface for centralized save contributor registry.
    /// Replaces expensive scene scanning with explicit registration.
    /// </summary>
    public interface ISaveContributorRegistry
    {
        /// <summary>
        /// Registers a contributor to participate in save/load operations.
        /// </summary>
        void Register(ISaveContributor contributor);

        /// <summary>
        /// Unregisters a contributor.
        /// </summary>
        void Unregister(ISaveContributor contributor);

        /// <summary>
        /// Gets all currently valid (non-destroyed) contributors.
        /// </summary>
        IReadOnlyList<ISaveContributor> GetValidContributors();
    }
}
