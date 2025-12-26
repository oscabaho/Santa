using AbilityUpgrade = Santa.Domain.Combat.AbilityUpgrade;

namespace Santa.Core
{
    /// <summary>
    /// Interface for the upgrade selection UI component.
    /// Displays available upgrade choices to the player.
    /// </summary>
    public interface IUpgradeUI
    {
        /// <summary>
        /// Shows the upgrade selection UI with two upgrade options.
        /// </summary>
        /// <param name="upgrade1">The first upgrade option to display.</param>
        /// <param name="upgrade2">The second upgrade option to display.</param>
        void ShowUpgrades(AbilityUpgrade upgrade1, AbilityUpgrade upgrade2);
    }
}
