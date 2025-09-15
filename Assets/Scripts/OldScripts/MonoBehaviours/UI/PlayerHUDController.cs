using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProyectSecret.Characters;
using ProyectSecret.Inventory;
using ProyectSecret.Events;
using ProyectSecret.Characters.Player;

namespace ProyectSecret.UI
{
    /// <summary>
    /// Controlador centralizado del HUD del jugador: vida, stamina, durabilidad de arma, quest e indicaciones.
    /// </summary>
    public class PlayerHUDController : MonoBehaviour
    {
        [Header("Stats UI")]
        [SerializeField] private Image healthFill;
        [SerializeField] private Image staminaFill;
        [SerializeField] private Image weaponDurabilityRadial;
        [SerializeField] private Color durabilityBaseColor = Color.green;
        [SerializeField] private Color durabilityWarningColor = Color.red;

        [Header("Quest & Guidance UI")]
        [SerializeField] private TMP_Text questInfoText;
        [SerializeField] private TMP_Text guidanceText;

        private PlayerHealthController playerHealth;
        private PlayerStaminaController playerStamina; // Añadido
        private PlayerEquipmentController equipmentController;
        private WeaponInstance subscribedWeaponInstance;

        // Delegados para guardar las suscripciones y poder desuscribirlas correctamente
        private System.Action<ProyectSecret.Stats.StatComponent> healthChangedHandler;
        private System.Action<ProyectSecret.Stats.StatComponent> staminaChangedHandler;
        private System.Action weaponStateChangedHandler;

        private void OnEnable()
        {
            // Suscribirse a eventos globales para saber cuándo aparece el jugador y cuándo cambia el inventario.
            GameEventBus.Instance.Subscribe<PlayerSpawnedEvent>(HandlePlayerSpawned);
            // Escuchar eventos específicos de equipamiento de armas es más directo y robusto.
            GameEventBus.Instance.Subscribe<PlayerWeaponEquippedEvent>(OnWeaponEquipped);
            GameEventBus.Instance.Subscribe<PlayerWeaponUnequippedEvent>(OnWeaponUnequipped);
        }

        private void OnDisable()
        {
            if (GameEventBus.Instance != null)
            {
                GameEventBus.Instance.Unsubscribe<PlayerSpawnedEvent>(HandlePlayerSpawned);
                GameEventBus.Instance.Unsubscribe<PlayerWeaponEquippedEvent>(OnWeaponEquipped);
                GameEventBus.Instance.Unsubscribe<PlayerWeaponUnequippedEvent>(OnWeaponUnequipped);
            }
            
            // Desuscribirse de todos los eventos del jugador si ya no estamos en la escena.
            UnsubscribeFromPlayerEvents();
        }

        private void HandlePlayerSpawned(PlayerSpawnedEvent evt)
        {
            // Si ya estábamos suscritos a un jugador anterior, nos desuscribimos primero.
            UnsubscribeFromPlayerEvents();

            // Obtenemos los componentes del nuevo jugador que ha aparecido.
            playerHealth = evt.PlayerObject.GetComponent<PlayerHealthController>();
            playerStamina = evt.PlayerObject.GetComponent<PlayerStaminaController>(); // Añadido
            equipmentController = evt.PlayerObject.GetComponent<PlayerEquipmentController>();

            // Nos suscribimos a los eventos del nuevo jugador.
            SubscribeToPlayerEvents();
        }

        private void SubscribeToPlayerEvents()
        {
            // Vida
            if (playerHealth != null && playerHealth.Health != null)
            {
                healthChangedHandler = (stat) => UpdateHealthBar(stat.CurrentValue, stat.MaxValue);
                playerHealth.Health.OnValueChanged += healthChangedHandler;
                UpdateHealthBar(playerHealth.Health.CurrentValue, playerHealth.Health.MaxValue);
            }
            // Stamina
            if (playerStamina != null && playerStamina.Stamina != null)
            {
                staminaChangedHandler = (stat) => UpdateStaminaBar(stat.CurrentValue, stat.MaxValue);
                playerStamina.Stamina.OnValueChanged += staminaChangedHandler;
                UpdateStaminaBar(playerStamina.Stamina.CurrentValue, playerStamina.Stamina.MaxValue);
            }
            
            // Arma: Actualizar con el arma equipada al aparecer.
            // Los eventos se encargarán de los cambios posteriores.
            if (equipmentController != null)
            {
                HandleWeaponChange(equipmentController.EquippedWeaponInstance);
            }
        }

        private void UnsubscribeFromPlayerEvents()
        {
            if (playerHealth != null)
            {
                if (playerHealth.Health != null && healthChangedHandler != null)
                    playerHealth.Health.OnValueChanged -= healthChangedHandler;
                if (playerStamina != null && playerStamina.Stamina != null && staminaChangedHandler != null)
                    playerStamina.Stamina.OnValueChanged -= staminaChangedHandler;
            }
            if (subscribedWeaponInstance != null)
            {
                subscribedWeaponInstance.OnStateChanged -= weaponStateChangedHandler;
                subscribedWeaponInstance = null;
            }
        }

        private void OnWeaponEquipped(PlayerWeaponEquippedEvent evt)
        {
            // Solo reaccionar si el evento es para el jugador que este HUD está siguiendo.
            if (playerHealth != null && evt.Player == playerHealth.gameObject)
            {
                HandleWeaponChange(evt.Weapon);
            }
        }

        private void OnWeaponUnequipped(PlayerWeaponUnequippedEvent evt)
        {
            if (playerHealth != null && evt.Player == playerHealth.gameObject)
            {
                HandleWeaponChange(null);
            }
        }

        private void HandleWeaponChange(WeaponInstance newWeapon)
        {
            // Desuscribirse del arma anterior para evitar fugas de memoria.
            if (subscribedWeaponInstance != null)
            {
                subscribedWeaponInstance.OnStateChanged -= weaponStateChangedHandler;
            }

            subscribedWeaponInstance = newWeapon;

            if (subscribedWeaponInstance != null)
            {
                weaponStateChangedHandler = UpdateWeaponDurability;
                subscribedWeaponInstance.OnStateChanged += weaponStateChangedHandler;
            }

            UpdateWeaponDurability();
        }

        private void UpdateWeaponDurability()
        {
            if (subscribedWeaponInstance != null)
            {
                UpdateWeaponDurabilityRadial(
                    subscribedWeaponInstance.CurrentDurability, 
                    subscribedWeaponInstance.WeaponData.MaxDurability);
            }
            else if (weaponDurabilityRadial != null)
            {
                // Si no hay arma equipada, ocultamos la barra de durabilidad.
                weaponDurabilityRadial.fillAmount = 0;
            }
        }

        private void UpdateHealthBar(float current, float max)
        {
            if (healthFill != null)
                healthFill.fillAmount = Mathf.Clamp01(current / max);
        }

        private void UpdateStaminaBar(float current, float max)
        {
            if (staminaFill != null)
                staminaFill.fillAmount = Mathf.Clamp01(current / max);
        }

        public void UpdateWeaponDurabilityRadial(float current, float max)
        {
            if (weaponDurabilityRadial == null) return;
            float percent = Mathf.Clamp01(current / max);
            weaponDurabilityRadial.fillAmount = percent;
            weaponDurabilityRadial.color = GetDurabilityColor(percent);
        }

        private Color GetDurabilityColor(float percent)
        {
            if (percent > 0.1f)
                return durabilityBaseColor;
            // Interpolación de color base a rojo intenso
            float t = Mathf.InverseLerp(0.1f, 0.01f, percent);
            return Color.Lerp(durabilityBaseColor, durabilityWarningColor, 1 - t);
        }

        public void SetQuestInfo(string questTitle, string questObjective)
        {
            if (questInfoText != null)
                questInfoText.text = $"<b>{questTitle}</b>\n{questObjective}";
        }

        public void SetGuidance(string message)
        {
            if (guidanceText != null)
                guidanceText.text = message;
        }
    }
}
