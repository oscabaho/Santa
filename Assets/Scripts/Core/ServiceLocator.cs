using System;
using System.Collections.Generic;

/// <summary>
/// Un localizador de servicios muy ligero usado para registrar y obtener interfaces de managers.
/// Mejora la encapsulación evitando depender de singletons públicos concretos.
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
    private static readonly object _lock = new object();

    public static void Register<TService>(TService implementation) where TService : class
    {
        if (implementation == null) throw new ArgumentNullException(nameof(implementation));
        lock (_lock)
        {
            _services[typeof(TService)] = implementation;
        }
    }

    public static TService Get<TService>() where TService : class
    {
        lock (_lock)
        {
            if (_services.TryGetValue(typeof(TService), out var impl))
            {
                return impl as TService;
            }
        }
        return null;
    }

    public static void Unregister<TService>() where TService : class
    {
        lock (_lock)
        {
            _services.Remove(typeof(TService));
        }
    }
}
