using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Manager que controla el ciclo de vida optimizado de la UpgradeUI.
/// Precarga la UI al inicio del combate y la libera al volver al menú.
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
        GameLog.Log("UpgradeUILifecycleManager: Initialized and monitoring game state.");
    }

    void ITickable.Tick()
    {
        GameState currentState = _gameStateService.CurrentState;

        // Si entramos en combate, precargar la UI
        if (currentState == GameState.Combat && _previousState != GameState.Combat)
        {
            OnEnterCombat();
        }
            // Si salimos del combate a exploración, liberar la UI (opcional)
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

        GameLog.Log("UpgradeUILifecycleManager: Entering combat. Preloading UpgradeUI...");
        await _upgradeUILoader.PreloadAsync();
        _hasPreloadedForCombat = true;
    }

        private void OnExitCombat()
    {
            GameLog.Log("UpgradeUILifecycleManager: Exiting combat. Releasing UpgradeUI resources (optional)...");
        _upgradeUILoader.Release();
        _hasPreloadedForCombat = false;
    }
}
