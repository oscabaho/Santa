using System.Collections.Generic;
using UnityEngine;
using ProyectSecret.Inventory.Items;

namespace ProyectSecret.Inventory
{
    /// <summary>
    /// Gestiona los slots de equipamiento del jugador (arma, armadura, accesorios, etc).
    /// </summary>
    [System.Serializable]
    public class EquipmentSlots
    {
        [SerializeField] private WeaponItem equippedWeapon;
        // Puedes agregar más slots aquí (armadura, accesorios, etc)

        public WeaponItem EquippedWeapon { get { return equippedWeapon; } }

        public void EquipWeapon(WeaponItem weapon)
        {
            equippedWeapon = weapon;
        }

        public void UnequipWeapon()
        {
            equippedWeapon = null;
        }
    }
}
