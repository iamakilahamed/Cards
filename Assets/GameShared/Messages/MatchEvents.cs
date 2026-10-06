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
}