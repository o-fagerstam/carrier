using System;
using System.Collections.Generic;
using UnityEngine;

namespace ServiceLocator {
    public class MonoBehaviourServiceLocator {
        private MonoBehaviourServiceLocator() {}

        private readonly Dictionary<string, MonoBehaviourService>
            _services = new Dictionary<string, MonoBehaviourService>();
    
        public static MonoBehaviourServiceLocator Current { get; private set; }

        public static void Initialize() {
            Current = new MonoBehaviourServiceLocator();
        }

        public T Get<T>() where T : MonoBehaviourService {
            string key = typeof(T).Name;
            if (!_services.ContainsKey(key)) {
                throw new InvalidOperationException($"{key} not registered with {GetType().Name}");
            }

            return (T) _services[key];
        }
    
        public void Register(MonoBehaviourService service, Type type){
            string key = type.Name;
            if (_services.ContainsKey(key)) {
                Debug.LogError($"Attempted to register service of type {key} which is already registered");
                return;
            }
            _services.Add(key, service);
        }

        public void Deregister(MonoBehaviourService service, Type type) {
            string key = type.Name;
            if (!_services.ContainsKey(key)) {
                Debug.LogError($"Attempted to unregister service of type {key} which is not registered");
                return;
            }

            if (_services[key] != service) {
                Debug.LogError($"Attempted to deregister service of type {key}, but another instance of the same type is registered");
                return;
            }

            _services.Remove(key);
        }
    }
}