using UnityEngine;

namespace GameClient.Board
{
    /// <summary>
    /// Acts as the valid target for drag-and-drop operations on the local player's side of a location.
    /// </summary>
    public class LocationDropZone : MonoBehaviour
    {
        [Header("Location Data")]
        public int LocationIndex;

        [Header("Player 1 Slots")]
        [SerializeField] private Transform[] _player1Slots = new Transform[4];

        [Header("Player 2 Slots")]
        [SerializeField] private Transform[] _player2Slots = new Transform[4];

        public Transform GetSlot(int playerId, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= 4) return null;
            return playerId == 1 ? _player1Slots[slotIndex] : _player2Slots[slotIndex];
        }
    }
}