using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerLobbyUiRegressionTests
{
    private readonly System.Collections.Generic.List<GameObject> createdObjects =
        new System.Collections.Generic.List<GameObject>();
    private bool hadRoomCode;
    private bool hadWaitingFlag;
    private string previousRoomCode;
    private string previousWaitingFlag;

    [SetUp]
    public void SetUp()
    {
        hadRoomCode = PlayerPrefs.HasKey("RoomCode");
        hadWaitingFlag = PlayerPrefs.HasKey("IsWaitingForOpponent");
        previousRoomCode = PlayerPrefs.GetString("RoomCode");
        previousWaitingFlag = PlayerPrefs.GetString("IsWaitingForOpponent");
    }

    [TearDown]
    public void TearDown()
    {
        for (int i = createdObjects.Count - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(createdObjects[i]);
        }

        createdObjects.Clear();
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        if (hadRoomCode) PlayerPrefs.SetString("RoomCode", previousRoomCode);
        if (hadWaitingFlag) PlayerPrefs.SetString("IsWaitingForOpponent", previousWaitingFlag);
    }

    [Test]
    public void SetJoinButtonText_UpdatesTmpLabel()
    {
        Component lobby = CreateLobbyComponent();
        GameObject labelObject = new GameObject("TmpLabel");
        createdObjects.Add(labelObject);
        Type textMeshProType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
        Assert.IsNotNull(textMeshProType, "Type TMPro.TextMeshProUGUI was not found.");
        Component label = labelObject.AddComponent(textMeshProType);

        SetPrivateField(lobby, "joinButtonLabel", label);
        InvokeSetJoinButtonText(lobby, "CONNECT");

        Assert.AreEqual("CONNECT", ReadTextProperty(label));
    }

    [Test]
    public void SetJoinButtonText_UpdatesLegacyLabel_WhenTmpLabelIsMissing()
    {
        Component lobby = CreateLobbyComponent();
        GameObject labelObject = new GameObject("LegacyLabel");
        createdObjects.Add(labelObject);
        Text label = labelObject.AddComponent<Text>();

        SetPrivateField(lobby, "joinButtonLabel", null);
        SetPrivateField(lobby, "joinButtonLegacyLabel", label);
        InvokeSetJoinButtonText(lobby, "CANCEL");

        Assert.AreEqual("CANCEL", label.text);
    }

    [Test]
    public void CancellationCannotClearAnotherRoomsLocalState()
    {
        Component lobby = CreateLobbyComponent();
        SetPrivateField(lobby, "playerId", "p1");
        SetPrivateField(lobby, "ownedRoomCode", "OLD");
        SetPrivateField(lobby, "matchmakingRequestVersion", 2);
        PlayerPrefs.SetString("RoomCode", "NEW");
        InvokePrivate(lobby, "ApplyCancellationResponse", CancelledResponse("OLD"), "OLD", 2);
        Assert.AreEqual("NEW", PlayerPrefs.GetString("RoomCode"));
    }

    [Test]
    public void CancelResponseRequiresMatchingTerminalStateBeforeClearingKeys()
    {
        Component lobby = CreateLobbyComponent();
        SetPrivateField(lobby, "playerId", "p1");
        SetPrivateField(lobby, "ownedRoomCode", "WAIT");
        SetPrivateField(lobby, "matchmakingRequestVersion", 2);
        PlayerPrefs.SetString("RoomCode", "WAIT");
        PlayerPrefs.SetString("IsWaitingForOpponent", "true");
        InvokePrivate(lobby, "ApplyCancellationResponse", "{\"success\":true}", "WAIT", 2);
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
        InvokePrivate(lobby, "ApplyCancellationResponse", CancelledResponse("WAIT"), "WAIT", 1);
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
        InvokePrivate(lobby, "ApplyCancellationResponse", CancelledResponse("WAIT"), "WAIT", 2);
        Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
        Assert.IsFalse(PlayerPrefs.HasKey("IsWaitingForOpponent"));
    }

    [Test]
    public void StartupStateRequiresExactRoomMembershipAndActiveState()
    {
        Type serviceType = Type.GetType("MultiplayerService, Assembly-CSharp");
        Assert.IsNotNull(serviceType);
        MethodInfo validate = serviceType.GetMethod("ValidateStartupState", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(validate);
        Type stateType = Type.GetType("MatchStateDto, Assembly-CSharp");
        Assert.IsNotNull(stateType);
        object state = JsonUtility.FromJson(
            "{\"schemaVersion\":2,\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\",\"status\":\"active\",\"phase\":\"loading_decks\",\"playersCount\":2,\"seats\":[{\"playerId\":\"p1\"},{\"playerId\":\"p2\"}],\"startup\":{\"matchedAt\":\"2026-10-06T10:00:00Z\"}}", stateType);
        Assert.DoesNotThrow(() => validate.Invoke(null, new[] { state, "ROOM", "p1" }));
        Assert.Throws<TargetInvocationException>(() => validate.Invoke(null, new[] { state, "OTHER", "p1" }));
        Assert.Throws<TargetInvocationException>(() => validate.Invoke(null, new[] { state, "ROOM", "outsider" }));
        stateType.GetField("status").SetValue(state, "completed");
        Assert.Throws<TargetInvocationException>(() => validate.Invoke(null, new[] { state, "ROOM", "p1" }));
    }

    [Test]
    public void CancelRejectedWhileWaitingRetainsRetryableState()
    {
        Component lobby = CreateLobbyComponent();
        SetPrivateField(lobby, "playerId", "p1");
        SetPrivateField(lobby, "ownedRoomCode", "WAIT");
        SetPrivateField(lobby, "matchmakingRequestVersion", 2);
        PlayerPrefs.SetString("RoomCode", "WAIT");
        string response = "{\"success\":false,\"matchState\":{\"roomCode\":\"WAIT\",\"viewerPlayerId\":\"p1\",\"status\":\"waiting\",\"phase\":\"waiting_for_players\",\"playersCount\":1,\"seats\":[{\"playerId\":\"p1\"}]}}";
        InvokePrivate(lobby, "ApplyCancellationResponse", response, "WAIT", 2);
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
        Assert.AreEqual("Error", lobby.GetType().GetField("matchmakingState", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lobby).ToString());
    }

    [Test]
    public void CancelLosingToMatchResumesOnlyTheConfirmedActiveRoom()
    {
        Component lobby = CreateLobbyComponent();
        SetPrivateField(lobby, "playerId", "p1");
        SetPrivateField(lobby, "ownedRoomCode", "MATCH");
        SetPrivateField(lobby, "matchmakingRequestVersion", 2);
        PlayerPrefs.SetString("RoomCode", "MATCH");
        string response = "{\"success\":false,\"matchState\":{\"schemaVersion\":2,\"roomCode\":\"MATCH\",\"viewerPlayerId\":\"p1\",\"status\":\"active\",\"phase\":\"selecting_fighters\",\"playersCount\":2,\"seats\":[{\"playerId\":\"p1\"},{\"playerId\":\"p2\"}],\"startup\":{\"matchedAt\":\"2026-10-06T10:00:00Z\",\"startedAt\":\"2026-10-06T10:00:01Z\",\"bothReady\":true}}}";
        InvokePrivate(lobby, "ApplyCancellationResponse", response, "MATCH", 2);
        Assert.AreEqual("MATCH", PlayerPrefs.GetString("RoomCode"));
        Assert.AreEqual("false", PlayerPrefs.GetString("IsWaitingForOpponent"));
        Assert.AreEqual("Starting", lobby.GetType().GetField("matchmakingState", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(lobby).ToString());
    }

    [Test]
    public void TerminalKeyCleanupCannotClearAnotherMatch()
    {
        GameObject root = new GameObject("MultiplayerServiceTest");
        createdObjects.Add(root);
        Type type = Type.GetType("MultiplayerService, Assembly-CSharp");
        Assert.IsNotNull(type);
        Component service = root.AddComponent(type);
        SetPrivateField(service, "roomCode", "OLD");
        PlayerPrefs.SetString("RoomCode", "NEW");
        InvokePrivate(service, "ClearOwnedRoomKeys");
        Assert.AreEqual("NEW", PlayerPrefs.GetString("RoomCode"));
    }

    private static string CancelledResponse(string roomCode)
    {
        return "{\"success\":true,\"roomCode\":\"" + roomCode
            + "\",\"matchState\":{\"roomCode\":\"" + roomCode
            + "\",\"viewerPlayerId\":\"p1\",\"status\":\"completed\",\"phase\":\"completed\",\"playersCount\":1,\"seats\":[{\"playerId\":\"p1\"}]}}";
    }

    [Test]
    public void WaitingRenewalRequiresCurrentRoomLifecycleAndOriginalDeadline()
    {
        Component lobby = CreateWaitingLobby();
        Assert.IsTrue((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "WAIT", 7));
        foreach (string state in new[] { "Cancelling", "Error", "Starting", "Idle", "Reconciling" })
        {
            SetLobbyState(lobby, state);
            Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "WAIT", 7), state);
        }
        SetLobbyState(lobby, "Waiting");
        Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 1, "WAIT", 7));
        Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "WAIT", 6));
        Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "OTHER", 7));
        SetPrivateField(lobby, "applicationPaused", true);
        Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "WAIT", 7));
        SetPrivateField(lobby, "applicationPaused", false);
        SetPrivateField(lobby, "waitingDeadline", Time.realtimeSinceStartup - 1f);
        Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "WAIT", 7));
    }

    [Test]
    public void StoppedWaitingRejectsLateHeartbeatWithoutChangingNewState()
    {
        Component lobby = CreateWaitingLobby();
        InvokePrivate(lobby, "StopWaiting");
        Assert.IsFalse((bool)InvokePrivate(lobby, "ApplyWaitingHeartbeatResponse", null, 2, "WAIT", 7));
        Assert.AreEqual("Waiting", ReadPrivateField(lobby, "matchmakingState").ToString());
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
    }

    [Test]
    public void WaitingHeartbeatFailureStopsRenewalButRetainsExactRoomForCancellation()
    {
        Component lobby = CreateWaitingLobby();
        Assert.IsFalse((bool)InvokePrivate(lobby, "ApplyWaitingHeartbeatResponse", null, 2, "WAIT", 7));
        Assert.AreEqual("Error", ReadPrivateField(lobby, "matchmakingState").ToString());
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
        Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "WAIT", 7));
    }

    [Test]
    public void WaitingHeartbeatRequiresExactRoomAndTerminalAckLeavesCanonicalPollInCharge()
    {
        Component lobby = CreateWaitingLobby();
        Assert.IsTrue((bool)InvokePrivate(lobby, "ApplyWaitingHeartbeatResponse",
            "{\"success\":true,\"roomCode\":\"WAIT\"}", 2, "WAIT", 7));
        Assert.IsFalse((bool)InvokePrivate(lobby, "ApplyWaitingHeartbeatResponse",
            "{\"success\":true,\"roomCode\":\"WAIT\",\"roomCompleted\":true}", 2, "WAIT", 7));
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
        Assert.AreEqual("Waiting", ReadPrivateField(lobby, "matchmakingState").ToString());
        Assert.IsFalse((bool)InvokePrivate(lobby, "ApplyWaitingHeartbeatResponse",
            "{\"success\":true,\"roomCode\":\"OTHER\"}", 2, "WAIT", 7));
        Assert.AreEqual("Error", ReadPrivateField(lobby, "matchmakingState").ToString());
    }

    [Test]
    public void ApplicationPauseInvalidatesWaitingCallbacksWithoutResettingSearchDeadline()
    {
        Component lobby = CreateWaitingLobby();
        object deadline = ReadPrivateField(lobby, "waitingDeadline");
        InvokePrivate(lobby, "OnApplicationPause", true);
        Assert.AreEqual(deadline, ReadPrivateField(lobby, "waitingDeadline"));
        Assert.IsTrue((bool)ReadPrivateField(lobby, "waitingResumeRequired"));
        Assert.IsFalse((bool)InvokePrivate(lobby, "CanRenewWaitingPresence", 2, "WAIT", 7));
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
    }

    [Test]
    public void ResumeWaitingUsesExistingDeadlineAndExactCanonicalRoom()
    {
        Component lobby = CreateWaitingLobby();
        object deadline = ReadPrivateField(lobby, "waitingDeadline");
        SetLobbyState(lobby, "Reconciling");
        SetPrivateField(lobby, "waitingResumeRequired", true);
        string response = "{\"success\":true,\"matchState\":{\"roomCode\":\"WAIT\",\"viewerPlayerId\":\"p1\",\"status\":\"waiting\",\"phase\":\"waiting_for_players\",\"playersCount\":1,\"seats\":[{\"playerId\":\"p1\"}]}}";
        InvokePrivate(lobby, "ApplyWaitingResumeResponse", response, "WAIT", 2);
        Assert.AreEqual("Waiting", ReadPrivateField(lobby, "matchmakingState").ToString());
        Assert.AreEqual(deadline, ReadPrivateField(lobby, "waitingDeadline"));
        Assert.IsFalse((bool)ReadPrivateField(lobby, "waitingResumeRequired"));
    }

    [Test]
    public void ResumeCannotAcceptLateOrPausedResponseAndTerminalClearsOnlyOwnedRoom()
    {
        Component lobby = CreateWaitingLobby();
        SetLobbyState(lobby, "Reconciling");
        SetPrivateField(lobby, "applicationPaused", true);
        InvokePrivate(lobby, "ApplyWaitingResumeResponse", CancelledResponse("WAIT"), "WAIT", 2);
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
        SetPrivateField(lobby, "applicationPaused", false);
        InvokePrivate(lobby, "ApplyWaitingResumeResponse", CancelledResponse("WAIT"), "WAIT", 1);
        Assert.AreEqual("WAIT", PlayerPrefs.GetString("RoomCode"));
        InvokePrivate(lobby, "ApplyWaitingResumeResponse", CancelledResponse("WAIT"), "WAIT", 2);
        Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
        Assert.AreEqual("Idle", ReadPrivateField(lobby, "matchmakingState").ToString());
    }

    [Test]
    public void WaitingConfirmedWhilePausedDoesNotStartPollOrHeartbeat()
    {
        Component lobby = CreateWaitingLobby();
        SetPrivateField(lobby, "applicationPaused", true);
        InvokePrivate(lobby, "BeginWaiting", true);
        Assert.AreEqual("Waiting", ReadPrivateField(lobby, "matchmakingState").ToString());
        Assert.IsTrue((bool)ReadPrivateField(lobby, "waitingResumeRequired"));
        Assert.IsNull(ReadPrivateField(lobby, "waitForOpponentCoroutine"));
        Assert.IsNull(ReadPrivateField(lobby, "waitingHeartbeatCoroutine"));
        Assert.That((float)ReadPrivateField(lobby, "waitingDeadline"), Is.InRange(Time.realtimeSinceStartup + 59f, Time.realtimeSinceStartup + 60f));
    }

    [Test]
    public void PauseDuringCancellationDoesNotInvalidateItsPendingResponse()
    {
        Component lobby = CreateWaitingLobby();
        SetLobbyState(lobby, "Cancelling");
        SetPrivateField(lobby, "cancelRequested", true);
        InvokePrivate(lobby, "OnApplicationPause", true);
        Assert.AreEqual(2, ReadPrivateField(lobby, "matchmakingRequestVersion"));
        Assert.IsFalse((bool)ReadPrivateField(lobby, "waitingResumeRequired"));
        InvokePrivate(lobby, "ApplyCancellationResponse", CancelledResponse("WAIT"), "WAIT", 2);
        Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
    }

    private Component CreateWaitingLobby()
    {
        Component lobby = CreateLobbyComponent();
        SetPrivateField(lobby, "playerId", "p1");
        SetPrivateField(lobby, "ownedRoomCode", "WAIT");
        SetPrivateField(lobby, "matchmakingRequestVersion", 2);
        SetPrivateField(lobby, "waitingPresenceVersion", 7);
        SetPrivateField(lobby, "waitingDeadline", Time.realtimeSinceStartup + 60f);
        SetLobbyState(lobby, "Waiting");
        PlayerPrefs.SetString("RoomCode", "WAIT");
        return lobby;
    }

    private static object ReadPrivateField(object owner, string name)
    {
        FieldInfo field = owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, name);
        return field.GetValue(owner);
    }

    private static void SetLobbyState(object owner, string state)
    {
        FieldInfo field = owner.GetType().GetField("matchmakingState", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(owner, Enum.Parse(field.FieldType, state));
    }

    private static object InvokePrivate(object owner, string name, params object[] arguments)
    {
        MethodInfo method = owner.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(method, name + " was not found.");
        return method.Invoke(owner, arguments);
    }

    private Component CreateLobbyComponent()
    {
        GameObject root = new GameObject("LobbyUiTest");
        createdObjects.Add(root);

        Type lobbyType = System.Type.GetType("MultiplayerLobbyUI, Assembly-CSharp");
        Assert.IsNotNull(lobbyType, "Type MultiplayerLobbyUI was not found in Assembly-CSharp.");
        return root.AddComponent(lobbyType);
    }

    private static void InvokeSetJoinButtonText(object lobby, string text)
    {
        MethodInfo method = lobby
            .GetType()
            .GetMethod("SetJoinButtonText", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, "Method SetJoinButtonText was not found.");
        method.Invoke(lobby, new object[] { text });
    }

    private static void SetPrivateField(object instance, string fieldName, object value)
    {
        FieldInfo field = instance
            .GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(field, $"Field {fieldName} was not found.");
        field.SetValue(instance, value);
    }

    private static string ReadTextProperty(Component component)
    {
        PropertyInfo property = component
            .GetType()
            .GetProperty("text", BindingFlags.Instance | BindingFlags.Public);
        Assert.IsNotNull(property, $"Property text was not found on {component.GetType().Name}.");
        return property.GetValue(component) as string;
    }
}

public class LeaderboardSceneRegressionTests
{
    [Test]
    public void PlayFabManagerLeaderboard_IsSceneLocal()
    {
        string source = ReadSource("Leaderboard", "PlayFabManagerLeaderboard.cs");

        StringAssert.DoesNotContain(
            "DontDestroyOnLoad",
            source,
            "Leaderboard scene manager must stay scene-local; DontDestroyOnLoad only works on root objects."
        );
        StringAssert.DoesNotContain(
            "static PlayFabManagerLeaderboard Instance",
            source,
            "Leaderboard scene manager should not own a persistent singleton instance."
        );
    }

    [Test]
    public void RecordsLoaderOwnsLeaderboardRequestAfterSubscribing()
    {
        string source = ReadSource("Leaderboard", "RecordsLoader.cs");

        StringAssert.Contains(
            "OnLeaderboardLoaded +=",
            source,
            "RecordsLoader should subscribe before requesting leaderboard data."
        );
        StringAssert.Contains(
            "GetLeaderboard()",
            source,
            "RecordsLoader should request leaderboard data after it is ready to receive the event."
        );
        StringAssert.Contains(
            "OnLeaderboardLoaded -=",
            source,
            "RecordsLoader should unsubscribe when destroyed."
        );
    }

    private static string ReadSource(params string[] relativeParts)
    {
        string path = Path.Combine(Application.dataPath, "Scripts", Path.Combine(relativeParts));
        Assert.IsTrue(File.Exists(path), $"{Path.GetFileName(path)} source was not found.");
        return File.ReadAllText(path);
    }
}
