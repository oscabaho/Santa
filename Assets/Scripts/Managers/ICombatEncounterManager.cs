using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Manages the lifecycle of a combat encounter, including pooling, initialization, and cleanup.
/// </summary>
public interface ICombatEncounterManager
{
    /// <summary>
    /// Starts a combat encounter and returns the result when it finishes.
    /// </summary>
    /// <param name="encounter">The encounter configuration to start.</param>
    /// <returns>True if the player won, false otherwise.</returns>
    Task<bool> StartEncounterAsync(CombatEncounter encounter);
}
