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

    [SetUp]
    public void SetUp()
    {
        hadRoom = PlayerPrefs.HasKey("RoomCode");
        hadIntent = PlayerPrefs.HasKey(IntentKey);
        previousRoom = PlayerPrefs.GetString("RoomCode");
        previousIntent = PlayerPrefs.GetString(IntentKey);
        hadWaiting = PlayerPrefs.HasKey("IsWaitingForOpponent");
        previousWaiting = PlayerPrefs.GetString("IsWaitingForOpponent");
        PlayerPrefs.DeleteKey(IntentKey);
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
        FieldInfo field = component.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(field, name); field.SetValue(component, value);
    }
    private static object Invoke(Component component, string name, params object[] args)
    {
        MethodInfo method = component.GetType().GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(method, name); return method.Invoke(component, args);
    }
}
