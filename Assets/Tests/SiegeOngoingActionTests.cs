using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

public class SiegeOngoingActionTests
{
    [Test]
    public void FromJson_ParsesSiegeOngoingActionPayload()
    {
        string json = @"{
            'cardId': 'siege_card',
            'name': 'Saladin',
            'health': 24,
            'maxHealth': 28,
            'attack': 7,
            'defense': 17,
            'ongoingActions': [
                {
                    'actionId': 'siege:siege_card:1',
                    'type': 'siege',
                    'sourceAttackId': 30,
                    'sourceCardId': 'siege_card',
                    'targetCardId': 'enemy_card',
                    'phase': 'building',
                    'turnsRemaining': 1,
                    'payload': {
                        'defenseBonusApplied': 10,
                        'finalDamageMin': 8,
                        'finalDamageMax': 14
                    }
                }
            ]
        }";

        object data = InvokeSelectedCardFromJson("P1", json);
        Assert.IsNotNull(data);

        Array ongoingActions = ReadField<Array>(data, "ongoingActions");
        Assert.IsNotNull(ongoingActions);
        Assert.AreEqual(1, ongoingActions.Length);

        object action = ongoingActions.GetValue(0);
        Assert.AreEqual("siege:siege_card:1", ReadField<string>(action, "actionId"));
        Assert.AreEqual("siege", ReadField<string>(action, "type"));
        Assert.AreEqual(30, ReadField<int>(action, "sourceAttackId"));
        Assert.AreEqual("siege_card", ReadField<string>(action, "sourceCardId"));
        Assert.AreEqual("enemy_card", ReadField<string>(action, "targetCardId"));
        Assert.AreEqual("building", ReadField<string>(action, "phase"));
        Assert.AreEqual(1, ReadField<int>(action, "turnsRemaining"));

        object payload = ReadField<object>(action, "payload");
        Assert.IsNotNull(payload);
        Assert.AreEqual(10, ReadField<int>(payload, "defenseBonusApplied"));
        Assert.AreEqual(8, ReadField<int>(payload, "finalDamageMin"));
        Assert.AreEqual(14, ReadField<int>(payload, "finalDamageMax"));
    }

    [Test]
    public void TryBuild_ParsesSiegeTimelineSteps()
    {
        var battleResult = new Dictionary<string, object>
        {
            {
                "timelineV2", new Dictionary<string, object>
                {
                    { "version", 2 },
                    { "steps", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "type", "OngoingActionStarted" },
                                { "actorCardId", "siege_card" },
                                { "targetCardId", "enemy_card" },
                                { "actionType", "siege" },
                                { "actionId", "siege:siege_card:1" },
                                { "turnsRemaining", 1 },
                                { "source", "ongoingAction" },
                                { "note", "Saladin is building watch tower" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "OngoingActionProgress" },
                                { "actorCardId", "siege_card" },
                                { "targetCardId", "enemy_card" },
                                { "actionType", "siege" },
                                { "actionId", "siege:siege_card:1" },
                                { "turnsRemaining", 0 },
                                { "source", "ongoingAction" },
                                { "note", "Saladin is building siege equipment" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "OngoingActionResolved" },
                                { "actorCardId", "siege_card" },
                                { "targetCardId", "enemy_card" },
                                { "actionType", "siege" },
                                { "actionId", "siege:siege_card:1" },
                                { "turnsRemaining", 0 },
                                { "source", "ongoingAction" },
                                { "note", "Saladin is attacking gates" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "siege_card" },
                                { "targetCardId", "siege_card" },
                                { "statName", "DEF" },
                                { "amount", -10 },
                                { "source", "ongoingAction" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Damage" },
                                { "actorCardId", "siege_card" },
                                { "targetCardId", "enemy_card" },
                                { "amount", 11 },
                                { "source", "ongoingAction" }
                            }
                        }
                    }
                }
            }
        };

        object[] steps = InvokeBattleTimelineBuild(battleResult, out string error);

        Assert.IsNotNull(steps, error);
        Assert.AreEqual(5, steps.Length);

        Assert.AreEqual("OngoingActionStarted", ReadEnumName(steps[0], "Type"));
        Assert.AreEqual("siege", ReadField<string>(steps[0], "ActionType"));
        Assert.AreEqual("siege:siege_card:1", ReadField<string>(steps[0], "ActionId"));
        Assert.AreEqual(1, ReadField<int>(steps[0], "TurnsRemaining"));

        Assert.AreEqual("OngoingActionProgress", ReadEnumName(steps[1], "Type"));
        Assert.AreEqual(0, ReadField<int>(steps[1], "TurnsRemaining"));

        Assert.AreEqual("OngoingActionResolved", ReadEnumName(steps[2], "Type"));
        Assert.AreEqual("enemy_card", ReadField<string>(steps[2], "TargetCardId"));
        Assert.AreEqual("ongoingAction", ReadField<string>(steps[2], "Source"));

        Assert.AreEqual("StatChange", ReadEnumName(steps[3], "Type"));
        Assert.AreEqual("DEF", ReadField<string>(steps[3], "StatName"));
        Assert.AreEqual(-10, ReadField<int>(steps[3], "Amount"));

        Assert.AreEqual("Damage", ReadEnumName(steps[4], "Type"));
        Assert.AreEqual(11, ReadField<int>(steps[4], "Amount"));
        Assert.AreEqual("enemy_card", ReadField<string>(steps[4], "TargetCardId"));
        Assert.AreEqual("ongoingAction", ReadField<string>(steps[4], "Source"));
    }

    private static object InvokeSelectedCardFromJson(string ownerPlayerId, string json)
    {
        Type jobjectType = Type.GetType("Newtonsoft.Json.Linq.JObject, Newtonsoft.Json");
        Assert.IsNotNull(jobjectType, "Type JObject was not found.");

        MethodInfo parseMethod = jobjectType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
        Assert.IsNotNull(parseMethod, "Method JObject.Parse(string) was not found.");
        object payload = parseMethod.Invoke(null, new object[] { json });

        Type selectedCardDataType = Type.GetType("SelectedCardData, Assembly-CSharp");
        Assert.IsNotNull(selectedCardDataType, "Type SelectedCardData was not found in Assembly-CSharp.");

        MethodInfo fromJsonMethod = selectedCardDataType.GetMethod("FromJson", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(fromJsonMethod, "Method SelectedCardData.FromJson was not found.");
        return fromJsonMethod.Invoke(null, new[] { ownerPlayerId, payload });
    }

    private static object[] InvokeBattleTimelineBuild(Dictionary<string, object> battleResult, out string error)
    {
        error = null;
        Type builderType = Type.GetType("BattleTimelineBuilder, Assembly-CSharp");
        Assert.IsNotNull(builderType, "Type BattleTimelineBuilder was not found in Assembly-CSharp.");

        Type battleStepType = Type.GetType("BattleStep, Assembly-CSharp");
        Assert.IsNotNull(battleStepType, "Type BattleStep was not found in Assembly-CSharp.");

        Type listType = typeof(List<>).MakeGenericType(battleStepType);
        object[] args = { battleResult, null, null };

        MethodInfo tryBuildMethod = builderType.GetMethod("TryBuild", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(tryBuildMethod, "Method BattleTimelineBuilder.TryBuild was not found.");

        bool ok = (bool)tryBuildMethod.Invoke(null, args);
        error = args[2] as string;
        Assert.IsTrue(ok, error ?? "BattleTimelineBuilder.TryBuild returned false.");

        var stepsList = args[1] as IEnumerable;
        Assert.IsNotNull(stepsList, "TryBuild returned null steps collection.");

        var steps = new List<object>();
        foreach (object step in stepsList)
        {
            steps.Add(step);
        }

        return steps.ToArray();
    }

    private static T ReadField<T>(object instance, string fieldName)
    {
        Assert.IsNotNull(instance, $"Instance is null when reading field {fieldName}.");
        FieldInfo field = instance.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} was not found on {instance.GetType().Name}.");
        return (T)field.GetValue(instance);
    }

    private static string ReadEnumName(object instance, string fieldName)
    {
        object value = ReadField<object>(instance, fieldName);
        Assert.IsNotNull(value, $"Enum field {fieldName} is null.");
        return value.ToString();
    }
}
