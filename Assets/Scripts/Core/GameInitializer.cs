using Cysharp.Threading.Tasks;
using Santa.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using VContainer;

/// <summary>
/// Initializes core game systems at startup.
/// Shows initial UI (VirtualGamepad) as early as possible for input readiness.
/// </summary>
public class GameInitializer : MonoBehaviour
{
    private const string InitialUIPanelAddress = Santa.Core.Addressables.AddressableKeys.UIPanels.VirtualGamepad;
    private IUIManager _uiManager;
    // [Inject] - Removed. We find UIManager manually.
    // public void Construct(IUIManager uiManager) { ... }

    void Awake()
    {
        // Mobile Optimization: Set target frame rate
        Application.targetFrameRate = 60;

        _uiManager = FindFirstObjectByType<Santa.Presentation.UI.UIManager>();
        if (_uiManager == null)
        {
            // Try interface search fallback if needed, though concrete type usually works for Monobehaviours
            var obj = FindFirstObjectByType<Santa.Presentation.UI.UIManager>();
            if (obj != null) _uiManager = obj;
        }

        // Initialize Debug Console for APK troubleshooting
#if !UNITY_EDITOR
        if (FindFirstObjectByType<Santa.Core.Debug.RuntimeDebugConsole>() == null)
        {
            var debugGo = new GameObject("[DebugConsole]");
            debugGo.AddComponent<Santa.Core.Debug.RuntimeDebugConsole>();
        }
#endif
    }

    async Cysharp.Threading.Tasks.UniTaskVoid Start()
    {
        // Simply show the initial UI immediately when this component starts.
        // Since GameInitializer will now be in the Gameplay scene, this runs only when Gameplay loads.
        if (_uiManager != null)
        {
            await _uiManager.ShowPanel(InitialUIPanelAddress);
        }
        else
        {
            // If UIManager is missing, log error instead of creating duplicate
            GameLog.LogError("GameInitializer: UIManager is null. Cannot show initial UI.");
        }
    }
}
