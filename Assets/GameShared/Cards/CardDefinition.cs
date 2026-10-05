namespace GameShared.Cards
{
    /// <summary>
    /// The immutable blueprint of a card. 
    /// This represents the pure database entry.
    /// </summary>
    [System.Serializable]
    public struct CardDefinition
    {
        // Unique global ID for this specific card design in the database.
        public int Id;

        public string Name;
        public CardType Type;
        public CardArchetype Archetype;

        public int BaseCost;
        public int BasePower;
    }
}