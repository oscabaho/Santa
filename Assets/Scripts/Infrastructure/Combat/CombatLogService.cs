using Santa.Core;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Implementation of ICombatLogService for broadcasting combat messages.
    /// </summary>
    public class CombatLogService : ICombatLogService
    {
        public event System.Action<string, CombatLogType> OnMessageLogged;

        private readonly System.Collections.Generic.Queue<CombatLogEntry> _recentLogs = new System.Collections.Generic.Queue<CombatLogEntry>();
        private const int MaxLogHistory = 20;

        public void LogMessage(string message, CombatLogType type = CombatLogType.Info)
        {
            if (string.IsNullOrEmpty(message))
                return;

            // Also log to Unity console in dev builds for debugging
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"[COMBAT] {message}");
#endif

            // Add to history buffer
            _recentLogs.Enqueue(new CombatLogEntry(message, type));
            if (_recentLogs.Count > MaxLogHistory)
            {
                _recentLogs.Dequeue();
            }

            // Broadcast to UI
            OnMessageLogged?.Invoke(message, type);
        }

        public System.Collections.Generic.IEnumerable<CombatLogEntry> GetRecentLogs()
        {
            return _recentLogs;
        }

        public void Clear()
        {
            _recentLogs.Clear();
            // Clear event could be added if needed for specific UI reset behavior
            // For now, UI components handle their own clearing
        }
    }
}
