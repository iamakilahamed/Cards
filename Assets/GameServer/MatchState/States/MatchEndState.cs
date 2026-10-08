using CoreFramework.Events;
using CoreFramework.Logging;
using CoreFramework.StateMachine;
using GameShared.Messages;

namespace GameServer.MatchState.States
{
    /// <summary>
    /// Calculates final location victories, resolves tiebreakers, 
    /// distributes Cubes, and shuts down the match.
    /// </summary>
    public class MatchEndState : IState
    {
        private readonly StateMachine _stateMachine;
        private readonly ServerBoardState _board;

        public MatchEndState(StateMachine stateMachine, ServerBoardState board)
        {
            _stateMachine = stateMachine;
            _board = board;
        }

        public void Enter()
        {
            GameLogger.Log("--- Match End Phase Started ---");

            int p1LocationWins = 0;
            int p2LocationWins = 0;

            int p1TotalPower = 0;
            int p2TotalPower = 0;

            // Tally locations and total power
            foreach (var location in _board.Locations)
            {
                int p1Power = location.CalculatePlayerPower(1);
                int p2Power = location.CalculatePlayerPower(2);

                p1TotalPower += p1Power;
                p2TotalPower += p2Power;

                if (p1Power > p2Power) p1LocationWins++;
                else if (p2Power > p1Power) p2LocationWins++;
            }

            // Determine winner
            int winningPlayerId = 0;    // Default to 0 (Tie)

            if (p1LocationWins > p2LocationWins)
            {
                winningPlayerId = 1;
            }
            else if (p2LocationWins > p1LocationWins)
            {
                winningPlayerId = 2;
            }
            else
            {
                // Tiebreaker Protocol (Total Power)
                GameLogger.Log("Locations are tied. Resolving via Total Power Differential...");

                if (p1TotalPower > p2TotalPower) winningPlayerId = 1;
                else if (p2TotalPower > p1TotalPower) winningPlayerId = 2;
            }

            // Calculate Payout
            int finalCubePayout = winningPlayerId == 0 ? 0 : _board.CubeStakes;

            LogOutcome(winningPlayerId, finalCubePayout, p1TotalPower, p2TotalPower);

            // Broadcast to Clients
            EventBus.Publish(new MatchEndedEvent
            {
                WinningPlayerId = winningPlayerId,
                CubePayout = finalCubePayout,
                Player1TotalPower = p1TotalPower,
                Player2TotalPower = p2TotalPower
            });

            // In a production server, we would now upload the results to the database 
            // and dispose of the match instance to free up memory.
        }

        public void Tick() { }
        public void Exit() { }

        private void LogOutcome(int winner, int payout, int p1Power, int p2Power)
        {
            if (winner == 0)
            {
                GameLogger.Log($"MATCH DRAW! P1 Power: {p1Power} | P2 Power: {p2Power}. No Cubes exchanged.");
            }
            else
            {
                GameLogger.Log($"MATCH COMPLETE! Player {winner} wins {payout} Cubes. P1 Power: {p1Power} | P2 Power: {p2Power}.");
            }
        }
    }
}