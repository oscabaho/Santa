using UnityEngine;

/// <summary>
/// Lightweight runtime logging wrapper. Calls through to UnityEngine.Debug only when
/// one of the following is true: running in the Editor, a Development build, or when
/// the GAME_LOGS_ENABLED symbol is defined.
/// This makes it easy to strip logs from release/mobile builds by not defining the symbol.
/// </summary>
public static class GameLog
{
    // Enable logs when in the editor or in a development build.
#if UNITY_EDITOR || DEVELOPMENT_BUILD || GAME_LOGS_ENABLED
    public static void Log(object message) => Debug.Log(message);
    public static void Log(object message, UnityEngine.Object context) => Debug.Log(message, context);
    public static void LogWarning(object message) => Debug.LogWarning(message);
    public static void LogWarning(object message, UnityEngine.Object context) => Debug.LogWarning(message, context);
    public static void LogError(object message) => Debug.LogError(message);
    public static void LogError(object message, UnityEngine.Object context) => Debug.LogError(message, context);
    public static void LogFormat(string format, params object[] args) => Debug.LogFormat(format, args);
    public static void LogException(System.Exception ex) => Debug.LogException(ex);
#else
    // No-op in release builds unless GAME_LOGS_ENABLED is specified.
    public static void Log(object message) { }
    public static void Log(object message, UnityEngine.Object context) { }
    public static void LogWarning(object message) { }
    public static void LogWarning(object message, UnityEngine.Object context) { }
    public static void LogError(object message) { }
    public static void LogError(object message, UnityEngine.Object context) { }
    public static void LogFormat(string format, params object[] args) { }
    public static void LogException(System.Exception ex) { }
#endif
}