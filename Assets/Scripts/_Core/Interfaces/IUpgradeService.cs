using System.Collections.Generic;
using AbilityUpgrade = Santa.Domain.Combat.AbilityUpgrade;

namespace Santa.Core
{
    /// <summary>
    /// Interface for managing ability upgrades and player combat statistics.
    /// Handles presenting upgrade choices to the player and applying selected upgrades.
    /// Also exposes current player stats that abilities can query during combat.
    /// </summary>
    public interface IUpgradeService
    {
        /// <summary>
        /// Presents upgrade options to the player through the upgrade UI.
        /// Typically called after winning a combat encounter.
        /// </summary>
        void PresentUpgradeOptions();
        
        /// <summary>
        /// Applies the selected upgrade to the player's stats or abilities.
        /// </summary>
        /// <param name="upgrade">The ability upgrade to apply.</param>
        void ApplyUpgrade(AbilityUpgrade upgrade);

        /// <summary>
        /// Gets the current direct attack damage value.
        /// </summary>
        int DirectAttackDamage { get; }
        
        /// <summary>
        /// Gets the current area attack damage value.
        /// </summary>
        int AreaAttackDamage { get; }
        
        /// <summary>
        /// Gets the current special attack damage value.
        /// </summary>
        int SpecialAttackDamage { get; }
        
        /// <summary>
        /// Gets the chance (0.0 to 1.0) that a special attack will miss.
        /// </summary>
        float SpecialAttackMissChance { get; }
        
        /// <summary>
        /// Gets the amount of action points recovered per turn.
        /// </summary>
        int APRecoveryAmount { get; }
        
        /// <summary>
        /// Gets the maximum action points the player can have.
        /// </summary>
        int MaxActionPoints { get; }
        
        /// <summary>
        /// Gets the maximum health the player can have.
        /// </summary>
        int MaxHealth { get; }
        
        /// <summary>
        /// Gets the global reduction in AP cost for all abilities.
        /// </summary>
        int GlobalAPCostReduction { get; }
        
        /// <summary>
        /// Gets the global bonus to action speed for all abilities.
        /// </summary>
        int GlobalActionSpeedBonus { get; }
        
        /// <summary>
        /// Gets the chance (0.0 to 1.0) that an attack will be a critical hit.
        /// </summary>
        float CriticalHitChance { get; }
    }
}
