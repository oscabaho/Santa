using System.Collections.Generic;
using UnityEngine;

namespace Santa.Domain.Combat
{
    /// <summary>
    /// Defines the category of targeting for an ability.
    /// This is primarily used by the UI and AI to understand the general nature of the targeting.
    /// </summary>
    public enum TargetingStyle
    {
        Self,
        SingleEnemy,
        SingleAlly,
        AllEnemies,
        AllAllies
    }

    /// <summary>
    /// Abstract base class for the Strategy Pattern, defining how an ability finds its targets.
    /// </summary>
    public abstract class TargetingStrategy : ScriptableObject
{
    /// <summary>
    /// Gets the high-level style of targeting.
    /// </summary>
    public abstract TargetingStyle Style { get; }

    /// <summary>
    /// Resolves the targets for a given action and populates the results list.
    /// </summary>
    /// <param name="caster">The combatant performing the action.</param>
    /// <param name="primaryTarget">The primary target selected by the caster (can be null).</param>
    /// <param name="allCombatants">A read-only list of all combatants in the battle.</param>
    /// <param name="results">The list to populate with the final targets.</param>
    public abstract void ResolveTargets(GameObject caster, GameObject primaryTarget, IReadOnlyList<GameObject> allCombatants, List<GameObject> results, Ability ability);
}
}
