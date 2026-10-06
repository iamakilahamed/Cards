namespace GameShared.Rules
{
    /// <summary>
    /// The definitive ruleset for a match. 
    /// Passed into the Server's board state upon initialization.
    /// </summary>
    [System.Serializable]
    public struct MatchRules
    {
        public int DeckSize;
        public int MaxHandSize;
        public int InitialDrawSize;
        public int NumberOfLocations;
        public int MaxCardsPerLocation;
        public int MaxTurns;
        public int MaxEnergy;

        /// <summary>
        /// Returns the standard default ruleset.
        /// </summary>
        public static MatchRules CreateDefault()
        {
            return new MatchRules
            {
                DeckSize = 12,
                MaxHandSize = 7,
                InitialDrawSize = 3,
                NumberOfLocations = 3,
                MaxCardsPerLocation = 4,
                MaxTurns = 6,
                MaxEnergy = 6
            };
        }
    }
}