using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

public class BattleRoundCoordinatorTests
{
    [Test]
    public void TryGetSuccessFlag_ReturnsTrue_ForDictionaryPayload()
    {
        object payload = new Dictionary<string, object> { { "success", true } };

        bool success = InvokeTryGetSuccessFlag(payload, out var flag);

        Assert.IsTrue(success);
        Assert.IsTrue(flag);
    }

    [Test]
    public void TryGetSuccessFlag_ReturnsFalse_ForMissingFlag()
    {
        object payload = new Dictionary<string, object> { { "message", "ok" } };

        bool success = InvokeTryGetSuccessFlag(payload, out var flag);

        Assert.IsFalse(success);
        Assert.IsFalse(flag);
    }

    [Test]
    public void IsBothPlayersReadyFromMatchState_ReturnsTrue_WhenReadyFlagIsSet()
    {
        object matchState = CreateMatchState(
            phase: "waiting_for_result_ack",
            bothReady: true);

        bool ready = InvokeIsBothPlayersReadyFromMatchState(matchState);

        Assert.IsTrue(ready);
    }

    [Test]
    public void IsBothPlayersReadyFromMatchState_ReturnsTrue_WhenPhaseIsSelectingAttacks()
    {
        object matchState = CreateMatchState(
            phase: "selecting_attacks",
            bothReady: false);

        bool ready = InvokeIsBothPlayersReadyFromMatchState(matchState);

        Assert.IsTrue(ready);
    }

    [Test]
    public void IsPlayerMarkedReadyInMatchState_ReadsReadyFlagByPlayerId()
    {
        object matchState = CreateMatchState(
            phase: "waiting_for_result_ack",
            bothReady: false,
            readyByPlayerId: new Dictionary<string, bool>
            {
                { "p1", false }
            });

        bool ready = InvokeIsPlayerMarkedReadyInMatchState(matchState, "p1");

        Assert.IsFalse(ready);
    }

    [Test]
    public void HasNextTurnReadyPhaseAdvanced_ReturnsTrue_WhenPhaseIsWaitingForAttacks()
    {
        object matchState = CreateMatchState(
            phase: "waiting_for_attacks",
            bothReady: false);

        bool advanced = InvokeHasNextTurnReadyPhaseAdvanced(matchState);

        Assert.IsTrue(advanced);
    }

    [Test]
    public void HasNextTurnReadyPhaseAdvanced_ReturnsFalse_WhenPhaseIsWaitingForResultAck()
    {
        object matchState = CreateMatchState(
            phase: "waiting_for_result_ack",
            bothReady: false);

        bool advanced = InvokeHasNextTurnReadyPhaseAdvanced(matchState);

        Assert.IsFalse(advanced);
    }

    private static bool InvokeTryGetSuccessFlag(object payload, out bool flag)
    {
        Type coordinatorType = Type.GetType("BattleRoundCoordinator, Assembly-CSharp");
        Assert.IsNotNull(coordinatorType, "Type BattleRoundCoordinator was not found in Assembly-CSharp.");

        MethodInfo method = coordinatorType.GetMethod("TryGetSuccessFlag", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, "Method TryGetSuccessFlag was not found.");

        object[] args = { payload, false };
        bool ok = (bool)method.Invoke(null, args);
        flag = (bool)args[1];
        return ok;
    }

    private static bool InvokeIsBothPlayersReadyFromMatchState(object matchState)
    {
        Type coordinatorType = Type.GetType("BattleRoundCoordinator, Assembly-CSharp");
        Assert.IsNotNull(coordinatorType, "Type BattleRoundCoordinator was not found in Assembly-CSharp.");

        Type matchStateType = Type.GetType("MatchStateDto, Assembly-CSharp");
        Assert.IsNotNull(matchStateType, "Type MatchStateDto was not found in Assembly-CSharp.");

        MethodInfo method = coordinatorType.GetMethod("IsBothPlayersReadyFromMatchState", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, "Method IsBothPlayersReadyFromMatchState was not found.");

        return (bool)method.Invoke(null, new[] { matchState });
    }

    private static bool InvokeIsPlayerMarkedReadyInMatchState(object matchState, string playerId)
    {
        Type coordinatorType = Type.GetType("BattleRoundCoordinator, Assembly-CSharp");
        Assert.IsNotNull(coordinatorType, "Type BattleRoundCoordinator was not found in Assembly-CSharp.");

        MethodInfo method = coordinatorType.GetMethod("IsPlayerMarkedReadyInMatchState", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, "Method IsPlayerMarkedReadyInMatchState was not found.");

        return (bool)method.Invoke(null, new[] { matchState, playerId });
    }

    private static bool InvokeHasNextTurnReadyPhaseAdvanced(object matchState)
    {
        Type coordinatorType = Type.GetType("BattleRoundCoordinator, Assembly-CSharp");
        Assert.IsNotNull(coordinatorType, "Type BattleRoundCoordinator was not found in Assembly-CSharp.");

        MethodInfo method = coordinatorType.GetMethod("HasNextTurnReadyPhaseAdvanced", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(method, "Method HasNextTurnReadyPhaseAdvanced was not found.");

        return (bool)method.Invoke(null, new[] { matchState });
    }

    private static object CreateMatchState(string phase, bool bothReady, Dictionary<string, bool> readyByPlayerId = null)
    {
        Type matchStateType = Type.GetType("MatchStateDto, Assembly-CSharp");
        Assert.IsNotNull(matchStateType, "Type MatchStateDto was not found in Assembly-CSharp.");

        Type nextTurnReadyType = Type.GetType("MatchNextTurnReadyDto, Assembly-CSharp");
        Assert.IsNotNull(nextTurnReadyType, "Type MatchNextTurnReadyDto was not found in Assembly-CSharp.");

        object matchState = Activator.CreateInstance(matchStateType);
        object nextTurnReady = Activator.CreateInstance(nextTurnReadyType);

        SetField(matchState, "phase", phase);
        SetField(nextTurnReady, "bothReady", bothReady);
        SetField(nextTurnReady, "readyByPlayerId", readyByPlayerId ?? new Dictionary<string, bool>());
        SetField(nextTurnReady, "readyPlayerIds", new List<string>());
        SetField(nextTurnReady, "readyCount", 0);
        SetField(matchState, "nextTurnReady", nextTurnReady);

        return matchState;
    }

    private static void SetField(object instance, string fieldName, object value)
    {
        Assert.IsNotNull(instance);
        FieldInfo field = instance.GetType().GetField(fieldName);
        Assert.IsNotNull(field, $"Field {fieldName} was not found on {instance.GetType().Name}.");
        field.SetValue(instance, value);
    }
}
