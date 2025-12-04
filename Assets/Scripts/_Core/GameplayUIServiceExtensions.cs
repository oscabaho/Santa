using System;
using Santa.Core;
using UnityEngine;

namespace Santa.UI
{
    public static class GameplayUIServiceExtensions
{
    /// <summary>
    /// Executes the given action immediately if the gameplay UI is ready, otherwise subscribes
    /// to the Ready event and executes it once, then unsubscribes. The action is skipped if the
    /// owner MonoBehaviour is destroyed or disabled before readiness.
    /// </summary>
    public static void WhenReady(this IGameplayUIService service, MonoBehaviour owner, Action action)
    {
        if (service == null || owner == null || action == null)
            return;

        if (service.IsReady)
        {
            SafeInvoke(owner, action);
            return;
        }

        void Handler()
        {
            // Unsubscribe first to ensure single execution
            service.Ready -= Handler;
            SafeInvoke(owner, action);
        }

        service.Ready += Handler;
    }

    private static void SafeInvoke(MonoBehaviour owner, Action action)
    {
        try
        {
            if (owner != null && owner.isActiveAndEnabled)
            {
                action();
            }
        }
        catch (Exception ex)
        {
            GameLog.LogError($"GameplayUIServiceExtensions.WhenReady action threw: {ex.Message}");
        }
    }
}
}
