using System.Collections.Generic;
using GameShared.Cards;
using GameShared.Rules;

namespace GameServer.MatchState
{
    public class PlayerState
    {
        public int PlayerId { get; private set; }

        public int MaxEnergy { get; set; }
        public int CurrentEnergy { get; set; }

        // Turn Flags
        public bool HasLockedIn { get; set; }   // True when the player presses "End Turn"
        public bool HasSnapped { get; set; }

        public List<CardInstance> Deck { get; private set; }
        public List<CardInstance> Hand { get; private set; }

        public PlayerState(int playerId, MatchRules rules)
        {
            PlayerId = playerId;

            Deck = new List<CardInstance>(rules.DeckSize);
            Hand = new List<CardInstance>(rules.MaxHandSize);
        }
    }
}