using System.Collections.Generic;
using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for AI brain components that make combat decisions.
    /// Used by both enemy and ally AI to choose actions during the selection phase.
    /// </summary>
    public interface IBrain
    {
        /// <summary>
        /// Chooses an action for this AI combatant based on the current combat state.
        /// </summary>
        /// <param name="playerAction">The action the player has chosen (if any), useful for reactive AI.</param>
        /// <param name="allEnemies">List of all enemy combatants from this AI's perspective.</param>
        /// <param name="allAllies">List of all ally combatants from this AI's perspective.</param>
        /// <returns>A pending action representing the AI's chosen ability and target.</returns>
        PendingAction ChooseAction(
            PendingAction? playerAction,
            List<GameObject> allEnemies,
            List<GameObject> allAllies);
    }
}
