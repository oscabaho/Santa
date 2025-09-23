using System;
using System.Collections.Generic;

/// <summary>
/// Bus de eventos global para el proyecto. Permite publicar y suscribirse a eventos de tipo base.
/// </summary>
public class GameEventBus : IEventBus
{
    private static GameEventBus _instance;
    public static GameEventBus Instance => _instance ?? (_instance = new GameEventBus());

    // Register the global instance in the ServiceLocator for decoupled access.
    static GameEventBus()
    {
        ServiceLocator.Register<IEventBus>(Instance);
    }

    // Protect access to the subscriber dictionary for thread-safety and reentrancy.
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();
    private readonly object _lock = new object();

    /// <summary>
    /// Publica un evento a todos los suscriptores del tipo correspondiente.
    /// </summary>
    public void Publish<TEvent>(TEvent evt)
    {
        var type = typeof(TEvent);
        List<Delegate> snapshot = null;
        lock (_lock)
        {
            if (_subscribers.TryGetValue(type, out var delegates))
            {
                // Create a snapshot to avoid issues if subscribers modify the list during invocation
                snapshot = new List<Delegate>(delegates);
            }
        }

        if (snapshot == null) return;
        foreach (var del in snapshot)
        {
            if (del is Action<TEvent> action)
            {
                try
                {
                    action.Invoke(evt);
                }
                catch (Exception ex)
                {
                    // Ensure one failing subscriber doesn't prevent others from receiving the event.
                    UnityEngine.Debug.LogError($"GameEventBus: Exception in subscriber for {type.Name}: {ex}");
                }
            }
        }
    }

    /// <summary>
    /// Suscribe un handler a un tipo de evento.
    /// </summary>
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        lock (_lock)
        {
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();
            _subscribers[type].Add(handler);
        }
    }

    /// <summary>
    /// Desuscribe un handler de un tipo de evento.
    /// </summary>
    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        lock (_lock)
        {
            if (_subscribers.TryGetValue(type, out var delegates))
            {
                delegates.Remove(handler);
                if (delegates.Count == 0)
                    _subscribers.Remove(type);
            }
        }
    }
}
