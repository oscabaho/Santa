using UnityEngine;
using System;

/// <summary>
/// Manages the global state of the game (Exploration, Combat) using events to announce changes.
/// </summary>
public class GameStateManager : MonoBehaviour, IGameStateService
{
    internal static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        Exploration, // Player is moving around the world
        Combat       // Player is in a turn-based battle
    }

    public static GameState CurrentState { get; private set; }

    // Events to announce state changes to any subscribed scripts.
    public static event Action OnCombatStarted;
    public static event Action OnCombatEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Register service for decoupled access
        ServiceLocator.Register<IGameStateService>(this);

        // The game always starts in Exploration mode.
        CurrentState = GameState.Exploration;
    }

    private void OnDestroy()
    {
        var registered = ServiceLocator.Get<IGameStateService>();
        if ((UnityEngine.Object)registered == (UnityEngine.Object)this)
            ServiceLocator.Unregister<IGameStateService>();
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Switches the game to Combat mode and invokes the OnCombatStarted event.
    /// </summary>
    public static void StartCombat()
    {
        if (CurrentState == GameState.Combat) return;

        CurrentState = GameState.Combat;
        Debug.Log("Game State changed to: Combat");
        OnCombatStarted?.Invoke();
    }

    /// <summary>
    /// Switches the game back to Exploration mode and invokes the OnCombatEnded event.
    /// </summary>
    public static void EndCombat()
    {
        if (CurrentState == GameState.Exploration) return;

        CurrentState = GameState.Exploration;
        Debug.Log("Game State changed to: Exploration");
        OnCombatEnded?.Invoke();
    }

    // IGameStateService explicit implementation (events proxy to static events)
    event Action IGameStateService.OnCombatStarted
    {
        add { OnCombatStarted += value; }
        remove { OnCombatStarted -= value; }
    }

    event Action IGameStateService.OnCombatEnded
    {
        add { OnCombatEnded += value; }
        remove { OnCombatEnded -= value; }
    }

    void IGameStateService.StartCombat() => StartCombat();
    void IGameStateService.EndCombat() => EndCombat();
}
