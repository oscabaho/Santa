using System.Collections.Generic;

public interface ILevelService
{
    List<Enemy> GetEnemiesForCurrentLevel();
    void LiberateCurrentLevel();
    void AdvanceToNextLevel();
}
