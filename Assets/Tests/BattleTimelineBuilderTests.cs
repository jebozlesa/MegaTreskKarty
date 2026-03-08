using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BattleTimelineBuilderTests
{
    [Test]
    public void Build_AllBattleFixturesWithBattleResult_ProducesNonEmptyTimeline()
    {
        foreach (string fixturePath in EnumerateBattleFixturePaths())
        {
            string fileName = Path.GetFileName(fixturePath);
            var envelope = LoadFixtureEnvelope(fileName);
            if (envelope?.battleResult?.firstAttacker == null || envelope.battleResult.secondAttacker == null)
            {
                continue;
            }

            List<object> steps = BuildTimelineFromFixture(fileName);
            Assert.IsNotNull(steps, $"Steps should not be null for fixture {fileName}");
            Assert.IsTrue(steps.Count > 0, $"Timeline should not be empty for fixture {fileName}");
        }
    }

    [Test]
    public void Build_DeathFixtures_ContainDeathStep()
    {
        foreach (string fixturePath in EnumerateBattleFixturePaths())
        {
            string fileName = Path.GetFileName(fixturePath);
            var envelope = LoadFixtureEnvelope(fileName);
            if (envelope?.battleResult?.firstAttacker == null || envelope.battleResult.secondAttacker == null)
            {
                continue;
            }

            if (!envelope.battleResult.cardDied)
            {
                continue;
            }

            List<object> steps = BuildTimelineFromFixture(fileName);
            Assert.IsTrue(
                GetStepsByType(steps, "Death").Count > 0,
                $"Expected at least one Death step for cardDied fixture {fileName}");
        }
    }

    [Test]
    public void Build_BlockedFixtures_ContainBlockedStep()
    {
        foreach (string fixturePath in EnumerateBattleFixturePaths())
        {
            string fileName = Path.GetFileName(fixturePath);
            var envelope = LoadFixtureEnvelope(fileName);
            if (envelope?.battleResult?.firstAttacker == null || envelope.battleResult.secondAttacker == null)
            {
                continue;
            }

            bool firstBlocked = envelope.battleResult.firstAttacker.blocked;
            bool secondBlocked = envelope.battleResult.secondAttacker.blocked;
            if (!firstBlocked && !secondBlocked)
            {
                continue;
            }

            List<object> steps = BuildTimelineFromFixture(fileName);
            var blockedSteps = GetStepsByType(steps, "Blocked");
            Assert.IsTrue(blockedSteps.Count > 0, $"Expected Blocked step in fixture {fileName}");

            if (firstBlocked)
            {
                Assert.IsTrue(
                    blockedSteps.Any(s => GetStringField(s, "ActorCardId") == envelope.battleResult.firstAttacker.cardId),
                    $"Missing blocked step for first attacker in fixture {fileName}");
            }

            if (secondBlocked)
            {
                Assert.IsTrue(
                    blockedSteps.Any(s => GetStringField(s, "ActorCardId") == envelope.battleResult.secondAttacker.cardId),
                    $"Missing blocked step for second attacker in fixture {fileName}");
            }
        }
    }

    [Test]
    public void Build_FirstSecondOrder_UsesServerAttackerOrder()
    {
        var steps = BuildTimelineFromFixture("02_normal_round_both_attack.json");
        var attackSteps = GetStepsByType(steps, "Attack");

        Assert.GreaterOrEqual(attackSteps.Count, 2);
        Assert.AreEqual("card_A", GetStringField(attackSteps[0], "ActorCardId"));
        Assert.AreEqual("card_B", GetStringField(attackSteps[1], "ActorCardId"));
    }

    [Test]
    public void Build_BlockedScenario_MarksBlockedAndNoAttackDamageFromBlockedCard()
    {
        var steps = BuildTimelineFromFixture("04_first_blocked_second_attacks.json");
        var attackA = GetStepsByType(steps, "Attack").FirstOrDefault(s => GetStringField(s, "ActorCardId") == "card_A");

        Assert.IsNotNull(attackA, "Missing attack step for card_A");
        Assert.IsTrue(GetBoolField(attackA, "Blocked"));
        Assert.AreEqual(3, GetNullableIntField(attackA, "BlockedBy"));

        bool blockedCardDealtAttackDamage = GetStepsByType(steps, "Damage")
            .Any(s => GetStringField(s, "ActorCardId") == "card_A" &&
                      GetStringField(s, "TargetCardId") == "card_B" &&
                      GetStringField(s, "Source") == "attack" &&
                      GetIntField(s, "Amount") > 0);
        Assert.IsFalse(blockedCardDealtAttackDamage);
    }

    [Test]
    public void Build_SelfDamageScenario_ContainsSelfDamageSteps()
    {
        var steps = BuildTimelineFromFixture("10_both_dead_edge_case.json");
        var selfDamage = GetStepsByType(steps, "SelfDamage");

        Assert.IsTrue(selfDamage.Any(s => GetStringField(s, "ActorCardId") == "card_A" && GetIntField(s, "Amount") == 2));
        Assert.IsTrue(selfDamage.Any(s => GetStringField(s, "ActorCardId") == "card_B" && GetIntField(s, "Amount") == 2));
    }

    [Test]
    public void Build_DeathBeforeAttack_DeathComesBeforeSkippedAttack()
    {
        var steps = BuildTimelineFromFixture("06_death_before_attack_from_bleed.json");
        int deathIndex = IndexOfStep(steps, "Death", "card_B");
        int attackIndex = IndexOfStep(steps, "Attack", "card_B");

        Assert.GreaterOrEqual(deathIndex, 0, "Missing death step for card_B");
        Assert.GreaterOrEqual(attackIndex, 0, "Missing attack step for card_B");
        Assert.Less(deathIndex, attackIndex, "Death should be before skipped attack for card_B");
        Assert.IsTrue(GetBoolField(steps[attackIndex], "Skipped"));
    }

    [Test]
    public void Build_BothBlockedSleepAsceticism_ContainsBleedExposureAndBlockedSteps()
    {
        var steps = BuildTimelineFromFixture("03_both_blocked_sleep_asceticism.json");

        Assert.IsTrue(GetStepsByType(steps, "BleedTick")
            .Any(s => GetStringField(s, "ActorCardId") == "card_A" && GetIntField(s, "Amount") == 2));

        Assert.IsTrue(GetStepsByType(steps, "ExposureTick")
            .Any(s => GetStringField(s, "ActorCardId") == "card_B" && GetIntField(s, "Amount") == 1));

        Assert.IsTrue(GetStepsByType(steps, "Blocked")
            .Any(s => GetStringField(s, "ActorCardId") == "card_A" && GetNullableIntField(s, "BlockedBy") == 3));

        Assert.IsTrue(GetStepsByType(steps, "Blocked")
            .Any(s => GetStringField(s, "ActorCardId") == "card_B" && GetNullableIntField(s, "BlockedBy") == 2));
    }

    [Test]
    public void Build_FreshSleepBlock_EffectAppliedComesBeforeBlockedStep()
    {
        var steps = BuildTimelineFromFixture("05_second_blocked_by_fresh_sleep.json");

        int appliedIdx = IndexOfFirst(steps, s =>
            GetStepTypeName(s) == "EffectApplied" &&
            GetStringField(s, "ActorCardId") == "card_A" &&
            GetStringField(s, "TargetCardId") == "card_B" &&
            GetIntField(s, "EffectType") == 3);

        int blockedIdx = IndexOfFirst(steps, s =>
            GetStepTypeName(s) == "Blocked" &&
            GetStringField(s, "ActorCardId") == "card_B" &&
            GetNullableIntField(s, "BlockedBy") == 3);

        Assert.GreaterOrEqual(appliedIdx, 0, "Missing sleep effect applied step");
        Assert.GreaterOrEqual(blockedIdx, 0, "Missing blocked-by-sleep step");
        Assert.Less(appliedIdx, blockedIdx, "Freshly applied sleep should appear before blocked attack step");
    }

    [Test]
    public void Build_WokeUpRecovered_ContainsWakeUpAndRecoverySteps()
    {
        var battleResult = new Dictionary<string, object>
        {
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_A" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", true }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_B" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", true },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        Assert.IsTrue(GetStepsByType(steps, "WakeUp").Any(s => GetStringField(s, "ActorCardId") == "card_A"));
        Assert.IsTrue(GetStepsByType(steps, "Recovery").Any(s => GetStringField(s, "ActorCardId") == "card_B"));
    }

    [Test]
    public void Build_DoesNotDuplicateSingleAndArrayEffectApplied()
    {
        var steps = BuildTimelineFromFixture("05_second_blocked_by_fresh_sleep.json");
        var applied = GetStepsByType(steps, "EffectApplied")
            .Where(s => GetStringField(s, "ActorCardId") == "card_A" &&
                        GetStringField(s, "TargetCardId") == "card_B" &&
                        GetIntField(s, "EffectType") == 3)
            .ToList();

        Assert.AreEqual(1, applied.Count, "Sleep effect should be added once even when effectApplied and effectsApplied are both present.");
    }

    [Test]
    public void Build_PrefersTimelineV2_WhenPresent()
    {
        var battleResult = new Dictionary<string, object>
        {
            { "timelineV2", new Dictionary<string, object>
                {
                    { "version", 2 },
                    { "steps", new List<object>
                        {
                            new Dictionary<string, object>
                            {
                                { "type", "Attack" },
                                { "actorCardId", "timeline_A" },
                                { "targetCardId", "timeline_B" },
                                { "attackId", 99 }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Damage" },
                                { "actorCardId", "timeline_A" },
                                { "targetCardId", "timeline_B" },
                                { "amount", 7 },
                                { "source", "attack" }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_A" }, { "attackId", 1 }, { "damageDealt", 1 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_B" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        Assert.AreEqual("Attack", GetStepTypeName(steps[0]));
        Assert.AreEqual("timeline_A", GetStringField(steps[0], "ActorCardId"));
        Assert.AreEqual("timeline_B", GetStringField(steps[0], "TargetCardId"));
    }

    [Test]
    public void Build_FallsBackToLegacy_WhenTimelineV2IsInvalid()
    {
        var battleResult = new Dictionary<string, object>
        {
            { "timelineV2", new Dictionary<string, object> { { "version", 2 } } },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_A" }, { "attackId", 1 }, { "damageDealt", 3 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_B" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);
        var attack = GetStepsByType(steps, "Attack").FirstOrDefault();

        Assert.IsNotNull(attack, "Legacy fallback should still produce attack steps.");
        Assert.AreEqual("legacy_A", GetStringField(attack, "ActorCardId"));
    }

    [Test]
    public void Build_LegacyBlockedByKnockout_PreservesBlockedBy27()
    {
        var battleResult = new Dictionary<string, object>
        {
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_A" }, { "attackId", 1 }, { "damageDealt", 1 }, { "healAmount", 0 },
                    { "blocked", false }, { "blockedBy", null }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_B" }, { "attackId", 8 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", true }, { "blockedBy", 27 }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);
        var blocked = GetStepsByType(steps, "Blocked")
            .FirstOrDefault(s => GetStringField(s, "ActorCardId") == "card_B");

        Assert.IsNotNull(blocked, "Missing blocked step for card_B");
        Assert.AreEqual(27, GetNullableIntField(blocked, "BlockedBy"));
    }

    [Test]
    public void Build_TimelineV2BlockedByKnockout_PreservesBlockedBy27()
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
                                { "type", "Attack" },
                                { "actorCardId", "card_B" },
                                { "targetCardId", "card_A" },
                                { "attackId", 8 },
                                { "blocked", true },
                                { "blockedBy", 27 }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Blocked" },
                                { "actorCardId", "card_B" },
                                { "targetCardId", "card_B" },
                                { "blocked", true },
                                { "blockedBy", 27 },
                                { "effectType", 27 },
                                { "source", "effect" }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_A" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_B" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);
        var blocked = GetStepsByType(steps, "Blocked").FirstOrDefault();

        Assert.IsNotNull(blocked, "Missing blocked step from timelineV2.");
        Assert.AreEqual("card_B", GetStringField(blocked, "ActorCardId"));
        Assert.AreEqual(27, GetNullableIntField(blocked, "BlockedBy"));
    }

    [Test]
    public void Build_TimelineV2Attack_ParsesAttackerEffectsApplied()
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
                                { "type", "Attack" },
                                { "actorCardId", "card_A" },
                                { "targetCardId", "card_B" },
                                { "attackId", 7 },
                                { "attackerEffectsApplied", new List<object>
                                    {
                                        new Dictionary<string, object> { { "type", 1 }, { "duration", 3 } }
                                    }
                                }
                            }
                        }
                    }
                }
            },
            { "firstAttacker", new Dictionary<string, object> { { "cardId", "legacy_A" }, { "attackId", 1 } } },
            { "secondAttacker", new Dictionary<string, object> { { "cardId", "legacy_B" }, { "attackId", 1 } } }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);
        var attack = GetStepsByType(steps, "Attack").FirstOrDefault();

        Assert.IsNotNull(attack, "Missing attack step.");
        var attackerEffects = GetEffectListField(attack, "AttackerEffectsApplied");
        Assert.IsNotNull(attackerEffects, "AttackerEffectsApplied should be parsed.");
        Assert.AreEqual(1, attackerEffects.Count, "Expected one attacker-applied effect.");
        Assert.AreEqual("1", attackerEffects[0]["type"].ToString());
    }

    [Test]
    public void Build_LegacyAttack_ParsesAttackerEffectsApplied()
    {
        var battleResult = new Dictionary<string, object>
        {
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_A" }, { "attackId", 7 }, { "damageDealt", 5 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false },
                    { "attackerEffectsApplied", new List<object>
                        {
                            new Dictionary<string, object> { { "type", 1 }, { "duration", 3 } }
                        }
                    }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_B" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 },
                    { "blocked", false }, { "wokeUp", false }, { "recovered", false },
                    { "bleedDamage", 0 }, { "exposureDamage", 0 }, { "exposureRemoved", false }, { "isDead", false }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);
        var attack = GetStepsByType(steps, "Attack").FirstOrDefault(s => GetStringField(s, "ActorCardId") == "card_A");

        Assert.IsNotNull(attack, "Missing first attacker step.");
        var attackerEffects = GetEffectListField(attack, "AttackerEffectsApplied");
        Assert.IsNotNull(attackerEffects, "AttackerEffectsApplied should be parsed from legacy result.");
        Assert.AreEqual(1, attackerEffects.Count, "Expected one attacker-applied effect.");
        Assert.AreEqual("1", attackerEffects[0]["type"].ToString());
    }

    private static List<object> BuildTimelineFromFixture(string fileName)
    {
        var envelope = LoadFixtureEnvelope(fileName);
        Assert.IsNotNull(envelope);
        Assert.IsNotNull(envelope.battleResult);

        var battleResultDict = ToBattleResultDict(envelope.battleResult);
        return BuildTimelineFromBattleResultDict(battleResultDict);
    }

    private static FixtureEnvelope LoadFixtureEnvelope(string fileName)
    {
        string json = File.ReadAllText(Path.Combine(ResolveFixturesDir(), fileName));
        return JsonUtility.FromJson<FixtureEnvelope>(json);
    }

    private static IEnumerable<string> EnumerateBattleFixturePaths()
    {
        return Directory
            .GetFiles(ResolveFixturesDir(), "*.json", SearchOption.TopDirectoryOnly)
            .OrderBy(Path.GetFileName)
            .Where(path => !Path.GetFileName(path).Contains("waiting_response", StringComparison.OrdinalIgnoreCase));
    }

    private static List<object> BuildTimelineFromBattleResultDict(Dictionary<string, object> battleResultDict)
    {
        var builderType = Type.GetType("BattleTimelineBuilder, Assembly-CSharp");
        Assert.IsNotNull(builderType, "Type BattleTimelineBuilder was not found in Assembly-CSharp.");

        MethodInfo tryBuildMethod = builderType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "TryBuild" && m.GetParameters().Length == 3);
        Assert.IsNotNull(tryBuildMethod, "Method BattleTimelineBuilder.TryBuild was not found.");

        object[] args = { battleResultDict, null, null };
        bool ok = (bool)tryBuildMethod.Invoke(null, args);
        if (!ok)
        {
            Assert.Fail($"Timeline build failed: {args[2] as string}");
        }

        return ((IEnumerable)args[1]).Cast<object>().ToList();
    }

    private static int IndexOfFirst(List<object> steps, Func<object, bool> predicate)
    {
        for (int i = 0; i < steps.Count; i++)
        {
            if (predicate(steps[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static int IndexOfStep(List<object> steps, string typeName, string actorCardId)
    {
        for (int i = 0; i < steps.Count; i++)
        {
            if (GetStepTypeName(steps[i]) == typeName && GetStringField(steps[i], "ActorCardId") == actorCardId)
            {
                return i;
            }
        }

        return -1;
    }

    private static List<object> GetStepsByType(List<object> steps, string typeName)
    {
        return steps.Where(s => GetStepTypeName(s) == typeName).ToList();
    }

    private static string GetStepTypeName(object step)
    {
        object value = step.GetType().GetField("Type").GetValue(step);
        return value.ToString();
    }

    private static string GetStringField(object obj, string fieldName)
    {
        object value = obj.GetType().GetField(fieldName).GetValue(obj);
        return value == null ? string.Empty : value.ToString();
    }

    private static int GetIntField(object obj, string fieldName)
    {
        object value = obj.GetType().GetField(fieldName).GetValue(obj);
        return value == null ? 0 : (int)value;
    }

    private static bool GetBoolField(object obj, string fieldName)
    {
        object value = obj.GetType().GetField(fieldName).GetValue(obj);
        return value != null && (bool)value;
    }

    private static int? GetNullableIntField(object obj, string fieldName)
    {
        object value = obj.GetType().GetField(fieldName).GetValue(obj);
        if (value == null)
        {
            return null;
        }

        return (int)value;
    }

    private static List<Dictionary<string, object>> GetEffectListField(object obj, string fieldName)
    {
        object value = obj.GetType().GetField(fieldName).GetValue(obj);
        return value as List<Dictionary<string, object>>;
    }

    private static string ResolveFixturesDir()
    {
        string env = Environment.GetEnvironmentVariable("MEGA_TRESK_FIXTURES_DIR");
        if (!string.IsNullOrWhiteSpace(env) && Directory.Exists(env))
        {
            return env;
        }

        var candidates = new List<string>
        {
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "mega-tresk-server", "docs", "multiplayer", "fixtures")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "mega-tresk-server", "docs", "multiplayer", "fixtures")),
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "docs", "multiplayer", "fixtures"))
        };

        return candidates.First(Directory.Exists);
    }

    private static Dictionary<string, object> ToBattleResultDict(FixtureBattleResult result)
    {
        return new Dictionary<string, object>
        {
            { "firstAttacker", ToAttackerDict(result.firstAttacker) },
            { "secondAttacker", ToAttackerDict(result.secondAttacker) },
            { "cardDied", result.cardDied },
            { "winnerCardId", result.winnerCardId },
            { "loserCardId", result.loserCardId }
        };
    }

    private static Dictionary<string, object> ToAttackerDict(FixtureAttacker attacker)
    {
        int? blockedBy = attacker.blockedBy;
        if (attacker.blocked && blockedBy == null && attacker.effects != null && attacker.effects.Count > 0)
        {
            // Fallback for nullable parsing edge-cases in fixture deserialization.
            blockedBy = attacker.effects[0].type;
        }

        return new Dictionary<string, object>
        {
            { "cardId", attacker.cardId },
            { "attackId", attacker.attackId },
            { "damageReceived", attacker.damageReceived },
            { "damageDealt", attacker.damageDealt },
            { "healAmount", attacker.healAmount },
            { "blocked", attacker.blocked },
            { "blockedBy", blockedBy },
            { "selfDamage", attacker.selfDamage },
            { "attackerSelfDamage", attacker.attackerSelfDamage },
            { "bleedDamage", attacker.bleedDamage },
            { "exposureDamage", attacker.exposureDamage },
            { "exposureRemoved", attacker.exposureRemoved },
            { "wokeUp", attacker.wokeUp },
            { "recovered", attacker.recovered },
            { "effectApplied", ToEffectDict(attacker.effectApplied) },
            { "effectsApplied", ToEffectDictList(attacker.effectsApplied) },
            { "isDead", attacker.isDead }
        };
    }

    private static Dictionary<string, object> ToEffectDict(FixtureEffect effect)
    {
        if (effect == null)
        {
            return null;
        }

        return new Dictionary<string, object>
        {
            { "type", effect.type },
            { "duration", effect.duration },
            { "source", effect.source }
        };
    }

    private static List<object> ToEffectDictList(List<FixtureEffect> effects)
    {
        if (effects == null)
        {
            return new List<object>();
        }

        return effects.Select(e => (object)ToEffectDict(e)).ToList();
    }

    [Serializable]
    private class FixtureEnvelope
    {
        public FixtureBattleResult battleResult;
    }

    [Serializable]
    private class FixtureBattleResult
    {
        public FixtureAttacker firstAttacker;
        public FixtureAttacker secondAttacker;
        public bool cardDied;
        public string winnerCardId;
        public string loserCardId;
    }

    [Serializable]
    private class FixtureAttacker
    {
        public string cardId;
        public int attackId;
        public int damageReceived;
        public int damageDealt;
        public int healAmount;
        public bool blocked;
        public int? blockedBy;
        public int selfDamage;
        public int attackerSelfDamage;
        public int bleedDamage;
        public int exposureDamage;
        public bool exposureRemoved;
        public bool wokeUp;
        public bool recovered;
        public FixtureEffect effectApplied;
        public List<FixtureEffect> effectsApplied;
        public bool isDead;
        public List<FixtureEffect> effects;
    }

    [Serializable]
    private class FixtureEffect
    {
        public int type;
        public int duration;
        public string source;
    }
}
