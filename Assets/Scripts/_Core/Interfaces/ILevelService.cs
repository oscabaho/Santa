public interface ILevelService
{
    void LiberateCurrentLevel(); // Keep method name for now; could be renamed to FreeCurrentLevel if desired
    void AdvanceToNextLevel();
}