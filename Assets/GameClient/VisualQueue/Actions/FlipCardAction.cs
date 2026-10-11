using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using GameClient.Cards;

namespace GameClient.VisualQueue.Actions
{
    public class FlipCardAction : IVisualAction
    {
        private readonly CardView _card;
        private readonly bool _toFaceDown;
        private const float HALF_FLIP_DURATION = 0.15f;

        public FlipCardAction(CardView card, bool toFaceDown)
        {
            _card = card;
            _toFaceDown = toFaceDown;
        }

        public async Task ExecuteAsync()
        {
            var tcs = new TaskCompletionSource<bool>();

            // Calculate target rotations
            Vector3 startRot = _card.VisualPivot.localEulerAngles;
            Vector3 midRot = new Vector3(0, startRot.y + 90f, 0);
            Vector3 finalRot = _toFaceDown ? new Vector3(0, 180f, 0) : new Vector3(0, 0f, 0);

            // Rotate 90 degrees (card becomes paper-thin)
            _card.VisualPivot.DOLocalRotate(midRot, HALF_FLIP_DURATION)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    // Swap the graphic while it is invisible to the camera
                    _card.SetFaceDown(_toFaceDown);

                    // Complete the rotation to the final state
                    _card.VisualPivot.DOLocalRotate(finalRot, HALF_FLIP_DURATION)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() =>
                        {
                            // Optional: Add a small plop scale effect when it lands
                            _card.VisualPivot.DOPunchScale(new Vector3(0.1f, 0.1f, 0f), 0.15f, 1)
                                .OnComplete(() => tcs.SetResult(true));
                        });
                });

            await tcs.Task;

            // Add a slight delay after revealing before the next action starts
            await Task.Delay(250);
        }
    }
}