using UnityEngine;

namespace CoreFramework.Pooling
{
    /// <summary>
    /// Contract for the object pooling service.
    /// </summary>
    public interface IPoolService
    {
        GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null);
        void Despawn(GameObject instance);
    }
}