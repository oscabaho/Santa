namespace Santa.Core
{
    /// <summary>
    /// Type of combat log message for color-coding and filtering.
    /// </summary>
    public enum CombatLogType
    {
        Info,           // General information (white/gray)
        Damage,         // Normal damage dealt (yellow)
        Critical,       // Critical hits (orange/red)
        Miss,           // Attacks that missed (blue)
        Death,          // Combatant defeated (dark red)
        Heal,           // Healing effects (green)
        ActionPoints    // AP changes (cyan)
    }

    /// <summary>
    /// Represents a single log entry with message and type.
    /// </summary>
    public struct CombatLogEntry
    {
        public string Message;
        public CombatLogType Type;

        public CombatLogEntry(string message, CombatLogType type)
        {
            Message = message;
            Type = type;
        }
    }

    /// <summary>
    /// Service for broadcasting combat log messages to the UI.
    /// </summary>
    public interface ICombatLogService
    {
        /// <summary>
        /// Broadcast a combat message to all listeners.
        /// </summary>
        /// <param name="message">The message text to display</param>
        /// <param name="type">Type of message for styling</param>
        void LogMessage(string message, CombatLogType type = CombatLogType.Info);

        /// <summary>
        /// Event that fires when a new message is logged.
        /// Listeners receive (message, type).
        /// </summary>
        event System.Action<string, CombatLogType> OnMessageLogged;

        /// <summary>
        /// Retrieves the most recent log entries.
        /// Useful for UI components initializing late.
        /// </summary>
        System.Collections.Generic.IEnumerable<CombatLogEntry> GetRecentLogs();

        /// <summary>
        /// Clear all log messages (useful when starting new combat).
        /// </summary>
        void Clear();
    }
}
