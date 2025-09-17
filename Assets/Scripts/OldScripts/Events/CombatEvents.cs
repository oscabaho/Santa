using UnityEngine;

public class HitboxImpactEvent
{
    public WeaponItem WeaponData { get; }
    public Vector3 ImpactPoint { get; }
    public GameObject Target { get; } // El objeto que fue golpeado

    public HitboxImpactEvent(WeaponItem weaponData, Vector3 impactPoint, GameObject target)
    {
        WeaponData = weaponData;
        ImpactPoint = impactPoint;
        Target = target;
    }
}
