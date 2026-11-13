using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerInputWindowTests
{
    [Test]
    public void InputWindowRequiresOpenUnacceptedLocalAction()
    {
        Type windowType = RuntimeType("MatchInputWindowDto");
        Type serviceType = RuntimeType("MultiplayerService");
        MethodInfo actionable = serviceType.GetMethod("IsWindowActionable",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(actionable);
        object window = Activator.CreateInstance(windowType);
        Set(window, "kind", "attack");
        Set(window, "token", "WINDOW-1");
        Set(window, "openedAt", "2026-11-12T10:00:00Z");
        Set(window, "requiredPlayerIds", new List<string> { "p1", "p2" });
        Set(window, "acceptedByPlayerId", new Dictionary<string, bool>
            { ["p1"] = false, ["p2"] = true });

        Assert.IsTrue(Check(actionable, window, "p1", "attack", 1000));
        Assert.IsFalse(Check(actionable, window, "p2", "attack", 1000));
        Assert.IsFalse(Check(actionable, window, "p1", "fighter", 1000));
        Assert.IsFalse(Check(actionable, window, "p1", "attack", 0));
        Set(window, "openedAt", null);
        Assert.IsFalse(Check(actionable, window, "p1", "attack", 1000));
    }

    [Test]
    public void RepeatedSnapshotCannotExtendSameWindowDeadline()
    {
        var root = new GameObject("InputWindowTest");
        try
        {
            Component service = root.AddComponent(RuntimeType("MultiplayerService"));
            Set(service, "roomCode", "ROOM");
            Set(service, "myPlayerId", "p1");
            MethodInfo apply = service.GetType().GetMethod("ApplyInputWindowSnapshot");
            object first = State(5, "WINDOW-1", 20000, false);
            apply.Invoke(service, new[] { first });
            double firstDeadline = (double)Get(service, "inputDeadline");
            apply.Invoke(service, new[] { State(6, "WINDOW-1", 29000, false) });
            double repeatedDeadline = (double)Get(service, "inputDeadline");
            Assert.LessOrEqual(repeatedDeadline, firstDeadline);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test]
    public void PendingReplacementSnapshotAcceptsNullRemainingTime()
    {
        MethodInfo parse = RuntimeType("BattleContractMapper").GetMethod(
            "ParseMatchStateReadResult", BindingFlags.Static | BindingFlags.Public);
        Assert.IsNotNull(parse);

        object readResult = null;
        Assert.DoesNotThrow(() => readResult = parse.Invoke(null, new object[]
        {
            PendingReplacementEnvelope(), "ROOM", "p1"
        }));

        Assert.AreEqual("State", Get(readResult, "outcome").ToString());
        object state = Get(readResult, "state");
        object window = Get(state, "inputWindow");
        Assert.AreEqual("replacement", Get(window, "kind"));
        Assert.IsNull(Get(window, "remainingMs"));
    }

    [Test]
    public void PendingReplacementOpensSameTokenWithFreshCountdown()
    {
        var root = new GameObject("PendingReplacementWindowTest");
        try
        {
            Component service = root.AddComponent(RuntimeType("MultiplayerService"));
            Set(service, "roomCode", "ROOM");
            Set(service, "myPlayerId", "p1");
            MethodInfo apply = service.GetType().GetMethod("ApplyInputWindowSnapshot");
            MethodInfo currentToken = service.GetType().GetMethod("CurrentInputToken");

            apply.Invoke(service, new[] { ParseState(PendingReplacementEnvelope()) });
            Assert.IsNull(currentToken.Invoke(service, new object[] { "replacement" }));

            apply.Invoke(service, new[] { ParseState(OpenReplacementEnvelope()) });
            Assert.AreEqual("REPLACEMENT-1",
                currentToken.Invoke(service, new object[] { "replacement" }));
            int remaining = (int)service.GetType().GetProperty("InputRemainingMilliseconds")
                .GetValue(service);
            Assert.Greater(remaining, 25000);
            Assert.LessOrEqual(remaining, 30000);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [TestCase(32000, 30000)]
    [TestCase(30000, 30000)]
    [TestCase(12345, 12345)]
    [TestCase(0, 0)]
    public void CountdownDisplayNeverStartsAboveThirtySeconds(int remainingMs, int expectedMs)
    {
        MethodInfo clamp = RuntimeType("MultiplayerService").GetMethod(
            "ClampInputDisplayMilliseconds", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(clamp);
        Assert.AreEqual(expectedMs, clamp.Invoke(null, new object[] { remainingMs }));
    }

    [TestCase(30000, false)]
    [TestCase(15001, false)]
    [TestCase(15000, true)]
    [TestCase(1, true)]
    [TestCase(0, false)]
    public void CountdownPresentationIsVisibleOnlyFromFifteenSeconds(int remainingMs, bool expected)
    {
        MethodInfo visible = RuntimeType("MultiplayerUI").GetMethod(
            "ShouldShowInputCountdown", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(visible);
        Assert.AreEqual(expected, visible.Invoke(null, new object[] { remainingMs }));
    }

    [TestCase(4000, 0)]
    [TestCase(3000, 1)]
    [TestCase(2501, 1)]
    [TestCase(2500, 2)]
    [TestCase(2000, 1)]
    [TestCase(1500, 2)]
    [TestCase(1000, 1)]
    [TestCase(500, 2)]
    [TestCase(0, 0)]
    public void CountdownColorUsesApprovedHalfSecondPulse(int remainingMs, int expectedPhase)
    {
        MethodInfo phase = RuntimeType("MultiplayerUI").GetMethod(
            "GetInputCountdownColorPhase", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(phase);
        Assert.AreEqual(expectedPhase, phase.Invoke(null, new object[] { remainingMs }));
    }

    [Test]
    public void CountdownUrgencyColorsFaceThenRestoresAuthoredFace()
    {
        var root = new GameObject("InputCountdownColorTest");
        var faceObject = new GameObject("Face");
        try
        {
            faceObject.transform.SetParent(root.transform, false);
            Image face = faceObject.AddComponent<Image>();
            Color authoredFace = new Color(0.9f, 0.9f, 0.9f, 1f);
            face.color = authoredFace;
            Component ui = root.AddComponent(RuntimeType("MultiplayerUI"));
            Set(ui, "inputCountdownFace", face);
            Set(ui, "inputCountdownUrgentColor", Color.red);
            Set(ui, "inputCountdownPulseColor", Color.blue);
            ui.GetType().GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(ui, null);
            MethodInfo apply = ui.GetType().GetMethod(
                "ApplyInputCountdownColorPhase", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(apply);

            apply.Invoke(ui, new object[] { 1 });
            Assert.AreEqual(Color.red, face.color);

            apply.Invoke(ui, new object[] { 2 });
            Assert.AreEqual(Color.blue, face.color);

            apply.Invoke(ui, new object[] { 0 });
            Assert.AreEqual(authoredFace, face.color);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test]
    public void CountdownAlarmShakeKeepsConstantAmplitudeAndReturnsToOrigin()
    {
        MethodInfo evaluate = RuntimeType("MultiplayerUI").GetMethod(
            "EvaluateInputCountdownShake", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(evaluate);
        float start = (float)evaluate.Invoke(null, new object[] { 0f });
        float positive = (float)evaluate.Invoke(null, new object[] { 1f / 24f });
        float negative = (float)evaluate.Invoke(null, new object[] { 3f / 24f });
        float end = (float)evaluate.Invoke(null, new object[] { 1f });
        Assert.AreEqual(0f, start, 0.0001f);
        Assert.Greater(positive, 0f);
        Assert.Less(negative, 0f);
        Assert.AreEqual(0f, end, 0.0001f);
        Assert.AreEqual(Mathf.Abs(positive),
            Mathf.Abs((float)evaluate.Invoke(null, new object[] { 17f / 24f })), 0.0001f);
    }

    [Test]
    public void CountdownPresentationHasNoTranslationOrGlowConfiguration()
    {
        Type ui = RuntimeType("MultiplayerUI");
        Assert.IsNull(ui.GetField("inputCountdownShakeOffset",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.IsNull(ui.GetField("inputCountdownShakeOriginPosition",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.IsNull(ui.GetField("inputCountdownGlow",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        Assert.IsNull(ui.GetField("inputCountdownGlowAlpha",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
    }

    [Test]
    public void CountdownSoundsAreEdgeTriggeredAndResetBetweenWindows()
    {
        var root = new GameObject("InputCountdownPresentationTest");
        try
        {
            Component ui = root.AddComponent(RuntimeType("MultiplayerUI"));
            MethodInfo advance = ui.GetType().GetMethod(
                "AdvanceInputCountdownSoundState", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(advance);

            Assert.AreEqual(1, advance.Invoke(ui, new object[] { true, 15000 }));
            Assert.AreEqual(0, advance.Invoke(ui, new object[] { true, 14900 }));
            Assert.AreEqual(2, advance.Invoke(ui, new object[] { true, 3000 }));
            Assert.AreEqual(0, advance.Invoke(ui, new object[] { true, 2500 }));
            Assert.AreEqual(2, advance.Invoke(ui, new object[] { true, 2000 }));
            Assert.AreEqual(2, advance.Invoke(ui, new object[] { true, 1000 }));
            Assert.AreEqual(0, advance.Invoke(ui, new object[] { true, 900 }));
            Assert.AreEqual(0, advance.Invoke(ui, new object[] { false, 0 }));
            Assert.AreEqual(1, advance.Invoke(ui, new object[] { true, 15000 }));
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test]
    public void LateCountdownSnapshotPlaysOnlyCurrentUrgentCue()
    {
        var root = new GameObject("LateInputCountdownPresentationTest");
        try
        {
            Component ui = root.AddComponent(RuntimeType("MultiplayerUI"));
            MethodInfo advance = ui.GetType().GetMethod(
                "AdvanceInputCountdownSoundState", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(advance);

            Assert.AreEqual(2, advance.Invoke(ui, new object[] { true, 2200 }));
            Assert.AreEqual(0, advance.Invoke(ui, new object[] { true, 2100 }));
            Assert.AreEqual(2, advance.Invoke(ui, new object[] { true, 1900 }));
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    [Test]
    public void NoWinnerInputTimeoutIsLocalLossAndStaysInBattleScene()
    {
        var root = new GameObject("InputTimeoutTest");
        try
        {
            Component service = root.AddComponent(RuntimeType("MultiplayerService"));
            Component fight = root.AddComponent(RuntimeType("FightSystemMultiplayer"));
            ((Behaviour)fight).enabled = false;
            Set(service, "roomCode", "ROOM");
            Set(service, "myPlayerId", "p1");
            Set(service, "fightSystem", fight);
            PlayerPrefs.SetString("RoomCode", "ROOM");
            object state = State(7, "WINDOW-1", 0, true);
            bool routeToLobby = (bool)service.GetType().GetMethod("PrepareRemoteMatchCompletion",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(service, new[] { state });
            Assert.IsFalse(routeToLobby);
            Assert.AreEqual("LOST", fight.GetType().GetField("state").GetValue(fight).ToString());
            Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
        }
        finally
        {
            PlayerPrefs.DeleteKey("RoomCode");
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    [Test]
    public void MultiplayerSceneWiresOneCountdownToMultiplayerUi()
    {
        string scene = System.IO.File.ReadAllText(
            System.IO.Path.Combine(Application.dataPath, "Scenes", "Multiplayer.unity"));
        StringAssert.Contains("m_Name: InputCountdown", scene);
        StringAssert.DoesNotContain("RecordHandler", scene);
        StringAssert.IsMatch(@"multiplayerService: \{fileID: [1-9][0-9]*\}", scene);
        StringAssert.IsMatch(@"inputCountdownText: \{fileID: [1-9][0-9]*\}", scene);
        StringAssert.IsMatch(@"inputCountdownFrame: \{fileID: [1-9][0-9]*\}", scene);
        StringAssert.IsMatch(@"inputCountdownFace: \{fileID: [1-9][0-9]*\}", scene);
        StringAssert.DoesNotContain("inputCountdownGlow:", scene);
        StringAssert.DoesNotContain("m_Name: InputCountdownGlow", scene);
        StringAssert.DoesNotContain("inputCountdownGlowAlpha:", scene);
        StringAssert.DoesNotContain("inputCountdownShakeOffset:", scene);
        StringAssert.Contains(
            "m_SizeDelta: {x: 150, y: 120}\n  m_Pivot: {x: 0.5, y: 0.5}",
            scene.Replace("\r\n", "\n"));
        StringAssert.Contains(
            "inputCountdownRevealSound: {fileID: 8300000, guid: 3cf1d197019efbe4f9fbb16fdf25c63a, type: 3}",
            scene);
        StringAssert.Contains(
            "inputCountdownUrgencySound: {fileID: 8300000, guid: 4fc5e52d7401f7a49b4593e53f160c3f, type: 3}",
            scene);
    }

    private static object State(int version, string token, int remainingMs, bool completed)
    {
        string result = completed
            ? ",\"matchResult\":{\"reason\":\"input_timeout\",\"winnerPlayerId\":null,\"loserPlayerId\":null}"
            : string.Empty;
        string state = "{\"schemaVersion\":2,\"version\":" + version
            + ",\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\",\"status\":\""
            + (completed ? "completed" : "active") + "\",\"phase\":\""
            + (completed ? "completed" : "selecting_attacks") + "\",\"inputWindow\":{"
            + "\"token\":\"" + token + "\",\"kind\":\"attack\","
            + "\"openedAt\":\"2026-11-12T10:00:00Z\",\"remainingMs\":" + remainingMs + ","
            + "\"requiredPlayerIds\":[\"p1\",\"p2\"],\"acceptedByPlayerId\":{\"p1\":false,\"p2\":false}}"
            + result + "}";
        return ParseState("{\"success\":true,\"matchState\":" + state + "}");
    }

    private static object ParseState(string envelope)
    {
        MethodInfo parse = RuntimeType("BattleContractMapper").GetMethod(
            "ParseMatchStateResult", BindingFlags.Static | BindingFlags.Public);
        return parse.Invoke(null, new object[] { envelope });
    }

    private static string PendingReplacementEnvelope()
    {
        return ReplacementEnvelope(5, null, "null");
    }

    private static string OpenReplacementEnvelope()
    {
        return ReplacementEnvelope(6, "2026-11-13T12:00:00Z", "30000");
    }

    private static string ReplacementEnvelope(int version, string openedAt, string remainingMs)
    {
        string openedAtJson = openedAt == null ? "null" : "\"" + openedAt + "\"";
        return "{\"success\":true,\"matchState\":{\"schemaVersion\":2,\"version\":" + version
            + ",\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\",\"status\":\"active\""
            + ",\"phase\":\"waiting_for_result_ack\",\"inputWindow\":{"
            + "\"token\":\"REPLACEMENT-1\",\"kind\":\"replacement\",\"openedAt\":"
            + openedAtJson + ",\"remainingMs\":" + remainingMs
            + ",\"requiredPlayerIds\":[\"p1\"],\"acceptedByPlayerId\":{\"p1\":false}"
            + ",\"readyByPlayerId\":{\"p1\":false}}}}";
    }

    private static bool Check(MethodInfo method, object window, string playerId, string kind, int remainingMs)
    {
        return (bool)method.Invoke(null, new object[] { window, playerId, kind, remainingMs });
    }

    private static void Set(object target, string field, object value)
    {
        target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .SetValue(target, value);
    }

    private static object Get(object target, string field)
    {
        return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .GetValue(target);
    }

    private static Type RuntimeType(string name)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType(name);
            if (type != null) return type;
        }
        Assert.Fail("Runtime type not found: " + name);
        return null;
    }
}
