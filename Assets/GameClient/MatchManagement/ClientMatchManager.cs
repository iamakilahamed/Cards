using System.Collections.Generic;
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
        [SerializeField] private Transform[] _opponentLocations;

        [Header("Systems")]
        [SerializeField] private VisualQueueManager _visualQueue;

        private readonly Dictionary<int, CardView> _cardDictionary = new Dictionary<int, CardView>();

        // Tracks cards played during the current planning phase
        private readonly List<CardView> _stagedCardsThisTurn = new List<CardView>();

        // In a live networked game, the client determines its own ID on login.
        // For our local simulation, we hardcode the client to observe Player 1.
        private const int LOCAL_PLAYER_ID = 1;

        private void OnEnable()
        {
            EventBus.Subscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Subscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Subscribe<CardRevealedEvent>(OnCardRevealed);
            EventBus.Subscribe<OpponentCardsLockedEvent>(OnOpponentCardsLocked);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CardDrawnEvent>(OnCardDrawn);
            EventBus.Unsubscribe<CardPlayedEvent>(OnCardPlayed);
            EventBus.Unsubscribe<CardRevealedEvent>(OnCardRevealed);
            EventBus.Unsubscribe<OpponentCardsLockedEvent>(OnOpponentCardsLocked);
        }

        private void OnCardDrawn(CardDrawnEvent evt)
        {
            // We only instantiate UI cards if they belong to our local hand
            if (evt.PlayerId != LOCAL_PLAYER_ID) return;

            GameLogger.Log($"Client intercepted draw for NetworkId {evt.CardData.NetworkId}. Instantiating visual card...");

            // Retrive the immutable blueprint from the Data Layer
            CardDefinition blueprint = CardDatabase.GetCard(evt.CardData.DefinitionId);

            // Instantiate hidden card and track it
            CardView spawnedCard = Instantiate(_cardPrefab, _handZone);
            spawnedCard.gameObject.SetActive(false);
            _cardDictionary[evt.CardData.NetworkId] = spawnedCard;

            var drawAction = new DrawCardAction(spawnedCard, evt.CardData, blueprint);
            _visualQueue.Enqueue(drawAction);
        }

        private void OnCardPlayed(CardPlayedEvent evt)
        {
            if (evt.PlayerId != LOCAL_PLAYER_ID) return;

            if (_cardDictionary.TryGetValue(evt.CardNetworkId, out CardView playedCard))
            {
                GameLogger.Log($"Client confirming play for Card {evt.CardNetworkId}. Queuing stage animation...");

                // Keep the card face-up and just animate the staging "plop"
                var stageAction = new StageCardAction(playedCard);
                _visualQueue.Enqueue(stageAction);

                // Track it so we can flip it face-down when the turn ends
                _stagedCardsThisTurn.Add(playedCard);

                // TODO: Update Energy UI text with evt.NewManaBalance
            }
        }

        private void OnCardRevealed(CardRevealedEvent evt)
        {
            // Find the physical card using its NetworkId
            if (_cardDictionary.TryGetValue(evt.CardNetworkId, out CardView revealedCard))
            {
                GameLogger.Log($"Queueing reveal animation for Card {evt.CardNetworkId}");

                // TODO: Update the card's visual power text based on evt.CurrentPower

                // Push the face-up flip into the queue
                var flipAction = new FlipCardAction(revealedCard, toFaceDown: false);
                _visualQueue.Enqueue(flipAction);
            }
        }

        /// <summary>
        /// Triggered by the UI End Turn button. Locks in staged cards and notifies the server.
        /// </summary>
        public void ConfirmTurnAndLockIn()
        {
            GameLogger.Log("Locking in turn. Flipping staged cards face-down...");

            // Flip all staged cards face-down
            foreach (CardView card in _stagedCardsThisTurn)
            {
                _visualQueue.Enqueue(new FlipCardAction(card, toFaceDown: true));
            }

            // Clear the list for the next turn
            _stagedCardsThisTurn.Clear();

            // Tell the headless server we are ready
            EventBus.Publish(new EndTurnRequest { PlayerId = LOCAL_PLAYER_ID });
        }

        private void OnOpponentCardsLocked(OpponentCardsLockedEvent evt)
        {
            GameLogger.Log("Client spawning locked opponent cards...");

            foreach (var lockedCard in evt.LockedCards)
            {
                Transform targetZone = _opponentLocations[lockedCard.LocationIndex];

                // Spawn it directly into the opponent's layout zone
                CardView enemyCard = Instantiate(_cardPrefab, targetZone);

                // Force it face-down instantly without animating
                enemyCard.SetFaceDown(true);
                enemyCard.VisualPivot.localEulerAngles = new Vector3(0, 180f, 0);

                // Track it so the Reveal sequence can find it
                _cardDictionary[lockedCard.CardNetworkId] = enemyCard;
            }
        }
    }
}