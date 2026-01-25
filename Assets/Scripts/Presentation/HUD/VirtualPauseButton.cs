using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Infrastructure.Input;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace Santa.Presentation.HUD
{
    /// <summary>
    /// Pause button inside the VirtualGamepad UI.
    /// Calls Santa.Core.IPauseMenuService to toggle pause state.
    /// Hook this to the Button's OnClick.
    /// </summary>
    public class VirtualPauseButton : MonoBehaviour
    {
        private InputReader _input;
        private Santa.Core.IPauseMenuService _pauseService;
        private Button _button;
        private bool _wired;

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

        private void Awake()
        {
            // Try to locate a Button on this object or its children
            _button = GetComponent<Button>() ?? GetComponentInChildren<Button>(true);
            if (_button == null)
            {
                GameLog.LogWarning("VirtualPauseButton: No Button component found to wire OnClick.");
            }
        }

        private void OnEnable()
        {
            EnsureInputReader();
            if (_button != null && !_wired)
            {
                _button.onClick.AddListener(OnPauseClicked);
                _wired = true;
                GameLog.Log("VirtualPauseButton: OnClick wired to OnPauseClicked().");
            }
        }

        private void OnDisable()
        {
            if (_button != null && _wired)
            {
                _button.onClick.RemoveListener(OnPauseClicked);
                _wired = false;
            }
        }

        public void OnPauseClicked()
        {
            GameLog.Log($"VirtualPauseButton.OnPauseClicked: _pauseService = {(_pauseService != null ? "Available" : "NULL")}");

            // Toggle via service if available
            if (_pauseService == null)
            {
                // Fallback: Lazy find in scene (since controller is local in Gameplay)
                var found = FindFirstObjectByType<Santa.UI.PauseMenuController>();
                if (found != null) _pauseService = found;
            }

            if (_pauseService != null)
            {
                _pauseService.TogglePause().Forget();
            }
            else
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning("VirtualPauseButton: IPauseMenuService not available. Ensure PauseMenuController exists in scene.");
#endif
            }
        }
    }
}
