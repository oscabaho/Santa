using System.Collections.Generic;
using UnityEngine;

namespace Santa.Core
{
    /// <summary>
    /// Interface for executing combat actions.
    /// Handles the resolution and execution of abilities during the execution phase.
    /// </summary>
    public interface IActionExecutor
    {
        /// <summary>
        /// Executes a single combat action with all its effects.
        /// Resolves targets, validates caster state, and applies ability effects.
        /// </summary>
        /// <param name="action">The pending action to execute.</param>
        /// <param name="allCombatants">All combatants in the encounter for target resolution.</param>
        /// <param name="healthCache">Cached health controllers for performance optimization.</param>
        /// <param name="upgradeService">Service providing player stat modifiers from upgrades.</param>
        void Execute(
            PendingAction action, 
            IReadOnlyList<GameObject> allCombatants, 
            IReadOnlyDictionary<GameObject, IHealthController> healthCache, 
            IUpgradeService upgradeService);
    }
}
