using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerScoreLimitRegressionTests
{
    private GameObject root;
    private string previousRoom;
    private bool hadRoom;

    [SetUp]
    public void SetUp()
    {
        hadRoom = PlayerPrefs.HasKey("RoomCode");
        previousRoom = PlayerPrefs.GetString("RoomCode");
        root = new GameObject("ScoreLimitTest");
        root.SetActive(false);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        PlayerPrefs.DeleteKey("RoomCode");
        if (hadRoom) PlayerPrefs.SetString("RoomCode", previousRoom);
    }

    [Test]
    public void MatchStateMapperPreservesServerScore()
    {
        object state = RuntimeType("BattleContractMapper")
            .GetMethod("ParseMatchStateResult", BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, new object[]
            {
                "{\"success\":true,\"matchState\":{\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\"," +
                "\"score\":{\"version\":1,\"target\":3,\"byPlayerId\":{\"p1\":2,\"p2\":1}}}}"
            });

        object score = state.GetType().GetField("score").GetValue(state);
        Assert.IsNotNull(score);
        Assert.AreEqual(3, score.GetType().GetField("target").GetValue(score));
        object byPlayer = score.GetType().GetField("byPlayerId").GetValue(score);
        Assert.AreEqual(2, byPlayer.GetType().GetProperty("Item").GetValue(byPlayer, new object[] { "p1" }));
    }

    [Test]
    public void ScoreLimitCompletionWaitsForFinalTimelinePlayback()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Component fight = root.AddComponent(RuntimeType("FightSystemMultiplayer"));
        ((Behaviour)fight).enabled = false;
        Set(service, "roomCode", "ROOM");
        Set(service, "myPlayerId", "p1");
        Set(service, "localPrepared", true);
        Set(service, "serverStarted", true);
        Set(service, "fightSystem", fight);
        Set(fight, "multiplayerService", service);
        PlayerPrefs.SetString("RoomCode", "ROOM");
        root.SetActive(true);

        object completed = ScoreLimitState();
        Invoke(service, "ObserveBattleSnapshot", completed);

        Assert.IsFalse((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
        Assert.IsTrue((bool)service.GetType().GetProperty("CanReceiveBattleResult").GetValue(service));
        Assert.IsTrue((bool)service.GetType().GetProperty("HasStagedScoreLimitCompletion").GetValue(service));
        Assert.IsTrue(PlayerPrefs.HasKey("RoomCode"), "Room ownership must survive until playback finishes.");
        Assert.AreNotEqual("WON", fight.GetType().GetField("state").GetValue(fight).ToString());

        Assert.IsTrue((bool)Invoke(service, "CompleteStagedScoreLimitAfterPlayback"));
        Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
        Assert.AreEqual("WON", fight.GetType().GetField("state").GetValue(fight).ToString());
        Assert.IsFalse((bool)service.GetType().GetProperty("CanReceiveBattleResult").GetValue(service));
    }

    [TestCase("score_limit", "p1", "p2", "Leaderboard")]
    [TestCase("score_limit", "p2", "p1", "Leaderboard")]
    [TestCase("input_timeout", "p1", "p2", "Leaderboard")]
    [TestCase("input_timeout", "p2", "p1", "Leaderboard")]
    [TestCase("opponent_left", "p1", "p2", "Leaderboard")]
    [TestCase("surrender", "p1", "p2", "Leaderboard")]
    [TestCase("timeout", "p1", "p2", "Leaderboard")]
    [TestCase("timeout", "p2", "p1", "Lobby")]
    [TestCase("cancelled", null, null, "Lobby")]
    public void CompletedMatchChoosesApprovedDestination(
        string reason,
        string winner,
        string loser,
        string expectedDestination)
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Component fight = root.AddComponent(RuntimeType("FightSystemMultiplayer"));
        ((Behaviour)fight).enabled = false;
        Set(service, "roomCode", "ROOM");
        Set(service, "myPlayerId", "p1");
        Set(service, "fightSystem", fight);
        PlayerPrefs.SetString("RoomCode", "ROOM");
        root.SetActive(true);

        object completed = CompletedState(reason, winner, loser);
        object destination = Invoke(service, "PrepareRemoteMatchCompletion", completed);

        Assert.AreEqual(expectedDestination, destination.ToString());
    }

    [Test]
    public void MultiplayerServiceUsesLeaderboardRouteAfterResultDelay()
    {
        string source = System.IO.File.ReadAllText(System.IO.Path.Combine(
            Application.dataPath,
            "Scripts",
            "Multiplayer",
            "MultiplayerService.cs"
        ));

        StringAssert.Contains("SceneManager.LoadScene(LeaderboardScene);", source);
        StringAssert.DoesNotContain("ReturnToMainAfterCompletedPlayback", source);
    }

    [Test]
    public void KillCounterRendersServerScoreWithoutOwningMatchResult()
    {
        Component manager = root.AddComponent(RuntimeType("MultiplayerKillCounterManager"));
        Array playerIndicators = CreateIndicators("Player");
        Array enemyIndicators = CreateIndicators("Enemy");
        Set(manager, "playerKillIndicators", playerIndicators);
        Set(manager, "enemyKillIndicators", enemyIndicators);
        root.SetActive(true);

        object score = Activator.CreateInstance(RuntimeType("MatchScoreDto"));
        Set(score, "version", 1);
        Set(score, "target", 3);
        Set(score, "byPlayerId", new Dictionary<string, int>
        {
            ["p1"] = 2,
            ["p2"] = 1,
        });
        Invoke(manager, "RenderServerScore", score, "p1");

        Assert.AreEqual(2, manager.GetType().GetProperty("PlayerKillCount").GetValue(manager));
        Assert.AreEqual(1, manager.GetType().GetProperty("EnemyKillCount").GetValue(manager));
        Assert.IsTrue(IndicatorDead(playerIndicators, 0));
        Assert.IsTrue(IndicatorDead(playerIndicators, 1));
        Assert.IsFalse(IndicatorDead(playerIndicators, 2));
        Assert.IsTrue(IndicatorDead(enemyIndicators, 0));
        Assert.IsFalse(IndicatorDead(enemyIndicators, 1));
        Assert.IsNull(manager.GetType().GetMethod("OnPlayerKilledEnemyCard"));
        Assert.IsNull(manager.GetType().GetMethod("OnEnemyKilledPlayerCard"));
    }

    private Array CreateIndicators(string prefix)
    {
        Type indicatorType = RuntimeType("KillCounterUI");
        Array indicators = Array.CreateInstance(indicatorType, 3);
        for (int index = 0; index < 3; index++)
        {
            GameObject item = new GameObject(prefix + index);
            item.transform.SetParent(root.transform);
            item.AddComponent<Image>();
            indicators.SetValue(item.AddComponent(indicatorType), index);
        }
        return indicators;
    }

    private static bool IndicatorDead(Array indicators, int index)
    {
        object indicator = indicators.GetValue(index);
        return (bool)indicator.GetType().GetProperty("IsDead").GetValue(indicator);
    }

    private static object ScoreLimitState()
    {
        return CompletedState("score_limit", "p1", "p2", true);
    }

    private static object CompletedState(
        string reason,
        string winner,
        string loser,
        bool hasLastResult = false)
    {
        return JsonUtility.FromJson(
            "{\"schemaVersion\":2,\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\"," +
            "\"status\":\"completed\",\"phase\":\"completed\",\"playersCount\":2," +
            "\"battle\":{\"hasLastResult\":" + (hasLastResult ? "true" : "false") + "}," +
            "\"score\":{\"version\":1,\"target\":3,\"byPlayerId\":{\"p1\":3,\"p2\":2}}," +
            "\"matchResult\":{\"reason\":\"" + reason + "\",\"winnerPlayerId\":" +
            (winner == null ? "null" : "\"" + winner + "\"") + ",\"loserPlayerId\":" +
            (loser == null ? "null" : "\"" + loser + "\"") + "}}",
            RuntimeType("MatchStateDto")
        );
    }

    private static Type RuntimeType(string name)
    {
        Type type = Type.GetType(name + ", Assembly-CSharp");
        Assert.IsNotNull(type, "Type " + name + " was not found in Assembly-CSharp.");
        return type;
    }

    private static object Invoke(object target, string method, params object[] args)
    {
        MethodInfo info = target.GetType().GetMethod(method,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(info, "Method " + method + " was not found.");
        return info.Invoke(target, args);
    }

    private static void Set(object target, string field, object value)
    {
        FieldInfo info = target.GetType().GetField(field,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(info, "Field " + field + " was not found.");
        info.SetValue(target, value);
    }
}
