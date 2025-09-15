using UnityEngine;
using UnityEngine.InputSystem;
using System;
using ProyectSecret.Events;

namespace ProyectSecret.MonoBehaviours.Player
{
    /// <summary>
    /// Responsabilidad Única: Gestionar todas las entradas del jugador desde InputActionAsset.
    /// Expone los valores de entrada y eventos para que otros componentes los consuman.
    /// </summary>
    public class PlayerInputController : MonoBehaviour
    {
        [Header("Input Asset")]
        [field: SerializeField]
        public InputActionAsset InputActions { get; private set; }

        [Header("Gameplay Action Maps")]
        [SerializeField] private string dayActionMapName = "PlayerDay";
        [SerializeField] private string dayMoveActionName = "Look"; // "Look" se mapea a movimiento
        [SerializeField] private string dayInteractActionName = "Interact";
        [SerializeField] private string dayPreviousActionName = "Previous";
        [SerializeField] private string dayNextActionName = "Next";

        [SerializeField] private string nightActionMapName = "PlayerNight";
        [SerializeField] private string nightMoveActionName = "Move";
        [SerializeField] private string nightAttackActionName = "Attack";
        [SerializeField] private string nightInteractActionName = "Interact";

        // Eventos públicos para acciones
        public event Action OnAttackPressed;
        public event Action OnInteractPressed;

        // Propiedades públicas para estados
        public Vector2 MoveInput { get; private set; }

        private InputActionMap _currentActionMap;
        private InputAction _moveAction;
        private InputAction _attackAction;
        private InputAction _interactAction;
        // Acciones adicionales para el día
        private InputAction _previousAction;
        private InputAction _nextAction;

        private void Awake()
        {
            // Inicializa con el mapa de día por defecto
            // Los parámetros nulos indican que no hay acción de ataque, previous o next en el mapa de noche.
            // Esto es un ejemplo, ajusta según tus necesidades.
            // Empezamos con el mapa de día.
            SwitchGameplayMap(dayActionMapName, dayMoveActionName, null, dayInteractActionName, dayPreviousActionName, dayNextActionName);
        }

        private void OnEnable()
        {
            GameEventBus.Instance?.Subscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Instance?.Subscribe<NightStartedEvent>(OnNightStarted);
            // Las suscripciones a las acciones ahora se gestionan en SwitchActionMap
        }

        private void OnDisable()
        {
            GameEventBus.Instance?.Unsubscribe<DayStartedEvent>(OnDayStarted);
            GameEventBus.Instance?.Unsubscribe<NightStartedEvent>(OnNightStarted);
            
            // Desactivamos el mapa actual para limpiar todo
            _currentActionMap?.Disable();
        }

        private void Update()
        {
            MoveInput = _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        private void HandleAttack(InputAction.CallbackContext context) => OnAttackPressed?.Invoke();
        private void HandleInteract(InputAction.CallbackContext context) => OnInteractPressed?.Invoke();
        // Podrías añadir handlers para Previous/Next si es necesario
        // private void HandlePrevious(InputAction.CallbackContext context) => OnPreviousPressed?.Invoke();
        // private void HandleNext(InputAction.CallbackContext context) => OnNextPressed?.Invoke();

        private void OnDayStarted(DayStartedEvent evt) => SwitchGameplayMap(dayActionMapName, dayMoveActionName, null, dayInteractActionName, dayPreviousActionName, dayNextActionName);
        private void OnNightStarted(NightStartedEvent evt) => SwitchGameplayMap(nightActionMapName, nightMoveActionName, nightAttackActionName, nightInteractActionName, null, null);

        /// <summary>
        /// Cambia el mapa de acción de gameplay y reasigna todas las acciones.
        /// </summary>
        private void SwitchGameplayMap(string mapName, string moveName, string attackName, string interactName, string previousName, string nextName)
        {
            // 1. Desuscribirse de todas las acciones posibles del mapa anterior.
            if (_attackAction != null) _attackAction.performed -= HandleAttack;
            if (_interactAction != null) _interactAction.performed -= HandleInteract;
            if (_previousAction != null) _previousAction.performed -= HandleInteract; // Reemplaza con HandlePrevious si lo creas
            if (_nextAction != null) _nextAction.performed -= HandleInteract; // Reemplaza con HandleNext si lo creas

            // 2. Desactivar el mapa de acción actual.
            _currentActionMap?.Disable();

            // 3. Encontrar y asignar el nuevo mapa y sus acciones.
            _currentActionMap = InputActions.FindActionMap(mapName);
            if (_currentActionMap == null) { Debug.LogError($"Action Map '{mapName}' no encontrado.", this); return; }

            // Asignar cada acción, comprobando si el nombre es válido.
            _moveAction = !string.IsNullOrEmpty(moveName) ? _currentActionMap.FindAction(moveName) : null;
            _attackAction = !string.IsNullOrEmpty(attackName) ? _currentActionMap.FindAction(attackName) : null;
            _interactAction = !string.IsNullOrEmpty(interactName) ? _currentActionMap.FindAction(interactName) : null;
            _previousAction = !string.IsNullOrEmpty(previousName) ? _currentActionMap.FindAction(previousName) : null;
            _nextAction = !string.IsNullOrEmpty(nextName) ? _currentActionMap.FindAction(nextName) : null;

            // Validar que las acciones esenciales existan
            if (_moveAction == null && !string.IsNullOrEmpty(moveName)) Debug.LogWarning($"Acción de movimiento '{moveName}' no encontrada en el mapa '{mapName}'.", this);

            // 4. Suscribirse a los eventos de las NUEVAS acciones si existen.
            if (_attackAction != null) _attackAction.performed += HandleAttack;
            if (_interactAction != null) _interactAction.performed += HandleInteract;
            // if (_previousAction != null) _previousAction.performed += HandlePrevious;
            // if (_nextAction != null) _nextAction.performed += HandleNext;

            // 5. Activar el nuevo mapa.
            _currentActionMap.Enable();
        }

        /// <summary>
        /// Activa o desactiva el mapa de acciones de gameplay actual.
        /// </summary>
        public void EnableGameplayMap(bool enable)
        {
            if (enable)
                _currentActionMap?.Enable();
            else
                _currentActionMap?.Disable();
        }
    }
}