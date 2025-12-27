using Santa.Domain.Combat;

namespace Santa.Core
{
    /// <summary>
    /// Represents the outcome of a combat check.
    /// </summary>
    public enum CombatResult
    {
        Ongoing,
        Victory,
        Defeat
    }

    /// <summary>
    /// Defines a contract for a service that checks for win/loss conditions in combat.
    /// </summary>
    public interface IWinConditionChecker
    {
        /// <summary>
        /// Checks the current state of combat to determine if an end condition has been met.
        /// </summary>
        /// <param name="combatState">The current state of the combat.</param>
        /// <returns>The result of the check.</returns>
        CombatResult Check(CombatState combatState);
    }
}
