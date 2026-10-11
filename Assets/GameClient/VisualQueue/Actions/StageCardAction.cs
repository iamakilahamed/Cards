using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using GameClient.Cards;

namespace GameClient.VisualQueue.Actions
{
    public class StageCardAction : IVisualAction
    {
        private readonly CardView _card;
        private const float ANIMATION_DURATION = 0.25f;

        public StageCardAction(CardView card)
        {
            _card = card;
        }

        public async Task ExecuteAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            // Bounce the visual pivot slightly to indicate it has been staged
            _card.VisualPivot.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), ANIMATION_DURATION, 1)
                .OnComplete(() => tcs.SetResult(true));

            await tcs.Task;
        }
    }
}