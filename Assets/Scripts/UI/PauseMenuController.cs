using UnityEngine;
using VContainer;
using Cysharp.Threading.Tasks;

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

        [Inject]
        public void Construct(IUIManager uiManager, InputReader inputReader, ICombatService combatService)
        {
            _uiManager = uiManager;
            _input = inputReader;
            _combatService = combatService;
        }

        private void OnEnable()
        {
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

        private async void OnPauseInput()
        {
            await TogglePause();
        }

        public async UniTask ShowPauseMenu()
        {
               GameLog.Log("PauseMenuController.ShowPauseMenu: CALLED");
            // Don't allow pause during combat
            if (global::TurnBasedCombatManager.CombatIsInitialized)
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
               GameLog.Log($"PauseMenuController.ShowPauseMenu: Calling UIManager.ShowPanel({PauseMenuAddress})");
            await _uiManager.ShowPanel(PauseMenuAddress);
                // Hide exploration HUD while paused (VirtualGamepad)
                _uiManager.HidePanel("VirtualGamepad");
               GameLog.Log("PauseMenuController.ShowPauseMenu: FINISHED");
        }

        public void Resume()
        {
            if (!IsPaused) return;

            IsPaused = false;
            Time.timeScale = 1f;
            
            // Hide the pause menu panel via UIManager (CanvasGroup-based)
            _uiManager.HidePanel(PauseMenuAddress);
                // Restore exploration HUD (VirtualGamepad)
                _uiManager.ShowPanel("VirtualGamepad").Forget();
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
