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
    /// Gets a registered service. Logs an error if not found.
    /// </summary>
    public static T Get<T>()
    {
        if (TryGet<T>(out var service))
        {
            return service;
        }

        #if UNITY_EDITOR
        GameLog.LogError($"ServiceLocator: Service of type '{typeof(T).Name}' not found.");
        #endif

        return default;
    }

    /// <summary>
    /// Tries to get a registered service without logging an error if not found.
    /// </summary>
    /// <returns>True if the service was found, otherwise false.</returns>
    public static bool TryGet<T>(out T service)
    {
        var type = typeof(T);
        if (_services.TryGetValue(type, out var serviceObject))
        {
            service = (T)serviceObject;
            return true;
        }

        service = default;
        return false;
    }
}