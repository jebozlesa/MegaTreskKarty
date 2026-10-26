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
    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(31)]
    [TestCase(48)]
    [TestCase(124)]
    [TestCase(999)]
    public void Build_RejectsUnknownAttackBeforePublishingAnySteps(int attackId)
    {
        var result = AttackTimeline(AttackStep(1), AttackStep(attackId));
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(result, out var steps, out string error));
        Assert.IsEmpty(steps, "A rejected result must not expose a playable prefix.");
        StringAssert.Contains(attackId.ToString(), error);
    }

    [Test]
    public void Build_PreservesEveryImplementedAttackId()
    {
        for (int id = 1; id <= 123; id++)
        {
            if (id == 31 || id == 48) continue;
            var step = AttackStep(id, id == 107 || id == 121 ? ComboPayload(1, 2) : null);
            Assert.IsTrue(TryBuildTimelineFromBattleResultDict(AttackTimeline(step), out var steps, out string error),
                $"Attack {id}: {error}");
            Assert.AreEqual(id, GetIntField(steps[0], "AttackId"));
        }
    }

    [TestCase(107, null)]
    [TestCase(121, "")]
    [TestCase(107, "not-json")]
    [TestCase(107, "{}")]
    [TestCase(121, "{\"subAttacks\":[]}")]
    [TestCase(107, "{\"subAttacks\":[null]}")]
    [TestCase(121, "{\"subAttacks\":[{}]}")]
    [TestCase(107, "{\"subAttacks\":[{\"attackId\":1},{\"attackId\":999}]}")]
    [TestCase(121, "{\"subAttacks\":[{\"attackId\":118},{\"attackId\":0}]}")]
    [TestCase(107, "{\"subAttacks\":[{\"attackId\":1.5}]}")]
    [TestCase(121, "{\"subAttacks\":[{\"attackId\":\"1\"}]}")]
    public void Build_RejectsInvalidComboBeforeAnyAnimation(int attackId, string payload)
    {
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(
            AttackTimeline(AttackStep(1), AttackStep(attackId, payload)), out var steps, out string error));
        Assert.IsEmpty(steps);
        Assert.IsNotEmpty(error);
    }

    [TestCase(107)]
    [TestCase(121)]
    public void Build_ValidatesNestedComboBeforeAnyAnimation(int attackId)
    {
        string nested = "{\"subAttacks\":[{\"attackId\":107,\"attackResult\":\"{\\\"subAttacks\\\":[{\\\"attackId\\\":999}]}\"}]}";
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(
            AttackTimeline(AttackStep(attackId, nested)), out var steps, out string error));
        Assert.IsEmpty(steps);
        StringAssert.Contains("999", error);
    }

    [TestCase(107, true)]
    [TestCase(107, false)]
    [TestCase(121, true)]
    [TestCase(121, false)]
    public void Build_NonExecutedComboDoesNotRequireAnExecutionPayload(int attackId, bool blocked)
    {
        var step = AttackStep(attackId);
        step[blocked ? "blocked" : "skipped"] = true;
        Assert.IsTrue(TryBuildTimelineFromBattleResultDict(AttackTimeline(step), out _, out string error), error);
    }

    [TestCase(107)]
    [TestCase(121)]
    public void Build_ValidComboPreservesItsChosenSequence(int attackId)
    {
        foreach (string payload in new[] { ComboPayload(1), ComboPayload(1, 2) })
        {
            var steps = BuildTimelineFromBattleResultDict(AttackTimeline(AttackStep(attackId, payload)));
            Assert.AreEqual(payload, GetStringField(steps[0], "AttackResult"));
        }
    }

    [Test]
    public void AttackRegistry_HasNoFallbackAndRejectsUnknownDispatch()
    {
        Type registry = GetRuntimeType("AttackRegistry");
        Assert.IsNull(registry.GetMethod("ExecuteOrFallback", BindingFlags.Public | BindingFlags.Static));
        MethodInfo execute = registry.GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(execute);
        var error = Assert.Throws<TargetInvocationException>(() => execute.Invoke(null, new object[] { 999, null }));
        Assert.IsInstanceOf<ArgumentOutOfRangeException>(error.InnerException);
    }

    [Test]
    public void ComboPlayback_RejectsInvalidLaterAttackBeforeYieldingTheFirst()
    {
        object context = CreateAttackContext(ComboPayload(1, 999));
        MethodInfo play = GetRuntimeType("ComboAttackPlayback").GetMethod("PlaySequence");
        var playback = (IEnumerator)play.Invoke(null, new[] { context });
        Assert.Throws<InvalidOperationException>(() => playback.MoveNext());
    }

    [TestCase(1, 2)]
    [TestCase(118, 92)]
    public void ComboPlayback_ValidSequenceDispatchesChosenHandlersExactlyOnce(int first, int second)
    {
        MethodInfo play = GetRuntimeType("ComboAttackPlayback").GetMethod("PlaySequence");
        var playback = (IEnumerator)play.Invoke(null, new[] { CreateAttackContext(ComboPayload(first, second)) });
        foreach (int id in new[] { first, second })
        {
            Assert.IsTrue(playback.MoveNext());
            Assert.AreEqual("Attack" + id + "Handler", playback.Current.GetType().DeclaringType.Name);
        }
        Assert.IsFalse(playback.MoveNext());
    }

    private static object CreateAttackContext(string payload) => Activator.CreateInstance(
        GetRuntimeType("AttackExecutionContext"), new object[] {
            null, null, 0, 0, true, 0, null, null, null, null, null, null, payload, null });

    private static Type GetRuntimeType(string name) => Type.GetType(name + ", Assembly-CSharp", true);

    private static Dictionary<string, object> AttackStep(int id, string payload = null) =>
        new Dictionary<string, object> { { "type", "Attack" }, { "attackId", id },
            { "actorCardId", "actor" }, { "targetCardId", "target" }, { "attackResult", payload } };

    private static Dictionary<string, object> AttackTimeline(params Dictionary<string, object>[] steps) =>
        new Dictionary<string, object> { { "timelineV2", new Dictionary<string, object> {
            { "version", 2 }, { "steps", steps.Cast<object>().ToList() } } } };

    private static string ComboPayload(params int[] ids) =>
        "{\"subAttacks\":[" + string.Join(",", ids.Select(id => "{\"attackId\":" + id + "}")) + "]}";

    [TestCase("BleedTick")]
    [TestCase("BurnTick")]
    [TestCase("ExposureTick")]
    [TestCase("PoisonTick")]
    [TestCase("4")]
    [TestCase("5")]
    [TestCase("6")]
    [TestCase("20")]
    public void Build_RejectsRetiredNumericTickEvents(string type)
    {
        var result = ValueTimeline(new Dictionary<string, object> { { "type", type } });
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(result, out _, out string error));
        StringAssert.Contains("Unknown step type", error);
    }

    [TestCase("Damage", "bleed", 1, 2)]
    [TestCase("Damage", "burn", 16, 1)]
    [TestCase("Damage", "exposure", 4, 1)]
    [TestCase("Damage", "poison", 24, 1)]
    [TestCase("Heal", "famine", 7, 0)]
    public void Build_TypedPassiveValuePreservesKindAndAppliedAmount(string kind, string source, int effectType, int amount)
    {
        var steps = BuildTimelineFromBattleResultDict(ValueTimeline(PassiveValue(kind, source, effectType, amount)));
        Assert.AreEqual(1, steps.Count);
        Assert.AreEqual(kind, GetStepTypeName(steps[0]));
        Assert.AreEqual(effectType, GetIntField(steps[0], "EffectType"));
        Assert.AreEqual(amount, GetIntField(steps[0], "Amount"));
    }

    [TestCase("effectType", null)]
    [TestCase("effectType", 7)]
    [TestCase("effectType", 999)]
    [TestCase("effectType", "invalid")]
    [TestCase("type", "Heal")]
    [TestCase("source", "attack")]
    [TestCase("actorCardId", "")]
    [TestCase("targetCardId", " ")]
    [TestCase("amount", null)]
    [TestCase("amount", 0)]
    [TestCase("amount", -1)]
    [TestCase("amount", 1.5)]
    public void Build_RejectsMalformedPassiveValue(string key, object value)
    {
        var step = PassiveValue("Damage", "bleed", 1, 2);
        step[key] = value;
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(ValueTimeline(step), out _, out string error));
        StringAssert.Contains("passive value", error);
    }

    private static Dictionary<string, object> PassiveValue(string kind, string source, int effectType, int amount)
    {
        return new Dictionary<string, object> { { "type", kind }, { "source", source },
            { "effectType", effectType }, { "amount", amount },
            { "actorCardId", "subject" }, { "targetCardId", "subject" } };
    }

    private static Dictionary<string, object> ValueTimeline(Dictionary<string, object> step)
    {
        return new Dictionary<string, object> { { "timelineV2", new Dictionary<string, object> {
            { "version", 2 }, { "steps", new List<object> { step } } } } };
    }

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
        var blockedA = GetStepsByType(steps, "Blocked").FirstOrDefault(s => GetStringField(s, "ActorCardId") == "card_A");

        Assert.IsNotNull(attackA, "Missing attack step for card_A");
        Assert.IsTrue(GetBoolField(attackA, "Blocked"));
        Assert.AreEqual(3, GetNullableIntField(blockedA, "BlockedBy"));
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
    public void Build_AsceticismFixturePreservesCueBeforeOneDamageAndBlock()
    {
        var steps = BuildTimelineFromFixture("15_asceticism_single_damage.json");
        int cue = IndexOfStep(steps, "EffectTriggered", "ascetic_actor");
        int damage = IndexOfStep(steps, "Damage", "ascetic_actor");
        int blocked = IndexOfStep(steps, "Blocked", "ascetic_actor");
        Assert.GreaterOrEqual(cue, 0);
        Assert.AreEqual(2, GetIntField(steps[cue], "EffectType"));
        Assert.AreEqual("ascetic_actor", GetStringField(steps[cue], "TargetCardId"));
        Assert.Less(cue, damage);
        Assert.Less(damage, blocked);
        Assert.AreEqual(1, GetStepsByType(steps, "Damage").Count);
        Assert.AreEqual(0, GetStepsByType(steps, "SelfDamage").Count);
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

        Assert.IsTrue(GetStepsByType(steps, "Damage").Where(s => GetIntField(s, "EffectType") == 1)
            .Any(s => GetStringField(s, "ActorCardId") == "card_A" && GetIntField(s, "Amount") == 2));

        Assert.IsTrue(GetStepsByType(steps, "Damage").Where(s => GetIntField(s, "EffectType") == 4)
            .Any(s => GetStringField(s, "ActorCardId") == "card_B" && GetIntField(s, "Amount") == 1));

        var blockedSteps = GetStepsByType(steps, "Blocked");
        Assert.IsTrue(blockedSteps.Any(s => GetStringField(s, "ActorCardId") == "card_A"));
        Assert.IsTrue(blockedSteps.Any(s => GetStringField(s, "ActorCardId") == "card_B"));
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
            GetStringField(s, "ActorCardId") == "card_B");

        Assert.GreaterOrEqual(appliedIdx, 0, "Missing sleep effect applied step");
        Assert.GreaterOrEqual(blockedIdx, 0, "Missing blocked-by-sleep step");
        Assert.Less(appliedIdx, blockedIdx, "Freshly applied sleep should appear before blocked attack step");
    }

    [Test]
    public void Build_DrinkWineTimeline_ContainsHealStrengthBuffAndSelfSleep()
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
                                { "actorCardId", "drink_a" },
                                { "targetCardId", "drink_b" },
                                { "attackId", 22 },
                                { "attackResult", "drunk" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Heal" },
                                { "actorCardId", "drink_a" },
                                { "targetCardId", "drink_a" },
                                { "amount", 2 },
                                { "source", "attack" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "drink_a" },
                                { "targetCardId", "drink_a" },
                                { "statName", "STR" },
                                { "amount", 1 },
                                { "source", "attack" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "EffectApplied" },
                                { "actorCardId", "drink_a" },
                                { "targetCardId", "drink_a" },
                                { "effectType", 3 },
                                { "duration", 2 }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "drink_a" }, { "attackId", 22 }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "drink_b" }, { "attackId", 17 }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        Assert.IsTrue(GetStepsByType(steps, "Attack").Any(s => GetStringField(s, "ActorCardId") == "drink_a" && GetIntField(s, "AttackId") == 22));
        Assert.IsTrue(GetStepsByType(steps, "Heal").Any(s => GetStringField(s, "ActorCardId") == "drink_a" && GetStringField(s, "TargetCardId") == "drink_a" && GetIntField(s, "Amount") == 2));
        Assert.IsTrue(GetStepsByType(steps, "StatChange").Any(s => GetStringField(s, "ActorCardId") == "drink_a" && GetStringField(s, "TargetCardId") == "drink_a" && GetStringField(s, "StatName") == "STR" && GetIntField(s, "Amount") == 1));
        Assert.IsTrue(GetStepsByType(steps, "EffectApplied").Any(s => GetStringField(s, "ActorCardId") == "drink_a" && GetStringField(s, "TargetCardId") == "drink_a" && GetIntField(s, "EffectType") == 3));
    }
    [Test]
    public void Build_FlamingGunTimeline_ContainsDamageAndBurnEffect()
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
                                { "actorCardId", "flame_a" },
                                { "targetCardId", "flame_b" },
                                { "attackId", 23 },
                                { "attackResult", "burn" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Damage" },
                                { "actorCardId", "flame_a" },
                                { "targetCardId", "flame_b" },
                                { "amount", 3 },
                                { "source", "attack" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "EffectApplied" },
                                { "actorCardId", "flame_a" },
                                { "targetCardId", "flame_b" },
                                { "effectType", 16 },
                                { "duration", 1 }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "flame_a" }, { "attackId", 23 }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "flame_b" }, { "attackId", 17 }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        Assert.IsTrue(GetStepsByType(steps, "Attack").Any(s => GetStringField(s, "ActorCardId") == "flame_a" && GetIntField(s, "AttackId") == 23));
        Assert.IsTrue(GetStepsByType(steps, "Damage").Any(s => GetStringField(s, "ActorCardId") == "flame_a" && GetStringField(s, "TargetCardId") == "flame_b" && GetIntField(s, "Amount") == 3));
        Assert.IsTrue(GetStepsByType(steps, "EffectApplied").Any(s => GetStringField(s, "ActorCardId") == "flame_a" && GetStringField(s, "TargetCardId") == "flame_b" && GetIntField(s, "EffectType") == 16));
    }

    [Test]
    public void Build_CleaverTimeline_ContainsDamageAndBleedEffect()
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
                                { "actorCardId", "cleaver_a" },
                                { "targetCardId", "cleaver_b" },
                                { "attackId", 24 },
                                { "attackResult", "hit" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Damage" },
                                { "actorCardId", "cleaver_a" },
                                { "targetCardId", "cleaver_b" },
                                { "amount", 7 },
                                { "source", "attack" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "EffectApplied" },
                                { "actorCardId", "cleaver_a" },
                                { "targetCardId", "cleaver_b" },
                                { "effectType", 1 },
                                { "duration", 6 }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "cleaver_a" }, { "attackId", 24 }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "cleaver_b" }, { "attackId", 17 }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        Assert.IsTrue(GetStepsByType(steps, "Attack").Any(s => GetStringField(s, "ActorCardId") == "cleaver_a" && GetIntField(s, "AttackId") == 24));
        Assert.IsTrue(GetStepsByType(steps, "Damage").Any(s => GetStringField(s, "ActorCardId") == "cleaver_a" && GetStringField(s, "TargetCardId") == "cleaver_b" && GetIntField(s, "Amount") == 7));
        Assert.IsTrue(GetStepsByType(steps, "EffectApplied").Any(s => GetStringField(s, "ActorCardId") == "cleaver_a" && GetStringField(s, "TargetCardId") == "cleaver_b" && GetIntField(s, "EffectType") == 1 && GetIntField(s, "Duration") == 6));
    }



    [Test]
    public void Build_ConfusionTimeline_ContainsDecisionCueAndResolvedAttack()
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
                                { "type", "EffectTriggered" },
                                { "actorCardId", "confused_a" },
                                { "targetCardId", "confused_a" },
                                { "effectType", 17 },
                                { "source", "confusion" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Attack" },
                                { "attackId", 6 },
                                { "actorCardId", "confused_a" },
                                { "targetCardId", "confused_a" },
                                { "source", "confusion" }
                            }
                        }
                    }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        Assert.IsTrue(GetStepsByType(steps, "EffectTriggered").Any(s =>
            GetStringField(s, "ActorCardId") == "confused_a"
            && GetStringField(s, "TargetCardId") == "confused_a"
            && GetIntField(s, "EffectType") == 17
            && GetStringField(s, "Source") == "confusion"));
        Assert.IsTrue(GetStepsByType(steps, "Attack").Any(s =>
            GetStringField(s, "ActorCardId") == "confused_a"
            && GetStringField(s, "TargetCardId") == "confused_a"
            && GetStringField(s, "Source") == "confusion"));
    }

    [Test]
    [TestCase("ConfusionForgot")]
    [TestCase("21")]
    public void Build_RejectsRetiredConfusionStep(string stepType)
    {
        var battleResult = new Dictionary<string, object> {
            { "timelineV2", new Dictionary<string, object> {
                { "version", 2 }, { "steps", new List<object> {
                    new Dictionary<string, object> { { "type", stepType } }
                } }
            } }
        };
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(battleResult, out _, out string error));
        StringAssert.Contains("Unknown step type", error);
    }

    [Test]
    public void Build_BoostTimeline_ContainsSelfStatChangesAndOnePostAttackRemoval()
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
                                { "actorCardId", "boost_a" },
                                { "targetCardId", "boost_b" },
                                { "attackId", 26 },
                                { "attackResult", "self_buff" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "boost_a" },
                                { "targetCardId", "boost_a" },
                                { "statName", "ATT" },
                                { "amount", 3 }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "boost_a" },
                                { "targetCardId", "boost_a" },
                                { "statName", "STR" },
                                { "amount", 2 }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "boost_a" },
                                { "targetCardId", "boost_a" },
                                { "statName", "CHA" },
                                { "amount", -1 }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "boost_a" },
                                { "targetCardId", "boost_a" },
                                { "statName", "KNO" },
                                { "amount", -2 }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "EffectRemoved" },
                                { "actorCardId", "boost_a" },
                                { "targetCardId", "boost_a" },
                                { "effectType", 3 },
                                { "removalReason", "cleansed" },
                                { "source", "attack" }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "boost_a" }, { "attackId", 26 }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "boost_b" }, { "attackId", 17 }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        int attackIndex = steps.FindIndex(
            s => GetStringField(s, "Type") == "Attack" && GetStringField(s, "ActorCardId") == "boost_a"
        );
        int effectRemovedIndex = steps.FindIndex(
            s =>
                GetStringField(s, "Type") == "EffectRemoved"
                && GetStringField(s, "ActorCardId") == "boost_a"
                && GetIntField(s, "EffectType") == 3
        );
        Assert.That(attackIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(effectRemovedIndex, Is.GreaterThan(attackIndex));
        Assert.AreEqual(1, GetStepsByType(steps, "EffectRemoved").Count);
        Assert.AreEqual("cleansed", GetStringField(steps[effectRemovedIndex], "RemovalReason"));
        Assert.IsTrue(GetStepsByType(steps, "StatChange").Any(s => GetStringField(s, "ActorCardId") == "boost_a" && GetStringField(s, "TargetCardId") == "boost_a" && GetStringField(s, "StatName") == "ATT" && GetIntField(s, "Amount") == 3));
        Assert.IsTrue(GetStepsByType(steps, "StatChange").Any(s => GetStringField(s, "ActorCardId") == "boost_a" && GetStringField(s, "TargetCardId") == "boost_a" && GetStringField(s, "StatName") == "STR" && GetIntField(s, "Amount") == 2));
        Assert.IsTrue(GetStepsByType(steps, "StatChange").Any(s => GetStringField(s, "ActorCardId") == "boost_a" && GetStringField(s, "TargetCardId") == "boost_a" && GetStringField(s, "StatName") == "CHA" && GetIntField(s, "Amount") == -1));
        Assert.IsTrue(GetStepsByType(steps, "StatChange").Any(s => GetStringField(s, "ActorCardId") == "boost_a" && GetStringField(s, "TargetCardId") == "boost_a" && GetStringField(s, "StatName") == "KNO" && GetIntField(s, "Amount") == -2));
    }

    [Test]
    public void Build_CalmExpiryTimeline_ContainsPreAttackRestoreAndRemoval()
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
                                { "type", "EffectRemoved" },
                                { "actorCardId", "calm_a" },
                                { "targetCardId", "calm_a" },
                                { "effectType", 21 },
                                { "removalReason", "expired" },
                                { "source", "effect" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "calm_a" },
                                { "targetCardId", "calm_a" },
                                { "statName", "ATT" },
                                { "amount", 1 },
                                { "source", "effect" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "calm_a" },
                                { "targetCardId", "calm_a" },
                                { "statName", "STR" },
                                { "amount", 1 },
                                { "source", "effect" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "Attack" },
                                { "actorCardId", "calm_a" },
                                { "targetCardId", "calm_b" },
                                { "attackId", 17 },
                                { "attackResult", "miss" }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "calm_a" }, { "attackId", 17 }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "calm_b" }, { "attackId", 17 }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        int removalIndex = steps.FindIndex(
            s => GetStringField(s, "Type") == "EffectRemoved" && GetStringField(s, "ActorCardId") == "calm_a"
        );
        int attackBuffIndex = steps.FindIndex(
            s => GetStringField(s, "Type") == "StatChange" && GetStringField(s, "ActorCardId") == "calm_a" && GetStringField(s, "StatName") == "ATT"
        );
        int strengthBuffIndex = steps.FindIndex(
            s => GetStringField(s, "Type") == "StatChange" && GetStringField(s, "ActorCardId") == "calm_a" && GetStringField(s, "StatName") == "STR"
        );
        int attackIndex = steps.FindIndex(
            s => GetStringField(s, "Type") == "Attack" && GetStringField(s, "ActorCardId") == "calm_a"
        );

        Assert.That(removalIndex, Is.GreaterThanOrEqualTo(0));
        Assert.That(attackBuffIndex, Is.GreaterThan(removalIndex));
        Assert.That(strengthBuffIndex, Is.GreaterThan(removalIndex));
        Assert.That(attackIndex, Is.GreaterThan(strengthBuffIndex));
    }

    [Test]
    public void Build_DistinctExpiredEffects_KeepOneCanonicalExitPerCard()
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
                                { "type", "EffectRemoved" },
                                { "effectType", 3 },
                                { "removalReason", "expired" },
                                { "actorCardId", "card_A" },
                                { "targetCardId", "card_A" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "EffectRemoved" },
                                { "effectType", 2 },
                                { "removalReason", "expired" },
                                { "actorCardId", "card_B" },
                                { "targetCardId", "card_B" }
                            }
                        }
                    }
                }
            },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_A" }, { "attackId", 1 }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "card_B" }, { "attackId", 1 }
                }
            }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);

        Assert.AreEqual(2, GetStepsByType(steps, "EffectRemoved").Count);
        Assert.IsTrue(GetStepsByType(steps, "EffectRemoved").Any(s => GetStringField(s, "TargetCardId") == "card_A"));
        Assert.IsTrue(GetStepsByType(steps, "EffectRemoved").Any(s => GetStringField(s, "TargetCardId") == "card_B"));
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

    [TestCase("expired")]
    [TestCase("recovered")]
    [TestCase("cleansed")]
    public void Build_CanonicalEffectRemoval_PreservesReasonAndTarget(string reason)
    {
        var result = new Dictionary<string, object>
        {
            { "timelineV2", new Dictionary<string, object> { { "version", 2 },
                { "steps", new List<object> { new Dictionary<string, object> {
                    { "type", "EffectRemoved" }, { "actorCardId", "caster" },
                    { "targetCardId", "recipient" }, { "effectType", 4 }, { "removalReason", reason }
                } } }
            } }
        };
        var steps = BuildTimelineFromBattleResultDict(result);
        Assert.AreEqual(1, steps.Count);
        Assert.AreEqual("recipient", GetStringField(steps[0], "TargetCardId"));
        Assert.AreEqual(reason, GetStringField(steps[0], "RemovalReason"));
    }

    [TestCase("ExposureRemoved")]
    [TestCase("WakeUp")]
    [TestCase("Recovery")]
    [TestCase("7")]
    [TestCase("10")]
    [TestCase("11")]
    public void Build_RejectsRetiredEffectExitAliases(string type)
    {
        var result = new Dictionary<string, object> {
            { "timelineV2", new Dictionary<string, object> { { "version", 2 },
                { "steps", new List<object> { new Dictionary<string, object> { { "type", type } } } }
            } }
        };
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(result, out _, out string error));
        StringAssert.Contains("Unknown step type", error);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("unknown")]
    public void Build_RejectsMissingOrInvalidRemovalReason(string reason)
    {
        var result = new Dictionary<string, object> {
            { "timelineV2", new Dictionary<string, object> { { "version", 2 },
                { "steps", new List<object> { new Dictionary<string, object> {
                    { "type", "EffectRemoved" }, { "actorCardId", "caster" },
                    { "targetCardId", "recipient" }, { "effectType", 4 }, { "removalReason", reason }
                } } }
            } }
        };
        Assert.IsFalse(TryBuildTimelineFromBattleResultDict(result, out _, out string error));
        StringAssert.Contains("removal reason", error);
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
    public void Build_Fails_WhenTimelineV2IsInvalid()
    {
        var battleResult = new Dictionary<string, object>
        {
            { "timelineV2", new Dictionary<string, object> { { "version", 2 } } },
            {
                "firstAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_A" }, { "attackId", 1 }, { "damageDealt", 3 }, { "healAmount", 0 }
                }
            },
            {
                "secondAttacker", new Dictionary<string, object>
                {
                    { "cardId", "legacy_B" }, { "attackId", 1 }, { "damageDealt", 0 }, { "healAmount", 0 }
                }
            }
        };

        bool ok = TryBuildTimelineFromBattleResultDict(battleResult, out _, out string error);

        Assert.IsFalse(ok);
        StringAssert.Contains("timelineV2 invalid", error);
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
    public void Build_TimelineV2StatChange_ParsesStatNameAndAmount()
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
                                { "attackId", 15 }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "card_A" },
                                { "targetCardId", "card_B" },
                                { "statName", "ATT" },
                                { "amount", -1 },
                                { "source", "attack" }
                            },
                            new Dictionary<string, object>
                            {
                                { "type", "StatChange" },
                                { "actorCardId", "card_A" },
                                { "targetCardId", "card_B" },
                                { "statName", "DEF" },
                                { "amount", -1 },
                                { "source", "attack" }
                            }
                        }
                    }
                }
            },
            { "firstAttacker", new Dictionary<string, object> { { "cardId", "legacy_A" }, { "attackId", 1 } } },
            { "secondAttacker", new Dictionary<string, object> { { "cardId", "legacy_B" }, { "attackId", 1 } } }
        };

        List<object> steps = BuildTimelineFromBattleResultDict(battleResult);
        var statChanges = GetStepsByType(steps, "StatChange").ToList();

        Assert.AreEqual(2, statChanges.Count, "Expected two stat change steps from timelineV2.");
        Assert.AreEqual("ATT", GetStringField(statChanges[0], "StatName"));
        Assert.AreEqual(-1, GetIntField(statChanges[0], "Amount"));
        Assert.AreEqual("card_B", GetStringField(statChanges[0], "TargetCardId"));
        Assert.AreEqual("DEF", GetStringField(statChanges[1], "StatName"));
        Assert.AreEqual(-1, GetIntField(statChanges[1], "Amount"));
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

    private static IEnumerable<string> EnumerateBattleFixturePaths()
    {
        string fixturesDir = ResolveFixturesDir();
        Assert.IsTrue(Directory.Exists(fixturesDir), $"Fixtures directory not found: {fixturesDir}");

        var files = Directory.GetFiles(fixturesDir, "*.json")
            .Where(FixtureHasTimelineV2)
            .OrderBy(Path.GetFileName)
            .ToList();

        if (files.Count == 0)
        {
            Assert.Ignore("BattleTimelineBuilderTests fixture corpus does not contain timelineV2 yet.");
        }

        return files;
    }

    private static bool FixtureHasTimelineV2(string path)
    {
        return File.ReadAllText(path).Contains("\"timelineV2\"");
    }

    private static FixtureEnvelope LoadFixtureEnvelope(string fileName)
    {
        string path = Path.Combine(ResolveFixturesDir(), fileName);
        Assert.IsTrue(File.Exists(path), $"Fixture not found: {path}");

        string json = File.ReadAllText(path);
        var envelope = JsonUtility.FromJson<FixtureEnvelope>(json);
        Assert.IsNotNull(envelope, $"Envelope parse failed for fixture: {fileName}");
        return envelope;
    }

    private static List<object> BuildTimelineFromFixture(string fileName)
    {
        string path = Path.Combine(ResolveFixturesDir(), fileName);
        if (!FixtureHasTimelineV2(path))
        {
            Assert.Ignore($"Fixture {fileName} does not include timelineV2 yet.");
        }

        return BuildTimelineFromBattleResultDict(LoadFixtureBattleResultDict(fileName));
    }

    private static Dictionary<string, object> LoadFixtureBattleResultDict(string fileName)
    {
        string path = Path.Combine(ResolveFixturesDir(), fileName);
        Assert.IsTrue(File.Exists(path), $"Fixture not found: {path}");

        string json = File.ReadAllText(path);
        string battleResultJson = ExtractJsonObjectForProperty(json, "battleResult");
        Assert.IsFalse(string.IsNullOrWhiteSpace(battleResultJson), $"Fixture has no battleResult: {fileName}");

        return ParseJsonObjectToDict(battleResultJson, fileName);
    }

    private static Dictionary<string, object> ParseJsonObjectToDict(string json, string fileName)
    {
        var builderType = Type.GetType("BattleTimelineBuilder, Assembly-CSharp");
        Assert.IsNotNull(builderType, "Type BattleTimelineBuilder was not found in Assembly-CSharp.");

        MethodInfo toDictMethod = builderType.GetMethod("ToDict", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(toDictMethod, "Method BattleTimelineBuilder.ToDict was not found.");

        var result = toDictMethod.Invoke(null, new object[] { json }) as Dictionary<string, object>;
        Assert.IsNotNull(result, $"Fixture battleResult parse failed: {fileName}");
        return result;
    }

    private static string ExtractJsonObjectForProperty(string json, string propertyName)
    {
        string needle = $"\"{propertyName}\"";
        int propertyIndex = json.IndexOf(needle, StringComparison.Ordinal);
        if (propertyIndex < 0)
        {
            return null;
        }

        int colonIndex = json.IndexOf(':', propertyIndex + needle.Length);
        if (colonIndex < 0)
        {
            return null;
        }

        int objectStart = json.IndexOf('{', colonIndex + 1);
        if (objectStart < 0)
        {
            return null;
        }

        int depth = 0;
        bool inString = false;
        bool escaped = false;

        for (int i = objectStart; i < json.Length; i++)
        {
            char c = json[i];

            if (escaped)
            {
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString)
            {
                continue;
            }

            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return json.Substring(objectStart, i - objectStart + 1);
                }
            }
        }

        return null;
    }

    private static bool TryBuildTimelineFromBattleResultDict(
        Dictionary<string, object> battleResultDict,
        out List<object> steps,
        out string error)
    {
        steps = null;
        error = null;

        var builderType = Type.GetType("BattleTimelineBuilder, Assembly-CSharp");
        Assert.IsNotNull(builderType, "Type BattleTimelineBuilder was not found in Assembly-CSharp.");

        MethodInfo tryBuildMethod = builderType
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .FirstOrDefault(m => m.Name == "TryBuild" && m.GetParameters().Length == 3);
        Assert.IsNotNull(tryBuildMethod, "Method BattleTimelineBuilder.TryBuild was not found.");

        object[] args = { battleResultDict, null, null };
        bool ok = (bool)tryBuildMethod.Invoke(null, args);
        error = args[2] as string;

        if (args[1] is IEnumerable enumerable)
        {
            steps = enumerable.Cast<object>().ToList();
        }

        return ok;
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
        var dict = new Dictionary<string, object>
        {
            { "firstAttacker", ToAttackerDict(result.firstAttacker) },
            { "secondAttacker", ToAttackerDict(result.secondAttacker) },
            { "cardDied", result.cardDied },
            { "winnerCardId", result.winnerCardId },
            { "loserCardId", result.loserCardId }
        };

        if (result.timelineV2 != null)
        {
            dict["timelineV2"] = ToTimelineV2Dict(result.timelineV2);
        }

        return dict;
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

    private static Dictionary<string, object> ToTimelineV2Dict(FixtureTimelineV2 timeline)
    {
        return new Dictionary<string, object>
        {
            { "version", timeline.version },
            { "steps", ToTimelineStepDictList(timeline.steps) }
        };
    }

    private static List<object> ToTimelineStepDictList(List<FixtureTimelineStep> steps)
    {
        if (steps == null)
        {
            return new List<object>();
        }

        return steps.Select(step => (object)ToTimelineStepDict(step)).ToList();
    }

    private static Dictionary<string, object> ToTimelineStepDict(FixtureTimelineStep step)
    {
        int? blockedBy = step.blockedBy;
        if (step.blocked && blockedBy == null && step.effectType > 0)
        {
            blockedBy = step.effectType;
        }

        return new Dictionary<string, object>
        {
            { "type", step.type },
            { "actorCardId", step.actorCardId },
            { "targetCardId", step.targetCardId },
            { "attackId", step.attackId },
            { "attackResult", step.attackResult },
            { "amount", step.amount },
            { "statName", step.statName },
            { "effectType", step.effectType },
            { "removalReason", step.removalReason },
            { "duration", step.duration },
            { "blocked", step.blocked },
            { "blockedBy", blockedBy },
            { "skipped", step.skipped },
            { "source", step.source },
            { "note", step.note },
            { "effectsApplied", ToEffectDictList(step.effectsApplied) },
            { "attackerEffectsApplied", ToEffectDictList(step.attackerEffectsApplied) }
        };
    }

    // Unity JsonUtility populates these fixture fields through reflection.
#pragma warning disable CS0649
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
        public FixtureTimelineV2 timelineV2;
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
    private class FixtureTimelineV2
    {
        public int version;
        public List<FixtureTimelineStep> steps;
    }

    [Serializable]
    private class FixtureTimelineStep
    {
        public string type;
        public string actorCardId;
        public string targetCardId;
        public int attackId;
        public string attackResult;
        public int amount;
        public string statName;
        public int effectType;
        public string removalReason;
        public int duration;
        public bool blocked;
        public int? blockedBy;
        public bool skipped;
        public string source;
        public string note;
        public List<FixtureEffect> effectsApplied;
        public List<FixtureEffect> attackerEffectsApplied;
    }

    [Serializable]
    private class FixtureEffect
    {
        public int type;
        public int duration;
        public string source;
    }
#pragma warning restore CS0649
}





