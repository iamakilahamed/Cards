using System.Collections.Generic;
using CoreFramework.Events;
using CoreFramework.Logging;
using GameServer.MatchManagement;
using GameServer.MatchState;
using GameShared.Cards;
using GameShared.Messages;
using GameShared.Rules;
using UnityEngine;

namespace GameServer.Testing
{
    public class MockServerRunner : MonoBehaviour
    {
        private ServerMatchManager _matchManager;
        private bool _turnOneExecuted = false;

        private void Start()
        {
            GameLogger.Log("MockServerRunner awake. Booting headless server...");

            // Load the Data Layer first
            CardDatabase.Initialize();

            // Define the rules
            MatchRules rules = MatchRules.CreateDefault();
            _matchManager = new ServerMatchManager(rules);

            // Create fake deck lists (Database IDs 1 through 12)
            List<int> p1Deck = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            List<int> p2Deck = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            // 3Start the match (Player 1 and Player 2)
            _matchManager.StartMatch(1, p1Deck, 2, p2Deck);

            GameLogger.Log("Press SPACE to execute Turn 1 Happy Path.");
        }

        private void Update()
        {
            _matchManager?.Tick();

            // The Mock Client Input (Happy Path)
            if (Input.GetKeyDown(KeyCode.Space) && !_turnOneExecuted)
            {
                ExecuteHappyPath();
                _turnOneExecuted = true;
            }

            // The Malicious Actor Input (Security Testing)
            if (Input.GetKeyDown(KeyCode.Return) && _turnOneExecuted)
            {
                ExecuteMaliciousPath();
            }
        }

        private void ExecuteHappyPath()
        {
            GameLogger.Log("--- EXECUTING MOCK TURN 1 ---");

            // Look into Player 1's memory to find a valid card they just drew
            PlayerState p1 = _matchManager.Board.GetPlayer(1);
            if (p1.Hand.Count == 0) return;

            int validCardNetworkId = p1.Hand[0].NetworkId;

            // Simulate Player 1 dragging a card to the first location
            EventBus.Publish(new PlayCardRequest
            {
                PlayerId = 1,
                CardNetworkId = validCardNetworkId,
                TargetSlotId = 0
            });

            // Simulate both players pressing the "End Turn" button
            EventBus.Publish(new EndTurnRequest { PlayerId = 1 });
            EventBus.Publish(new EndTurnRequest { PlayerId = 2 });
        }

        private void ExecuteMaliciousPath()
        {
            GameLogger.Log("--- EXECUTING MALICIOUS PAYLOADS ---");

            PlayerState p1 = _matchManager.Board.GetPlayer(1);
            if (p1.Hand.Count == 0) return;

            int validCardNetworkId = p1.Hand[0].NetworkId;

            // The Out-of-Bounds Exploit (Valid card, invalid location index)
            GameLogger.Log("Simulating Hack: Targeting Location 9...");
            EventBus.Publish(new PlayCardRequest
            {
                PlayerId = 1,
                CardNetworkId = validCardNetworkId,
                TargetSlotId = 9
            });

            // The Ghost Card Exploit (Invalid card, valid location index)
            GameLogger.Log("Simulating Hack: Playing non-existent Card 9999...");
            EventBus.Publish(new PlayCardRequest
            {
                PlayerId = 1,
                CardNetworkId = 9999,
                TargetSlotId = 1
            });

            // The Over-Lock Explot (Spamming the server with End Turn requests)
            GameLogger.Log("Simulating Hack: Spamming End Turn...");
            EventBus.Publish(new EndTurnRequest { PlayerId = 1 });
            EventBus.Publish(new EndTurnRequest { PlayerId = 1 });
            EventBus.Publish(new EndTurnRequest { PlayerId = 1 });
        }
    }
}