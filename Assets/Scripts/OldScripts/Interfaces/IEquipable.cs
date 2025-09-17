/// <summary>
/// Interfaz para objetos equipables (armas, armaduras, accesorios).
/// </summary>
public interface IEquipable
{
    EquipmentSlotType GetSlotType();
    void OnEquip(UnityEngine.GameObject user);
    void OnUnequip(UnityEngine.GameObject user);
    string GetId();
}
