#if ROYAL_RUMBLE_PARSER_TESTS
using System.Collections.Generic;
using NUnit.Framework;

public class RoyalRumbleSessionParserTests
{
    [Test]
    public void ParseSessionEnvelope_NormalizesDeckCardsAndCounts()
    {
        var raw = new Dictionary<string, object>
        {
            ["success"] = true,
            ["message"] = "ok",
            ["session"] = new Dictionary<string, object>
            {
                ["sessionId"] = "RR001",
                ["playerId"] = "player_1",
                ["status"] = "awaiting_attack",
                ["playerDeck"] = new Dictionary<string, object>
                {
                    ["deckName"] = "Player Deck",
                    ["deckSize"] = 1,
                    ["cards"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["CardID"] = "player_card",
                            ["PersonName"] = "Mahatma Gandhi",
                            ["CardPicture"] = "ghandi2",
                            ["Level"] = 1,
                            ["Health"] = 21,
                            ["MaxHealth"] = 27,
                            ["StyleID"] = 34,
                            ["Strength"] = 2,
                            ["Speed"] = 4,
                            ["Attack"] = 1,
                            ["Defense"] = 7,
                            ["Knowledge"] = 8,
                            ["Charisma"] = 8,
                            ["Experience"] = 0,
                            ["Attack1"] = 4,
                            ["Attack2"] = 92,
                            ["Attack3"] = 29,
                            ["Attack4"] = 52,
                            ["Color"] = new[] { 85, 122, 130 }
                        }
                    }
                },
                ["enemyDeck"] = new Dictionary<string, object>
                {
                    ["deckName"] = "Enemy Deck",
                    ["deckSize"] = 1,
                    ["cards"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["CardID"] = "enemy_card",
                            ["PersonName"] = "Hattori Hanzo",
                            ["CardPicture"] = "hanzo",
                            ["Level"] = 5,
                            ["Health"] = 20,
                            ["MaxHealth"] = 26,
                            ["StyleID"] = 39,
                            ["Strength"] = 4,
                            ["Speed"] = 5,
                            ["Attack"] = 4,
                            ["Defense"] = 4,
                            ["Knowledge"] = 5,
                            ["Charisma"] = 3,
                            ["Experience"] = 0,
                            ["Attack1"] = 85,
                            ["Attack2"] = 106,
                            ["Attack3"] = 107,
                            ["Attack4"] = 109,
                            ["Color"] = new[] { 132, 131, 127 }
                        }
                    }
                },
                ["attackCounts"] = new Dictionary<string, object>
                {
                    ["player"] = new Dictionary<string, object>
                    {
                        ["player_card"] = new Dictionary<string, object>
                        {
                            ["count1"] = 5,
                            ["count2"] = 4,
                            ["count3"] = 3,
                            ["count4"] = 2
                        }
                    },
                    ["enemy"] = new Dictionary<string, object>()
                },
                ["lastBattleResult"] = new Dictionary<string, object>
                {
                    ["firstAttacker"] = new Dictionary<string, object>
                    {
                        ["cardId"] = "player_card",
                        ["attackId"] = 4,
                        ["damageReceived"] = 0
                    },
                    ["secondAttacker"] = new Dictionary<string, object>
                    {
                        ["cardId"] = "enemy_card",
                        ["attackId"] = 109,
                        ["damageReceived"] = 6
                    }
                }
            },
            ["sessionSummary"] = new Dictionary<string, object>
            {
                ["sessionId"] = "RR001",
                ["status"] = "awaiting_attack",
                ["playerDeckCount"] = 1,
                ["enemyDeckCount"] = 1
            }
        };

        RoyalRumbleSessionEnvelopeDto envelope = RoyalRumbleSessionParser.ParseSessionEnvelope(raw);

        Assert.IsNotNull(envelope);
        Assert.IsNotNull(envelope.session);
        Assert.AreEqual("RR001", envelope.session.sessionId);
        Assert.AreEqual("player_card", envelope.session.playerDeck.cards[0].cardId);
        Assert.AreEqual("Mahatma Gandhi", envelope.session.playerDeck.cards[0].name);
        Assert.AreEqual("enemy_card", envelope.session.enemyDeck.cards[0].cardId);
        Assert.AreEqual("Hattori Hanzo", envelope.session.enemyDeck.cards[0].name);
        Assert.AreEqual(5, envelope.session.attackCounts.player["player_card"].count1);
        Assert.IsNotNull(envelope.session.lastBattleResult);
        Assert.AreEqual(1, envelope.sessionSummary.playerDeckCount);
        Assert.AreEqual(1, envelope.sessionSummary.enemyDeckCount);
    }

    [Test]
    public void ParseBattleEnvelope_MapsBattleFlagsAndBotPick()
    {
        var raw = new Dictionary<string, object>
        {
            ["success"] = true,
            ["battleResult"] = new Dictionary<string, object>
            {
                ["firstAttacker"] = new Dictionary<string, object>
                {
                    ["cardId"] = "player_card",
                    ["attackId"] = 29,
                    ["damageReceived"] = 0
                },
                ["secondAttacker"] = new Dictionary<string, object>
                {
                    ["cardId"] = "enemy_card",
                    ["attackId"] = 109,
                    ["damageReceived"] = 2
                }
            },
            ["botAttack"] = new Dictionary<string, object>
            {
                ["attackSlot"] = 4,
                ["attackId"] = 109,
                ["validSlots"] = new[] { 1, 4 },
                ["reason"] = "random_valid_slot"
            },
            ["runEnded"] = false,
            ["runStatus"] = "awaiting_attack",
            ["playerNeedsReplacement"] = false,
            ["enemyNeedsReplacement"] = true,
            ["sessionSummary"] = new Dictionary<string, object>
            {
                ["sessionId"] = "RR002",
                ["status"] = "awaiting_attack"
            }
        };

        RoyalRumbleBattleEnvelopeDto envelope = RoyalRumbleSessionParser.ParseBattleEnvelope(raw);

        Assert.IsNotNull(envelope);
        Assert.IsNotNull(envelope.battleResult);
        Assert.AreEqual(4, envelope.botAttack.attackSlot);
        Assert.AreEqual(109, envelope.botAttack.attackId);
        Assert.AreEqual(2, envelope.botAttack.validSlots.Count);
        Assert.AreEqual("random_valid_slot", envelope.botAttack.reason);
        Assert.IsFalse(envelope.runEnded);
        Assert.AreEqual("awaiting_attack", envelope.runStatus);
        Assert.IsFalse(envelope.playerNeedsReplacement);
        Assert.IsTrue(envelope.enemyNeedsReplacement);
        Assert.AreEqual("RR002", envelope.sessionSummary.sessionId);
    }
}

#endif

