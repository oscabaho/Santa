using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Santa.UI
{
    // Mobile-first Pause Menu living inside the Virtual Gamepad hierarchy
    public class VirtualPauseMenuBinder : MonoBehaviour
    {
        [Header("Panel Root")]
        [SerializeField] private GameObject pausePanelRoot;

        [Header("Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button exitButton; // optional

        [Header("Exit Options (Optional)")]
        [SerializeField] private string mainMenuSceneName; // if set, loads this scene on Exit

        private Santa.Core.Save.ISaveService _saveService;
        private ICombatService _combatService;
        private ILevelService _levelService; // optional for level flow
        private IGameStateService _gameState; // optional for global state

        [Inject]
        public void Construct(Santa.Core.Save.ISaveService saveService, ICombatService combatService = null, ILevelService levelService = null, IGameStateService gameState = null)
        {
            _saveService = saveService;
            _combatService = combatService;
            _levelService = levelService;
            _gameState = gameState;
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (loadButton != null) loadButton.onClick.AddListener(OnLoadClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (exitButton != null) exitButton.onClick.AddListener(OnExitClicked);
            RefreshButtons();
        }

        private void Unbind()
        {
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (loadButton != null) loadButton.onClick.RemoveListener(OnLoadClicked);
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (exitButton != null) exitButton.onClick.RemoveListener(OnExitClicked);
        }

        public void TogglePausePanel()
        {
            if (pausePanelRoot == null) return;
            bool next = !pausePanelRoot.activeSelf;
            pausePanelRoot.SetActive(next);
            if (next) RefreshButtons();
        }

        private void RefreshButtons()
        {
            bool canSave = _saveService?.CanSaveNow() ?? true;
            if (saveButton != null) saveButton.interactable = canSave;
            if (loadButton != null) loadButton.interactable = true;
        }

        private void OnSaveClicked()
        {
            _saveService?.Save();
            RefreshButtons();
        }

        private void OnLoadClicked()
        {
            if (_saveService != null && _saveService.TryLoad(out var data))
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"VirtualPauseMenu: Loaded save from scene '{data.sceneName}' at {data.savedAtUtc:u}.");
#endif
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("VirtualPauseMenu: No save data found.");
#endif
            }
        }

        private void OnResumeClicked()
        {
            if (pausePanelRoot != null)
            {
                pausePanelRoot.SetActive(false);
            }
        }

        private void OnExitClicked()
        {
            // Preferred: load main menu if a scene name is provided
            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
                return;
            }

            // Fallback: ensure we are in exploration state
            _gameState?.EndCombat(true);

            // Otherwise, minimal behavior: hide panel
            OnResumeClicked();
        }
    }
}
