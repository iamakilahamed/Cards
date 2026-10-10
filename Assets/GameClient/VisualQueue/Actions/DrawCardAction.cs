using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using GameClient.Cards;
using GameShared.Cards;

namespace GameClient.VisualQueue.Actions
{
    public class DrawCardAction : IVisualAction
    {
        private readonly CardView _prefab;
        private readonly Transform _handZone;
        private readonly CardInstance _instanceData;
        private readonly CardDefinition _blueprint;

        private const float AnimationDuration = 0.35f;
        private const int MillisecondsDelayBetweenDraws = 150;

        public DrawCardAction(CardView prefab, Transform handZone, CardInstance instanceData, CardDefinition blueprint)
        {
            _prefab = prefab;
            _handZone = handZone;
            _instanceData = instanceData;
            _blueprint = blueprint;
        }

        public async Task ExecuteAsync()
        {
            // Instantiate but hide it immediately
            CardView spawnedCard = Object.Instantiate(_prefab, _handZone);
            spawnedCard.Initialize(_instanceData, _blueprint);
            spawnedCard.transform.localScale = Vector3.zero;

            // Create a manual completion source
            var tcs = new TaskCompletionSource<bool>();

            // Play the tween and trigger the completion source when it finishes
            spawnedCard.transform.DOScale(Vector3.one, AnimationDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() => tcs.SetResult(true));

            // Wait for the DOTween OnComplete callback to fire
            await tcs.Task;

            // Add a tiny artificial pause so multiple draws feel rhythmic
            await Task.Delay(MillisecondsDelayBetweenDraws);
        }
    }
}
