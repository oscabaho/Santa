using System;

namespace Santa.Core
{
    /// <summary>
    /// Interface for event bus system enabling decoupled communication between systems.
    /// Uses a publish-subscribe pattern with strongly-typed events.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an event to all registered subscribers of that event type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to publish.</typeparam>
        /// <param name="evt">The event data to publish.</param>
        void Publish<TEvent>(TEvent evt);
        
        /// <summary>
        /// Subscribes a handler to receive events of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to subscribe to.</typeparam>
        /// <param name="handler">The callback to invoke when the event is published.</param>
        void Subscribe<TEvent>(Action<TEvent> handler);
        
        /// <summary>
        /// Unsubscribes a handler from receiving events of the specified type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to unsubscribe from.</typeparam>
        /// <param name="handler">The callback to remove from the subscription list.</param>
        void Unsubscribe<TEvent>(Action<TEvent> handler);
    }
}
