using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using CoreFramework.Events;
using CoreFramework.Logging;
using GameShared.Messages;
using GameClient.Board;

namespace GameClient.Cards
{
    /// <summary>
    /// Handles physical drag-and-drop UI interactions and dispatches the intent to the server.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(CardView))]
    public class CardDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("Drag Animation Settings")]
        [SerializeField] private float _dragScale = 1.1f;
        [SerializeField] private float _animationDuration = 0.2f;

        private CanvasGroup _canvasGroup;
        private CardView _cardView;

        private Transform _originalParent;
        private int _originalSiblingIndex;
        private Transform _dragLayer;
        private bool _isLocked = false;

        // Hardcoded for our local simulation. In a real game, this is provided by the network manager.
        private const int LOCAL_PLAYER_ID = 1;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _cardView = GetComponent<CardView>();
        }

        private void Start()
        {
            // Find the topmost Canvas to act as the free-floating drag layer
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                _dragLayer = canvas.transform;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isLocked) return;

            // Remember the layout group we came from (Hand Zone)
            _originalParent = transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();

            // Escape the layout group by parenting to the root canvas
            transform.SetParent(_dragLayer);

            // Disable raycast blocking so the mouse can see the Drop Zones behind the card
            _canvasGroup.blocksRaycasts = false;

            // Visual pop-up effect
            transform.DOScale(Vector3.one * _dragScale, _animationDuration).SetEase(Ease.OutBack);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isLocked) return;

            transform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isLocked) return;

            // Re-enable raycasts
            _canvasGroup.blocksRaycasts = true;

            // Shrink back to normal scale
            transform.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack);

            // What did we drop it on?
            GameObject hoveredObject = eventData.pointerCurrentRaycast.gameObject;
            LocationDropZone dropZone = hoveredObject != null ? hoveredObject.GetComponentInParent<LocationDropZone>() : null;

            if (dropZone != null && !dropZone.IsFull)
            {
                // --- VALID DROP ---
                GameLogger.Log($"UI: Card {_cardView.NetworkId} dropped onto Location {dropZone.LocationIndex}. Sending request to Server...");

                // Lock the UI physically
                _isLocked = true;
                transform.SetParent(dropZone.transform);

                // Publish the handshake to the Server
                EventBus.Publish(new PlayCardRequest
                {
                    PlayerId = LOCAL_PLAYER_ID,
                    CardNetworkId = _cardView.NetworkId,
                    TargetSlotId = dropZone.LocationIndex
                });
            }
            else
            {
                // --- INVALID DROP ---
                // Snap back into the Hand Layout Group
                transform.SetParent(_originalParent);
                transform.SetSiblingIndex(_originalSiblingIndex);
            }
        }
    }
}