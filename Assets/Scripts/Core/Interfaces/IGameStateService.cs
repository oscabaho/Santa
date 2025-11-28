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
    /// <param name="playerWon">True if the player won the combat, false otherwise.</param>
    void EndCombat(bool playerWon);

    /// <summary>
    /// Event fired when combat begins.
    /// </summary>
    event Action OnCombatStarted;

    /// <summary>
    /// Event fired when combat ends. Parameter indicates if player won.
    /// </summary>
    event Action<bool> OnCombatEnded;
}