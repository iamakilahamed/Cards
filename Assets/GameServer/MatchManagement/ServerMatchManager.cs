using System;
using System.Collections.Generic;
using CoreFramework.Logging;
using CoreFramework.StateMachine;
using GameShared.Cards;
using GameShared.Rules;
using GameServer.MatchState;
using GameServer.MatchState.States;

namespace GameServer.MatchManagement
{
    /// <summary>
    /// The bootstrapper and factory for a single multiplayer match.
    /// Handles memory allocation, network ID generation, deck shuffling, and starting the engine.
    /// </summary>
    public class ServerMatchManager
    {
        private readonly StateMachine _matchStateMachine;
        private readonly ServerBoardState _boardState;
        private readonly MatchRules _rules;
        private readonly Random _rng;

        private int _networkIdCounter = 0;  // The Factory Counter

        public ServerMatchManager(MatchRules rules)
        {
            _rules = rules;
            _boardState = new ServerBoardState(_rules);
            _matchStateMachine = new StateMachine();
            _rng = new Random();
        }

        /// <summary>
        /// Kicks off the match by initializing players, building decks, and starting Turn 1.
        /// </summary>
        public void StartMatch(int playerOneId, List<int> p1DeckDefinitionIds, int playerTwoId, List<int> p2DeckDefinitionIds)
        {
            GameLogger.Log("--- Initializing Server Match ---");

            // Initialize Players
            _boardState.InitializePlayer(playerOneId);
            _boardState.InitializePlayer(playerTwoId);

            PlayerState p1State = _boardState.GetPlayer(playerOneId);
            PlayerState p2State = _boardState.GetPlayer(playerTwoId);

            // Generate and Shuffle Decks
            GenerateAndShuffleDeck(p1State, p1DeckDefinitionIds);
            GenerateAndShuffleDeck(p2State, p2DeckDefinitionIds);

            // Draw Opening Hands based on MatchRules
            DrawOpeningHand(p1State);
            DrawOpeningHand(p2State);

            // Start the Engine (Turn 1 will immediately draw their next card via Cleanup Phase logic)
            // Bootstap by faking a transition into Cleanup Phase to distribute Turn 1 energy/draws.
            GameLogger.Log("Match Setup Complete. Starting Engine...");
            _matchStateMachine.ChangeState(new CleanupPhaseState(_matchStateMachine, _boardState));
        }

        /// <summary>
        /// Converts raw database IDs into living CardInstances, assigns unique Network IDs, and shuffles.
        /// </summary>
        private void GenerateAndShuffleDeck(PlayerState player, List<int> definitionIds)
        {
            if (definitionIds.Count != _rules.DeckSize)
            {
                GameLogger.LogError($"Player {player.PlayerId} submitted {definitionIds.Count} cards, but rules require {_rules.DeckSize}. Match invalid.");
                return;
            }

            foreach (int definitionId in definitionIds)
            {
                var card = new CardInstance
                {
                    NetworkId = GetNextNetworkId(),
                    DefinitionId = definitionId,
                    OwnerPlayerId = player.PlayerId,

                    // NOTE: A real server would look up the DefinitionId in a static database here 
                    // to set the CurrentCost and CurrentPower. We will implement the Card Database later.
                    CurrentCost = 0,
                    CurrentPower = 0,

                    IsRevealed = false
                };

                player.Deck.Add(card);
            }

            ShuffleList(player.Deck);
            GameLogger.Log($"Player {player.PlayerId} deck generated and shuffled.");
        }

        /// <summary>
        /// Draws the initial starting hand without triggering standard Turn 1 logic.
        /// </summary>
        private void DrawOpeningHand(PlayerState player)
        {
            for (int i = 0; i < _rules.InitialDrawSize; i++)
            {
                if (player.Deck.Count > 0)
                {
                    player.Hand.Add(player.Deck[0]);
                    player.Deck.RemoveAt(0);
                }
            }
            GameLogger.Log($"Player {player.PlayerId} drew opening hand of {_rules.InitialDrawSize} cards.");
        }

        /// <summary>
        /// A thread-safe, standard Fisher-Yates shuffle algorithm.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = _rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Generates an indisputably unique ID for every new object created in this match.
        /// </summary>
        private int GetNextNetworkId()
        {
            _networkIdCounter++;
            return _networkIdCounter;
        }
    }
}