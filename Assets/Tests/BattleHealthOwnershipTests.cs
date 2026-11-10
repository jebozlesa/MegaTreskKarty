using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class BattleHealthOwnershipTests
{
    private GameObject root;

    [SetUp]
    public void SetUp()
    {
        root = new GameObject("HealthOwnership", typeof(RectTransform));
        root.SetActive(false);
    }

    [TearDown]
    public void TearDown() => UnityEngine.Object.DestroyImmediate(root);

    [TestCase(true)]
    [TestCase(false)]
    public void LibraryHealthSortUsesThePermanentStatNotBattleMaximum(bool ascending)
    {
        Type cardType = Runtime("GeneratedCard");
        Array cards = Array.CreateInstance(cardType, 3);
        int[] health = { 24, 28, 21 };
        int[] irrelevantMaxima = { 99, 0, 80 };
        for (int index = 0; index < cards.Length; index++)
        {
            object card = Activator.CreateInstance(cardType);
            Set(card, "CardID", "owned-" + index);
            Set(card, "Health", health[index]);
            Set(card, "MaxHealth", irrelevantMaxima[index]);
            cards.SetValue(card, index);
        }
        object criterion = Enum.Parse(Runtime("LibrarySortCriterion"), "Health");
        IList result = (IList)Runtime("LibraryCardSorter").GetMethod("SortCards")
            .Invoke(null, new object[] { cards, criterion, ascending });
        int[] expected = ascending ? new[] { 21, 24, 28 } : new[] { 28, 24, 21 };
        for (int index = 0; index < result.Count; index++)
            Assert.AreEqual(expected[index], Read(result[index], "Health"));
        Assert.AreEqual(24, Read(cards.GetValue(0), "Health"));
        Assert.AreEqual(99, Read(cards.GetValue(0), "MaxHealth"));
    }

    [TestCase(21)]
    [TestCase(0)]
    [TestCase(28)]
    public void CardStartPreservesTheMaximumAssignedByItsCreator(int currentHealth)
    {
        var card = Card(currentHealth, 28);
        var databaseType = Runtime("Database");
        var instance = databaseType.GetField("Instance");
        var previous = instance.GetValue(null);
        try
        {
            instance.SetValue(null, root.AddComponent(databaseType));
            var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true);
            Set(card, "nameText", Child("Name").AddComponent(textType));
            Set(card, "levelText", Child("Level").AddComponent(textType));
            Set(card, "cardImage", Child("Portrait").AddComponent<Image>());
            Set(card, "background", Child("Background").AddComponent<Image>());
            Set(card, "image", "__health_test_no_asset__");
            card.GetType().GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(card, null);
            Assert.AreEqual(28, Property(card, "maxHealth"));
            Assert.AreEqual(currentHealth, Property(card, "health"));
        }
        finally { instance.SetValue(null, previous); }
    }

    [TestCase("RoyalRumbleSessionParser", true)]
    [TestCase("RoyalRumbleSessionParser", false)]
    [TestCase("CampaignOnlineSessionParser", true)]
    [TestCase("CampaignOnlineSessionParser", false)]
    public void SoloParsingAndRepeatedBoardSyncPreserveBothSidesMaximums(string parserName, bool pascalCase)
    {
        var controllerType = Runtime(parserName == "RoyalRumbleSessionParser"
            ? "RoyalRumbleShellController" : "CampaignOnlineShellController");
        var controller = root.AddComponent(controllerType);
        var sync = controllerType.GetMethod("ApplyCardSnapshotToRenderedCard", BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (string side in new[] { "player", "enemy" })
        {
            Component card = Card(28, 28);
            Component bar = Bar(out var fill);
            foreach (int hp in new[] { 28, 21, 16, 26, 0 })
            {
                object snapshot = ParseCard(parserName, side, hp, 28, pascalCase);
                sync.Invoke(controller, new[] { snapshot, card, bar });
                Assert.AreEqual(28, Property(card, "maxHealth"), side);
                Assert.AreEqual(hp, Property(card, "health"), side);
                Assert.That(fill.fillAmount, Is.EqualTo(hp / 28f).Within(0.0001f), side);
                Assert.IsTrue(fill.enabled);
            }
            // A replacement or a genuine authoritative max change must not reuse a stale cap.
            sync.Invoke(controller, new[] { ParseCard(parserName, side + "-replacement", 12, 30, pascalCase), card, bar });
            Assert.AreEqual(30, Property(card, "maxHealth"));
            Assert.That(fill.fillAmount, Is.EqualTo(0.4f).Within(0.0001f));
        }
    }

    [TestCase("RoyalRumbleSessionParser", true, null)]
    [TestCase("RoyalRumbleSessionParser", false, null)]
    [TestCase("RoyalRumbleSessionParser", true, 0)]
    [TestCase("RoyalRumbleSessionParser", false, -1)]
    [TestCase("CampaignOnlineSessionParser", true, null)]
    [TestCase("CampaignOnlineSessionParser", false, null)]
    [TestCase("CampaignOnlineSessionParser", true, 0)]
    [TestCase("CampaignOnlineSessionParser", false, -1)]
    public void SoloParserRejectsMissingOrInvalidMaximumInsteadOfUsingCurrentHp(string parserName, bool pascalCase, int? maximum)
    {
        var error = Assert.Throws<TargetInvocationException>(() => ParseCard(parserName, "injured", 21, maximum, pascalCase));
        Assert.IsInstanceOf<FormatException>(error.InnerException);
        StringAssert.Contains("maxHealth", error.InnerException.Message);
        StringAssert.Contains("injured", error.InnerException.Message);
    }

    [TestCase("RoyalRumbleCardMapper", 0)]
    [TestCase("RoyalRumbleCardMapper", -1)]
    [TestCase("CampaignOnlineCardMapper", 0)]
    [TestCase("CampaignOnlineCardMapper", -1)]
    [TestCase("SelectedCardData", 0)]
    [TestCase("SelectedCardData", -1)]
    public void RenderMappingRejectsInvalidMaximum(string mapper, int maximum)
    {
        object snapshot = Snapshot("injured", 21, maximum);
        var method = Runtime(mapper).GetMethod("ToGeneratedCard");
        var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(
            mapper == "SelectedCardData" ? snapshot : null,
            mapper == "SelectedCardData" ? null : new[] { snapshot }));
        Assert.IsInstanceOf<FormatException>(error.InnerException);
        StringAssert.Contains("maxHealth", error.InnerException.Message);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void MultiplayerSelectionCopiesActualCardMaximumNotCurrentHpOrAnOldDefinition(bool hasDefinition)
    {
        Component card = Card(21, 28);
        object definition = hasDefinition ? Activator.CreateInstance(Runtime("GeneratedCard")) : null;
        if (definition != null) Set(definition, "MaxHealth", 24);
        object snapshot = Runtime("SelectedCardData").GetMethod("FromCard").Invoke(null, new[] { card, definition, "player" });
        Assert.AreEqual(21, Read(snapshot, "health"));
        Assert.AreEqual(28, Read(snapshot, "maxHealth"));
        object rendered = Runtime("SelectedCardData").GetMethod("ToGeneratedCard").Invoke(snapshot, null);
        Assert.AreEqual(28, Read(rendered, "MaxHealth"));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void MultiplayerSelectionRejectsInvalidCardMaximum(int maximum)
    {
        Component card = Card(21, maximum);
        var error = Assert.Throws<TargetInvocationException>(() => Runtime("SelectedCardData").GetMethod("FromCard")
            .Invoke(null, new object[] { card, null, "player" }));
        Assert.IsInstanceOf<FormatException>(error.InnerException);
    }

    [TestCase(0, 21)]
    [TestCase(-1, 21)]
    [TestCase(0, 0)]
    public void InvalidMaximumHidesFillWithoutChangingGameplayAndValidBindingRecovers(int maximum, int currentHealth)
    {
        Component card = Card(currentHealth, maximum);
        Component bar = Bar(out var fill);
        var setBar = bar.GetType().GetMethod("SetBar");
        LogAssert.Expect(LogType.Error, new Regex(@"\[HP_BAR\].*invalid maxHealth"));
        setBar.Invoke(bar, new object[] { card });
        Assert.IsFalse(fill.enabled);
        Assert.AreEqual(0f, fill.fillAmount);
        Assert.IsFalse(float.IsNaN(fill.fillAmount));
        Assert.AreEqual(currentHealth, Property(card, "health"));
        Assert.AreEqual(maximum, Property(card, "maxHealth"));
        card.GetType().GetProperty("maxHealth").SetValue(card, 28);
        setBar.Invoke(bar, new object[] { card });
        Assert.IsTrue(fill.enabled);
        Assert.That(fill.fillAmount, Is.EqualTo(currentHealth / 28f).Within(0.0001f));
    }

    [Test]
    public void UnboundHealthUpdateDoesNotDivideByZeroOrDrawAFullBar()
    {
        Component bar = Bar(out var fill);
        LogAssert.Expect(LogType.Error, new Regex(@"\[HP_BAR\].*invalid maxHealth"));
        bar.GetType().GetMethod("SetHP").Invoke(bar, new object[] { 21f });
        Assert.IsFalse(fill.enabled);
        Assert.AreEqual(0f, fill.fillAmount);
    }

    [TestCase("RoyalRumbleCardMapper")]
    [TestCase("CampaignOnlineCardMapper")]
    [TestCase("SelectedCardData")]
    public void RenderMappingPreservesAnExplicitMaximumEvenWhenCurrentHealthIsZero(string mapper)
    {
        object snapshot = Snapshot("dead", 0, 28);
        object generated = Runtime(mapper).GetMethod("ToGeneratedCard").Invoke(
            mapper == "SelectedCardData" ? snapshot : null,
            mapper == "SelectedCardData" ? null : new[] { snapshot });
        Assert.AreEqual(0, Read(generated, "Health"));
        Assert.AreEqual(28, Read(generated, "MaxHealth"));
    }

    [TestCase(null)]
    [TestCase(0)]
    [TestCase(-1)]
    public void MultiplayerRoomHandRejectsBadMaximumBeforeCreatingAnyCards(int? maximum)
    {
        Component hand = MultiplayerHand(out _, out var player);
        object roomCards = RoomCards(21, maximum);
        var error = Assert.Throws<TargetInvocationException>(() => Invoke(hand, "CreateMultiplayerHandFromRoom", roomCards));
        Assert.IsInstanceOf<FormatException>(error.InnerException);
        StringAssert.Contains("c4", error.InnerException.Message);
        StringAssert.Contains("maxHealth", error.InnerException.Message);
        Assert.AreEqual(0, ((IList)Read(player, "hand")).Count);
        Assert.AreEqual(0, ((IList)PrivateField(hand, "preparedCards")).Count);
    }

    [TestCase(21)]
    [TestCase(28)]
    public void MultiplayerRoomHandSurvivesStartAndIsPreparedWithExplicitMaximum(int currentHealth)
    {
        Component hand = MultiplayerHand(out _, out var player);
        Invoke(hand, "CreateMultiplayerHandFromRoom", RoomCards(currentHealth, 28));
        IList cards = (IList)PrivateField(hand, "preparedCards");
        Assert.AreEqual(5, cards.Count);
        RunPreparedCardStarts(cards, () =>
        {
            foreach (Component card in cards)
            {
                Assert.AreEqual(currentHealth, Property(card, "health"));
                Assert.AreEqual(28, Property(card, "maxHealth"));
                Assert.IsTrue(((IList)Read(player, "hand")).Contains(card));
                object selected = Runtime("SelectedCardData").GetMethod("FromCard")
                    .Invoke(null, new object[] { card, null, "player" });
                Assert.AreEqual(28, Read(selected, "maxHealth"));
            }
            var completion = FinishPreparation(hand);
            Assert.IsTrue(completion.Task.IsCompletedSuccessfully,
                "Preparation status: " + completion.Task.Status + "; " + completion.Task.Exception);
            Assert.IsTrue(completion.Task.Result);
        });
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void MultiplayerHandCannotBecomePreparedIfMaximumIsCorruptedAfterStart(int invalidMaximum)
    {
        Component hand = MultiplayerHand(out var service, out _);
        Invoke(hand, "CreateMultiplayerHandFromRoom", RoomCards(21, 28));
        IList cards = (IList)PrivateField(hand, "preparedCards");
        RunPreparedCardStarts(cards, () =>
        {
            var invalidCard = (Component)cards[4];
            invalidCard.GetType().GetProperty("maxHealth").SetValue(invalidCard, invalidMaximum);
            var completion = FinishPreparation(hand);
            Assert.IsTrue(completion.Task.IsFaulted);
            StringAssert.Contains("c4", completion.Task.Exception.InnerException.Message);
            StringAssert.Contains("maxHealth", completion.Task.Exception.InnerException.Message);
            Assert.IsFalse((bool)service.GetType().GetProperty("CanInteractWithBattle").GetValue(service));
        });
    }

    private Component MultiplayerHand(out Component service, out Component player)
    {
        var hand = root.AddComponent(Runtime("MultiplayerHandManager"));
        service = root.AddComponent(Runtime("MultiplayerService"));
        var fight = root.AddComponent(Runtime("FightSystemMultiplayer"));
        ((Behaviour)fight).enabled = false;
        var owner = Child("Player");
        player = owner.AddComponent(Runtime("Player"));
        Set(fight, "player", player);
        Set(fight, "hrac", owner);
        Set(hand, "fightSystem", fight);
        Set(hand, "multiplayerService", service);
        Set(hand, "attackDescriptions", root.AddComponent(Runtime("AttackDescriptions")));
        var template = Card(21, 28);
        ((Behaviour)template).enabled = false;
        var textType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro", true);
        GameObject Visual(string name)
        {
            var visual = new GameObject(name, typeof(RectTransform));
            visual.transform.SetParent(template.transform, false);
            return visual;
        }
        Set(template, "nameText", Visual("Name").AddComponent(textType));
        Set(template, "levelText", Visual("Level").AddComponent(textType));
        Set(template, "cardImage", Visual("Portrait").AddComponent<Image>());
        Set(template, "background", Visual("Background").AddComponent<Image>());
        Visual("CardBack");
        Set(hand, "kartaPrefab", template.gameObject);
        return hand;
    }

    private static object RoomCards(int health, int? maximum)
    {
        var rows = new List<Dictionary<string, object>>();
        for (int index = 0; index < 5; index++)
        {
            var row = new Dictionary<string, object>
            {
                ["CardID"] = "c" + index,
                ["PersonName"] = "Fighter " + index,
                ["CardPicture"] = "__health_test_no_asset__",
                ["Health"] = health,
                ["Attack1"] = 1, ["Attack2"] = 2, ["Attack3"] = 3, ["Attack4"] = 4,
            };
            if (index != 4 || maximum.HasValue) row["MaxHealth"] = index == 4 ? maximum.Value : 28;
            rows.Add(row);
        }
        var arrayType = Type.GetType("Newtonsoft.Json.Linq.JArray, Newtonsoft.Json", true);
        return arrayType.GetMethod("FromObject", new[] { typeof(object) }).Invoke(null, new object[] { rows });
    }

    private void RunPreparedCardStarts(IList cards, Action assertions)
    {
        var databaseType = Runtime("Database");
        var databaseInstance = databaseType.GetField("Instance");
        object previousDatabase = databaseInstance.GetValue(null);
        var databaseObject = new GameObject("InactiveHealthTestDatabase");
        databaseObject.SetActive(false);
        var texture = new Texture2D(2, 2);
        var sprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.zero);
        try
        {
            databaseInstance.SetValue(null, databaseObject.AddComponent(databaseType));
            foreach (Component card in cards)
            {
                Invoke(card, "Start");
                ((Image)Read(card, "cardImage")).sprite = sprite;
            }
            root.SetActive(true);
            assertions();
        }
        finally
        {
            databaseInstance.SetValue(null, previousDatabase);
            UnityEngine.Object.DestroyImmediate(databaseObject);
            UnityEngine.Object.DestroyImmediate(sprite);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    private static System.Threading.Tasks.TaskCompletionSource<bool> FinishPreparation(Component hand)
    {
        var completion = new System.Threading.Tasks.TaskCompletionSource<bool>();
        var iterator = (IEnumerator)Invoke(hand, "FinishPreparation", 0, completion);
        Assert.IsTrue(iterator.MoveNext());
        Assert.IsFalse(iterator.MoveNext());
        return completion;
    }

    private static object PrivateField(object target, string name) => target.GetType()
        .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);

    private static object Invoke(object target, string name, params object[] arguments) => target.GetType()
        .GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, arguments);

    private static object ParseCard(string parser, string id, int hp, int? max, bool pascalCase)
    {
        var card = new Dictionary<string, object>
        {
            [pascalCase ? "CardID" : "cardId"] = id,
            [pascalCase ? "Health" : "health"] = hp,
        };
        if (max.HasValue) card[pascalCase ? "MaxHealth" : "maxHealth"] = max.Value;
        var payload = new Dictionary<string, object>
        {
            ["success"] = true,
            ["session"] = new Dictionary<string, object>
            {
                ["playerId"] = "player",
                ["playerDeck"] = new Dictionary<string, object> { ["cards"] = new[] { card } }
            }
        };
        object envelope = Runtime(parser).GetMethod("ParseSessionEnvelope").Invoke(null, new object[] { payload });
        object session = Read(envelope, "session");
        return ((IList)Read(Read(session, "playerDeck"), "cards"))[0];
    }

    private Component Card(int hp, int max)
    {
        var card = Child("Fighter").AddComponent(Runtime("Kard"));
        Set(card, "cardId", "injured");
        Set(card, "cardName", "Injured Fighter");
        card.GetType().GetProperty("health").SetValue(card, hp);
        card.GetType().GetProperty("maxHealth").SetValue(card, max);
        return card;
    }

    private Component Bar(out Image fill)
    {
        var bar = Child("HealthBar").AddComponent(Runtime("HealthBar"));
        fill = Child("Fill").AddComponent<Image>();
        Set(bar, "health", fill);
        return bar;
    }

    private GameObject Child(string name)
    {
        var child = new GameObject(name, typeof(RectTransform));
        child.transform.SetParent(root.transform, false);
        return child;
    }

    private static object Snapshot(string id, int hp, int max)
    {
        object snapshot = Activator.CreateInstance(Runtime("SelectedCardData"));
        Set(snapshot, "cardId", id);
        Set(snapshot, "health", hp);
        Set(snapshot, "maxHealth", max);
        return snapshot;
    }

    private static Type Runtime(string name) => Type.GetType(name + ", Assembly-CSharp", true);
    private static object Read(object target, string field) => target.GetType().GetField(field).GetValue(target);
    private static object Property(object target, string property) => target.GetType().GetProperty(property).GetValue(target);
    private static void Set(object target, string field, object value) => target.GetType().GetField(field).SetValue(target, value);
}
