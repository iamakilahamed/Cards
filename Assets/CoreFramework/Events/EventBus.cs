using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFramework.Events
{
    /// <summary>
    /// A generic, type-safe message broker. 
    /// Enforces the use of structs for events to ensure zero garbage collection allocation.
    /// </summary>
    public static class EventBus
    {
        // Maps the Type of the event struct to the delegate chain holding all subscribers.
        private static readonly Dictionary<Type, Delegate> _subscribers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// Subscribes a method to listen for a specific event type.
        /// MUST be paired with Unsubscribe in OnDisable/OnDestroy to prevent memory leaks.
        /// </summary>
        /// <typeparam name="T">An event struct</typeparam>
        /// <param name="listener">The method to call when the event fires</param>
        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (_subscribers.TryGetValue(eventType, out Delegate currentDel))
            {
                _subscribers[eventType] = Delegate.Combine(currentDel, listener);
            }
            else
            {
                _subscribers.Add(eventType, listener);
            }
        }

        /// <summary>
        /// Unsubscribes a method from an event.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (_subscribers.TryGetValue(eventType, out Delegate currentDel))
            {
                Delegate newDel = Delegate.Remove(currentDel, listener);

                if (newDel == null)
                {
                    _subscribers.Remove(eventType);
                }
                else
                {
                    _subscribers[eventType] = newDel;
                }
            }
        }

        /// <summary>
        /// Broadcasts an event to all subscribed listeners.
        /// </summary>
        /// <typeparam name="T">An event struct</typeparam>
        /// <param name="eventData">The data payload</param>
        public static void Publish<T>(T eventData) where T : struct
        {
            Type eventType = typeof(T);

            if (_subscribers.TryGetValue(eventType, out Delegate currentDel))
            {
                var action = currentDel as Action<T>;
                action?.Invoke(eventData);
            }
        }

        /// <summary>
        /// Clears all event subscriptions. Call this only during a hard application reset.
        /// </summary>
        public static void ClearAll()
        {
            _subscribers.Clear();
        }
    }
}