using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides context for transition tasks, giving them access to scene-specific objects.
/// </summary>
public class TransitionContext
{
    private readonly Dictionary<TargetId, GameObject> _targets = new Dictionary<TargetId, GameObject>();

    public void AddTarget(TargetId id, GameObject obj)
    {
        if (obj != null)
        {
            _targets[id] = obj;
        }
    }

    public GameObject GetTarget(TargetId id)
    {
        _targets.TryGetValue(id, out var target);
        return target;
    }
}
