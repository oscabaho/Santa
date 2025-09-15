using UnityEngine;
using ProyectSecret.Interfaces;
using ProyectSecret.Inventory;
using ProyectSecret.Inventory.Items;
using ProyectSecret.Combat.Behaviours;
using System.Collections;
using ProyectSecret.Events;
using ProyectSecret.MonoBehaviours.Player;
using ProyectSecret.Combat.SceneManagement;
using ProyectSecret.Managers; // Para el HitboxManager

namespace ProyectSecret.Characters.Player
{
    public class PlayerEquipmentController : MonoBehaviour, IPlayerEquipmentController, IPersistentData
    {
        private PlayerPointSwitcher pointSwitcher;
        private GameObject currentWeaponVisual;
        private GameObject currentHitboxInstance;
        
        public WeaponInstance EquippedWeaponInstance { get; private set; }
        public EquipmentSlots EquipmentSlots { get; private set; }

        private void Awake()
        {
            EquipmentSlots = new EquipmentSlots();
            pointSwitcher = GetComponent<PlayerPointSwitcher>();

            if (pointSwitcher == null)
            {
                Debug.LogError("PlayerEquipmentController requiere un PlayerPointSwitcher en el mismo GameObject para funcionar correctamente.", this);
            }
        }

        public void EquipWeapon(WeaponItem weaponItem)
        {
            UnequipWeapon();
            if (weaponItem == null) return;
            var weaponInstance = new WeaponInstance(weaponItem);
            EquipWeaponInstance(weaponInstance);
        }

        public void EquipWeaponInstance(WeaponInstance instance)
        {
            if (instance == null) return;

            EquippedWeaponInstance = instance;

            if (instance.WeaponData.WeaponPrefab != null && pointSwitcher?.ActiveWeaponPoint != null)
            {
                currentWeaponVisual = Instantiate(instance.WeaponData.WeaponPrefab, pointSwitcher.ActiveWeaponPoint);
                currentWeaponVisual.transform.localPosition = Vector3.zero;
                currentWeaponVisual.transform.localRotation = Quaternion.identity;
                currentWeaponVisual.transform.localScale = Vector3.one;
            }

            instance.WeaponData?.OnEquip(gameObject);
            GameEventBus.Instance?.Publish(new PlayerWeaponEquippedEvent(gameObject, instance));
        }

        public void UnequipWeapon()
        {
            if (currentWeaponVisual != null)
            {
                Destroy(currentWeaponVisual);
                currentWeaponVisual = null;
            }

            if (EquippedWeaponInstance != null)
            {
                EquippedWeaponInstance.WeaponData?.OnUnequip(gameObject);
                GameEventBus.Instance?.Publish(new PlayerWeaponUnequippedEvent(gameObject, EquippedWeaponInstance));
            }

            EquippedWeaponInstance = null;
        }

        public bool Attack()
        {
            if (EquippedWeaponInstance == null || currentHitboxInstance != null)
                return false;

            var weaponData = EquippedWeaponInstance.WeaponData;
            var hitboxSpawnPoint = pointSwitcher?.ActiveHitBoxPoint;

            if (weaponData.HitBoxPrefab == null || hitboxSpawnPoint == null)
                return false;

            // 1. Obtener la hitbox del pool. Vendrá desactivada.
            var hitbox = HitboxManager.Instance?.Get(weaponData.HitBoxPrefab);
            if (hitbox == null) return false;

            currentHitboxInstance = hitbox.gameObject;

            // 2. Posicionar y emparentar la hitbox ANTES de activarla.
            currentHitboxInstance.transform.SetParent(hitboxSpawnPoint, false);
            currentHitboxInstance.transform.localPosition = Vector3.zero;
            currentHitboxInstance.transform.localRotation = Quaternion.identity;
            
            // 3. Activar el GameObject.
            currentHitboxInstance.SetActive(true);

            // Reproducir el VFX de ataque si está definido en el arma.
            if (!string.IsNullOrEmpty(weaponData.AttackVfxKey))
            {
                GameEventBus.Instance?.Publish(new PlayVFXRequest(weaponData.AttackVfxKey, hitboxSpawnPoint.position, hitboxSpawnPoint.rotation));
            }

            // 4. Inicializar y empezar el ciclo de vida del ataque.
            hitbox.Initialize(EquippedWeaponInstance, gameObject);
            hitbox.EnableDamage();
            StartCoroutine(HitboxLifecycle(hitbox, weaponData.AttackDuration));

            return true;
        }

        private IEnumerator HitboxLifecycle(WeaponHitbox hitbox, float duration)
        {
            yield return new WaitForSeconds(duration);
            
            // Comprobamos que la hitbox y su GameObject todavía existen antes de intentar desactivarlos.
            if (hitbox != null && hitbox.gameObject != null && hitbox.gameObject.activeSelf)
            {
                hitbox.DisableDamage();
                // Al desactivar el GameObject, se llamará a OnDisable() en WeaponHitbox,
                // que lo devolverá al pool automáticamente.
                hitbox.gameObject.SetActive(false); 
            }
            // Nos aseguramos de limpiar la referencia para poder atacar de nuevo.
            currentHitboxInstance = null;
        }

        public bool CanAttack()
        {
            return EquippedWeaponInstance != null && currentHitboxInstance == null;
        }

        #region IPersistentData Implementation

        public void SaveData(PlayerPersistentData data)
        {
            if (EquippedWeaponInstance != null)
            {
                data.equippedWeaponId = EquippedWeaponInstance.WeaponData?.Id;
                data.equippedWeaponDurability = EquippedWeaponInstance.CurrentDurability;
                data.equippedWeaponHits = EquippedWeaponInstance.Hits;
            }
            else
            {
                data.equippedWeaponId = null;
                data.equippedWeaponDurability = 0;
                data.equippedWeaponHits = 0;
            }
        }

        public void LoadData(PlayerPersistentData data, ItemDatabase itemDatabase)
        {
            if (!string.IsNullOrEmpty(data.equippedWeaponId))
            {
                var weaponItem = itemDatabase.GetItem(data.equippedWeaponId) as WeaponItem;
                if (weaponItem == null) return;
                
                var weaponInstance = new WeaponInstance(weaponItem);
                weaponInstance.SetDurability(data.equippedWeaponDurability);
                weaponInstance.SetHits(data.equippedWeaponHits);
                EquipWeaponInstance(weaponInstance);
            }
        }

        #endregion
    }
}
