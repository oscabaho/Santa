using UnityEngine;
using VContainer;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Initializes core game systems at startup.
/// Shows initial UI (VirtualGamepad) as early as possible for input readiness.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    private const string InitialUIPanelAddress = Santa.Core.Addressables.AddressableKeys.UIPanels.VirtualGamepad;
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
    }

    async void Start()
    {
        // Show the initial UI immediately to ensure input/UI is ready before gameplay interactions.
        if (!_shown && _uiManager != null)
        {
            _ = _uiManager.ShowPanel(InitialUIPanelAddress);
            _shown = true;
        }
        else if (_uiManager == null)
        {
            // Fallback: Instantiate VirtualGamepad directly via Addressables so exploration UI is available
            try
            {
                AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(InitialUIPanelAddress);
                var go = await handle.Task;
                if (go != null)
                {
                    _shown = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    GameLog.Log("GameInitializer: Loaded VirtualGamepad via Addressables fallback.");
#endif
                }
                else
                {
                    GameLog.LogError("GameInitializer: Failed to load VirtualGamepad via Addressables fallback.");
                }
            }
            catch (System.Exception ex)
            {
                GameLog.LogError($"GameInitializer: Addressables fallback failed for VirtualGamepad. Error: {ex.Message}");
            }
        }
    }
}
