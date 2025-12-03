namespace Santa.Core
{
    /// <summary>
    /// Interface for objects that can inflict durability damage on a weapon when hit.
    /// </summary>
    public interface IWeaponDamager
    {
        int GetDurabilityDamage();
    }
}
