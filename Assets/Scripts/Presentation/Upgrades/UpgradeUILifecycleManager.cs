using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Manager que controla el ciclo de vida optimizado de la UpgradeUI.
/// Preloads the Upgrade UI at combat start and releases it when returning to the menu.
/// </summary>
public class UpgradeUILifecycleManager : IStartable, ITickable
{
    private readonly UpgradeUILoader _upgradeUILoader;
    private readonly IGameStateService _gameStateService;

    private GameState _previousState;
    private bool _hasPreloadedForCombat = false;

    [Inject]
    public UpgradeUILifecycleManager(
        UpgradeUILoader upgradeUILoader,
        IGameStateService gameStateService)
    {
        _upgradeUILoader = upgradeUILoader;
        _gameStateService = gameStateService;
    }

    void IStartable.Start()
    {
        _previousState = _gameStateService.CurrentState;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("UpgradeUILifecycleManager: Initialized and monitoring game state.");
#endif
    }

    void ITickable.Tick()
    {
        GameState currentState = _gameStateService.CurrentState;

        // On entering combat, preload the UI
        if (currentState == GameState.Combat && _previousState != GameState.Combat)
        {
            OnEnterCombat();
        }
        // On leaving combat to exploration, release the UI (optional)
        else if (currentState == GameState.Exploration && _previousState == GameState.Combat)
        {
            OnExitCombat();
        }

        _previousState = currentState;
    }

    private async void OnEnterCombat()
    {
        if (_hasPreloadedForCombat)
            return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("UpgradeUILifecycleManager: Entering combat. Preloading UpgradeUI...");
#endif
        await _upgradeUILoader.PreloadAsync();
        _hasPreloadedForCombat = true;
    }

    private void OnExitCombat()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        GameLog.Log("UpgradeUILifecycleManager: Exiting combat. Releasing UpgradeUI resources (optional)...");
#endif
        _upgradeUILoader.Release();
        _hasPreloadedForCombat = false;
    }
}
