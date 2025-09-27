using System;

// The enum is defined here so any service client can access it without needing a reference to the concrete manager class.
public enum GameState
{
    Exploration, // Player is moving around the world
    Combat       // Player is in a turn-based battle
}

public interface IGameStateService
{
    /// <summary>
    /// Gets the current state of the game.
    /// </summary>
    GameState CurrentState { get; }

    /// <summary>
    /// Switches the game to Combat mode.
    /// </summary>
    void StartCombat();

    /// <summary>
    /// Switches the game back to Exploration mode.
    /// </summary>
    void EndCombat();

    /// <summary>
    /// Event fired when combat begins.
    /// </summary>
    event Action OnCombatStarted;

    /// <summary>
    /// Event fired when combat ends.
    /// </summary>
    event Action OnCombatEnded;
}