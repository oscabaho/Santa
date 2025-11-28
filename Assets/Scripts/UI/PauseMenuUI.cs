using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Santa.UI
{
    // Simple pause menu UI that allows saving only during exploration (not in combat)
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button resumeButton;

        private Santa.Core.Save.ISaveService _saveService;
        private ICombatService _combatService;

        [Inject]
        public void Construct(Santa.Core.Save.ISaveService saveService, ICombatService combatService)
        {
            _saveService = saveService;
            _combatService = combatService;
        }

        private void OnEnable()
        {
            RefreshButtons();
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (loadButton != null) loadButton.onClick.AddListener(OnLoadClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        }

        private void OnDisable()
        {
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (loadButton != null) loadButton.onClick.RemoveListener(OnLoadClicked);
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
        }

        private void RefreshButtons()
        {
            var canSave = _saveService?.CanSaveNow() ?? true;
            if (saveButton != null) saveButton.interactable = canSave;
            // Loading is always allowed from pause
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
                // Minimal feedback; real load would restore state via services
                Debug.Log($"PauseMenuUI: Loaded save from scene '{data.sceneName}' at {data.savedAtUtc:u}.");
            }
            else
            {
                Debug.LogWarning("PauseMenuUI: No save data found.");
            }
        }

        private void OnResumeClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
