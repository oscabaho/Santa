/// <summary>
/// Interfaz para facilitar pruebas y mocks del sistema de equipamiento.
/// </summary>
public interface IPlayerEquipmentController
{
    WeaponInstance EquippedWeaponInstance { get; }
    EquipmentSlots EquipmentSlots { get; }
    void EquipWeaponInstance(WeaponInstance instance);
    void EquipWeapon(WeaponItem weaponItem);
    void UnequipWeapon();
}
