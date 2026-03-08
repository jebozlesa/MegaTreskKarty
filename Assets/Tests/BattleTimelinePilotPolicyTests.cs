using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BattleTimelinePilotPolicyTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("battle.timeline.v2.enabled");
        PlayerPrefs.DeleteKey("battle.timeline.v1.fallback");
        PlayerPrefs.DeleteKey("battle.timeline.v2.hard_mode");
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("battle.timeline.v2.enabled");
        PlayerPrefs.DeleteKey("battle.timeline.v1.fallback");
        PlayerPrefs.DeleteKey("battle.timeline.v2.hard_mode");
    }

    [Test]
    public void IsEligible_ReturnsTrue_ForSimpleAttacksWithoutEdgeCases()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        bool eligible = InvokeIsEligible(parsed);

        Assert.IsTrue(eligible);
    }

    [Test]
    public void IsEligible_ReturnsTrue_WhenAttackIsBlocked()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyAttackBlocked", true);
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        bool eligible = InvokeIsEligible(parsed);

        Assert.IsTrue(eligible);
    }

    [Test]
    public void IsEligible_AllowsEffects_WhenNoOtherEdgeCases()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 5);
        SetField(parsed, "MyEffectsApplied", new List<Dictionary<string, object>> { new Dictionary<string, object> { { "type", 3 } } });
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        bool eligible = InvokeIsEligible(parsed);

        Assert.IsTrue(eligible);
    }

    [Test]
    public void IsEligible_ReturnsFalse_WhenAttackerSelfEffectsPresent()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyAttackerEffects", new List<Dictionary<string, object>> { new Dictionary<string, object> { { "type", 1 } } });
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        bool eligible = InvokeIsEligible(parsed);

        Assert.IsFalse(eligible);
    }

    [Test]
    public void IsEligible_ReturnsFalse_WhenStatChangesPresent()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        object statChanges = CreateAttackStatChanges();
        SetField(statChanges, "attackerAttack", 1);
        SetField(parsed, "MyStatChanges", statChanges);
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        bool eligible = InvokeIsEligible(parsed);

        Assert.IsFalse(eligible);
    }

    [Test]
    public void ShouldRunTimeline_ReturnsTrue_WhenV2EnabledAndPayloadPresent()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        var battleResult = new Dictionary<string, object>
        {
            { "timelineV2", new Dictionary<string, object> { { "version", 2 }, { "steps", new List<object>() } } }
        };

        bool shouldRun = InvokeShouldRunTimeline(battleResult, parsed, out string mode);

        Assert.IsTrue(shouldRun);
        Assert.AreEqual("v2", mode);
    }

    [Test]
    public void ShouldRunTimeline_UsesV1Fallback_WhenV2PayloadMissing()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());
        PlayerPrefs.SetInt("battle.timeline.v2.hard_mode", 0);

        var battleResult = new Dictionary<string, object>();

        bool shouldRun = InvokeShouldRunTimeline(battleResult, parsed, out string mode);

        Assert.IsTrue(shouldRun);
        Assert.AreEqual("v1_fallback", mode);
    }

    [Test]
    public void ShouldRunTimeline_ReturnsFalse_WhenV2PayloadMissingAndFallbackDisabled()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        PlayerPrefs.SetInt("battle.timeline.v2.hard_mode", 0);
        PlayerPrefs.SetInt("battle.timeline.v1.fallback", 0);

        var battleResult = new Dictionary<string, object>();

        bool shouldRun = InvokeShouldRunTimeline(battleResult, parsed, out string mode);

        Assert.IsFalse(shouldRun);
        Assert.AreEqual("v2_no_payload", mode);
    }

    [Test]
    public void ShouldRunTimeline_ReturnsFalse_WhenV2PayloadMissingAndHardModeEnabled()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());
        PlayerPrefs.SetInt("battle.timeline.v2.hard_mode", 1);

        var battleResult = new Dictionary<string, object>();

        bool shouldRun = InvokeShouldRunTimeline(battleResult, parsed, out string mode);

        Assert.IsFalse(shouldRun);
        Assert.AreEqual("v2_hard_missing", mode);
    }

    [Test]
    public void ShouldRunTimeline_ReturnsFalse_WhenV2Disabled()
    {
        object parsed = CreateParsedBattleResult();
        SetField(parsed, "MyAttackId", 1);
        SetField(parsed, "EnemyAttackId", 2);
        SetField(parsed, "MyStatChanges", CreateAttackStatChanges());
        SetField(parsed, "EnemyStatChanges", CreateAttackStatChanges());

        PlayerPrefs.SetInt("battle.timeline.v2.enabled", 0);

        var battleResult = new Dictionary<string, object>
        {
            { "timelineV2", new Dictionary<string, object> { { "version", 2 }, { "steps", new List<object>() } } }
        };

        bool shouldRun = InvokeShouldRunTimeline(battleResult, parsed, out string mode);

        Assert.IsFalse(shouldRun);
        Assert.AreEqual("v2_disabled", mode);
    }

    private static bool InvokeIsEligible(object parsed)
    {
        Type policyType = Type.GetType("BattleTimelinePilotPolicy, Assembly-CSharp");
        Assert.IsNotNull(policyType, "Type BattleTimelinePilotPolicy was not found in Assembly-CSharp.");

        MethodInfo method = policyType.GetMethod("IsEligible", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(method, "Method BattleTimelinePilotPolicy.IsEligible was not found.");

        return (bool)method.Invoke(null, new[] { parsed });
    }

    private static bool InvokeShouldRunTimeline(
        Dictionary<string, object> battleResult,
        object parsed,
        out string mode)
    {
        Type policyType = Type.GetType("BattleTimelinePilotPolicy, Assembly-CSharp");
        Assert.IsNotNull(policyType, "Type BattleTimelinePilotPolicy was not found in Assembly-CSharp.");

        MethodInfo method = policyType.GetMethod("ShouldRunTimeline", BindingFlags.Public | BindingFlags.Static);
        Assert.IsNotNull(method, "Method BattleTimelinePilotPolicy.ShouldRunTimeline was not found.");

        object[] args = { battleResult, parsed, null };
        bool result = (bool)method.Invoke(null, args);
        mode = args[2] as string;
        return result;
    }

    private static object CreateParsedBattleResult()
    {
        Type parsedType = Type.GetType("ParsedBattleResult, Assembly-CSharp");
        Assert.IsNotNull(parsedType, "Type ParsedBattleResult was not found in Assembly-CSharp.");

        object parsed = Activator.CreateInstance(parsedType);

        SetField(parsed, "MyEffectsApplied", new List<Dictionary<string, object>>());
        SetField(parsed, "EnemyEffectsApplied", new List<Dictionary<string, object>>());
        SetField(parsed, "MyAttackerEffects", new List<Dictionary<string, object>>());
        SetField(parsed, "EnemyAttackerEffects", new List<Dictionary<string, object>>());
        SetField(parsed, "MyBleedDamages", new List<int>());
        SetField(parsed, "EnemyBleedDamages", new List<int>());

        return parsed;
    }

    private static object CreateAttackStatChanges()
    {
        Type statType = Type.GetType("AttackStatChanges, Assembly-CSharp");
        Assert.IsNotNull(statType, "Type AttackStatChanges was not found in Assembly-CSharp.");
        return Activator.CreateInstance(statType);
    }

    private static void SetField(object target, string fieldName, object value)
    {
        FieldInfo field = target.GetType().GetField(fieldName);
        Assert.IsNotNull(field, $"Missing field '{fieldName}' on {target.GetType().Name}");
        field.SetValue(target, value);
    }
}
