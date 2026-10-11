using UnityEngine;
using UnityEngine.EventSystems;
using CoreFramework.Logging;

namespace GameClient.Board
{
    /// <summary>
    /// Acts as the valid target for drag-and-drop operations on the local player's side of a location.
    /// </summary>
    public class LocationDropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Configuration")]
        [SerializeField] private int _locationIndex;    // 0 = Left, 1 = Middle, 2 = Right

        public int LocationIndex => _locationIndex;

        // Check if the zone already has 4 cards (children)
        public bool IsFull => transform.childCount >= 4;

        public void OnDrop(PointerEventData eventData)
        {
            // The DragController will handle the actual logic, but this confirms the drop registered.
            GameLogger.Log($"Location {_locationIndex}: Received a dropped card!");
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Optional: We can highlight the zone later when a card hovers over it
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Optional: Remove highlight
        }
    }
}