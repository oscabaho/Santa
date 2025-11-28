using UnityEngine;
using VContainer;

namespace Santa.UI
{
    // Listens to pause input and toggles the Pause Menu panel via UIManager
    public class PauseMenuController : MonoBehaviour
    {
        private IUIManager _uiManager;
        private InputReader _input;
        private const string PauseMenuAddress = Santa.Core.Addressables.AddressableKeys.UIPanels.PauseMenu;

        [Inject]
        public void Construct(IUIManager uiManager, InputReader inputReader)
        {
            _uiManager = uiManager;
            _input = inputReader;
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.PauseEvent += OnPause;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.PauseEvent -= OnPause;
            }
        }

        private async void OnPause()
        {
            // Show the Pause Menu panel; it will enable/disable Save based on combat state
            await _uiManager.ShowPanel(PauseMenuAddress);
        }
    }
}
