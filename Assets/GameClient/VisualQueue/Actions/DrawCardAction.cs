using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using GameClient.Cards;
using GameShared.Cards;

namespace GameClient.VisualQueue.Actions
{
    public class DrawCardAction : IVisualAction
    {
        private readonly CardView _cardInstance;
        private readonly CardInstance _instanceData;
        private readonly CardDefinition _blueprint;

        private const float ANIMATION_DURATION = 0.35f;
        private const int MILLISECONDS_DELAY_BETWEEN_DRAWS = 150;

        public DrawCardAction(CardView prefab, CardInstance instanceData, CardDefinition blueprint)
        {
            _cardInstance = prefab;
            _instanceData = instanceData;
            _blueprint = blueprint;
        }

        public async Task ExecuteAsync()
        {
            _cardInstance.Initialize(_instanceData, _blueprint);
            _cardInstance.transform.localScale = Vector3.zero;
            _cardInstance.gameObject.SetActive(true);

            var tcs = new TaskCompletionSource<bool>();

            _cardInstance.transform.DOScale(Vector3.one, ANIMATION_DURATION)
                .SetEase(Ease.OutBack)
                .OnComplete(() => tcs.SetResult(true));

            await tcs.Task;
            await Task.Delay(MILLISECONDS_DELAY_BETWEEN_DRAWS);
        }
    }
}
