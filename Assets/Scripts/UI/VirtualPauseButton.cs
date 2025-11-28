using UnityEngine;
using VContainer;

namespace Santa.UI
{
    // Attach to a Pause icon Button; call OnPauseClicked from OnClick
    public class VirtualPauseButton : MonoBehaviour
    {
        [SerializeField] private VirtualPauseMenuBinder binder; // optional
        private InputReader _input;

        [Inject]
        public void Construct(InputReader input)
        {
            _input = input;
        }

        public void OnPauseClicked()
        {
            // Toggle panel if binder is present
            if (binder == null)
            {
                // Try to find binder on parent hierarchy lazily
                binder = GetComponentInParent<VirtualPauseMenuBinder>(true);
            }
            binder?.TogglePausePanel();

            // Raise pause event for any listeners
            _input?.RaisePause();
        }
    }
}
