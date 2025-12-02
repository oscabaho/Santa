using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using VContainer;
using Santa.UI;
using TMPro;

namespace Santa.UI
{
    /// <summary>
    /// Pause menu UI for exploration mode.
    /// Allows saving, loading, and resuming gameplay.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button resumeButton;
        
        // Optional runtime-created UI
        private TextMeshProUGUI _lastSaveLabel;
        private CanvasGroup _toastGroup;
        private TextMeshProUGUI _toastLabel;

        private bool _isSaving; // debounce flag

        private Santa.Core.Save.ISaveService _saveService;
        private Santa.Core.IPauseMenuService _pauseService;
        private PauseMenuAnimator _animator;

        [Inject]
        public void Construct(Santa.Core.Save.ISaveService saveService, Santa.Core.IPauseMenuService pauseService)
        {
            _saveService = saveService;
            _pauseService = pauseService;
            _animator = GetComponent<PauseMenuAnimator>();
        }

        private void OnEnable()
        {
            RefreshButtons();
            UpdateLastSaveLabel();
            UpdateLoadButtonVisibility();
            if (_animator != null)
            {
                _animator.FadeIn().Forget();
            }
            if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
            if (loadButton != null) loadButton.onClick.AddListener(OnLoadClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            // Ensure buttons reflect paused state even if IsPaused is set just after show
            Cysharp.Threading.Tasks.UniTask.Void(async () =>
            {
                await Cysharp.Threading.Tasks.UniTask.NextFrame();
                RefreshButtons();
                UpdateLastSaveLabel();
                UpdateLoadButtonVisibility();
            });
        }

        private void OnDisable()
        {
            if (saveButton != null) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (loadButton != null) loadButton.onClick.RemoveListener(OnLoadClicked);
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
        }

        public void RefreshButtons()
        {
            // Enable Save when allowed by service or while paused
            var canSave = (_saveService?.CanSaveNow() ?? false) || (_pauseService?.IsPaused ?? false);
            if (saveButton != null) saveButton.interactable = canSave;
            // Loading only allowed when a save exists
            var hasSave = _saveService != null && _saveService.TryGetLastSaveTimeUtc(out var _);
            if (loadButton != null) loadButton.interactable = hasSave;
        }

        private async void OnSaveClicked()
        {
            await SaveFlow();
        }

        private async Cysharp.Threading.Tasks.UniTask SaveFlow()
        {
            if (_isSaving) return;
            if (_saveService == null) return;

            _isSaving = true;
            if (saveButton != null) saveButton.interactable = false; // debounce

            _saveService.Save();
            // Immediately reflect local device time without waiting for storage
            UpdateLastSaveLabelWithLocalNow();
            
            // Wait a frame for storage to flush before checking save existence
            await Cysharp.Threading.Tasks.UniTask.NextFrame();
            UpdateLoadButtonVisibility();

            await ShowToast("Guardado", 1.5f);

            _isSaving = false;
            RefreshButtons();
        }

        private void OnLoadClicked()
        {
            if (_saveService != null && _saveService.TryLoad(out var data))
            {
<<<<<<< Updated upstream
                // Minimal feedback; real load would restore state via services
                Debug.Log($"PauseMenuUI: Loaded save from scene '{data.sceneName}' at {data.savedAtUtc:u}.");
=======
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"PauseMenuUI: Loaded save from scene '{data.sceneName}' at {data.savedAtUtc:u}.");
#endif
                // Resume after loading
                _pauseService?.Resume();
>>>>>>> Stashed changes
            }
            else
            {
                Debug.LogWarning("PauseMenuUI: No save data found.");
            }
        }

        private void OnResumeClicked()
        {
            if (_animator != null)
            {
                _animator.FadeOut().Forget();
            }
            _pauseService?.Resume();
        }

        private void UpdateLastSaveLabel()
        {
            if (_saveService != null && _saveService.TryGetLastSaveTimeUtc(out var utc))
            {
                EnsureLastSaveLabel();
                if (_lastSaveLabel != null)
                {
                    // Additional guard against default dates
                    if (utc != default && utc.Year >= 2000)
                    {
                        var local = utc.ToLocalTime();
                        _lastSaveLabel.text = $"Último guardado: {local:dd/MM/yyyy HH:mm}";
                    }
                    else
                    {
                        Destroy(_lastSaveLabel.gameObject);
                        _lastSaveLabel = null;
                    }
                }
            }
            else
            {
                // No save exists yet: remove/hide the label entirely
                if (_lastSaveLabel != null)
                {
                    Destroy(_lastSaveLabel.gameObject);
                    _lastSaveLabel = null;
                }
            }
        }

        private void UpdateLastSaveLabelWithLocalNow()
        {
            EnsureLastSaveLabel();
            if (_lastSaveLabel == null) return;
            var local = System.DateTime.Now;
            _lastSaveLabel.text = $"Último guardado: {local:dd/MM/yyyy HH:mm}";
        }

        private void UpdateLoadButtonVisibility()
        {
            // Check if a valid save exists (TryGetLastSaveTimeUtc already validates)
            var hasSave = _saveService != null && _saveService.TryGetLastSaveTimeUtc(out var _);
            if (loadButton != null)
            {
                // Hide entirely if there is no previous save
                loadButton.gameObject.SetActive(hasSave);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.Log($"PauseMenuUI: Load button visibility = {hasSave}");
#endif
            }
        }

        private void EnsureLastSaveLabel()
        {
            if (_lastSaveLabel != null) return;
            var go = new GameObject("LastSaveLabel", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            // Centered below the title near the top
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0, -84);
            rt.sizeDelta = new Vector2(900, 48);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.color = Santa.Core.Config.UIColors.InfoText;
            text.fontSize = 28; // base size
            text.enableAutoSizing = true; // adapt to resolution
            text.fontSizeMin = 20;
            text.fontSizeMax = 40;
            text.raycastTarget = false;
            text.text = "";
            _lastSaveLabel = text;
        }

        private async UniTask ShowToast(string message, float durationSeconds)
        {
            EnsureToastUI();
            if (_toastLabel == null || _toastGroup == null) return;
            _toastLabel.text = message;
            // Fade in
            _toastGroup.alpha = 0f;
            float t = 0f;
            while (t < 0.15f)
            {
                t += UnityEngine.Time.unscaledDeltaTime;
                _toastGroup.alpha = Mathf.Clamp01(t / 0.15f);
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            _toastGroup.alpha = 1f;

            // Hold
            await Cysharp.Threading.Tasks.UniTask.Delay(TimeSpan.FromSeconds(durationSeconds), DelayType.UnscaledDeltaTime);

            // Fade out
            t = 0f;
            while (t < 0.2f)
            {
                t += UnityEngine.Time.unscaledDeltaTime;
                _toastGroup.alpha = 1f - Mathf.Clamp01(t / 0.2f);
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }
            _toastGroup.alpha = 0f;
        }

        private void EnsureToastUI()
        {
            if (_toastGroup != null && _toastLabel != null) return;
            var go = new GameObject("Toast", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0f);
            rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0, 40);
            rt.sizeDelta = new Vector2(560, 72);

            var cg = go.AddComponent<CanvasGroup>();
            cg.interactable = false;
            cg.blocksRaycasts = false;
            cg.alpha = 0f;

            var bg = go.AddComponent<Image>();
            bg.color = Santa.Core.Config.UIColors.ToastBackground;
            bg.raycastTarget = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var lrt = labelGo.GetComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0);
            lrt.anchorMax = new Vector2(1, 1);
            lrt.offsetMin = new Vector2(16, 8);
            lrt.offsetMax = new Vector2(-16, -8);
            var text = labelGo.AddComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.color = Santa.Core.Config.UIColors.Success;
            text.fontSize = 30;
            text.enableAutoSizing = true;
            text.fontSizeMin = 24;
            text.fontSizeMax = 40;
            text.text = string.Empty;
            text.raycastTarget = false;

            _toastGroup = cg;
            _toastLabel = text;
        }
    }
}
