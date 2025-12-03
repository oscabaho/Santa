using Cysharp.Threading.Tasks;
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
        private VContainer.IObjectResolver _resolver;
        private IUIManager _uiManager;
        private ICombatService _combatService;
        private InputReader _input;
        private const string PauseMenuAddress = Santa.Core.Addressables.AddressableKeys.UIPanels.PauseMenu;

        public bool IsPaused { get; private set; }

        // Lazy resolve UIManager to break circular dependency (UIManager injects IPauseMenuService)
        private IUIManager UIManager => _uiManager ??= _resolver.Resolve<IUIManager>();

        [Inject]
        public void Construct(IObjectResolver resolver, ICombatService combatService, InputReader inputReader = null)
        {
            _resolver = resolver;
            _input = inputReader;
            _combatService = combatService;

            if (_input == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("PauseMenuController: InputReader not assigned. Pause via Escape key will not work.");
#endif
            }
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

            // Hide the pause menu panel via UIManager (CanvasGroup-based)
            UIManager.HidePanel(PauseMenuAddress);
            // Restore exploration HUD (VirtualGamepad)
            UIManager.ShowPanel(Santa.Core.Addressables.AddressableKeys.UIPanels.VirtualGamepad).Forget();
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
