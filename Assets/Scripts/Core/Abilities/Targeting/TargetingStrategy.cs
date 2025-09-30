using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all targeting strategies. 
/// Inherit from this to create new ways for abilities to find targets.
/// </summary>
public abstract class TargetingStrategy : ScriptableObject
{
    public abstract bool RequiresTarget { get; }

    public abstract void FindTargets(PendingAction action, List<GameObject> allies, List<GameObject> enemies, List<GameObject> finalTargets);
}
