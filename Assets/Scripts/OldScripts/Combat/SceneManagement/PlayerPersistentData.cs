using UnityEngine;
using System.Collections.Generic;
using ProyectSecret.Inventory;
using ProyectSecret.Inventory.Items;
using ProyectSecret.Characters.Player;
using ProyectSecret.Interfaces;

namespace ProyectSecret.Combat.SceneManagement
{
    [System.Serializable]
    public class SerializableInventoryData
    {
        // Ahora solo guardamos los IDs de todos los ítems, incluidas las armas.
        [Tooltip("Lista de IDs de los ítems en el inventario.")]
        public List<string> itemIds = new List<string>();
    }

    /// <summary>
    /// ScriptableObject para transferir datos persistentes del jugador entre escenas.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerPersistentData", menuName = "Combat/PlayerPersistentData")]
    public class PlayerPersistentData : ScriptableObject
    {
        [Header("Player Stats")]
        [SerializeField] public int playerHealth;
        [SerializeField] public int playerStamina;
        [SerializeField] public string equippedWeaponId;
        [SerializeField] public float equippedWeaponDurability;
        [SerializeField] public int equippedWeaponHits;
        [SerializeField] public SerializableInventoryData inventoryData = new SerializableInventoryData();

        [Header("State Flags")]
        [Tooltip("Se activa si el jugador vuelve de una derrota en combate.")]
        public bool CameFromDefeat = false;
        [Tooltip("Indica si hay una posición guardada para usar.")]
        public bool HasSavedPosition = false;
        public Vector3 LastPosition;

        public void SaveFromPlayer(GameObject player, bool savePosition = true)
        {
            foreach (var persistentComponent in player.GetComponents<IPersistentData>())
            {
                persistentComponent.SaveData(this);
            }

            if (savePosition)
            {
                LastPosition = player.transform.position;
                HasSavedPosition = true;
            }
        }

        public void ApplyToPlayer(GameObject player, ItemDatabase itemDatabase)
        {
            foreach (var persistentComponent in player.GetComponents<IPersistentData>())
            {
                persistentComponent.LoadData(this, itemDatabase);
            }
        }

        /// <summary>
        /// Resetea todos los datos a sus valores por defecto.
        /// Útil para empezar una nueva partida.
        /// </summary>
        public void ResetData()
        {
            playerHealth = 0; // O tu valor inicial por defecto
            playerStamina = 0; // O tu valor inicial por defecto
            equippedWeaponId = null;
            equippedWeaponDurability = 0;
            equippedWeaponHits = 0;
            inventoryData = new SerializableInventoryData();
            CameFromDefeat = false;
            HasSavedPosition = false;
            LastPosition = Vector3.zero;
        }
    }
}
