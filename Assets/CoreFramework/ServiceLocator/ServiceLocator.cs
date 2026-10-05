using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoreFramework.ServiceLocator
{
    /// <summary>
    /// A generic Service Locator that decouples systems by allowing them to register and 
    /// retrieve dependencies via their interface types.
    /// </summary>
    public static class ServiceLocator
    {
        // Private backing dictionary mapping Interface tpyes to their concrete implementations.
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Registers a service to a locator.
        /// Example: ServiceLocator.Register<IAudioService>(new AudioSystem());
        /// </summary>
        public static void Register<T>(T service)
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                Debug.LogError($"[ServiceLocator] Cannot register {type.Name}. It is already registered.");
                return;
            }

            _services.Add(type, service);
        }

        /// <summary>
        /// Retrieves a service from the locator. Fails fast if the service is missing.
        /// Example: var audio = ServiceLocator.Get<IAudioService>();
        /// </summary>
        public static T Get<T>()
        {
            Type type = typeof(T);

            if (!_services.TryGetValue(type, out object service))
            {
                throw new InvalidOperationException($"[ServiceLocator] Service {type.Name} is not registered!");
            }

            return (T)service;
        }

        /// <summary>
        /// Removes a service from the locator. Used during scene cleanup or shutdown.
        /// </summary>
        public static void Unregister<T>()
        {
            Type type = typeof(T);

            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
            }
        }

        /// <summary>
        /// Clears all registered services. Call this during a hard reset of the application.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}