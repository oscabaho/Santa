using UnityEngine;

/// <summary>
/// Event published when a visual effect managed by VFXManager has finished.
/// </summary>
public class VFXCompletedEvent
{
    public GameObject TargetObject { get; }

    public VFXCompletedEvent(GameObject targetObject)
    {
        TargetObject = targetObject;
    }
}
