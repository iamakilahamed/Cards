namespace GameShared.Cards
{
    /// <summary>
    /// The dynamic, living state of a card during a specific match.
    /// This is the data passed back and forth between the Server and the Client.
    /// </summary>
    public struct CardInstance
    {
        // The unique ID for this specific physical card in THIS match.
        public int NetworkId;

        // The ID pointing back to the CardDefinition so the UI knows what picture to load.
        public int DefinitionId;

        // The dynmaic ID of the player who currently owns/controls this card.
        public int OwnerPlayerId;

        // The current, modified stats
        public int CurrentCost;
        public int CurrentPower;

        public bool IsRevealed;
    }
}