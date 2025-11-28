using System.Threading.Tasks;
using VContainer;
using VContainer.Unity;

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
        _ = PreloadPanelsAsync();
    }

    private async Task PreloadPanelsAsync()
    {
        await _uiManager.PreloadPanel(Santa.Core.Addressables.AddressableKeys.UIPanels.CombatUI);
        await _uiManager.PreloadPanel(Santa.Core.Addressables.AddressableKeys.UIPanels.PauseMenu);
    }
}
