using UnityEngine;

/// <summary>
/// Manages the global state of the game, such as Exploration or Combat.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public enum GameState
    {
        Exploration, // Player is moving around the world
        Combat       // Player is in a turn-based battle
    }

    // Simple static property for easy access from any script.
    public static GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // The game always starts in Exploration mode.
        CurrentState = GameState.Exploration;
    }

    /// <summary>
    /// Call this method to switch the game to Combat mode.
    /// </summary>
    public static void StartCombat()
    {
        CurrentState = GameState.Combat;
        // Here you could also trigger events, like GameEventBus.Instance.Publish(new CombatStartedEvent());
        Debug.Log("Game State changed to: Combat");
    }

    /// <summary>
    /// Call this method to switch the game back to Exploration mode.
    /// </summary>
    public static void EndCombat()
    {
        CurrentState = GameState.Exploration;
        // Here you could also trigger events, like GameEventBus.Instance.Publish(new CombatEndedEvent());
        Debug.Log("Game State changed to: Exploration");
    }
}
