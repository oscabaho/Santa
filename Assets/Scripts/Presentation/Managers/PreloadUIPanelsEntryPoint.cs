using Cysharp.Threading.Tasks;
using Santa.Core;
using Santa.Core.Addressables;
using VContainer;
using VContainer.Unity;

namespace Santa.Presentation.UI
{

    /// <summary>
    /// Entry point to preload frequently used UI panels to avoid first-use hitches.
    /// Currently preloads CombatUI and PauseMenu on app start.
    /// </summary>
    public class PreloadUIPanelsEntryPoint : IStartable
    {
        private readonly IUIManager _uiManager;

        [Inject]
        public PreloadUIPanelsEntryPoint(IUIManager uiManager)
        {
            _uiManager = uiManager;
        }

        public void Start()
        {
            // Fire-and-forget preload of CombatUI and PauseMenu
            PreloadPanelsAsync().Forget();
        }

        private async UniTaskVoid PreloadPanelsAsync()
        {
            try
            {
                await _uiManager.PreloadPanel(Santa.Core.Addressables.AddressableKeys.UIPanels.CombatUI);
                await _uiManager.PreloadPanel(Santa.Core.Addressables.AddressableKeys.UIPanels.PauseMenu);
            }
            catch (System.Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                GameLog.LogWarning($"PreloadUIPanelsEntryPoint: Failed to preload panels. Check Addressables configuration. Error: {ex.Message}");
#else
            _ = ex;
#endif
            }
        }
    }
}
