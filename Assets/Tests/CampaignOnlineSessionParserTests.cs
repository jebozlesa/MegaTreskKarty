using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

public class CampaignOnlineSessionParserTests
{
    [Test]
    public void ParseSessionEnvelope_MapsCampaignSessionAndNormalizesCards()
    {
        var raw = new Dictionary<string, object>
        {
            ["success"] = true,
            ["session"] = new Dictionary<string, object>
            {
                ["sessionId"] = "CMP001",
                ["schemaVersion"] = 1,
                ["mode"] = "campaign",
                ["playerId"] = "player_1",
                ["status"] = "awaiting_attack",
                ["active"] = new Dictionary<string, object>
                {
                    ["playerCardId"] = "player_card",
                    ["enemyCardId"] = "campaign:bushido:mission_1:slot_1:style_10001"
                },
                ["progress"] = new Dictionary<string, object>
                {
                    ["turnNumber"] = 2,
                    ["battleCount"] = 1,
                    ["defeatedEnemyCount"] = 0,
                    ["playerDeaths"] = 0,
                    ["campaignId"] = "bushido",
                    ["levelId"] = 1
                },
                ["modeConfig"] = new Dictionary<string, object>
                {
                    ["playerDeckSize"] = 5,
                    ["enemyDeckSize"] = 5,
                    ["campaignId"] = "bushido",
                    ["missionId"] = 1,
                    ["botStrategy"] = new Dictionary<string, object>
                    {
                        ["type"] = "random_valid"
                    }
                },
                ["playerDeck"] = new Dictionary<string, object>
                {
                    ["deckId"] = "player_deck",
                    ["deckName"] = "Player Deck",
                    ["deckSize"] = 1,
                    ["cards"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["cardId"] = "player_card",
                            ["name"] = "Shaka Zulu",
                            ["image"] = "shaka",
                            ["level"] = 1,
                            ["health"] = 25,
                            ["maxHealth"] = 25,
                            ["styleId"] = 24,
                            ["strength"] = 7,
                            ["speed"] = 7,
                            ["attack"] = 7,
                            ["defense"] = 5,
                            ["knowledge"] = 5,
                            ["charisma"] = 6,
                            ["experience"] = 0,
                            ["attack1"] = 71,
                            ["attack2"] = 1,
                            ["attack3"] = 2,
                            ["attack4"] = 77,
                            ["color"] = new[] { 200, 100, 50 },
                            ["effects"] = new object[]
                            {
                                new Dictionary<string, object>
                                {
                                    ["type"] = "3",
                                    ["duration"] = 1,
                                    ["appliedTurn"] = 0,
                                    ["value"] = 0
                                }
                            }
                        }
                    }
                },
                ["enemyDeck"] = new Dictionary<string, object>
                {
                    ["deckId"] = "campaign:bushido:mission_1",
                    ["deckName"] = "Heian Genesis",
                    ["deckSize"] = 1,
                    ["cards"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["CardID"] = "campaign:bushido:mission_1:slot_1:style_10001",
                            ["PersonName"] = "Monk",
                            ["CardPicture"] = "japmnich",
                            ["Level"] = 1,
                            ["Health"] = 30,
                            ["MaxHealth"] = 30,
                            ["StyleID"] = 10001,
                            ["Strength"] = 5,
                            ["Speed"] = 6,
                            ["Attack"] = 1,
                            ["Defense"] = 6,
                            ["Knowledge"] = 5,
                            ["Charisma"] = 1,
                            ["Experience"] = 0,
                            ["Attack1"] = 1,
                            ["Attack2"] = 2,
                            ["Attack3"] = 91,
                            ["Attack4"] = 72,
                            ["Color"] = new[] { 252, 255, 255 },
                            ["TurnEffects"] = new object[0],
                            ["OngoingActions"] = new object[]
                            {
                                new Dictionary<string, object>
                                {
                                    ["actionId"] = "act_1",
                                    ["type"] = "buffaloHorns",
                                    ["sourceAttackId"] = 77,
                                    ["sourceCardId"] = "player_card",
                                    ["targetCardId"] = "campaign:bushido:mission_1:slot_1:style_10001",
                                    ["phase"] = "charging",
                                    ["turnsRemaining"] = 1
                                }
                            }
                        }
                    }
                },
                ["attackCounts"] = new Dictionary<string, object>
                {
                    ["player"] = new Dictionary<string, object>
                    {
                        ["player_card"] = new Dictionary<string, object>
                        {
                            ["count1"] = 6,
                            ["count2"] = 30,
                            ["count3"] = 27,
                            ["count4"] = 7
                        }
                    },
                    ["enemy"] = new Dictionary<string, object>()
                }
            },
            ["sessionSummary"] = new Dictionary<string, object>
            {
                ["sessionId"] = "CMP001",
                ["mode"] = "campaign",
                ["status"] = "awaiting_attack",
                ["active"] = new Dictionary<string, object>
                {
                    ["playerCardId"] = "player_card",
                    ["enemyCardId"] = "campaign:bushido:mission_1:slot_1:style_10001"
                },
                ["progress"] = new Dictionary<string, object>
                {
                    ["campaignId"] = "bushido",
                    ["levelId"] = 1
                }
            }
        };

        object envelope = InvokeParseSessionEnvelope(raw);
        object session = ReadField<object>(envelope, "session");
        object active = ReadField<object>(session, "active");
        object progress = ReadField<object>(session, "progress");
        object modeConfig = ReadField<object>(session, "modeConfig");
        object playerDeck = ReadField<object>(session, "playerDeck");
        object enemyDeck = ReadField<object>(session, "enemyDeck");
        object playerCard = ReadListItem(ReadField<object>(playerDeck, "cards"), 0);
        object enemyCard = ReadListItem(ReadField<object>(enemyDeck, "cards"), 0);
        object attackCounts = ReadField<object>(session, "attackCounts");
        IDictionary playerCounts = ReadField<IDictionary>(attackCounts, "player");
        object playerCardCounts = playerCounts["player_card"];
        object summary = ReadField<object>(envelope, "sessionSummary");

        Assert.IsNotNull(envelope);
        Assert.AreEqual("CMP001", ReadField<string>(session, "sessionId"));
        Assert.AreEqual("campaign", ReadField<string>(session, "mode"));
        Assert.AreEqual("awaiting_attack", ReadField<string>(session, "status"));
        Assert.AreEqual("player_card", ReadField<string>(active, "playerCardId"));
        Assert.AreEqual("campaign:bushido:mission_1:slot_1:style_10001", ReadField<string>(active, "enemyCardId"));
        Assert.AreEqual("bushido", ReadField<string>(progress, "campaignId"));
        Assert.AreEqual(1, ReadField<int?>(progress, "levelId"));
        Assert.AreEqual("bushido", ReadField<string>(modeConfig, "campaignId"));
        Assert.AreEqual(1, ReadField<int>(modeConfig, "missionId"));
        Assert.AreEqual("random_valid", ReadField<string>(modeConfig, "botStrategy"));
        Assert.AreEqual("Shaka Zulu", ReadField<string>(playerCard, "name"));
        Assert.AreEqual("Monk", ReadField<string>(enemyCard, "name"));
        Assert.AreEqual(10001, ReadField<int>(enemyCard, "styleId"));
        Assert.AreEqual(91, ReadField<int>(enemyCard, "attack3"));
        Assert.AreEqual(6, ReadField<int>(playerCardCounts, "count1"));
        Assert.IsNotNull(ReadField<object>(playerCard, "effects"));
        Assert.IsNotNull(ReadField<object>(enemyCard, "ongoingActions"));
        Assert.AreEqual("campaign", ReadField<string>(summary, "mode"));
    }

    [Test]
    public void ParseBattleEnvelope_MapsCampaignFlagsAndDoesNotExposeRecordSync()
    {
        var raw = new Dictionary<string, object>
        {
            ["success"] = true,
            ["battleResult"] = new Dictionary<string, object>
            {
                ["cardDied"] = true,
                ["winnerCardId"] = "player_card",
                ["loserCardId"] = "enemy_card"
            },
            ["botAttack"] = new Dictionary<string, object>
            {
                ["attackSlot"] = 2,
                ["attackId"] = 2,
                ["validSlots"] = new[] { 1, 2, 4 },
                ["reason"] = "random_valid_slot"
            },
            ["playerNeedsReplacement"] = false,
            ["enemyNeedsReplacement"] = true,
            ["runEnded"] = false,
            ["runStatus"] = "awaiting_attack",
            ["recordSync"] = new Dictionary<string, object>
            {
                ["score"] = 99
            },
            ["sessionSummary"] = new Dictionary<string, object>
            {
                ["sessionId"] = "CMP002",
                ["mode"] = "campaign",
                ["status"] = "awaiting_attack"
            }
        };

        object envelope = InvokeParseBattleEnvelope(raw);
        object botAttack = ReadField<object>(envelope, "botAttack");
        IList validSlots = ReadField<IList>(botAttack, "validSlots");
        object summary = ReadField<object>(envelope, "sessionSummary");

        Assert.IsNotNull(envelope);
        Assert.IsNotNull(ReadField<object>(envelope, "battleResult"));
        Assert.AreEqual(2, ReadField<int>(botAttack, "attackSlot"));
        Assert.AreEqual(2, ReadField<int>(botAttack, "attackId"));
        Assert.AreEqual(3, validSlots.Count);
        Assert.IsFalse(ReadField<bool>(envelope, "playerNeedsReplacement"));
        Assert.IsTrue(ReadField<bool>(envelope, "enemyNeedsReplacement"));
        Assert.IsFalse(ReadField<bool>(envelope, "runEnded"));
        Assert.AreEqual("awaiting_attack", ReadField<string>(envelope, "runStatus"));
        Assert.AreEqual("campaign", ReadField<string>(summary, "mode"));
        Assert.IsNull(envelope.GetType().GetField("recordSync"), "Campaign battle DTO must not expose Royal Rumble recordSync.");
    }

    private static object InvokeParseSessionEnvelope(object payload)
    {
        return InvokeParserMethod("ParseSessionEnvelope", payload);
    }

    private static object InvokeParseBattleEnvelope(object payload)
    {
        return InvokeParserMethod("ParseBattleEnvelope", payload);
    }

    private static object InvokeParserMethod(string methodName, object payload)
    {
        Type parserType = Type.GetType("CampaignOnlineSessionParser, Assembly-CSharp");
        Assert.IsNotNull(parserType, "Type CampaignOnlineSessionParser was not found in Assembly-CSharp.");

        MethodInfo method = parserType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(method, $"Method CampaignOnlineSessionParser.{methodName} was not found.");
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
