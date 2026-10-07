using CoreFramework.Events;
using CoreFramework.Logging;
using CoreFramework.StateMachine;
using GameShared.Messages;

namespace GameServer.MatchState.States
{
    /// <summary>
    /// Executes the deterministic "Reveal" step.
    /// Calculates priority, flips cards, and resolves abilities.
    /// </summary>
    public class RevealPhaseState : IState
    {
        private readonly StateMachine _stateMachine;
        private readonly ServerBoardState _board;

        public RevealPhaseState(StateMachine stateMachine, ServerBoardState board)
        {
            _stateMachine = stateMachine;
            _board = board;
        }

        public void Enter()
        {
            GameLogger.Log("--- Reveal Phase Started ---");

            // Determine who is currently winning the board
            int priorityPlayerId = _board.GetPriorityPlayerId();
            int nonPriorityPlayerId = priorityPlayerId == 1 ? 2 : 1;

            // In case of a tie (GetPriorityPlayerId returns 3), we default to Player 1 for now.
            // In a real match, ties are broken by a random coin flip at the start of the game.
            if (priorityPlayerId == 3)
            {
                priorityPlayerId = 1;
                nonPriorityPlayerId = 2;
            }

            GameLogger.Log($"Player {priorityPlayerId} has Priority.");

            // Reveal Priority Player's cards left to right
            RevealCardsForPlayer(priorityPlayerId);

            // Reveal Non-Priority Player's cards left to right
            RevealCardsForPlayer(nonPriorityPlayerId);

            _stateMachine.ChangeState(new CleanupPhaseState(_stateMachine, _board));
        }

        public void Tick()
        {
            // The Server calculates the entire reveal phase in a single frame.
            // It does not wait for the visual animations. 
        }

        public void Exit()
        {
            GameLogger.Log("--- Reveal Phase Complete ---");
        }

        private void RevealCardsForPlayer(int playerId)
        {
            foreach (var location in _board.Locations)
            {
                var playerSlots = playerId == 1 ? location.Player1Cards : location.Player2Cards;

                // Loop through the 4 slots at this location
                for (int i = 0; i < playerSlots.Length; i++)
                {
                    // A NetworkId of 0 means the struct is empty
                    if (playerSlots[i].NetworkId != 0 && !playerSlots[i].IsRevealed)
                    {
                        // Modify the Server's memory
                        playerSlots[i].IsRevealed = true;

                        GameLogger.Log($"Flipping Card {playerSlots[i].NetworkId} for Player {playerId} at Location {location.LocationIndex}.");

                        // Broadcast the indisputable fact to the Clients
                        EventBus.Publish(new CardRevealedEvent
                        {
                            CardNetworkId = playerSlots[i].NetworkId,
                            PlayerId = playerId,
                            LocationIndex = location.LocationIndex,
                            CurrentPower = playerSlots[i].CurrentPower
                        });

                        // NOTE: In the future, this exact spot is where we will check the card's 
                        // DefinitionId, see if it has an "On Reveal" ability, and execute it!
                    }
                }
            }
        }
    }
}