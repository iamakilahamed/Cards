using CoreFramework.Events;
using CoreFramework.Logging;
using GameClient.Cards;
using GameClient.VisualQueue;
using GameClient.VisualQueue.Actions;
using GameShared.Cards;
using GameShared.Messages;
using UnityEngine;

namespace GameClient.MatchManagement
{
    /// <summary>
    /// Listens to the EventBus and translates server state into visual GameObjects.
    /// </summary>
    public class ClientMatchManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private CardView _cardPrefab;

        [Header("UI Zones")]
        [SerializeField] private Transform _handZone;

        [Header("Systems")]
        [SerializeField] private VisualQueueManager _visualQueue;

        // In a live networked game, the client determines its own ID on login.
        // For our local simulation, we hardcode the client to observe Player 1.
        private const int LocalPlayerId = 1;

        private void OnEnable()
        {
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
        }

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            // We only instantiate UI cards if they belong to our local hand
            if (evt.PlayerId != LocalPlayerId) return;

            GameLogger.Log($"Client intercepted draw for NetworkId {evt.CardData.NetworkId}. Instantiating visual card...");

            // Retrive the immutable blueprint from the Data Layer
            CardDefinition blueprint = CardDatabase.GetCard(evt.CardData.DefinitionId);

            // Push it to the visual queue
            var drawAction = new DrawCardAction(_cardPrefab, _handZone, evt.CardData, blueprint);
            _visualQueue.Enqueue(drawAction);
        }
    }
}