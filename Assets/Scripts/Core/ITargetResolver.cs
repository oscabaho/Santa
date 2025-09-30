using System.Collections.Generic;
using UnityEngine;

public interface ITargetResolver
{
    void ResolveTargets(PendingAction action, IReadOnlyList<GameObject> allCombatants, List<GameObject> finalTargets);
}
