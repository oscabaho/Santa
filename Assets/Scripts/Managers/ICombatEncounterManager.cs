using Cysharp.Threading.Tasks;
using Santa.Infrastructure.Combat;

/// <summary>
/// Interface for managing combat encounters.
/// </summary>
public interface ICombatEncounterManager
{
    /// <summary>
    /// Starts a combat encounter asynchronously.
    /// </summary>
    /// <param name="encounter">The combat encounter to start.</param>
    /// <returns>True if the player won the combat, false otherwise.</returns>
    UniTask<bool> StartEncounterAsync(CombatEncounter encounter);
}
