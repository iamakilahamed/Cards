namespace CoreFramework.Pooling
{
    /// <summary>
    /// Implement this on any MonoBehaviour attached to a pooled prefab.
    /// The PoolService will automatically call these when spawning/despawning.
    /// </summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}