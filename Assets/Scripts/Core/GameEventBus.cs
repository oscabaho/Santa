using System;
using System.Collections.Generic;

/// <summary>
/// Bus de eventos global para el proyecto. Permite publicar y suscribirse a eventos de tipo base.
/// </summary>
public class GameEventBus
{
    private static GameEventBus _instance;
    public static GameEventBus Instance => _instance ?? (_instance = new GameEventBus());

    private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

    /// <summary>
    /// Publica un evento a todos los suscriptores del tipo correspondiente.
    /// </summary>
    public void Publish<TEvent>(TEvent evt)
    {
        var type = typeof(TEvent);
        if (_subscribers.TryGetValue(type, out var delegates))
        {
            foreach (var del in delegates)
            {
                if (del is Action<TEvent> action)
                    action.Invoke(evt);
            }
        }
    }

    /// <summary>
    /// Suscribe un handler a un tipo de evento.
    /// </summary>
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (!_subscribers.ContainsKey(type))
            _subscribers[type] = new List<Delegate>();
        _subscribers[type].Add(handler);
    }

    /// <summary>
    /// Desuscribe un handler de un tipo de evento.
    /// </summary>
    public void Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        var type = typeof(TEvent);
        if (_subscribers.TryGetValue(type, out var delegates))
        {
            delegates.Remove(handler);
            if (delegates.Count == 0)
                _subscribers.Remove(type);
        }
    }
}
