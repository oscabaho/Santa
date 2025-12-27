using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for managing transitions between exploration and combat states.
    /// Handles the setup and teardown of combat encounters.
    /// </summary>
    public interface ICombatTransitionService
    {
        /// <summary>
        /// Initiates combat by setting up the combat scene and transitioning game state.
        /// </summary>
        /// <param name="combatSceneParent">The GameObject parent that contains the combat scene visuals.</param>
        void StartCombat(GameObject combatSceneParent);
        
        /// <summary>
        /// Ends the current combat encounter and transitions back to exploration.
        /// </summary>
        /// <param name="playerWon">True if the player won the combat, false if defeated.</param>
        void EndCombat(bool playerWon);
    }
}