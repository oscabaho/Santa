using System.Collections.Generic;
using UnityEngine;
using Santa.Core.Config;

namespace Santa.Infrastructure.Combat
{
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

                var enemies = new List<GameObject>();

                foreach (var combatantHealth in combatantsInChildren)
                {
                    _combatants.Add(combatantHealth.gameObject);

                    // Identify enemies: Not the player
                    // We assume the player has the "Player" tag or a CombatPlayerIdentifier component
                    if (!combatantHealth.CompareTag(GameConstants.Tags.Player) && combatantHealth.GetComponent<CombatPlayerIdentifier>() == null)
                    {
                        enemies.Add(combatantHealth.gameObject);
                    }
                }

                AssignEnemyPositions(enemies);

                GameLog.Log($"CombatArena lazy-loaded {_combatants.Count} combatants ({enemies.Count} enemies).");
            }
            return _combatants;
        }
    }

    private void AssignEnemyPositions(List<GameObject> enemies)
    {
        if (enemies.Count == 0) return;

        // Sort enemies by local X position (assuming camera looks along Z)
        // We use transform.position.x (world space) which works if the arena is aligned. 
        // Better to use local position relative to arena root if the arena is rotated?
        // Let's use world X for now as most arenas are likely aligned or we want screen-space left/right.

        enemies.Sort((a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        for (int i = 0; i < enemies.Count; i++)
        {
            var enemy = enemies[i];
            var id = enemy.GetComponent<CombatPositionIdentifier>();
            if (id == null)
            {
                id = enemy.AddComponent<CombatPositionIdentifier>();
            }

            // Logic for assigning positions based on count
            if (enemies.Count == 1)
            {
                id.Position = CombatPosition.Center;
            }
            else if (enemies.Count == 2)
            {
                // If 2 enemies, assign Left and Right
                id.Position = (i == 0) ? CombatPosition.Left : CombatPosition.Right;
            }
            else if (enemies.Count == 3)
            {
                // If 3 enemies: Left, Center, Right
                if (i == 0) id.Position = CombatPosition.Left;
                else if (i == 1) id.Position = CombatPosition.Center;
                else id.Position = CombatPosition.Right;
            }
            else
            {
                // Fallback for > 3 enemies, just distribute roughly or default to Center
                // For now, let's just log a warning if we have more than UI supports
                if (i == 0) id.Position = CombatPosition.Left;
                else if (i == enemies.Count - 1) id.Position = CombatPosition.Right;
                else id.Position = CombatPosition.Center;
            }
        }
    }
}
}