using UnityEngine;
using UnityEngine.UI;
using VContainer;
using Cysharp.Threading.Tasks;

namespace Santa.UI
{
    /// <summary>
    /// Pause button inside the VirtualGamepad UI.
    /// Calls IPauseMenuService to toggle pause state instead of directly manipulating panels.
    /// Hook this to the Button's OnClick.
    /// </summary>
    public class VirtualPauseButton : MonoBehaviour
    {
        private InputReader _input;
        private Santa.Core.IPauseMenuService _pauseService;
        private Button _button;
        private bool _wired;

        [Inject]
        public void Construct(InputReader input, Santa.Core.IPauseMenuService pauseService = null)
        {
               GameLog.Log($"VirtualPauseButton.Construct: pauseService = {(pauseService != null ? "INJECTED" : "NULL")}");
            _input = input;
            _pauseService = pauseService; // may be null if controller not in scene
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
