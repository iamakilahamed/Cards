using UnityEngine;

namespace GameClient.Zones
{
    public class HandZone : MonoBehaviour
    {
        [Header("Layout Settings")]
        [Tooltip("The horizontal distance between each card.")]
        [SerializeField] private float _cardSpacing = 1.5f;

        [Tooltip("The microscopic Z-axis step to prevent physical mesh clipping.")]
        [SerializeField] private float _zOffsetPerCard = -0.01f;

        /// <summary>
        /// Calculates the world position for a specific card in the hand.
        /// </summary>
        public Vector3 GetCardPosition(int cardIndex, int totalCardsInHand)
        {
            if (totalCardsInHand <= 1) return transform.position;

            // Find the total horizontal width of all the cards combined
            float totalWidth = (totalCardsInHand - 1) * _cardSpacing;

            // Find the starting X coordinate so the hand remains perfectly centered
            float startX = -totalWidth / 2f;

            // Calculate this specific card's offsets
            float targetX = startX + (cardIndex * _cardSpacing);
            float targetZ = cardIndex * _zOffsetPerCard;

            // Add the calculated local offsets to the HandZone's world position
            return transform.position + new Vector3(targetX, 0f, targetZ);
        }
    }
}