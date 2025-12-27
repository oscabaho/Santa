using Cysharp.Threading.Tasks;

namespace Santa.Core
{
    /// <summary>
    /// Service for managing pause menu state and time control.
    /// Only active during exploration (not in combat).
    /// </summary>
    public interface IPauseMenuService
    {
        /// <summary>
        /// True if game is currently paused
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Shows the pause menu and pauses game time.
        /// Only works during exploration mode.
        /// </summary>
        UniTask ShowPauseMenu();

        /// <summary>
        /// Hides the pause menu and resumes game time.
        /// </summary>
        void Resume();

        /// <summary>
        /// Toggles pause state
        /// </summary>
        UniTask TogglePause();
    }
}
