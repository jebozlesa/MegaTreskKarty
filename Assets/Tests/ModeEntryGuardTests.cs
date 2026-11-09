using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class ModeEntryGuardTests
{
    private GameObject root;
    private Component entry;
    private Component missing;
    private Component unavailable;
    private bool hadRoom;
    private string previousRoom;

    [SetUp]
    public void SetUp()
    {
        hadRoom = PlayerPrefs.HasKey("RoomCode");
        previousRoom = PlayerPrefs.GetString("RoomCode");
        PlayerPrefs.DeleteKey("RoomCode");
        root = new GameObject("ModeEntryGuard");
        entry = root.AddComponent(Runtime("PlayButton"));
        missing = Dialog("MissingDeck"); unavailable = Dialog("CheckUnavailable");
        Set(entry, "missingDeckDialog", missing);
        Set(entry, "entryCheckUnavailableDialog", unavailable);
        Set(entry, "deckContextId", "mp");
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(root);
        PlayerPrefs.DeleteKey("RoomCode");
        if (hadRoom) PlayerPrefs.SetString("RoomCode", previousRoom);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ConfirmedReadinessSelectsNavigationOrTheAuthoredMissingDeckNotice(bool canEnter)
    {
        object response = Response("p1", "mp", true, canEnter, canEnter ? null : "missing_active_deck");
        Assert.AreEqual(canEnter, Apply(response));
        Assert.AreEqual(!canEnter, missing.gameObject.activeSelf);
        Assert.IsFalse(unavailable.gameObject.activeSelf);
    }

    [TestCase("other", "mp", true, false, "missing_active_deck")]
    [TestCase("p1", "campaign:bushido", true, false, "missing_active_deck")]
    [TestCase("p1", "mp", false, false, null)]
    [TestCase("p1", "mp", true, false, null)]
    public void UnreliableOrMismatchedResponseIsNeverReportedAsMissingDeck(string player, string context, bool success, bool canEnter, string reason)
    {
        Assert.IsFalse(Apply(Response(player, context, success, canEnter, reason)));
        Assert.IsFalse(missing.gameObject.activeSelf);
        Assert.IsTrue(unavailable.gameObject.activeSelf);
    }

    [Test]
    public void MissingResponseShowsOnlyCheckUnavailable()
    {
        Assert.IsFalse(Apply(null));
        Assert.IsFalse(missing.gameObject.activeSelf);
        Assert.IsTrue(unavailable.gameObject.activeSelf);
    }

    [Test]
    public void MissingReadinessFlagCannotBeMistakenForAConfirmedDeckDenial()
    {
        object response = Response("p1", "mp", true, false, "missing_active_deck");
        Set(response, "canEnter", null);
        Assert.IsFalse(Apply(response));
        Assert.IsFalse(missing.gameObject.activeSelf);
        Assert.IsTrue(unavailable.gameObject.activeSelf);
    }

    [Test]
    public void ContradictorySuccessAndErrorCannotPermitNavigation()
    {
        object response = Response("p1", "mp", true, true, null);
        Set(response, "error", "mode_entry_check_failed");
        Assert.IsFalse(Apply(response));
        Assert.IsTrue(unavailable.gameObject.activeSelf);
    }

    [Test]
    public void ExistingRoomBypassesOnlyFreshMpMenuGateNotCampaignGate()
    {
        PlayerPrefs.SetString("RoomCode", "OLD");
        Assert.IsTrue((bool)Invoke("ShouldReconcileOwnedRoom"));
        Set(entry, "deckContextId", "campaign:bushido");
        Assert.IsFalse((bool)Invoke("ShouldReconcileOwnedRoom"));
    }

    [Test]
    public void OnlyOneMenuEntryCheckCanOwnLoadingAcrossDifferentButtons()
    {
        var otherObject = new GameObject("OtherEntry"); otherObject.transform.SetParent(root.transform);
        Component other = otherObject.AddComponent(Runtime("PlayButton"));
        Assert.IsTrue((bool)Invoke("TryBeginEntryCheck"));
        Assert.IsFalse((bool)other.GetType().GetMethod("TryBeginEntryCheck", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(other, null));
        Invoke("EndEntryCheck");
        Assert.IsTrue((bool)other.GetType().GetMethod("TryBeginEntryCheck", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(other, null));
    }

    [Test]
    public void InactiveEntryCannotAcquireLoadingOwnership()
    {
        root.SetActive(false);
        Assert.IsFalse((bool)Invoke("TryBeginEntryCheck"));
    }

    [Test]
    public void DisabledOwnersLateCallbackCannotOwnOrReleaseAnotherButtonsRequest()
    {
        Assert.IsTrue((bool)Invoke("TryBeginEntryCheck"));
        Assert.IsTrue((bool)Invoke("OwnsEntryCheck", 0));
        root.SetActive(false);
        Assert.IsFalse((bool)Invoke("OwnsEntryCheck", 0));
        root.SetActive(true);
        var otherObject = new GameObject("NewOwner"); otherObject.transform.SetParent(root.transform);
        Component other = otherObject.AddComponent(Runtime("PlayButton"));
        Assert.IsTrue((bool)other.GetType().GetMethod("TryBeginEntryCheck", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(other, null));
        Invoke("EndEntryCheck");
        Assert.IsTrue((bool)other.GetType().GetMethod("OwnsEntryCheck", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(other, new object[] { 0 }));
    }

    [TestCase("deck_unavailable", "p1", true, false)]
    [TestCase("mode_entry_check_failed", "p1", false, true)]
    [TestCase("deck_unavailable", "other", false, false)]
    public void LobbyDistinguishesMissingDeckFromUnverifiedAdmissionAndChecksPlayerOwnership(string error, string player, bool showMissing, bool showUnavailable)
    {
        Component lobby = root.AddComponent(Runtime("MultiplayerLobbyUI"));
        Set(lobby, "playerId", "p1"); Set(lobby, "mpDeckUnavailableDialog", missing);
        Set(lobby, "entryCheckUnavailableDialog", unavailable);
        MethodInfo parse = lobby.GetType().GetMethod("ParseResponse", BindingFlags.Static | BindingFlags.NonPublic);
        object response = parse.Invoke(null, new object[] { "{\"success\":false,\"playerId\":\"" + player + "\",\"contextId\":\"mp\",\"error\":\"" + error + "\"}" });
        bool handled = (bool)lobby.GetType().GetMethod("TryHandleAdmissionRejection", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(lobby, new[] { response });
        Assert.AreEqual(showMissing || showUnavailable, handled);
        Assert.AreEqual(showMissing, missing.gameObject.activeSelf);
        Assert.AreEqual(showUnavailable, unavailable.gameObject.activeSelf);
    }

    private bool Apply(object response) => (bool)Invoke("ApplyEntryResponse", response, "p1", "mp");
    private object Response(string player, string context, bool success, bool canEnter, string reason)
    {
        object response = Activator.CreateInstance(Runtime("ModeEntryStateResponse"));
        Set(response, "playerId", player); Set(response, "contextId", context);
        Set(response, "success", success); Set(response, "canEnter", canEnter);
        Set(response, "reason", reason); return response;
    }
    private Component Dialog(string name)
    {
        var go = new GameObject(name); go.transform.SetParent(root.transform);
        Component dialog = go.AddComponent(Runtime("ConfirmDialogController")); go.SetActive(false); return dialog;
    }
    private static Type Runtime(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static void Set(object target, string name, object value)
    {
        FieldInfo field = target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.IsNotNull(field, name); field.SetValue(target, value);
    }
    private object Invoke(string name, params object[] args)
    {
        MethodInfo method = entry.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.IsNotNull(method, name); return method.Invoke(entry, args);
    }
}
