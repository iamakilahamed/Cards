using System.Collections.Generic;
using GameShared.Cards;

namespace GameShared.Messages
{
    /// <summary>
    /// Broadcast by the Server after a PlayCardRequest is validated.
    /// Both clients receive this simultaneously to play the visual animations.
    /// </summary>
    public struct CardPlayedEvent
    {
        public int PlayerId;
        public int CardNetworkId;
        public int TargetSlotId;

        // The server dictates the new mana balance so the client UI syncs perfectly.
        public int NewManaBalance;
    }

    /// <summary>
    /// Broadcast by the Server when a player draws a card.
    /// </summary>
    public struct CardDrawnEvent
    {
        public int PlayerId;

        // We pass the full generated CardInstance so the UI knows exactly what stats to render.
        public CardInstance CardData;
    }

    /// <summary>
    /// Broadcast when the server officially ticks the turn over.
    /// </summary>
    public struct TurnStartedEvent
    {
        public int ActivePlayerId;
        public int TurnNumber;
    }

    /// <summary>
    /// Broadcast by the Server when a face-down card is flipped.
    /// </summary>
    public struct CardRevealedEvent
    {
        public int CardNetworkId;
        public int PlayerId;
        public int LocationIndex;
        public int CurrentPower;
    }

    /// <summary>
    /// Broadcast by the Server when the match concludes and stakes are finalized.
    /// </summary>
    public struct MatchEndedEvent
    {
        public int WinningPlayerId; // 1 = Player 1, 2 = Player 2, 0 = Tie
        public int CubePayout;      // The number of cubes won (or 0 if tie)
        public int Player1TotalPower;
        public int Player2TotalPower;
    }

    /// <summary>
    /// Broadcast by the Server at the start of the Reveal Phase.
    /// Tells the client where the opponent placed their face-down cards.
    /// </summary>
    public struct OpponentCardsLockedEvent
    {
        public int OpponentId;

        public struct LockedCardInfo
        {
            public int CardNetworkId;
            public int LocationIndex;
        }

        public List<LockedCardInfo> LockedCards;
    }
}