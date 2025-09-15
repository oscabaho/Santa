using UnityEngine;
using ProyectSecret.Components;
using ProyectSecret.Inventory;
using ProyectSecret.Events;
using ProyectSecret.Characters.Player;

namespace ProyectSecret.Combat.Behaviours
{
    /// <summary>
    /// Componente modular para lógica de ataque. Puede ser añadido a cualquier entidad.
    /// </summary>
    public class AttackComponent : MonoBehaviour
    {
        // El cooldown del ataque y el coste de estamina ahora se obtendrán del arma equipada.

        [Header("Dependencies")]
        [Tooltip("Componente de estamina del que se descontará el coste del ataque.")]
        [SerializeField] private StaminaComponentBehaviour staminaBehaviour;
        [Tooltip("Controlador de equipamiento que gestiona el arma a usar.")]
        [SerializeField] private PlayerEquipmentController equipmentController;
        
        private float lastAttackTime = -999f;

        private void Awake()
        {
            // Es mejor validar las dependencias que asignarlas silenciosamente.
            // Esto fuerza a una configuración correcta desde el Inspector y previene errores.
            if (staminaBehaviour == null)
            {
                Debug.LogError("AttackComponent: StaminaComponentBehaviour no está asignado en el Inspector.", this);
            }
            if (equipmentController == null)
            {
                Debug.LogError("AttackComponent: PlayerEquipmentController no está asignado en el Inspector.", this);
            }
        }

        /// <summary>
        /// Inicia el ataque: activa el collider del arma equipada y consume stamina.
        /// </summary>
        public void TryAttack()
        {
            var weaponInstance = equipmentController?.EquippedWeaponInstance;
            if (weaponInstance == null || !equipmentController.CanAttack())
                return;

            // Obtener el cooldown del arma. Si attackSpeed es 0, evitamos división por cero.
            float currentAttackCooldown = weaponInstance.WeaponData.AttackSpeed > 0 ? 1f / weaponInstance.WeaponData.AttackSpeed : float.MaxValue;
            if (Time.time - lastAttackTime < currentAttackCooldown)
                return;

            // Obtener el coste de estamina del arma actual. Asumo que WeaponItem tiene una propiedad StaminaCost.
            int currentStaminaCost = weaponInstance.WeaponData.StaminaCost;

            if (staminaBehaviour == null || !staminaBehaviour.Stamina.HasEnough(currentStaminaCost))
            {
                #if UNITY_EDITOR
                Debug.Log("No hay suficiente stamina para atacar.");
                #endif
                return;
            }
            if (equipmentController.Attack())
            {
                staminaBehaviour.Stamina.UseStamina(currentStaminaCost);
                lastAttackTime = Time.time;

                // Publicar un evento para notificar que se usó estamina.
                // PlayerHealthController escuchará este evento para reiniciar su contador.
                GameEventBus.Instance.Publish(new PlayerActionUsedStaminaEvent(gameObject, currentStaminaCost));
            }
        }
    }
}
