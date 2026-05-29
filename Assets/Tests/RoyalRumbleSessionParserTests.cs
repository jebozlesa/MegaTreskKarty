using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
                ["playerDeck"] = new Dictionary<string, object>
                {
                    ["deckSize"] = 1,
                    ["loaded"] = true
                },
                ["enemyDeck"] = new Dictionary<string, object>
                {
                    ["deckSize"] = 1,
                    ["loaded"] = true
                }
            }
        };

        object envelope = InvokeParseSessionEnvelope(raw);
        object session = ReadField<object>(envelope, "session");
        object playerDeck = ReadField<object>(session, "playerDeck");
        object enemyDeck = ReadField<object>(session, "enemyDeck");
        object playerCard = ReadListItem(ReadField<object>(playerDeck, "cards"), 0);
        object enemyCard = ReadListItem(ReadField<object>(enemyDeck, "cards"), 0);
        object attackCounts = ReadField<object>(session, "attackCounts");
        IDictionary playerCounts = ReadField<IDictionary>(attackCounts, "player");
        object playerCardCounts = playerCounts["player_card"];
        object sessionSummary = ReadField<object>(envelope, "sessionSummary");
        object summaryPlayerDeck = ReadField<object>(sessionSummary, "playerDeck");
        object summaryEnemyDeck = ReadField<object>(sessionSummary, "enemyDeck");

        Assert.IsNotNull(envelope);
        Assert.IsNotNull(session);
        Assert.AreEqual("RR001", ReadField<string>(session, "sessionId"));
        Assert.AreEqual("player_card", ReadField<string>(playerCard, "cardId"));
        Assert.AreEqual("Mahatma Gandhi", ReadField<string>(playerCard, "name"));
        Assert.AreEqual("enemy_card", ReadField<string>(enemyCard, "cardId"));
        Assert.AreEqual("Hattori Hanzo", ReadField<string>(enemyCard, "name"));
        Assert.AreEqual(5, ReadField<int>(playerCardCounts, "count1"));
        Assert.IsNotNull(ReadField<object>(session, "lastBattleResult"));
        Assert.AreEqual(1, ReadField<int>(summaryPlayerDeck, "deckSize"));
        Assert.AreEqual(1, ReadField<int>(summaryEnemyDeck, "deckSize"));
    }

    [Test]
    public void ParseSessionEnvelope_MapsCanonicalSoloSessionContract()
    {
        var raw = new Dictionary<string, object>
        {
            ["success"] = true,
            ["session"] = new Dictionary<string, object>
            {
                ["sessionId"] = "RR_CANONICAL",
                ["schemaVersion"] = 1,
                ["mode"] = "royal_rumble",
                ["playerId"] = "player_1",
                ["status"] = "awaiting_replacement",
                ["active"] = new Dictionary<string, object>
                {
                    ["playerCardId"] = "player_card",
                    ["enemyCardId"] = "enemy_card"
                },
                ["progress"] = new Dictionary<string, object>
                {
                    ["turnNumber"] = 7,
                    ["battleCount"] = 3,
                    ["defeatedEnemyCount"] = 2,
                    ["playerDeaths"] = 1,
                    ["bestSubmittedScore"] = 2,
                    ["pendingRecordScore"] = 4,
                    ["lastRecordError"] = "timeout"
                },
                ["modeConfig"] = new Dictionary<string, object>
                {
                    ["playerDeckSize"] = 5,
                    ["enemyDeckSize"] = 50,
                    ["botStrategy"] = new Dictionary<string, object>
                    {
                        ["type"] = "random_valid"
                    }
                }
            },
            ["sessionSummary"] = new Dictionary<string, object>
            {
                ["sessionId"] = "RR_CANONICAL",
                ["status"] = "awaiting_replacement",
                ["active"] = new Dictionary<string, object>
                {
                    ["playerCardId"] = "player_card",
                    ["enemyCardId"] = "enemy_card"
                },
                ["progress"] = new Dictionary<string, object>
                {
                    ["turnNumber"] = 7,
                    ["battleCount"] = 3,
                    ["defeatedEnemyCount"] = 2,
                    ["playerDeaths"] = 1
                }
            }
        };

        object envelope = InvokeParseSessionEnvelope(raw);
        object session = ReadField<object>(envelope, "session");
        object active = ReadField<object>(session, "active");
        object progress = ReadField<object>(session, "progress");
        object modeConfig = ReadField<object>(session, "modeConfig");
        object sessionSummary = ReadField<object>(envelope, "sessionSummary");
        object summaryActive = ReadField<object>(sessionSummary, "active");
        object summaryProgress = ReadField<object>(sessionSummary, "progress");

        Assert.IsNotNull(envelope);
        Assert.IsNotNull(session);
        Assert.AreEqual("player_card", ReadField<string>(active, "playerCardId"));
        Assert.AreEqual("enemy_card", ReadField<string>(active, "enemyCardId"));
        Assert.AreEqual(7, ReadField<int>(progress, "turnNumber"));
        Assert.AreEqual(3, ReadField<int>(progress, "battleCount"));
        Assert.AreEqual(2, ReadField<int>(progress, "defeatedEnemyCount"));
        Assert.AreEqual(1, ReadField<int>(progress, "playerDeaths"));
        Assert.AreEqual(2, ReadField<int>(progress, "bestSubmittedScore"));
        Assert.AreEqual(4, ReadField<int?>(progress, "pendingRecordScore"));
        Assert.AreEqual("timeout", ReadField<string>(progress, "lastRecordError"));
        Assert.AreEqual("random_valid", ReadField<string>(modeConfig, "botStrategy"));
        Assert.IsNotNull(sessionSummary);
        Assert.AreEqual("enemy_card", ReadField<string>(summaryActive, "enemyCardId"));
        Assert.AreEqual(2, ReadField<int>(summaryProgress, "defeatedEnemyCount"));
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

        object envelope = InvokeParseBattleEnvelope(raw);
        object botAttack = ReadField<object>(envelope, "botAttack");
        IList validSlots = ReadField<IList>(botAttack, "validSlots");
        object sessionSummary = ReadField<object>(envelope, "sessionSummary");

        Assert.IsNotNull(envelope);
        Assert.IsNotNull(ReadField<object>(envelope, "battleResult"));
        Assert.AreEqual(4, ReadField<int>(botAttack, "attackSlot"));
        Assert.AreEqual(109, ReadField<int>(botAttack, "attackId"));
        Assert.AreEqual(2, validSlots.Count);
        Assert.AreEqual("random_valid_slot", ReadField<string>(botAttack, "reason"));
        Assert.IsFalse(ReadField<bool>(envelope, "runEnded"));
        Assert.AreEqual("awaiting_attack", ReadField<string>(envelope, "runStatus"));
        Assert.IsFalse(ReadField<bool>(envelope, "playerNeedsReplacement"));
        Assert.IsTrue(ReadField<bool>(envelope, "enemyNeedsReplacement"));
        Assert.AreEqual("RR002", ReadField<string>(sessionSummary, "sessionId"));
    }

    [Test]
    public void ParseBattleEnvelope_MapsProgressiveRecordSync()
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
                }
            },
            ["recordSync"] = new Dictionary<string, object>
            {
                ["submitted"] = false,
                ["pending"] = true,
                ["skipped"] = false,
                ["score"] = 5,
                ["error"] = "PlayFab unavailable"
            }
        };

        object envelope = InvokeParseBattleEnvelope(raw);
        object recordSync = ReadField<object>(envelope, "recordSync");

        Assert.IsNotNull(envelope);
        Assert.IsNotNull(recordSync);
        Assert.IsFalse(ReadField<bool>(recordSync, "submitted"));
        Assert.IsTrue(ReadField<bool>(recordSync, "pending"));
        Assert.IsFalse(ReadField<bool>(recordSync, "skipped"));
        Assert.AreEqual(5, ReadField<int>(recordSync, "score"));
        Assert.AreEqual("PlayFab unavailable", ReadField<string>(recordSync, "error"));
    }

    [Test]
    public void RoyalRumbleRecordDisplay_DoesNotLowerLoadedPlayFabRecord()
    {
        object session = BuildSessionWithProgress(defeatedEnemyCount: 2, bestSubmittedScore: 0);

        Assert.AreEqual(7, ResolveVisibleRecord(7, session));
    }

    [Test]
    public void RoyalRumbleRecordDisplay_AdvancesWhenRunBeatsDisplayedRecord()
    {
        object session = BuildSessionWithProgress(defeatedEnemyCount: 8, bestSubmittedScore: 7);

        Assert.AreEqual(8, ResolveVisibleRecord(7, session));
    }

    [Test]
    public void RoyalRumbleRecordDisplay_UsesRecordSyncScoreWhenSessionPatchIsNotAvailable()
    {
        object session = BuildSessionWithProgress(defeatedEnemyCount: 8, bestSubmittedScore: 7);
        object recordSync = BuildRecordSync(submitted: true, pending: false, score: 9);

        Assert.AreEqual(9, ResolveVisibleRecord(7, session, recordSync));
    }

    private static object InvokeParseSessionEnvelope(object payload)
    {
        return InvokeParserMethod("ParseSessionEnvelope", payload);
    }

    private static object InvokeParseBattleEnvelope(object payload)
    {
        return InvokeParserMethod("ParseBattleEnvelope", payload);
    }

    private static int ResolveVisibleRecord(
        int currentVisibleRecord,
        object session,
        object recordSync = null)
    {
        Type resolverType = Type.GetType("RoyalRumbleRecordDisplay, Assembly-CSharp");
        Assert.IsNotNull(resolverType, "Type RoyalRumbleRecordDisplay was not found in Assembly-CSharp.");

        MethodInfo method = resolverType.GetMethod("ResolveVisibleRecord", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(method, "Method RoyalRumbleRecordDisplay.ResolveVisibleRecord was not found.");

        return (int)method.Invoke(null, new[] { (object)currentVisibleRecord, session, recordSync });
    }

    private static object BuildSessionWithProgress(
        int defeatedEnemyCount,
        int bestSubmittedScore,
        int? pendingRecordScore = null)
    {
        object envelope = InvokeParseSessionEnvelope(new Dictionary<string, object>
        {
            ["success"] = true,
            ["session"] = new Dictionary<string, object>
            {
                ["sessionId"] = "RR_RECORD_TEST",
                ["playerId"] = "player_1",
                ["status"] = "awaiting_attack",
                ["progress"] = new Dictionary<string, object>
                {
                    ["defeatedEnemyCount"] = defeatedEnemyCount,
                    ["bestSubmittedScore"] = bestSubmittedScore,
                    ["pendingRecordScore"] = pendingRecordScore
                }
            }
        });

        return ReadField<object>(envelope, "session");
    }

    private static object BuildRecordSync(bool submitted, bool pending, int score)
    {
        object envelope = InvokeParseBattleEnvelope(new Dictionary<string, object>
        {
            ["success"] = true,
            ["recordSync"] = new Dictionary<string, object>
            {
                ["submitted"] = submitted,
                ["pending"] = pending,
                ["score"] = score
            }
        });

        return ReadField<object>(envelope, "recordSync");
    }

    private static object InvokeParserMethod(string methodName, object payload)
    {
        Type parserType = Type.GetType("RoyalRumbleSessionParser, Assembly-CSharp");
        Assert.IsNotNull(parserType, "Type RoyalRumbleSessionParser was not found in Assembly-CSharp.");

        MethodInfo method = parserType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(method, $"Method RoyalRumbleSessionParser.{methodName} was not found.");
        return method.Invoke(null, new[] { payload });
    }

    private static object ReadListItem(object listObject, int index)
    {
        Assert.IsInstanceOf<IList>(listObject);
        IList list = (IList)listObject;
        Assert.Greater(list.Count, index);
        return list[index];
    }

    private static T ReadField<T>(object obj, string fieldName)
    {
        Assert.IsNotNull(obj, $"Cannot read field '{fieldName}' on null object.");
        FieldInfo field = obj.GetType().GetField(fieldName);
        Assert.IsNotNull(field, $"Missing field '{fieldName}' on {obj.GetType().Name}.");
        object value = field.GetValue(obj);
        if (value == null)
        {
            return default;
        }

        return (T)value;
    }
}
