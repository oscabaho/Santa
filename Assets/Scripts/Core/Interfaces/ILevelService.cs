namespace Santa.Core
{
    /// <summary>
    /// Interface for managing level progression and state.
    /// Handles level liberation (completion) and advancement to next levels.
    /// </summary>
    public interface ILevelService
    {
        /// <summary>
        /// Marks the current level as liberated (completed).
        /// Typically changes visual state to show the level is cleared.
        /// </summary>
        void LiberateCurrentLevel();
        
        /// <summary>
        /// Advances to the next level in the progression sequence.
        /// </summary>
        void AdvanceToNextLevel();
    }
}