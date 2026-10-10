using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GameClient.VisualQueue
{
    /// <summary>
    /// Queues and executes visual actions sequentially to prevent UI overlap.
    /// </summary>
    public class VisualQueueManager : MonoBehaviour
    {
        private readonly Queue<IVisualAction> _actionQueue = new Queue<IVisualAction>();
        private bool _isProceessing = false;

        public void Enqueue(IVisualAction action)
        {
            _actionQueue.Enqueue(action);

            if (!_isProceessing)
            {
                _ = ProcessQueueAsync();    // Fire and forget the async loop
            }
        }

        private async Task ProcessQueueAsync()
        {
            _isProceessing = true;

            while (_actionQueue.Count > 0)
            {
                IVisualAction nextAction = _actionQueue.Dequeue();

                // Await the specific DOTween sequence or logic
                await nextAction.ExecuteAsync();
            }

            _isProceessing = false;
        }
    }
}

































































































































