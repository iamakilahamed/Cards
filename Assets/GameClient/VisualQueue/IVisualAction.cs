using System.Threading.Tasks;

namespace GameClient.VisualQueue
{
    /// <summary>
    /// Represents a single visual animation step in the client's queue.
    /// </summary>
    public interface IVisualAction
    {
        Task ExecuteAsync();
    }
}