using UnityEngine;
using System;

/// <summary>
/// Manages the global state of the game (Exploration, Combat) using events to announce changes.
/// This is a service-based implementation, intended to be accessed via the IGameStateService interface.
/// </summary>
public class GameStateManager : MonoBehaviour, IGameStateService
{
    // Private singleton instance. Access should be through the ServiceLocator.
    private static GameStateManager Instance { get; set; }

    // --- IGameStateService Implementation ---

    public GameState CurrentState { get; private set; }

    public event Action OnCombatStarted;
    public event Action OnCombatEnded;

    // --- Unity Methods ---

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Register this instance as the IGameStateService
        ServiceLocator.Register<IGameStateService>(this);

        // The game always starts in Exploration mode.
        CurrentState = GameState.Exploration;
    }

    private void OnDestroy()
    {
        // Unregister the service if this is the instance being destroyed.
        if (Instance == this)
        {
            ServiceLocator.Unregister<IGameStateService>();
            Instance = null;
        }
    }

    // --- Public Methods (from Interface) ---

    /// <summary>
    /// Switches the game to Combat mode and invokes the OnCombatStarted event.
    /// </summary>
    public void StartCombat()
    {
        if (CurrentState == GameState.Combat) return;

        CurrentState = GameState.Combat;
        GameLog.Log("Game State changed to: Combat");
        OnCombatStarted?.Invoke();
    }

    /// <summary>
    /// Switches the game back to Exploration mode and invokes the OnCombatEnded event.
    /// </summary>
    public void EndCombat()
    {
        if (CurrentState == GameState.Exploration) return;

        CurrentState = GameState.Exploration;
        GameLog.Log("Game State changed to: Exploration");
        OnCombatEnded?.Invoke();
    }

    /// <summary>
    /// Sets the game state. Fires combat start/end events when transitioning into or out of Combat.
    /// Implements IGameStateService.SetState.
    /// </summary>
    public void SetState(GameState state)
    {
        if (CurrentState == state) return;

        var previous = CurrentState;

        if (state == GameState.Combat)
        {
            StartCombat();
            return;
        }

        if (previous == GameState.Combat)
        {
            CurrentState = state;
            GameLog.Log($"Game State changed to: {state}");
            OnCombatEnded?.Invoke();
            return;
        }

        CurrentState = state;
        GameLog.Log($"Game State changed to: {state}");
    }
}