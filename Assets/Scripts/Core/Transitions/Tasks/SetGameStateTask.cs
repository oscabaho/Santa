using System.Collections;
using UnityEngine;

/// <summary>
/// A transition task that sets the game state via a service.
/// Assumes a service implementing IGameStateService is registered with the ServiceLocator.
/// </summary>
[CreateAssetMenu(fileName = "NewSetGameStateTask", menuName = "Transitions/Tasks/Set Game State")]
public class SetGameStateTask : TransitionTask
{
    [SerializeField]
    private GameState newGameState;

    public override IEnumerator Execute(TransitionContext context)
    {
        // This is a placeholder for your actual game state service interface.
        // You should have an interface like this for managing game state.
        // Example: public interface IGameStateService { void SetState(GameState state); }
        var gameStateService = ServiceLocator.Get<IGameStateService>();
        if (gameStateService != null)
        {
            gameStateService.SetState(newGameState);
        }
        else
        {
            GameLog.LogWarning("SetGameStateTask: IGameStateService not found in ServiceLocator.");
        }
        yield break;
    }
}