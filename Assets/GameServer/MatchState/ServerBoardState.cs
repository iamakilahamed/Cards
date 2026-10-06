using System.Collections.Generic;
using CoreFramework.Logging;
using GameShared.Rules;

namespace GameServer.MatchState
{
    public class ServerBoardState
    {
        public int CurrentTurn { get; set; }
        public int CubeStakes { get; set; }

        public ServerLocation[] Locations { get; private set; }

        public MatchRules Rules { get; private set; }
        private readonly Dictionary<int, PlayerState> _players = new Dictionary<int, PlayerState>();

        public ServerBoardState(MatchRules rules)
        {
            Rules = rules;
            CurrentTurn = 1;
            CubeStakes = 1;

            Locations = new ServerLocation[Rules.NumberOfLocations];
            for (int i = 0; i < Rules.NumberOfLocations; i++)
            {
                Locations[i] = new ServerLocation(i, Rules.MaxCardsPerLocation);
            }
        }

        public void InitializePlayer(int playerId)
        {
            if (!_players.ContainsKey(playerId))
            {
                _players.Add(playerId, new PlayerState(playerId, Rules));
                GameLogger.Log($"Player {playerId} joined the match.");
            }
        }

        public PlayerState GetPlayer(int playerId)
        {
            _players.TryGetValue(playerId, out PlayerState player);
            return player;
        }

        /// <summary>
        /// Determines who has Priority (who flips their cards first)
        /// </summary>
        public int GetPriorityPlayerId()
        {
            int p1Wins = 0;
            int p2Wins = 0;

            foreach (var loc in Locations)
            {
                int p1Power = loc.CalculatePlayerPower(1);
                int p2Power = loc.CalculatePlayerPower(2);

                if (p1Power > p2Power) p1Wins++;
                else if (p2Power > p1Power) p2Wins++;
            }

            if (p1Wins > p2Wins) return 1;
            if (p2Wins > p1Wins) return 2;

            return 3;   // tie: no one wins
        }
    }
}