using GameShared.Cards;

namespace GameServer.MatchState
{
    /// <summary>
    /// Represents one of the 3 zones in a Snap match.
    /// Tracks the cards placed by both players and calculates total power.
    /// </summary>
    public class ServerLocation
    {
        public int LocationIndex;   // 0 = Left, 1 = Middle, 2 = Right
        public int LocationDefinitionId;    // The blueprint ID
        public bool IsRevealed;     // Locations reveal dynamically on Turns 1, 2 and 3

        public CardInstance[] Player1Cards { get; private set; }
        public CardInstance[] Player2Cards { get; private set; }

        public ServerLocation(int index, int maxCardsPerLocation)
        {
            LocationIndex = index;
            Player1Cards = new CardInstance[maxCardsPerLocation];
            Player2Cards = new CardInstance[maxCardsPerLocation];
        }

        /// <summary>
        /// Calculates the total power for a player at this specific location.
        /// </summary>
        public int CalculatePlayerPower(int playerId)
        {
            int totalPower = 0;
            var cards = playerId == 1 ? Player1Cards : Player2Cards;

            foreach (var card in cards)
            {
                // Only count the power if the struct is populated and the card is revealed
                if (card.NetworkId != 0 && card.IsRevealed)
                {
                    totalPower += card.CurrentPower;
                }
            }
            return totalPower;
        }
    }
}