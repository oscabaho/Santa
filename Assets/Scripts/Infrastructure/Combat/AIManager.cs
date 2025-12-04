using System.Collections.Generic;
using UnityEngine;
using Santa.Core;
using Santa.Core.Config;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Handles the logic for AI combatants to choose their actions during the planning phase.
    /// </summary>
    public class AIManager : MonoBehaviour, IAIManager
    {
        private readonly List<GameObject> _tempEnemies = new List<GameObject>(8);
        private readonly List<GameObject> _tempAllies = new List<GameObject>(8);

        public void PlanActions(
            IReadOnlyList<GameObject> allCombatants,
            GameObject player,
            IReadOnlyDictionary<GameObject, IBrain> brainCache,
            IReadOnlyDictionary<GameObject, IActionPointController> apCache,
            List<PendingAction> pendingActions,
            PendingAction? playerAction)
        {
            _tempEnemies.Clear();
            _tempAllies.Clear();

            // Ensure the player is always in the allies list
            if (player != null)
            {
                _tempAllies.Add(player);
            }

            // Classify combatants: Allies include anyone tagged "Player" (except player to avoid duplicates)
            // Enemies are anyone tagged "Enemy"
            foreach (var c in allCombatants)
            {
                if (c == null || c == player) continue; // Skip null and player (already added)

                if (c.CompareTag(GameConstants.Tags.Enemy))
                {
                    _tempEnemies.Add(c);
                }
                else if (c.CompareTag(GameConstants.Tags.Player))
                {
                    // Allies with "Player" tag (e.g., summoned units, NPCs)
                    _tempAllies.Add(c);
                }
            }

            foreach (var combatant in allCombatants)
            {
                if (combatant == null || combatant == player || !combatant.activeInHierarchy) continue;

                if (brainCache.TryGetValue(combatant, out var brain) && apCache.TryGetValue(combatant, out var aiAP))
                {
                    // CRITICAL: Pass enemies and allies in CORRECT order
                    // EnemyBrain needs _tempAllies (Players) as enemies to attack
                    // AllyBrain needs _tempEnemies (Enemies) as enemies to attack
                    PendingAction aiAction = brain.ChooseAction(playerAction, _tempEnemies, _tempAllies);

                    if (aiAction.Ability != null && aiAP.CurrentValue >= aiAction.Ability.ApCost)
                    {
                        aiAP.AffectValue(-aiAction.Ability.ApCost);
                        pendingActions.Add(aiAction);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                        GameLog.Log($"{combatant.name} submitted action: {aiAction.Ability.AbilityName}");
#endif
                    }
                }
            }
        }
    }
}
