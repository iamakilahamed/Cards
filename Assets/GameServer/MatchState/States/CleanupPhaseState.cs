using System.Runtime.CompilerServices;
using CoreFramework.Events;
using CoreFramework.Logging;
using CoreFramework.StateMachine;
using GameShared.Messages;

namespace GameServer.MatchState.States
{
    /// <summary>
    /// Resets the board for the next turn, hands out energy, and draws cards.
    /// Loops back to the Planning Phase unless the max turn limit is reached.
    /// </summary>
    public class CleanupPhaseState : IState
    {
        private readonly StateMachine _stateMachine;
        private readonly ServerBoardState _board;

        public CleanupPhaseState(StateMachine stateMachine, ServerBoardState board)
        {
            _stateMachine = stateMachine;
            _board = board;
        }

        public void Enter()
        {
            GameLogger.Log("--- Cleanup Phase Started ---");

            // Advance the Turn Counter
            _board.CurrentTurn++;

            // Check for Match End
            // If we surpass max Turns, the game is over.
            if (_board.CurrentTurn > _board.Rules.MaxTurns)
            {
                GameLogger.Log($"Max turns ({_board.Rules.MaxTurns}) reached. Transitioning to Match End.");
                _stateMachine.ChangeState(new MatchEndState(_stateMachine, _board));
                return;
            }

            // Prepare players for new turn
            SetupNewTurnForPlayer(1);
            SetupNewTurnForPlayer(2);

            EventBus.Publish(new TurnStartedEvent
            {
                ActivePlayerId = 0,     // 0 implies the simultaneous turns
                TurnNumber = _board.CurrentTurn
            });

            // Loop the state machine back to the start
            _stateMachine.ChangeState(new PlanningPhaseState(_stateMachine, _board));
        }

        public void Tick()
        {
            // Executed instantly on Enter, no Tick logic required.
        }

        public void Exit()
        {
            // Transitioning out of Cleanup
        }

        private void SetupNewTurnForPlayer(int playerId)
        {
            var player = _board.GetPlayer(playerId);
            if (player == null) return;

            // Cap the energy dynamically based on the MatchRules
            player.MaxEnergy = _board.CurrentTurn > _board.Rules.MaxEnergy
                ? _board.Rules.MaxEnergy
                : _board.CurrentTurn;

            player.CurrentEnergy = player.MaxEnergy;

            // Draw a card
            if (player.Deck.Count > 0)
            {
                // Check if they have room in their hand
                if (player.Hand.Count < player.Hand.Capacity)
                {
                    var drawnCard = player.Deck[0];
                    player.Deck.RemoveAt(0);
                    player.Hand.Add(drawnCard);

                    EventBus.Publish(new CardDrawnEvent
                    {
                        PlayerId = playerId,
                        CardData = drawnCard
                    });

                    GameLogger.Log($"Player {playerId} drew Card {drawnCard.NetworkId}.");
                }
                else
                {
                    GameLogger.LogWarning($"Player {playerId}'s hand is full!");
                }
            }
        }
    }
}