using UnityEngine;

/// <summary>
/// Event published when a hitbox connects with a damageable object.
/// Carries data needed for spawning impact effects.
/// </summary>
public class HitboxImpactEvent
{
    public string ImpactVfxKey { get; }
    public Vector3 ImpactPoint { get; }
    public GameObject Target { get; }

    public HitboxImpactEvent(string impactVfxKey, Vector3 impactPoint, GameObject target)
    {
        ImpactVfxKey = impactVfxKey;
        ImpactPoint = impactPoint;
        Target = target;
    }
}
