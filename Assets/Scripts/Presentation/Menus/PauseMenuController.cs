using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Core.Addressables;
using Santa.Infrastructure.Input;
using UnityEngine;
using VContainer;

namespace Santa.UI
{
    /// <summary>
    /// Service for managing pause menu state and time control during exploration.
    /// Listens to pause input and toggles the pause menu via UIManager.
    /// </summary>
    public class PauseMenuController : MonoBehaviour, Santa.Core.IPauseMenuService
    {
        private IUIManager _uiManager;
        private ICombatService _combatService;
        private InputReader _input;
        private const string PauseMenuAddress = Santa.Core.Addressables.AddressableKeys.UIPanels.PauseMenu;

        public bool IsPaused { get; private set; }

        // Lazy resolve UIManager from scene to avoid circular dependency and injection issues
        private IUIManager UIManager
        {
            get
            {
                 if (_uiManager == null)
                 {
                     _uiManager = FindFirstObjectByType<Santa.Presentation.UI.UIManager>();
                     if (_uiManager == null)
                     {
                         // Last ditch effort: Try to find by interface
                         _uiManager = (IUIManager)FindFirstObjectByType(typeof(Santa.Presentation.UI.UIManager));
                     }
                 }
                 
                 if (_uiManager == null)
                 {
                     GameLog.LogError("PauseMenuController: CRITICAL - UIManager not found in scene!");
                 }

                 return _uiManager;
            }
        }

        // [Inject] removed to support safe runtime discovery
        // public void Construct(...) ...

        private void EnsureInputReader()
        {
            if (_input == null)
            {
                 var readers = Resources.FindObjectsOfTypeAll<InputReader>();
                 if (readers != null && readers.Length > 0)
                 {
                     _input = readers[0];
                 }
            }
        }

        private void OnEnable()
        {
            EnsureInputReader();
            if (_input != null)
            {
                _input.PauseEvent += OnPauseInput;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.PauseEvent -= OnPauseInput;
            }
        }

        private void OnPauseInput()
        {
            try
            {
                TogglePause().Forget();
            }
            catch (System.Exception ex)
            {
                GameLog.LogError($"PauseMenuController.OnPauseInput: Exception during toggle: {ex.Message}");
                GameLog.LogException(ex);
            }
        }

        public async UniTask ShowPauseMenu()
        {
            GameLog.Log("PauseMenuController.ShowPauseMenu: CALLED");
            // Don't allow pause during combat
            if (Santa.Infrastructure.Combat.TurnBasedCombatManager.CombatIsInitialized)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("PauseMenuController.ShowPauseMenu: BLOCKED - Combat is active");
#endif
                return;
            }

            if (IsPaused) return;

            GameLog.Log("PauseMenuController.ShowPauseMenu: Setting Time.timeScale = 0");
            IsPaused = true;
            Time.timeScale = 0f;
            if (UIManager == null)
            {
                GameLog.LogError("PauseMenuController.ShowPauseMenu: Cannot show pause menu because UIManager is null.");
                return;
            }

            GameLog.Log($"PauseMenuController.ShowPauseMenu: Calling UIManager.ShowPanel({PauseMenuAddress})");
            await UIManager.ShowPanel(PauseMenuAddress);
            // Hide exploration HUD while paused (VirtualGamepad)
            UIManager.HidePanel(Santa.Core.Addressables.AddressableKeys.UIPanels.VirtualGamepad);
            GameLog.Log("PauseMenuController.ShowPauseMenu: FINISHED");
        }

        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused = false;
            Time.timeScale = 1f;

            if (UIManager != null)
            {
                // Hide the pause menu panel via UIManager (CanvasGroup-based)
                UIManager.HidePanel(PauseMenuAddress);
                // Restore exploration HUD (VirtualGamepad)
                UIManager.ShowPanel(Santa.Core.Addressables.AddressableKeys.UIPanels.VirtualGamepad).Forget();
            }
        }

        public async UniTask TogglePause()
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                await ShowPauseMenu();
            }
        }
    }
}
