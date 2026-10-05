namespace GameShared.Cards
{
    /// <summary>
    /// Represents the core type of the card.
    /// </summary>
    public enum CardType : byte
    {
        None = 0,
        Unit = 1,     // Characters placed on the board
        Spell = 2,    // One-time effects
    }

    /// <summary>
    /// Represents the archetype of the card.
    /// </summary>
    public enum CardArchetype : byte
    {
        Neutral = 0,
        Warrior = 1,
        Mage = 2,
        Rogue = 3
    }
}