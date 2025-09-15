using System.Collections.Generic;
using ProyectSecret.Inventory.Items;
using UnityEngine;

namespace ProyectSecret.Inventory
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<MysteryItem> itemsList = new List<MysteryItem>();
        private Dictionary<string, MysteryItem> itemsDict;

        private void OnEnable()
        {
            itemsDict = new Dictionary<string, MysteryItem>();
            foreach (var item in itemsList)
            {
                if (item != null && !itemsDict.ContainsKey(item.Id))
                    itemsDict.Add(item.Id, item);
            }
        }

        public MysteryItem GetItem(string id)
        {
            if (itemsDict == null)
                OnEnable();
            itemsDict.TryGetValue(id, out var item);
            return item;
        }
    }
}
