using System.Collections.Generic;
using CoreFramework.Logging;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

namespace CoreFramework.Pooling
{
    /// <summary>
    /// A generic wrapper around Unity's native ObjectPool.
    /// Automatically manages creation, retrieval, and releasing of prefabs.
    /// </summary>
    public class PoolService : IPoolService
    {
        // Maps the original Prefab to its specific ObjectPool
        private readonly Dictionary<GameObject, ObjectPool<GameObject>> _prefabPools = new Dictionary<GameObject, ObjectPool<GameObject>>();

        // Maps an active Instance's ID back to its original Prefab so we know which pool to return it to
        private readonly Dictionary<int, GameObject> _instanceToPrefab = new Dictionary<int, GameObject>();

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolService] Attempted to spawn a null prefab.");
                return null;
            }

            // If we don't have a pool for this prefab yet, create one
            if (!_prefabPools.TryGetValue(prefab, out var pool))
            {
                pool = CreatePoolForPrefab(prefab, parent);
                _prefabPools[prefab] = pool;
            }

            // Get an instance from the native pool
            GameObject instance = pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);

            if (parent != null)
            {
                instance.transform.SetParent(parent);
            }

            // Map this specific instance ID back to its prefab for when we despawn it
            _instanceToPrefab[instance.GetInstanceID()] = prefab;

            // Trigger OnSpawn for any IPoolable scripts attached to the object
            var poolables = instance.GetComponentsInChildren<IPoolable>();
            foreach (var poolable in poolables)
            {
                poolable.OnSpawn();
            }

            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null) return;

            int instanceId = instance.GetInstanceID();

            // Find which prefab this instance belongs to 
            if (_instanceToPrefab.TryGetValue(instanceId, out GameObject prefab))
            {
                // Trigger OnDespawn so the object can clean its dirty state
                var poolables = instance.GetComponentsInChildren<IPoolable>();
                foreach (var poolable in poolables)
                {
                    poolable.OnDespawn();
                }

                // Remove the tracking reference and return it to the native pool
                _instanceToPrefab.Remove(instanceId);
                _prefabPools[prefab].Release(instance);
            }
            else
            {
                Debug.LogWarning($"[PoolService] Tried to despawn {instance.name}, but it was not created by the PoolService. Destroying normally.");
                Object.Destroy(instance);
            }
        }

        private ObjectPool<GameObject> CreatePoolForPrefab(GameObject prefab, Transform defaultParent)
        {
            return new ObjectPool<GameObject>(
                createFunc: () => Object.Instantiate(prefab, defaultParent),
                actionOnGet: (obj) => obj.SetActive(true),
                actionOnRelease: (obj) => obj.SetActive(false),
                actionOnDestroy: (obj) => Object.Destroy(obj),
                collectionCheck: false, // Turn off in production to save CPU cycles
                defaultCapacity: 10,
                maxSize: 100    // Prevents infinite memory leaks if a bug spawns too many objects
            );
        }
    }
}