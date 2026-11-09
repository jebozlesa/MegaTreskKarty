using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class MultiplayerStartupRegressionTests
{
    private GameObject root;
    private string previousRoom;
    private string previousIntent;
    private bool hadRoom;
    private bool hadIntent;
    private bool hadWaiting;
    private string previousWaiting;
    private const string IntentKey = "PendingStartupCancellationRoomCode";
    private const string MissingRoomNoticeKey = "PendingMultiplayerMissingRoomNoticePlayerId";
    private bool hadMissingRoomNotice;
    private string previousMissingRoomNotice;

    [SetUp]
    public void SetUp()
    {
        hadRoom = PlayerPrefs.HasKey("RoomCode");
        hadIntent = PlayerPrefs.HasKey(IntentKey);
        previousRoom = PlayerPrefs.GetString("RoomCode");
        previousIntent = PlayerPrefs.GetString(IntentKey);
        hadWaiting = PlayerPrefs.HasKey("IsWaitingForOpponent");
        previousWaiting = PlayerPrefs.GetString("IsWaitingForOpponent");
        hadMissingRoomNotice = PlayerPrefs.HasKey(MissingRoomNoticeKey);
        previousMissingRoomNotice = PlayerPrefs.GetString(MissingRoomNoticeKey);
        PlayerPrefs.DeleteKey(IntentKey);
        PlayerPrefs.DeleteKey(MissingRoomNoticeKey);
        root = new GameObject("StartupTest");
        root.SetActive(false);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey(IntentKey);
        if (hadRoom) PlayerPrefs.SetString("RoomCode", previousRoom);
        if (hadIntent) PlayerPrefs.SetString(IntentKey, previousIntent);
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        if (hadWaiting) PlayerPrefs.SetString("IsWaitingForOpponent", previousWaiting);
        PlayerPrefs.DeleteKey(MissingRoomNoticeKey);
        if (hadMissingRoomNotice)
            PlayerPrefs.SetString(MissingRoomNoticeKey, previousMissingRoomNotice);
    }

    [Test]
    public void StartupCardsRequireFiveDistinctNonemptyIds()
    {
        Type type = RuntimeType("MultiplayerHandManager");
        MethodInfo validate = type.GetMethod("ValidateStartupCardIds", BindingFlags.Static | BindingFlags.NonPublic);
        Assert.IsNotNull(validate);
        Assert.DoesNotThrow(() => validate.Invoke(null, new object[] { new[] { "a", "b", "c", "d", "e" } }));
        foreach (string[] ids in new[] { new[] { "a" }, new[] { "a", "b", "c", "d", "d" }, new[] { "a", "b", "c", "d", "" } })
            Assert.Throws<TargetInvocationException>(() => validate.Invoke(null, new object[] { ids }));
    }

    [Test]
    public void PreparationCancellationInvalidatesExistingGeneration()
    {
        Component hand = root.AddComponent(RuntimeType("MultiplayerHandManager"));
        Set(hand, "preparationGeneration", 2);
        Invoke(hand, "CancelPreparation");
        Assert.Greater((int)Get(hand, "preparationGeneration"), 2);
    }

    [Test]
    public void LatePreparationCannotCompleteAfterCancellation()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Component hand = root.AddComponent(RuntimeType("MultiplayerHandManager"));
        hand.GetType().GetField("multiplayerService").SetValue(hand, service);
        root.SetActive(true);
        Set(hand, "preparationGeneration", 2);
        Assert.IsTrue((bool)Invoke(hand, "OwnsPreparation", 2));
        var completion = new System.Threading.Tasks.TaskCompletionSource<bool>();
        var iterator = (System.Collections.IEnumerator)Invoke(hand, "FinishPreparation", 2, completion);
        Assert.IsTrue(iterator.MoveNext());
        Invoke(hand, "CancelPreparation");
        Assert.IsFalse(iterator.MoveNext());
        Assert.IsTrue(completion.Task.IsCanceled);
        Assert.IsFalse((bool)Invoke(hand, "OwnsPreparation", 2));
    }

    [Test]
    public void LobbyAcknowledgementClearsOnlyItsOwnStartupIntent()
    {
        Component lobby = root.AddComponent(RuntimeType("MultiplayerLobbyUI"));
        Set(lobby, "ownedRoomCode", "OLD");
        PlayerPrefs.SetString("RoomCode", "OLD");
        PlayerPrefs.SetString(IntentKey, "NEW");
        Invoke(lobby, "ClearOwnedRoom", "OLD");
        Assert.AreEqual("NEW", PlayerPrefs.GetString(IntentKey));
        Set(lobby, "ownedRoomCode", "OLD");
        PlayerPrefs.SetString("RoomCode", "OLD");
        PlayerPrefs.SetString(IntentKey, "OLD");
        Invoke(lobby, "ClearOwnedRoom", "OLD");
        Assert.IsFalse(PlayerPrefs.HasKey(IntentKey));
    }

    [Test]
    public void PendingStartupAbortCannotResumeEvenAnAlreadyStartedRoom()
    {
        Component lobby = root.AddComponent(RuntimeType("MultiplayerLobbyUI"));
        Set(lobby, "playerId", "p1"); Set(lobby, "ownedRoomCode", "ROOM");
        Set(lobby, "matchmakingRequestVersion", 2);
        PlayerPrefs.SetString("RoomCode", "ROOM"); PlayerPrefs.SetString(IntentKey, "ROOM");
        root.SetActive(true);
        string response = "{\"success\":false,\"matchState\":{\"schemaVersion\":2,\"roomCode\":\"ROOM\","
            + "\"viewerPlayerId\":\"p1\",\"status\":\"active\",\"phase\":\"selecting_fighters\",\"playersCount\":2,"
            + "\"seats\":[{\"playerId\":\"p1\"},{\"playerId\":\"p2\"}],\"startup\":{\"matchedAt\":\"2026-10-06T10:00:00Z\","
            + "\"startedAt\":\"2026-10-06T10:00:01Z\",\"bothReady\":true}}}";
        Invoke(lobby, "ApplyCancellationResponse", response, "ROOM", 2);
        Assert.AreEqual("Error", Get(lobby, "matchmakingState").ToString());
        Assert.AreEqual("ROOM", PlayerPrefs.GetString(IntentKey));
        Assert.AreEqual("ROOM", PlayerPrefs.GetString("RoomCode"));
    }

    [Test]
    public void LobbyRejectsActiveRoomsWithoutStartupContract()
    {
        Component lobby = root.AddComponent(RuntimeType("MultiplayerLobbyUI"));
        Set(lobby, "playerId", "p1");
        const string response = "{\"matchState\":{\"schemaVersion\":2,\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\","
            + "\"status\":\"active\",\"phase\":\"loading_decks\",\"playersCount\":2,\"seats\":[{\"playerId\":\"p1\"},{\"playerId\":\"p2\"}]}}";
        Assert.IsNull(Invoke(lobby, "ParseOwnedState", response, "ROOM"));
    }

    [Test]
    public void ServerStartAloneCannotEnableBattleBeforeLocalPreparation()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        root.SetActive(true);
        Set(service, "serverStarted", true);
        Assert.IsFalse((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
        Set(service, "localPrepared", true);
        Assert.IsTrue((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
        Set(service, "startupAborted", true);
        Assert.IsFalse((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
    }

    [Test]
    public void PollingCannotExtendOriginalStartupBudget()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Set(service, "roomCode", "ROOM"); Set(service, "myPlayerId", "p1");
        double original = Time.realtimeSinceStartupAsDouble + 10;
        Set(service, "startupDeadline", original);
        object state = State(25000);
        Invoke(service, "ApplyStartupSnapshot", state, Time.realtimeSinceStartupAsDouble);
        Assert.LessOrEqual((double)Get(service, "startupDeadline"), original);
        state = State(3000);
        Invoke(service, "ApplyStartupSnapshot", state, Time.realtimeSinceStartupAsDouble);
        Assert.LessOrEqual((double)Get(service, "startupDeadline"), Time.realtimeSinceStartupAsDouble + 3.1);
    }

    [Test]
    public void PreparationMayNotAcknowledgeWhilePaused()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        root.SetActive(true);
        Set(service, "localPrepared", true);
        Set(service, "startupDeadline", Time.realtimeSinceStartupAsDouble + 10);
        Assert.IsTrue((bool)service.GetType().GetProperty("CanAcknowledgeStartup").GetValue(service));
        Set(service, "applicationPaused", true);
        Assert.IsFalse((bool)service.GetType().GetProperty("CanAcknowledgeStartup").GetValue(service));
    }

    [Test]
    public void PreparationMayNotAcknowledgeWithoutApplicationFocus()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        root.SetActive(true);
        Set(service, "localPrepared", true);
        Set(service, "startupDeadline", Time.realtimeSinceStartupAsDouble + 10);
        Invoke(service, "OnApplicationFocus", false);
        Assert.IsFalse((bool)service.GetType().GetProperty("CanAcknowledgeStartup").GetValue(service));
        Invoke(service, "OnApplicationFocus", true);
        Assert.IsTrue((bool)service.GetType().GetProperty("CanAcknowledgeStartup").GetValue(service));
    }

    [Test]
    public void PreparationRequiresAllFiveInitializedOwnedVisuals()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Component hand = root.AddComponent(RuntimeType("MultiplayerHandManager"));
        Component fight = root.AddComponent(RuntimeType("FightSystemMultiplayer"));
        Component player = root.AddComponent(RuntimeType("Player"));
        hand.GetType().GetField("multiplayerService").SetValue(hand, service);
        hand.GetType().GetField("fightSystem").SetValue(hand, fight);
        fight.GetType().GetField("player").SetValue(fight, player);
        var cards = (System.Collections.IList)Get(hand, "preparedCards");
        var playerHand = (System.Collections.IList)player.GetType().GetField("hand").GetValue(player);
        root.SetActive(true);
        Set(hand, "preparationGeneration", 1);
        Texture2D texture = new Texture2D(2, 2);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);
        try
        {
            for (int i = 0; i < 5; i++)
            {
                var obj = new GameObject("Card" + i, typeof(RectTransform), typeof(UnityEngine.UI.Image));
                obj.transform.SetParent(root.transform);
                Component card = obj.AddComponent(RuntimeType("Kard"));
                var image = obj.GetComponent<UnityEngine.UI.Image>();
                image.sprite = sprite;
                card.GetType().GetField("cardImage").SetValue(card, image);
                cards.Add(card); playerHand.Add(card);
            }
            var missingImage = ((Component)cards[4]).GetComponent<UnityEngine.UI.Image>();
            missingImage.sprite = null;
            var incomplete = new System.Threading.Tasks.TaskCompletionSource<bool>();
            var iterator = (System.Collections.IEnumerator)Invoke(hand, "FinishPreparation", 1, incomplete);
            iterator.MoveNext(); iterator.MoveNext();
            Assert.IsTrue(incomplete.Task.IsFaulted);
            Assert.IsNotNull(incomplete.Task.Exception);
            missingImage.sprite = sprite;
            var complete = new System.Threading.Tasks.TaskCompletionSource<bool>();
            iterator = (System.Collections.IEnumerator)Invoke(hand, "FinishPreparation", 1, complete);
            iterator.MoveNext(); iterator.MoveNext();
            Assert.IsTrue(complete.Task.Result);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(sprite);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void TerminalCleanupOnlyClearsMatchingCancellationIntent()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Set(service, "roomCode", "OLD");
        PlayerPrefs.SetString("RoomCode", "OLD"); PlayerPrefs.SetString(IntentKey, "NEW");
        Invoke(service, "ClearOwnedRoomKeys");
        Assert.AreEqual("NEW", PlayerPrefs.GetString(IntentKey));
        PlayerPrefs.SetString("RoomCode", "OLD"); PlayerPrefs.SetString(IntentKey, "OLD");
        Invoke(service, "ClearOwnedRoomKeys");
        Assert.IsFalse(PlayerPrefs.HasKey(IntentKey));
    }

    [Test]
    public void MissingOwnedRoomBlocksBattleClearsOwnershipAndQueuesLobbyAcknowledgement()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Component fightSystem = root.AddComponent(RuntimeType("FightSystemMultiplayer"));
        ((Behaviour)fightSystem).enabled = false;
        Component board = root.AddComponent(RuntimeType("MultiplayerBoardManager"));
        Component submitter = root.AddComponent(RuntimeType("BattleSubmitter"));

        Set(service, "roomCode", "OLD");
        Set(service, "myPlayerId", "p1");
        Set(service, "localPrepared", true);
        Set(service, "serverStarted", true);
        Set(service, "fightSystem", fightSystem);
        Set(fightSystem, "multiplayerBoardManager", board);
        Set(fightSystem, "battleSubmitter", submitter);
        Set(board, "isSubmittingSelection", true);
        var selectionCancellation = new System.Threading.CancellationTokenSource();
        Set(board, "opponentSelectionCancellation", selectionCancellation);
        Set(submitter, "isWaitingForBattle", true);
        PlayerPrefs.SetString("RoomCode", "OLD");
        PlayerPrefs.SetString("IsWaitingForOpponent", "false");
        root.SetActive(true);

        Assert.IsTrue((bool)Invoke(service, "PrepareMissingOwnedRoomRecovery", "OLD"));

        Assert.IsFalse((bool)service.GetType().GetProperty("CanContinueStartup").GetValue(service));
        Assert.IsFalse((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
        Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
        Assert.IsFalse(PlayerPrefs.HasKey("IsWaitingForOpponent"));
        Assert.IsFalse((bool)Get(board, "isSubmittingSelection"));
        Assert.IsTrue(selectionCancellation.IsCancellationRequested);
        Assert.IsFalse((bool)Get(submitter, "isWaitingForBattle"));
        Assert.AreEqual("p1", PlayerPrefs.GetString(MissingRoomNoticeKey));
        selectionCancellation.Dispose();
    }

    [Test]
    public void MissingRoomCallbackStopsOldLifecycleWithoutTouchingANewerRoom()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));

        Set(service, "roomCode", "OLD");
        Set(service, "myPlayerId", "p1");
        PlayerPrefs.SetString("RoomCode", "NEW");
        root.SetActive(true);

        Assert.IsFalse((bool)Invoke(service, "PrepareMissingOwnedRoomRecovery", "OLD"));

        Assert.AreEqual("NEW", PlayerPrefs.GetString("RoomCode"));
        Assert.IsFalse((bool)service.GetType().GetProperty("CanContinueStartup").GetValue(service));
        Assert.IsFalse(PlayerPrefs.HasKey(MissingRoomNoticeKey));
    }

    [Test]
    public void MissingOwnedRoomWithoutStoredRoomStillQueuesLobbyAcknowledgement()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));

        Set(service, "roomCode", "OLD");
        Set(service, "myPlayerId", "p1");
        Set(service, "localPrepared", true);
        Set(service, "serverStarted", true);
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.SetString("IsWaitingForOpponent", "false");
        root.SetActive(true);

        Assert.IsTrue((bool)Invoke(service, "PrepareMissingOwnedRoomRecovery", "OLD"));

        Assert.IsFalse((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
        Assert.IsFalse(PlayerPrefs.HasKey("IsWaitingForOpponent"));
        Assert.AreEqual("p1", PlayerPrefs.GetString(MissingRoomNoticeKey));
    }

    [Test]
    public void MissingRoomNoticeIsSingleUseAndAccountScoped()
    {
        Type noticeState = RuntimeType("MultiplayerMissingRoomNoticeState");
        MethodInfo queue = noticeState.GetMethod("Queue", BindingFlags.Static | BindingFlags.Public);
        MethodInfo consume = noticeState.GetMethod("Consume", BindingFlags.Static | BindingFlags.Public);
        Assert.IsNotNull(queue);
        Assert.IsNotNull(consume);

        queue.Invoke(null, new object[] { "p1" });
        Assert.IsFalse((bool)consume.Invoke(null, new object[] { "p2" }));
        Assert.IsFalse(PlayerPrefs.HasKey(MissingRoomNoticeKey));

        queue.Invoke(null, new object[] { "p1" });
        Assert.IsTrue((bool)consume.Invoke(null, new object[] { "p1" }));
        Assert.IsFalse((bool)consume.Invoke(null, new object[] { "p1" }));
    }

    [TestCase("p2", "p1", true)]
    [TestCase("p1", "p2", false)]
    [TestCase(null, null, true)]
    public void StartedTimeoutStopsPendingWorkAndRoutesOnlyNonWinnersToLobby(string winner, string loser, bool lobby)
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Component fight = root.AddComponent(RuntimeType("FightSystemMultiplayer"));
        ((Behaviour)fight).enabled = false;
        Component board = root.AddComponent(RuntimeType("MultiplayerBoardManager"));
        Component submitter = root.AddComponent(RuntimeType("BattleSubmitter"));
        Set(service, "roomCode", "ROOM"); Set(service, "myPlayerId", "p1");
        Set(service, "localPrepared", true); Set(service, "serverStarted", true);
        Set(service, "fightSystem", fight); Set(fight, "multiplayerBoardManager", board);
        Set(fight, "battleSubmitter", submitter); Set(fight, "multiplayerService", service);
        Set(submitter, "fightSystem", fight);
        Set(submitter, "isWaitingForBattle", true); Set(board, "isSubmittingSelection", true);
        var cancellation = new System.Threading.CancellationTokenSource();
        Set(board, "opponentSelectionCancellation", cancellation);
        PlayerPrefs.SetString("RoomCode", "ROOM");
        root.SetActive(true);
        try
        {
            object completed = CompletedState(winner, loser);
            Assert.AreEqual(lobby, (bool)Invoke(service, "PrepareRemoteMatchCompletion", completed));
            Assert.IsFalse((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
            Assert.IsFalse((bool)Get(submitter, "isWaitingForBattle"));
            Assert.IsFalse((bool)Get(board, "isSubmittingSelection"));
            Assert.IsTrue(cancellation.IsCancellationRequested);
            Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
            Assert.AreEqual(lobby, PlayerPrefs.HasKey(MissingRoomNoticeKey));
            if (!lobby) Assert.AreEqual("WON", fight.GetType().GetField("state").GetValue(fight).ToString());
            int version = (int)Get(service, "lifecycleVersion");
            Invoke(submitter, "OnBattleResponse", new object[] { null });
            Assert.IsFalse((bool)Invoke(service, "PrepareRemoteMatchCompletion", completed));
            Assert.AreEqual(version, Get(service, "lifecycleVersion"));
        }
        finally { cancellation.Dispose(); }
    }

    [Test]
    public void LateCompletionStopsOldLifecycleWithoutChangingNewRoomOrQueueingNotice()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Set(service, "roomCode", "ROOM"); Set(service, "myPlayerId", "p1");
        PlayerPrefs.SetString("RoomCode", "NEW");
        root.SetActive(true);
        Assert.IsFalse((bool)Invoke(service, "PrepareRemoteMatchCompletion", CompletedState("p2", "p1")));
        Assert.IsFalse((bool)service.GetType().GetProperty("CanContinueStartup").GetValue(service));
        Assert.AreEqual("NEW", PlayerPrefs.GetString("RoomCode"));
        Assert.IsFalse(PlayerPrefs.HasKey(MissingRoomNoticeKey));
    }

    [Test]
    public void NormalCompletionDoesNotCancelTheBattleResultPlaybackOwner()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Component fight = root.AddComponent(RuntimeType("FightSystemMultiplayer"));
        ((Behaviour)fight).enabled = false;
        Component submitter = root.AddComponent(RuntimeType("BattleSubmitter"));
        Set(service, "roomCode", "ROOM"); Set(service, "myPlayerId", "p1");
        Set(service, "fightSystem", fight); Set(fight, "battleSubmitter", submitter);
        Set(submitter, "isWaitingForBattle", true);
        PlayerPrefs.SetString("RoomCode", "ROOM");
        object completed = CompletedState("p1", "p2");
        object result = completed.GetType().GetField("matchResult").GetValue(completed);
        result.GetType().GetField("reason").SetValue(result, "no_cards_left");
        root.SetActive(true);
        Assert.IsFalse((bool)Invoke(service, "PrepareRemoteMatchCompletion", completed));
        Assert.IsTrue((bool)Get(submitter, "isWaitingForBattle"));
        Assert.IsFalse(PlayerPrefs.HasKey(MissingRoomNoticeKey));
    }

    [Test]
    public void ActiveSnapshotCannotBeMisreadAsTerminalCompletion()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Set(service, "roomCode", "ROOM"); Set(service, "myPlayerId", "p1");
        PlayerPrefs.SetString("RoomCode", "ROOM");
        root.SetActive(true);
        Assert.IsFalse((bool)Invoke(service, "PrepareRemoteMatchCompletion", State(10000)));
        Assert.IsTrue((bool)service.GetType().GetProperty("CanContinueStartup").GetValue(service));
        Assert.AreEqual("ROOM", PlayerPrefs.GetString("RoomCode"));
        Assert.IsFalse(PlayerPrefs.HasKey(MissingRoomNoticeKey));
    }

    [TestCase("OTHER", "p1")]
    [TestCase("ROOM", "other-player")]
    public void CompletionFromAnotherRoomOrPlayerCannotEndCurrentLifecycle(string room, string player)
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Set(service, "roomCode", "ROOM"); Set(service, "myPlayerId", "p1");
        PlayerPrefs.SetString("RoomCode", "ROOM");
        root.SetActive(true);
        object completed = CompletedState("p2", "p1");
        completed.GetType().GetField("roomCode").SetValue(completed, room);
        completed.GetType().GetField("viewerPlayerId").SetValue(completed, player);
        Assert.IsFalse((bool)Invoke(service, "PrepareRemoteMatchCompletion", completed));
        Assert.IsTrue((bool)service.GetType().GetProperty("CanContinueStartup").GetValue(service));
        Assert.AreEqual("ROOM", PlayerPrefs.GetString("RoomCode"));
    }

    [TestCase("p2", "p1", true)]
    [TestCase("p1", "p2", false)]
    [TestCase(null, null, true)]
    public void LobbyRetainedTimeoutShowsExistingNoticeOnlyForNonWinners(string winner, string loser, bool notice)
    {
        Component lobby = root.AddComponent(RuntimeType("MultiplayerLobbyUI"));
        var dialogObject = new GameObject("MatchUnavailableDialog");
        dialogObject.transform.SetParent(root.transform);
        Component dialog = dialogObject.AddComponent(RuntimeType("ConfirmDialogController"));
        dialogObject.SetActive(false);
        Component bubble = AttachAuthoredDialogText(dialog, "Authored match ending.");
        Set(lobby, "playerId", "p1"); Set(lobby, "ownedRoomCode", "ROOM");
        Set(lobby, "matchUnavailableDialog", dialog);
        PlayerPrefs.SetString("RoomCode", "ROOM");
        Assert.IsTrue((bool)Invoke(lobby, "TryHandleCompletedRoom", CompletedState(winner, loser), "ROOM"));
        Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
        Assert.AreEqual(notice, dialogObject.activeSelf);
        Assert.AreEqual("Authored match ending.", bubble.GetType().GetProperty("text").GetValue(bubble));
    }

    [Test]
    public void StartedHeartbeatDefaultAndSavedSceneUseTenSeconds()
    {
        Component service = root.AddComponent(RuntimeType("MultiplayerService"));
        Assert.AreEqual(10f, Get(service, "heartbeatInterval"));
        string scene = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "Scenes", "Multiplayer.unity"));
        StringAssert.Contains("heartbeatInterval: 10", scene);
    }

    private static object CompletedState(string winner, string loser)
    {
        return JsonUtility.FromJson("{\"schemaVersion\":2,\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\","
            + "\"status\":\"completed\",\"phase\":\"completed\",\"playersCount\":2,"
            + "\"seats\":[{\"playerId\":\"p1\"},{\"playerId\":\"p2\"}],"
            + "\"startup\":{\"startedAt\":\"2026-11-08T12:00:00Z\",\"bothReady\":true},"
            + "\"matchResult\":{\"reason\":\"timeout\",\"winnerPlayerId\":" + (winner == null ? "null" : "\"" + winner + "\"")
            + ",\"loserPlayerId\":" + (loser == null ? "null" : "\"" + loser + "\"") + "}}", RuntimeType("MatchStateDto"));
    }

    [Test]
    public void LobbyConsumesQueuedNoticeAndShowsAcknowledgementOnce()
    {
        Type noticeState = RuntimeType("MultiplayerMissingRoomNoticeState");
        noticeState.GetMethod("Queue", BindingFlags.Static | BindingFlags.Public)
            .Invoke(null, new object[] { "p1" });

        Component lobby = root.AddComponent(RuntimeType("MultiplayerLobbyUI"));
        GameObject dialogObject = new GameObject("MatchUnavailableDialog");
        dialogObject.transform.SetParent(root.transform);
        Component dialog = dialogObject.AddComponent(RuntimeType("ConfirmDialogController"));
        dialogObject.SetActive(false);
        Component bubble = AttachAuthoredDialogText(dialog, "Authored match ending.");
        Set(lobby, "playerId", "p1");
        Set(lobby, "matchUnavailableDialog", dialog);

        Assert.IsTrue((bool)Invoke(lobby, "TryShowPendingMissingRoomNotice"));
        Assert.IsTrue(dialogObject.activeSelf);
        Assert.IsFalse(PlayerPrefs.HasKey(MissingRoomNoticeKey));
        Assert.IsFalse((bool)Invoke(lobby, "TryShowPendingMissingRoomNotice"));
        Assert.AreEqual("Authored match ending.", bubble.GetType().GetProperty("text").GetValue(bubble));
    }

    [Test]
    public void LobbyExactMissingRoomClearsOwnershipAndShowsAcknowledgement()
    {
        Component lobby = root.AddComponent(RuntimeType("MultiplayerLobbyUI"));
        GameObject dialogObject = new GameObject("MatchUnavailableDialog");
        dialogObject.transform.SetParent(root.transform);
        Component dialog = dialogObject.AddComponent(RuntimeType("ConfirmDialogController"));
        dialogObject.SetActive(false);
        Component bubble = AttachAuthoredDialogText(dialog, "Authored match ending.");
        Set(lobby, "playerId", "p1");
        Set(lobby, "ownedRoomCode", "OLD");
        Set(lobby, "matchUnavailableDialog", dialog);
        PlayerPrefs.SetString("RoomCode", "OLD");

        const string response = "{\"success\":false,\"error\":\"room_not_found\",\"roomCode\":\"OLD\"}";
        Assert.IsTrue((bool)Invoke(lobby, "TryHandleMissingRoom", response, "OLD"));
        Assert.IsFalse(PlayerPrefs.HasKey("RoomCode"));
        Assert.IsTrue(dialogObject.activeSelf);
        Assert.AreEqual("Authored match ending.", bubble.GetType().GetProperty("text").GetValue(bubble));
        Invoke(dialog, "Confirm");
        Assert.IsFalse(dialogObject.activeSelf);
    }

    [Test]
    public void LobbyRecoveryRetiresRuntimeCopyAndItsSavedSceneOverride()
    {
        Type lobby = RuntimeType("MultiplayerLobbyUI");
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        Assert.IsNull(lobby.GetField("matchUnavailableMessage", fields));
        Assert.IsNull(lobby.GetField("DefaultMatchUnavailableMessage", fields));
        string scene = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "Scenes", "MultiplayerLobby.unity"));
        StringAssert.DoesNotContain("matchUnavailableMessage:", scene);
    }

    private Component AttachAuthoredDialogText(Component dialog, string copy)
    {
        var bubbleObject = new GameObject("AuthoredBubble", typeof(RectTransform));
        bubbleObject.transform.SetParent(dialog.transform);
        Component bubble = bubbleObject.AddComponent(Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true));
        bubble.GetType().GetProperty("text").SetValue(bubble, copy);
        Set(dialog, "messageText", bubble);
        return bubble;
    }

    [Test]
    public void OldOrMissingStartupContractIsRejected()
    {
        MethodInfo validate = RuntimeType("MultiplayerService").GetMethod("ValidateStartupState", BindingFlags.Static | BindingFlags.NonPublic);
        object state = State(10000);
        Assert.DoesNotThrow(() => validate.Invoke(null, new[] { state, "ROOM", "p1" }));
        state.GetType().GetField("startup").SetValue(state, null);
        Assert.Throws<TargetInvocationException>(() => validate.Invoke(null, new[] { state, "ROOM", "p1" }));
    }

    private static object State(int remaining)
    {
        return JsonUtility.FromJson("{\"schemaVersion\":2,\"roomCode\":\"ROOM\",\"viewerPlayerId\":\"p1\",\"status\":\"active\",\"phase\":\"loading_decks\",\"playersCount\":2,"
            + "\"seats\":[{\"playerId\":\"p1\"},{\"playerId\":\"p2\"}],\"startup\":{\"matchedAt\":\"2026-10-06T10:00:00.000Z\",\"remainingMs\":" + remaining + "}}", RuntimeType("MatchStateDto"));
    }
    private static Type RuntimeType(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static object Get(Component component, string name) => component.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(component);
    private static void Set(Component component, string name, object value)
    {
        FieldInfo field = component.GetType().GetField(
            name,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
        );
        Assert.IsNotNull(field, name); field.SetValue(component, value);
    }
    private static object Invoke(Component component, string name, params object[] args)
    {
        MethodInfo method = component.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, name); return method.Invoke(component, args);
    }
}
