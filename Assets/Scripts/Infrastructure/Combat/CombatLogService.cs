using Santa.Core;

namespace Santa.Infrastructure.Combat
{
    /// <summary>
    /// Implementation of ICombatLogService for broadcasting combat messages.
    /// </summary>
    public class CombatLogService : ICombatLogService
    {
        public event System.Action<string, CombatLogType> OnMessageLogged;

        public void LogMessage(string message, CombatLogType type = CombatLogType.Info)
        {
            if (string.IsNullOrEmpty(message))
                return;

            // Also log to Unity console in dev builds for debugging
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GameLog.Log($"[COMBAT] {message}");
#endif

            // Broadcast to UI
            OnMessageLogged?.Invoke(message, type);
        }

        public void Clear()
        {
            // Clear event could be added if needed for specific UI reset behavior
            // For now, UI components handle their own clearing
        }
    }
}
