using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Cysharp.Threading.Tasks; // For UniTask + Forget extension

namespace Santa.UI
{
    // LEGACY: Previously handled an embedded pause menu inside VirtualGamepad.
    // Now superseded by IPauseMenuService + Addressables-based PauseMenu panel.
    // Kept for backward compatibility; can be removed once prefab migration completes.
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
        private Santa.Core.IPauseMenuService _pauseService;

        [Inject]
        public void Construct(Santa.Core.Save.ISaveService saveService, Santa.Core.IPauseMenuService pauseService = null)
        {
            _saveService = saveService;
            _pauseService = pauseService;
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
            // Prefer centralized pause service if available
            if (_pauseService != null)
            {
                _pauseService.TogglePause().Forget();
                return;
            }

            // Fallback legacy embedded panel
            if (pausePanelRoot == null) return;
            var cg = pausePanelRoot.GetComponent<CanvasGroup>();
            if (cg == null) cg = pausePanelRoot.AddComponent<CanvasGroup>();

            bool show = cg.alpha <= 0.001f;
            if (show)
            {
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                RefreshButtons();
            }
            else
            {
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
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
            if (_pauseService != null)
            {
                _pauseService.Resume();
            }
            else if (pausePanelRoot != null)
            {
                var cg = pausePanelRoot.GetComponent<CanvasGroup>();
                if (cg == null) cg = pausePanelRoot.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }

        private void OnExitClicked()
        {
            if (_pauseService != null)
            {
                _pauseService.Resume();
            }
            if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                OnResumeClicked();
            }
        }
    }
}
