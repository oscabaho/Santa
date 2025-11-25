using System;
using UnityEngine;

/// <summary>
/// Manages the global state of the game (Exploration, Combat) using events to announce changes.
/// This is a service-based implementation, intended to be accessed via the IGameStateService interface.
/// </summary>
public class GameStateManager : MonoBehaviour, IGameStateService
{
    public GameState CurrentState { get; private set; }

    public event Action OnCombatStarted;
    public event Action<bool> OnCombatEnded;

    // --- Unity Methods ---

    private void Awake()
    {
        // The game always starts in Exploration mode.
        CurrentState = GameState.Exploration;
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
    public void EndCombat(bool playerWon)
    {
        if (CurrentState == GameState.Exploration) return;

        CurrentState = GameState.Exploration;
        GameLog.Log($"Game State changed to: Exploration. Player Won: {playerWon}");
        OnCombatEnded?.Invoke(playerWon);
    }
}