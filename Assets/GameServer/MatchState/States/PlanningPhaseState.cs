using CoreFramework.Events;
using CoreFramework.Logging;
using CoreFramework.StateMachine;
using GameShared.Messages;
using GameShared.Cards;

namespace GameServer.MatchState.States
{
    /// <summary>
    /// The simultaneous planning phase. The server listens for card plays 
    /// and waits for both players to lock in their turns.
    /// </summary>
    public class PlanningPhaseState : IState
    {
        private readonly StateMachine _stateMachine;
        private readonly ServerBoardState _board;

        public PlanningPhaseState(StateMachine stateMachine, ServerBoardState board)
        {
            _stateMachine = stateMachine;
            _board = board;
        }

        public void Enter()
        {
            GameLogger.Log($"--- Turn {_board.CurrentTurn}: Planning Phase Started ---");

            // Reset lock-in status for the new turn
            _board.GetPlayer(1).HasLockedIn = false;
            _board.GetPlayer(2).HasLockedIn = false;

            EventBus.Subscribe<PlayCardRequest>(OnPlayCardRequest);
            EventBus.Subscribe<EndTurnRequest>(OnEndTurnRequest);
        }

        public void Tick()
        {
            // In a real environment, we would also check a 30-second turn timer here.

            bool p1Locked = _board.GetPlayer(1).HasLockedIn;
            bool p2Locked = _board.GetPlayer(2).HasLockedIn;

            if (p1Locked && p2Locked)
            {
                GameLogger.Log("Both players locked in. Transitioning to Reveal Phase.");
                _stateMachine.ChangeState(new RevealPhaseState(_stateMachine, _board));
            }
        }

        public void Exit()
        {
            // Planning phase is over.
            // Any late packets from lagging clients will be safely ignore.
            EventBus.Unsubscribe<PlayCardRequest>(OnPlayCardRequest);
            EventBus.Unsubscribe<EndTurnRequest>(OnEndTurnRequest);
        }

        private void OnEndTurnRequest(EndTurnRequest req)
        {
            var player = _board.GetPlayer(req.PlayerId);
            if (player != null)
            {
                player.HasLockedIn = true;
                GameLogger.Log($"Player {req.PlayerId} locked in.");
            }
        }

        private void OnPlayCardRequest(PlayCardRequest req)
        {
            var player = _board.GetPlayer(req.PlayerId);

            // SECURITY: Ignore if player is invalid, or if they already pressed End Turn
            if (player == null || player.HasLockedIn) return;

            // SECURITY: Validate the card actually exists in their hand
            int handIndex = player.Hand.FindIndex(c => c.NetworkId == req.CardNetworkId);
            if (handIndex == -1)
            {
                GameLogger.LogWarning($"Player {req.PlayerId} tried to play a card they don't hold!");
                return;
            }

            CardInstance cardToPlay = player.Hand[handIndex];

            // SECURITY: Validate Energy
            if (player.CurrentEnergy < cardToPlay.CurrentCost)
            {
                GameLogger.LogWarning($"Player {req.PlayerId} doesn't have enough energy.");
                return;
            }

            // Players target a Location (0, 1, or 2). The server finds the first empty slot.
            int locationIndex = req.TargetSlotId;
            if (locationIndex < 0 || locationIndex >= _board.Locations.Length) return;

            ServerLocation targetLocation = _board.Locations[locationIndex];
            CardInstance[] playerBoardSlots = req.PlayerId == 1 ? targetLocation.Player1Cards : targetLocation.Player2Cards;

            // Find the first empty slot at this location
            int emptySlotIndex = -1;
            for (int i = 0; i < playerBoardSlots.Length; i++)
            {
                if (playerBoardSlots[i].NetworkId == 0) // 0 means empty struct
                {
                    emptySlotIndex = i;
                    break;
                }
            }

            if (emptySlotIndex == -1)
            {
                GameLogger.LogWarning($"Location {locationIndex} is full for Player {req.PlayerId}.");
                return;
            }

            // --- ALL CHECKS PASSED. EXECUTE THE MOVE ---

            // Deduct Energy & Remove from hand
            player.CurrentEnergy -= cardToPlay.CurrentCost;
            player.Hand.RemoveAt(handIndex);

            // Place on board (CRITICAL: Face Down!)
            cardToPlay.IsRevealed = false;
            playerBoardSlots[emptySlotIndex] = cardToPlay;

            GameLogger.Log($"Player {req.PlayerId} played Card {req.CardNetworkId} face-down at Location {locationIndex}.");

            // Broadcast to Clients so their UI updates
            EventBus.Publish(new CardPlayedEvent
            {
                PlayerId = req.PlayerId,
                CardNetworkId = req.CardNetworkId,
                TargetSlotId = locationIndex,
                NewManaBalance = player.CurrentEnergy
            });
        }
    }
}