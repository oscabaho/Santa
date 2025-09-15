using UnityEngine;
using ProyectSecret.Interfaces;
using ProyectSecret.Core;

namespace ProyectSecret.Inventory.Items
{
    [CreateAssetMenu(fileName = "StatRestoreItem", menuName = "Inventory/Stat Restore Item")]
    public class StatRestoreItem : MysteryItem, IUsableItem
    {
        [Header("Stat Restore Properties")]
        [SerializeField] private StatType statToRestore;
        [SerializeField] private int amountToRestore = 25;

        public bool IsConsumable => true;

        public void Use(GameObject user)
        {
            var statManager = user.GetComponent<StatManager>();
            if (statManager == null)
            {
                Debug.LogWarning($"StatManager no encontrado en {user.name}");
                return;
            }

            var stat = statManager.GetStat(statToRestore);
            if (stat != null)
            {
                stat.AffectValue(amountToRestore);
                Debug.Log($"Se usó {DisplayName}, restaurando {amountToRestore} de {statToRestore}.");
            }
        }
    }
}