using System;

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