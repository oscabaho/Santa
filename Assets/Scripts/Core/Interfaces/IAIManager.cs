using System.Collections.Generic;
using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for AI action planning service.
    /// Handles the logic for non-player combatants to choose their actions during the selection phase.
    /// </summary>
    public interface IAIManager
    {
        /// <summary>
        /// Plans actions for all non-player combatants during the selection phase.
        /// Each AI will use their brain to choose an action based on the current state.
        /// </summary>
        /// <param name="allCombatants">All combatants in the current encounter.</param>
        /// <param name="player">The player GameObject reference.</param>
        /// <param name="brainCache">Cache of AI brain components for each combatant.</param>
        /// <param name="apCache">Cache of action point controllers for each combatant.</param>
        /// <param name="pendingActions">List to populate with planned actions.</param>
        /// <param name="playerAction">The action the player has chosen (if any), used for AI decision-making.</param>
        void PlanActions(
            IReadOnlyList<GameObject> allCombatants,
            GameObject player,
            IReadOnlyDictionary<GameObject, IBrain> brainCache,
            IReadOnlyDictionary<GameObject, IActionPointController> apCache,
            List<PendingAction> pendingActions,
            PendingAction? playerAction);
    }
}
