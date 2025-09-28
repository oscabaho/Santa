using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents a combat area, automatically detecting all combatants within it.
/// This component should be on the root GameObject of a pre-staged combat scene.
/// </summary>
public class CombatArena : MonoBehaviour
{
    private List<GameObject> _combatants;

    /// <summary>
    /// A list of all combat-ready entities (player and enemies) found within this arena.
    /// It lazy-loads the list on first access.
    /// </summary>
    public List<GameObject> Combatants
    {
        get
        {
            if (_combatants == null)
            {
                _combatants = new List<GameObject>();
                // Find all components of type HealthComponentBehaviour in children, which defines a combatant
                var combatantsInChildren = GetComponentsInChildren<HealthComponentBehaviour>(true); // include inactive children

                foreach (var combatantHealth in combatantsInChildren)
                {
                    _combatants.Add(combatantHealth.gameObject);
                }
                Debug.Log($"CombatArena lazy-loaded {_combatants.Count} combatants.");
            }
            return _combatants;
        }
    }
}