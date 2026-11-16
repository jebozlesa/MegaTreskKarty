using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class LeaderboardRuntimeTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (GameObject item in createdObjects)
        {
            if (item != null) UnityEngine.Object.DestroyImmediate(item);
        }
        createdObjects.Clear();
    }

    [Test]
    public void DtoParsesResolvedModePublicWindowAndAvailableModes()
    {
        Type stateType = Runtime("LeaderboardStateResponse");
        object state = JsonUtility.FromJson(
            "{\"success\":true,\"mode\":\"mp\",\"availableModes\":[\"mp\",\"rr\"],\"currentPlayerId\":\"me\",\"entries\":[{\"rank\":92,\"playerId\":\"me\",\"displayName\":\"ME\",\"score\":1016}]}",
            stateType
        );

        Assert.AreEqual("mp", Field(state, "mode"));
        CollectionAssert.AreEqual(new[] { "mp", "rr" }, (IList)Field(state, "availableModes"));
        Assert.AreEqual("me", Field(state, "currentPlayerId"));
        IList entries = (IList)Field(state, "entries");
        Assert.AreEqual(92, Field(entries[0], "rank"));
        Assert.IsNull(stateType.GetField("currentPlayer"),
            "The retired private/current-player status contract must not survive in the DTO.");
        Assert.IsNull(RuntimeOrNull("LeaderboardCurrentPlayerDto"));
    }

    [Test]
    public async Task ServiceWithoutServerManagerReturnsNull()
    {
        GameObject root = Create("Service");
        Component service = root.AddComponent(Runtime("LeaderboardService"));
        LogAssert.Expect(
            LogType.Error,
            "[LeaderboardService] GetLeaderboardStateAsync(mp): serverFunctionsManager is not assigned"
        );
        object taskObject = service.GetType()
            .GetMethod("GetLeaderboardStateAsync")
            .Invoke(service, new object[] { "player", "mp" });
        await (Task)taskObject;
        Assert.IsNull(taskObject.GetType().GetProperty("Result").GetValue(taskObject));
    }

    [Test]
    public void AcceptedStateRendersOnlyPublicRowsHighlightsCurrentAndCachesMode()
    {
        Component controller = CreateController(out Transform rows);
        object state = CreateState("mp", new[] { "mp", "rr" }, "me", new[]
        {
            Entry(91, "above", "ABOVE", 1040),
            Entry(92, "me", "ME", 1016),
            Entry(93, "below", "BELOW", 1001),
        });

        ApplyLoadedState(controller, state);

        Assert.AreEqual("mp", controller.GetType().GetProperty("SelectedMode").GetValue(controller));
        Assert.AreEqual(3, rows.childCount);
        Assert.AreEqual("92.", RowTexts(rows.GetChild(1).gameObject)[0]);
        Assert.AreEqual(Color.yellow, RowImage(rows.GetChild(1).gameObject).color);
        Assert.AreEqual(Color.white, RowImage(rows.GetChild(0).gameObject).color);
        IDictionary cache = (IDictionary)PrivateField(controller, "modeCache");
        Assert.AreEqual(1, cache.Count);
        Assert.IsTrue(cache.Contains("mp"));
    }

    [Test]
    public void AvailableModesCycleAndSingleRrModeCannotExposeMp()
    {
        Component controller = CreateController(out _);
        ApplyLoadedState(controller, CreateState(
            "mp", new[] { "mp", "rr" }, "me", new object[0]));

        Assert.AreEqual("rr", ResolveRelativeMode(controller, 1));
        Assert.AreEqual("rr", ResolveRelativeMode(controller, -1));

        ApplyLoadedState(controller, CreateState(
            "rr", new[] { "rr" }, "me", new object[0]));
        Assert.AreEqual("rr", ResolveRelativeMode(controller, 1));
        Assert.AreEqual("rr", ResolveRelativeMode(controller, -1));
        CollectionAssert.AreEqual(
            new[] { "rr" },
            (IList)PrivateField(controller, "availableModes")
        );
    }

    [Test]
    public void UnknownResolvedModeIsRejectedInsteadOfSilentlyBecomingMp()
    {
        object state = CreateState(
            "campaign", new[] { "campaign" }, "me", new object[0]);

        Assert.IsFalse((bool)Runtime("LeaderboardController")
            .GetMethod("IsValidState", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new[] { state }));
    }

    [Test]
    public void InitialModeRequestIsValidatedConsumedOnceAndDefaultsToMp()
    {
        Type controller = Runtime("LeaderboardController");
        MethodInfo queue = controller.GetMethod(
            "QueueInitialMode",
            BindingFlags.Static | BindingFlags.Public
        );
        MethodInfo consume = controller.GetMethod(
            "ConsumeInitialMode",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        Assert.IsNotNull(queue, "LeaderboardController must own the one-shot mode handoff.");
        Assert.IsNotNull(consume, "LeaderboardController must consume the handoff before loading.");
        Assert.AreEqual("mp", consume.Invoke(null, null));
        Assert.IsTrue((bool)queue.Invoke(null, new object[] { "rr" }));
        Assert.AreEqual("rr", consume.Invoke(null, null));
        Assert.AreEqual("mp", consume.Invoke(null, null), "The RR request must not leak into later visits.");

        LogAssert.Expect(
            LogType.Error,
            "[LeaderboardController] Unsupported initial leaderboard mode: campaign"
        );
        Assert.IsFalse((bool)queue.Invoke(null, new object[] { "campaign" }));
        Assert.AreEqual("mp", consume.Invoke(null, null));
    }

    [Test]
    public void RoyalRumbleRunEndRoutesAfterFeedbackWithRrPreference()
    {
        string source = File.ReadAllText(Path.Combine(
            Application.dataPath,
            "Scripts",
            "RoyalRumble",
            "RoyalRumbleShellController.cs"
        ));

        int feedback = source.IndexOf(
            "PlayCardProgressionFeedback(envelope.cardProgression",
            StringComparison.Ordinal
        );
        int runEndGate = source.IndexOf("if (envelope.runEnded)", feedback, StringComparison.Ordinal);
        int route = source.IndexOf("ReturnToLeaderboardAfterRunEnd()", runEndGate, StringComparison.Ordinal);

        Assert.GreaterOrEqual(feedback, 0, "RR progression feedback hook is missing.");
        Assert.Greater(runEndGate, feedback, "RR must finish result/progression feedback before routing.");
        Assert.Greater(route, runEndGate, "A completed RR run must invoke its reviewed route.");
        StringAssert.Contains("private const float RunEndLeaderboardDelaySeconds = 2f;", source);
        StringAssert.Contains("yield return new WaitForSeconds(RunEndLeaderboardDelaySeconds);", source);
        StringAssert.Contains("LeaderboardController.QueueInitialMode(\"rr\")", source);
        StringAssert.Contains("SceneManager.LoadScene(LeaderboardScene);", source);
    }

    [Test]
    public void SwipeDetectorRequiresEightyPixelDominantGesture()
    {
        GameObject eventSystemObject = Create("EventSystem");
        EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
        Component detector = Create("Swipe").AddComponent(Runtime("SwipeDetector"));
        SetField(detector, "minimumSwipeDistance", 80f);

        int leftCount = 0;
        UnityEvent left = new UnityEvent();
        left.AddListener(() => leftCount++);
        SetField(detector, "OnSwipeLeft", left);

        Swipe(detector, eventSystem, Vector2.zero, new Vector2(-79f, 0f));
        Assert.AreEqual(0, leftCount, "A short drag must not switch leaderboards.");

        Swipe(detector, eventSystem, Vector2.zero, new Vector2(-81f, 0f));
        Assert.AreEqual(1, leftCount);

        Swipe(detector, eventSystem, Vector2.zero, new Vector2(-100f, 150f));
        Assert.AreEqual(1, leftCount, "A predominantly vertical drag belongs to ScrollRect.");
    }

    [Test]
    public void DraftTabTitleAndPrivateStatusPathsStayRemoved()
    {
        Type controller = Runtime("LeaderboardController");
        foreach (string fieldName in new[]
        {
            "currentPlayerStatusRoot", "currentPlayerRowsParent", "currentPlayerProgressText",
            "titleText", "multiplayerTabActiveIndicator", "royalRumbleTabActiveIndicator",
        })
        {
            Assert.IsNull(controller.GetField(fieldName), fieldName);
        }
        Assert.IsNull(controller.GetMethod("ShowMultiplayer"));
        Assert.IsNull(controller.GetMethod("ShowRoyalRumble"));
        Assert.NotNull(controller.GetField("leaderboardScrollRect"));
        Assert.NotNull(controller.GetField("backgroundImage"));
        Assert.NotNull(controller.GetField("modeBackgrounds"));
        Assert.IsNull(controller.GetField("loadingPanel"));

        string source = File.ReadAllText(
            Path.Combine(Application.dataPath, "Scripts", "Leaderboard", "LeaderboardController.cs")
        );
        StringAssert.Contains("SceneLoadingOverlay.Show()", source);
        StringAssert.Contains("SceneLoadingOverlay.Hide()", source);
    }

    [Test]
    public void ClientLeaderboardWritersAndLegacyOwnersStayRemoved()
    {
        string scripts = Path.Combine(Application.dataPath, "Scripts");
        Assert.IsFalse(File.Exists(Path.Combine(scripts, "Leaderboard", "PlayFabManagerLeaderboard.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(scripts, "Leaderboard", "RecordsLoader.cs")));

        string manager = File.ReadAllText(Path.Combine(scripts, "PlayFabManager.cs"));
        StringAssert.DoesNotContain("UpdatePlayerStatistics", manager);
        StringAssert.DoesNotContain("SendLeaderboard", manager);
        StringAssert.DoesNotContain("GetLeaderboard(", manager);

        string service = File.ReadAllText(Path.Combine(scripts, "Leaderboard", "LeaderboardService.cs"));
        StringAssert.DoesNotContain("PlayFabClientAPI", service);
        StringAssert.Contains("GetLeaderboardState", service);
    }

    [Test]
    public void LeaderboardSceneOwnsBackgroundScrollAndSwipeWiring()
    {
        string scene = File.ReadAllText(Path.Combine(Application.dataPath, "Scenes", "Leaderboard.unity"));
        string swipeMeta = File.ReadAllText(Path.Combine(Application.dataPath, "Scripts", "SwipeDetector.cs.meta"));
        Match guid = Regex.Match(swipeMeta, @"(?m)^guid:\s*(?<value>\S+)");

        Assert.IsTrue(guid.Success, "SwipeDetector script guid must be readable from its meta file.");
        StringAssert.Contains($"guid: {guid.Groups["value"].Value}", scene);
        StringAssert.Contains("leaderboardScrollRect: {fileID:", scene);
        StringAssert.DoesNotContain("leaderboardScrollRect: {fileID: 0}", scene);
        StringAssert.Contains("backgroundImage: {fileID:", scene);
        StringAssert.DoesNotContain("backgroundImage: {fileID: 0}", scene);
        StringAssert.Contains("mode: mp", scene);
        StringAssert.Contains("mode: rr", scene);
        StringAssert.Contains("minimumSwipeDistance: 80", scene);
        Assert.AreEqual(1, Regex.Matches(scene, "m_MethodName: SelectNextMode").Count);
        Assert.AreEqual(1, Regex.Matches(scene, "m_MethodName: SelectPreviousMode").Count);
        StringAssert.Contains("guid: 545bec8b424f92f4aba22e383ec6f895", scene);
        StringAssert.DoesNotContain("loadingPanel:", scene);
        StringAssert.DoesNotContain("currentPlayerStatusRoot:", scene);
        StringAssert.DoesNotContain("titleText:", scene);
    }

    private Component CreateController(out Transform rows)
    {
        Component controller = Create("Controller").AddComponent(Runtime("LeaderboardController"));
        rows = CreateUi("Rows").transform;
        GameObject content = Create("Content");
        GameObject error = Create("Error");
        GameObject scrollObject = CreateUi("Scroll");
        ScrollRect scrollRect = scrollObject.AddComponent<ScrollRect>();
        scrollRect.content = (RectTransform)rows;
        Image background = Create("Background").AddComponent<Image>();

        SetField(controller, "rowPrefab", CreateRow("RowPrefab"));
        SetField(controller, "rowsParent", rows);
        SetField(controller, "contentRoot", content);
        SetField(controller, "errorPanel", error);
        SetField(controller, "leaderboardScrollRect", scrollRect);
        SetField(controller, "backgroundImage", background);
        return controller;
    }

    private object CreateState(string mode, string[] modes, string playerId, object[] entries)
    {
        object state = Activator.CreateInstance(Runtime("LeaderboardStateResponse"));
        SetField(state, "success", true);
        SetField(state, "mode", mode);
        SetField(state, "availableModes", new List<string>(modes));
        SetField(state, "currentPlayerId", playerId);
        IList list = (IList)Activator.CreateInstance(
            typeof(List<>).MakeGenericType(Runtime("LeaderboardEntryDto"))
        );
        foreach (object entry in entries) list.Add(entry);
        SetField(state, "entries", list);
        return state;
    }

    private static object Entry(int rank, string playerId, string displayName, int score)
    {
        object entry = Activator.CreateInstance(Runtime("LeaderboardEntryDto"));
        SetField(entry, "rank", rank);
        SetField(entry, "playerId", playerId);
        SetField(entry, "displayName", displayName);
        SetField(entry, "score", score);
        return entry;
    }

    private static void ApplyLoadedState(Component controller, object state)
    {
        controller.GetType().GetMethod("ApplyLoadedState", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(controller, new[] { state });
    }

    private static string ResolveRelativeMode(Component controller, int direction)
    {
        return (string)controller.GetType()
            .GetMethod("ResolveRelativeMode", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(controller, new object[] { direction });
    }

    private static void Swipe(
        Component detector,
        EventSystem eventSystem,
        Vector2 start,
        Vector2 end)
    {
        PointerEventData data = new PointerEventData(eventSystem) { position = start };
        detector.GetType().GetMethod("OnBeginDrag").Invoke(detector, new object[] { data });
        data.position = end;
        detector.GetType().GetMethod("OnEndDrag").Invoke(detector, new object[] { data });
    }

    private GameObject CreateRow(string name)
    {
        GameObject row = Create(name);
        GameObject image = Create(name + "Image");
        image.transform.SetParent(row.transform, false);
        image.AddComponent<Image>();
        Type textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true);
        foreach (string label in new[] { "Place", "Name", "Score" })
        {
            GameObject text = Create(label);
            text.transform.SetParent(image.transform, false);
            text.AddComponent(textType);
        }
        return row;
    }

    private static Image RowImage(GameObject row) =>
        row.transform.GetChild(0).GetComponent<Image>();

    private static string[] RowTexts(GameObject row)
    {
        Type textType = Type.GetType("TMPro.TMP_Text, Unity.TextMeshPro", true);
        Array texts = row.GetComponentsInChildren(textType, true);
        string[] values = new string[texts.Length];
        for (int index = 0; index < texts.Length; index++)
        {
            values[index] = (string)textType.GetProperty("text").GetValue(texts.GetValue(index));
        }
        return values;
    }

    private GameObject Create(string name)
    {
        GameObject item = new GameObject(name);
        createdObjects.Add(item);
        return item;
    }

    private GameObject CreateUi(string name)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        createdObjects.Add(item);
        return item;
    }

    private static object Field(object target, string name) =>
        target.GetType().GetField(name).GetValue(target);

    private static object PrivateField(object target, string name) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);

    private static void SetField(object target, string name, object value) =>
        target.GetType().GetField(name).SetValue(target, value);

    private static Type Runtime(string name) => Type.GetType(name + ", Assembly-CSharp", true);

    private static Type RuntimeOrNull(string name) => Type.GetType(name + ", Assembly-CSharp", false);
}
