using UnityEngine;
using System.Collections.Generic;

// This class is moved from TurnBasedCombatManager to follow SRP.
// Its only responsibility is to resolve the targets of an ability.
public class TargetResolver : ITargetResolver
{
    private readonly List<GameObject> _tempEnemies = new List<GameObject>(8);
    private readonly List<GameObject> _tempAllies = new List<GameObject>(8);

    public void ResolveTargets(PendingAction action, IReadOnlyList<GameObject> allCombatants, List<GameObject> finalTargets)
    {
        if (action.Ability.Targeting == null)
        {
            Debug.LogWarning($"Ability '{action.Ability.AbilityName}' has no Targeting Strategy assigned.");
            return;
        }

        _tempEnemies.Clear();
        _tempAllies.Clear();
        for (int i = 0; i < allCombatants.Count; i++)
        {
            var c = allCombatants[i];
            if (c == null || !c.activeInHierarchy) continue;
            if (c.CompareTag("Enemy")) _tempEnemies.Add(c);
            else _tempAllies.Add(c);
        }

        action.Ability.Targeting.FindTargets(action, _tempAllies, _tempEnemies, finalTargets);
    }
}
