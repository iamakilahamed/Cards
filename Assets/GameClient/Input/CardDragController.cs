using CoreFramework.Events;
using CoreFramework.Logging;
using GameClient.Cards;
using GameClient.Board;
using GameShared.Messages;
using UnityEngine;
using DG.Tweening;

namespace GameClient.Input
{
    public class CardDragController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private LayerMask _cardLayer;
        [SerializeField] private LayerMask _dropZoneLayer;
        [SerializeField] private float _hoverHeight = 2.0f;
        [SerializeField] private float _dragSmoothness = 15f;

        private Camera _mainCamera;
        private CardView _grabbedCard;
        private Vector3 _originalPosition;
        private Vector3 _dragOffset;

        private Plane _dragPlane;

        private const int LOCAL_PLAYER_ID = 1;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _dragPlane = new Plane(Vector3.up, new Vector3(0, _hoverHeight, 0));
        }

        private void Update()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                TryGrabCard();
            }
            else if (UnityEngine.Input.GetMouseButton(0) && _grabbedCard != null)
            {
                DragCard();
            }
            else if (UnityEngine.Input.GetMouseButtonUp(0) && _grabbedCard != null)
            {
                ReleaseCard();
            }
        }

        private void TryGrabCard()
        {
            Ray ray = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hti, 100f, _cardLayer))
            {
                _grabbedCard = hti.collider.GetComponentInParent<CardView>();
                if (_grabbedCard != null)
                {
                    _originalPosition = _grabbedCard.transform.position;

                    if (_dragPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        _dragOffset = _grabbedCard.transform.position - hitPoint;
                    }

                    _grabbedCard.transform.DOMoveY(_hoverHeight, 0.15f).SetEase(Ease.OutQuad);

                    _grabbedCard.VisualPivot.DOLocalRotate(new Vector3(-15f, 0f, 0f), 0.15f);
                }
            }
        }

        private void DragCard()
        {
            Ray ray = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                Vector3 targetPos = ray.GetPoint(enter) + _dragOffset;
                targetPos.y = _hoverHeight;

                _grabbedCard.transform.position = Vector3.Lerp(
                    _grabbedCard.transform.position, targetPos, Time.deltaTime * _dragSmoothness);
            }
        }

        private void ReleaseCard()
        {
            Ray ray = _mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);

            _grabbedCard.VisualPivot.DOLocalRotate(Vector3.zero, 0.2f);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _dropZoneLayer))
            {
                var dropZone = hit.collider.GetComponent<LocationDropZone>();
                if (dropZone != null)
                {
                    GameLogger.Log($"Dropped Card {_grabbedCard.NetworkId} on Location {dropZone.LocationIndex}");

                    EventBus.Publish(new PlayCardRequest
                    {
                        PlayerId = LOCAL_PLAYER_ID,
                        CardNetworkId = _grabbedCard.NetworkId,
                        TargetSlotId = dropZone.LocationIndex
                    });

                    _grabbedCard = null;
                    return;
                }
            }

            _grabbedCard.transform.DOMove(_originalPosition, 0.25f).SetEase(Ease.OutBack);
            _grabbedCard = null;
        }
    }
}