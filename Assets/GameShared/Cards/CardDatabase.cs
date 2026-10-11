using System.Collections.Generic;
using CoreFramework.Logging;

namespace GameShared.Cards
{
    /// <summary>
    /// The global repository for all card blueprints.
    /// </summary>
    public static class CardDatabase
    {
        // Data Generation Constants
        private const int TOTAL_MOCK_CARDS = 12;
        private const int GENERIC_BASE_COST = 1;
        private const int GENERIC_BASE_POWER = 1;

        // String Constants
        private const string GENERIC_CARD_PREFIX = "Generic Card";
        private const string FALLBACK_CARD_NAME = "Unknown";

        private static readonly Dictionary<int, CardDefinition> _cards = new Dictionary<int, CardDefinition>();
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            // Generate some mock data for our headless tests
            AddCard(new CardDefinition { DefinitionId = 1, CardName = "Hawkeye", BaseCost = 1, BasePower = 1 });
            AddCard(new CardDefinition { DefinitionId = 2, CardName = "Medusa", BaseCost = 2, BasePower = 2 });
            AddCard(new CardDefinition { DefinitionId = 3, CardName = "Ironheart", BaseCost = 3, BasePower = 0 });
            AddCard(new CardDefinition { DefinitionId = 4, CardName = "Jessica Jones", BaseCost = 4, BasePower = 4 });
            AddCard(new CardDefinition { DefinitionId = 5, CardName = "Iron Man", BaseCost = 5, BasePower = 0 });
            AddCard(new CardDefinition { DefinitionId = 6, CardName = "Hulk", BaseCost = 6, BasePower = 12 });

            // Generate remaining generic cards dynamically based on current dictioanry count up to the constant limit
            int startingGenericId = _cards.Count + 1;
            for (int i = startingGenericId; i <= TOTAL_MOCK_CARDS; i++)
            {
                AddCard(new CardDefinition
                {
                    DefinitionId = i,
                    CardName = $"{GENERIC_CARD_PREFIX} {i}",
                    BaseCost = GENERIC_BASE_COST,
                    BasePower = GENERIC_BASE_POWER
                });
            }

            _isInitialized = true;
            GameLogger.Log($"CardDatabase initialized with {_cards.Count} blueprints.");
        }

        private static void AddCard(CardDefinition card)
        {
            if (!_cards.ContainsKey(card.DefinitionId))
            {
                _cards.Add(card.DefinitionId, card);
            }
        }

        public static CardDefinition GetCard(int definitionId)
        {
            if (_cards.TryGetValue(definitionId, out var card))
            {
                return card;
            }

            GameLogger.LogWarning($"CardDatabase: Missing DefinitionId {definitionId}. Returning fallback struct.");

            return new CardDefinition
            {
                DefinitionId = definitionId,
                CardName = FALLBACK_CARD_NAME,
                BaseCost = 1,
                BasePower = 0
            };
        }
    }
}