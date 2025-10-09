using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple static Service Locator for managing global services.
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    /// <summary>
    /// Registers a service instance.
    /// </summary>
    public static void Register<T>(T service)
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            #if UNITY_EDITOR
            GameLog.LogWarning($"ServiceLocator: Service of type '{type.Name}' is already registered. Overwriting.");
            #endif
            _services[type] = service;
        }
        else
        {
            _services.Add(type, service);
        }
    }

    /// <summary>
    /// Unregisters a service instance.
    /// </summary>
    public static void Unregister<T>()
    {
        var type = typeof(T);
        if (_services.ContainsKey(type))
        {
            _services.Remove(type);
        }
    }

    /// <summary>
    /// Gets a registered service.
    /// </summary>
    /// <returns>The service instance, or null if not found.</returns>
    public static T Get<T>()
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var service))
        {
            return (T)service;
        }

    #if UNITY_EDITOR
    // Don't log an error for the initial request in the initializer, as the service might be about to be created.
    // The initializer itself should handle warnings if the service is still null after instantiation.
    GameLog.LogError($"ServiceLocator: Service of type '{type.Name}' not found.");
    #endif

        return default;
    }
}