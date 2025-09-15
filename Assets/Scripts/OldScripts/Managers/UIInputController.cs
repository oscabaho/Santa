using UnityEngine;
using UnityEngine.InputSystem;
using ProyectSecret.MonoBehaviours.Player;

namespace ProyectSecret.Managers
{
    /// <summary>
    /// Clase estática para gestionar el cambio entre los mapas de acción de Gameplay y UI.
    /// </summary>
    public static class UIInputController
    {
        private static PlayerInputController _playerInput;
        private static InputActionMap _uiActionMap;
        private const string UIMapName = "UI";

        // Este método se llama automáticamente al iniciar el juego.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            // Buscamos la instancia del controlador del jugador.
            _playerInput = Object.FindFirstObjectByType<PlayerInputController>();
            if (_playerInput == null) return;

            // Obtenemos el mapa de UI del mismo asset.
            _uiActionMap = _playerInput.InputActions.FindActionMap(UIMapName);
            if (_uiActionMap == null)
            {
                Debug.LogError($"UIInputController: No se pudo encontrar el Action Map llamado '{UIMapName}'.");
            }
        }

        public static void EnableUIMap()
        {
            if (_playerInput == null || _uiActionMap == null) return;

            // Desactivamos el control del jugador y activamos el de la UI.
            _playerInput.EnableGameplayMap(false);
            _uiActionMap.Enable();
        }

        public static void EnableGameplayMap()
        {
            if (_playerInput == null || _uiActionMap == null) return;

            // Desactivamos el control de la UI y reactivamos el del jugador.
            _uiActionMap.Disable();
            _playerInput.EnableGameplayMap(true);
        }
    }
}