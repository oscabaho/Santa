using UnityEngine;
using ProyectSecret.Characters;

namespace ProyectSecret.Inventory.Items
{
    [CreateAssetMenu(fileName = "CursedWeapon", menuName = "Inventory/CursedWeaponItem")]
    public class CursedWeaponItem : WeaponItem
    {
        [Header("Efectos de la Maldición")]
        [Tooltip("Penalización a la vida máxima mientras el arma está equipada. Debe ser un valor negativo.")]
        [SerializeField] private int maxHealthModifier = -20;

        public override void OnEquip(GameObject user)
        {
            base.OnEquip(user);
            var healthController = user.GetComponent<PlayerHealthController>();
            if (healthController != null && healthController.Health != null)
            {
                healthController.Health.ModifyMaxValue(maxHealthModifier);
                Debug.Log($"Maldición aplicada: Vida máxima modificada en {maxHealthModifier}");
            }
        }

        public override void OnUnequip(GameObject user)
        {
            base.OnUnequip(user);
            var healthController = user.GetComponent<PlayerHealthController>();
            if (healthController != null && healthController.Health != null)
            {
                // Invertimos el modificador para restaurar la vida máxima
                healthController.Health.ModifyMaxValue(-maxHealthModifier);
                Debug.Log($"Maldición retirada: Vida máxima restaurada.");
            }
        }
    }
}