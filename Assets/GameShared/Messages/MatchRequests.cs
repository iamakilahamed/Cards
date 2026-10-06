namespace GameShared.Messages
{
    /// <summary>
    /// Sent by the Client to ask permission to play a card.
    /// Notice we do NOT send the card's cost or power, only its ID.
    /// </summary>
    public struct PlayCardRequest
    {
        public int PlayerId;
        public int CardNetworkId;
        public int TargetSlotId; // e.g., Slot 1, 2, or 3 on the board
    }

    /// <summary>
    /// Sent by the Client when they hit the "End Turn" button.
    /// </summary>
    public struct EndTurnRequest
    {
        public int PlayerId;
    }
}