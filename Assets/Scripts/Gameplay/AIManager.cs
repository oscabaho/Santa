using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines the contract for a service that handles AI action planning.
/// </summary>
public interface IAIManager
{
    /// <summary>
    /// Gathers actions from all non-player combatants.
    /// </summary>
    void PlanActions(
        IReadOnlyList<GameObject> allCombatants,
        GameObject player,
        IReadOnlyDictionary<GameObject, IBrain> brainCache,
        IReadOnlyDictionary<GameObject, IActionPointController> apCache,
        List<PendingAction> pendingActions,
        PendingAction? playerAction);
}

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
        foreach (var c in allCombatants)
        {
            if (c == null) continue;
            if (c.CompareTag("Enemy")) _tempEnemies.Add(c);
            else _tempAllies.Add(c);
        }

        foreach (var combatant in allCombatants)
        {
            if (combatant == null || combatant == player || !combatant.activeInHierarchy) continue;

            if (brainCache.TryGetValue(combatant, out var brain) && apCache.TryGetValue(combatant, out var aiAP))
            {
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
