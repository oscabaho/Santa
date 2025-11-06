using System.Threading.Tasks;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Entry point to preload frequently used UI panels to avoid first-use hitches.
/// Currently preloads CombatUI on app start.
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
        // Fire-and-forget preload of CombatUI
        _ = PreloadCombatUIAsync();
    }

    private async Task PreloadCombatUIAsync()
    {
        await _uiManager.PreloadPanel(UIPanelAddresses.CombatUI);
    }
}
