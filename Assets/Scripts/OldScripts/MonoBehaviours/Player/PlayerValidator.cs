using UnityEngine;
using ProyectSecret.Combat.Behaviours;
using ProyectSecret.Characters.Player;
using ProyectSecret.Components;
using ProyectSecret.Inventory;

namespace ProyectSecret.MonoBehaviours.Player
{
    /// <summary>
    /// RESPONSABILIDAD ÚNICA: Validar que el GameObject del Jugador tiene todos los componentes necesarios.
    /// Este script no tiene lógica en sí mismo; su único propósito es usar [RequireComponent]
    /// para forzar la adición de todos los scripts esenciales en el editor de Unity.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PaperMarioPlayerMovement))]
    [RequireComponent(typeof(PlayerCameraController))]
    [RequireComponent(typeof(PlayerAnimationController))]
    [RequireComponent(typeof(PlayerAudioController))]
    [RequireComponent(typeof(PlayerAttackInput))]
    [RequireComponent(typeof(PlayerInteractionController))]
    [RequireComponent(typeof(PlayerEquipmentController))]
    [RequireComponent(typeof(PlayerInventory))]
    [RequireComponent(typeof(PlayerPointSwitcher))]
    [RequireComponent(typeof(AttackComponent))]
    [RequireComponent(typeof(HealthComponentBehaviour))]
    [RequireComponent(typeof(StaminaComponentBehaviour))]
    public class PlayerValidator : MonoBehaviour { }
}