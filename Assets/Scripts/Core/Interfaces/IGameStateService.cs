using System;

public interface IGameStateService
{
    void StartCombat();
    void EndCombat();
    event Action OnCombatStarted;
    event Action OnCombatEnded;
}
