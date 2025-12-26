using System.Linq;
using Santa.Core;
using Santa.Domain.Combat;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// The default implementation for checking win/loss conditions.
    /// - Victory: All enemies are defeated.
    /// - Defeat: The player is defeated.
    /// </summary>
    public class DefaultWinConditionChecker : IWinConditionChecker
    {
        public CombatResult Check(CombatState combatState)
        {
            // Check for defeat first
            if (combatState.HealthComponents.TryGetValue(combatState.Player, out var playerHealth) && playerHealth.CurrentValue <= 0)
            {
                return CombatResult.Defeat;
            }

            // Check for victory
            bool allEnemiesDefeated = combatState.Enemies.All(enemy => enemy == null || !enemy.activeInHierarchy);

            if (allEnemiesDefeated)
            {
                return CombatResult.Victory;
            }

            // If neither condition is met, the combat is ongoing
            return CombatResult.Ongoing;
        }
    }
}
