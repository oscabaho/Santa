using Cysharp.Threading.Tasks;

namespace Santa.Core
{
    /// <summary>
    /// Interface for managing combat encounters and transitions between exploration and combat.
    /// </summary>
    public interface ICombatEncounterManager
    {
        /// <summary>
        /// Starts a combat encounter asynchronously.
        /// </summary>
        /// <param name="encounter">The combat encounter configuration to start.</param>
        /// <returns>A task that completes with true if the player won the combat, false if defeated.</returns>
        UniTask<bool> StartEncounterAsync(ICombatEncounter encounter);
    }
}
