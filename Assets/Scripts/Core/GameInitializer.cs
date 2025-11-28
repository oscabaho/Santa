using UnityEngine;
using VContainer;

public class GameInitializer : MonoBehaviour
{
    private const string InitialUIPanelAddress = Santa.Core.Addressables.AddressableKeys.UIPanels.VirtualGamepad;
    private const string PauseMenuAddress = Santa.Core.Addressables.AddressableKeys.UIPanels.PauseMenu;
    private IUIManager _uiManager;
    private bool _shown;

    [Inject]
    public void Construct(IUIManager uiManager)
    {
        _uiManager = uiManager;
    }

    void Awake()
    {
        // Mobile Optimization: Set target frame rate
        Application.targetFrameRate = 60;

        // Try to show as early as possible (before first FixedUpdate) if DI already happened.
        if (_uiManager != null)
        {
            _ = _uiManager.ShowPanel(InitialUIPanelAddress);
            _shown = true;
        }
        else
        {
#if UNITY_EDITOR
            // Fallback (Editor only): attempt to find a UIManager in the scene if DI hasn't injected yet.
            var fallback = Object.FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
            if (fallback != null)
            {
                _uiManager = fallback;
                _ = _uiManager.ShowPanel(InitialUIPanelAddress);
                _shown = true;
                GameLog.Log("GameInitializer: Fallback acquired UIManager from scene hierarchy (Editor only).");
            }
#endif
        }
    }

    void Start()
    {
        // Show the initial UI immediately to ensure input/UI is ready before gameplay interactions.
        if (!_shown && _uiManager != null)
        {
            _ = _uiManager.ShowPanel(InitialUIPanelAddress);
            _shown = true;
        }
        else if (_uiManager == null)
        {
            GameLog.LogError("GameInitializer: IUIManager service was not injected. Make sure it's registered in a LifetimeScope.");
        }

        // Optional: Preload Pause Menu ready for input-triggered opening
        if (_uiManager != null)
        {
            _ = _uiManager.PreloadPanel(PauseMenuAddress);
        }
    }
}
