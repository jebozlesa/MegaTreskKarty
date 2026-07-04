using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class CardProgressionPlaybackTests
{
    [Test]
    public void CardProgressionPlayback_UpdatesCardStateAndUsesVisualOnlyAnimation()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Singleplayer", "CardProgressionPlayback.cs");

        StringAssert.Contains("public IEnumerator Play(CardProgressionDto progression, Kard card)", source);
        StringAssert.Contains("progression.applied == false", source);
        StringAssert.Contains("progression.xpGained <= 0", source);
        StringAssert.Contains("card.experience = progression.experienceAfter;", source);
        StringAssert.Contains("card.level = progression.levelAfter;", source);
        StringAssert.Contains("card.levelText.text = \"lvl \" + card.level;", source);
        StringAssert.Contains("card.EffectAnimations(progression.xpGained, \"XP\", XpColor)", source);
        StringAssert.Contains("card.EffectAnimations(progression.levelUps, \"LVL\", LevelColor)", source);
        Assert.IsFalse(source.Contains("+{progression.xpGained} XP"));
        Assert.IsFalse(source.Contains("LEVEL {progression.levelAfter}"));
        Assert.IsFalse(source.Contains("AddExperience("));
        Assert.IsFalse(source.Contains("PlayFab"));
    }

    [Test]
    public void Kard_DoesNotOwnPermanentXpOrStatProgression()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Kard.cs");

        Assert.IsFalse(source.Contains("AddExperience("));
        Assert.IsFalse(source.Contains("UpdateRandomStat("));
        Assert.IsFalse(source.Contains("CalculateExpForLevel("));
        Assert.IsFalse(source.Contains("UpdateCardData("));
        Assert.IsFalse(source.Contains("PlayFabCardManager"));
    }

    [Test]
    public void FightSystem_DoesNotAwardLocalCardExperience()
    {
        string source = ReadProjectFile("Assets", "Scripts", "FightSystem.cs");

        Assert.IsFalse(source.Contains("AddExperience("));
    }

    [Test]
    public void PlayFabCardManager_DoesNotWritePlayerCardsFromClientProgression()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Game", "PlayFabCardManager.cs");

        Assert.IsFalse(source.Contains("public IEnumerator UpdateCardData"));
        Assert.IsFalse(source.Contains("UpdateUserDataRequest"));
        Assert.IsFalse(source.Contains("PlayFabClientAPI.UpdateUserData"));
    }

    [Test]
    public void OnlineCardExperienceCurve_UsesApprovedCumulativeXpAnchors()
    {
        var curveType = System.Type.GetType("OnlineCardExperienceCurve, Assembly-CSharp");
        Assert.NotNull(curveType, "Online card XP display must use a shared client curve helper.");

        MethodInfo requiredXpForLevel = curveType.GetMethod("RequiredXpForLevel", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(requiredXpForLevel, "Missing RequiredXpForLevel(int).");

        Assert.AreEqual(0, InvokeRequiredXpForLevel(requiredXpForLevel, 1));
        Assert.AreEqual(5, InvokeRequiredXpForLevel(requiredXpForLevel, 2));
        Assert.AreEqual(120, InvokeRequiredXpForLevel(requiredXpForLevel, 10));
        Assert.AreEqual(6280, InvokeRequiredXpForLevel(requiredXpForLevel, 100));
    }

    [Test]
    public void AlbumCardDetails_ShowCumulativeExperienceAgainstApprovedNextLevelTarget()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Card.cs");

        StringAssert.Contains("OnlineCardExperienceCurve.RequiredXpForLevel(level + 1)", source);
        StringAssert.Contains("\"Experience: \" + experience + \" / \" +", source);
        Assert.IsFalse(source.Contains("0.2636521817872269"));
        Assert.IsFalse(source.Contains("5.356569536042434"));
    }

    [Test]
    public void AlbumDeckCards_CopyExperienceFromPlayerCards()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Album", "DeckManager.cs");

        int experienceAssignments = CountOccurrences(source, ".experience = existingCard.Experience;");

        Assert.GreaterOrEqual(
            experienceAssignments,
            2,
            "Deck panel cards must preserve XP both when the deck loads and when a card is added to the hand."
        );
    }

    [Test]
    public void OnlineSingleplayerShells_PlayCardProgressionFeedbackAfterViewRefresh()
    {
        string rrSource = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string campaignSource = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");

        AssertFeedbackHook(rrSource, "[RoyalRumbleShellController] Card XP feedback");
        AssertFeedbackHook(campaignSource, "[CampaignOnlineShellController] Card XP feedback");
    }

    private static void AssertFeedbackHook(string source, string expectedLogPrefix)
    {
        StringAssert.Contains("private readonly CardProgressionPlayback cardProgressionPlayback", source);
        StringAssert.Contains("private IEnumerator PlayCardProgressionFeedback(", source);
        StringAssert.Contains(expectedLogPrefix, source);
        StringAssert.Contains("experience=", source);
        StringAssert.Contains("Card XP feedback played", source);
        StringAssert.Contains("Card XP feedback skipped", source);
        StringAssert.Contains("reason=", source);
        StringAssert.Contains("cardProgressionPlayback.Play(progression, renderedPlayerActiveCard)", source);

        int feedbackCall = source.IndexOf("PlayCardProgressionFeedback(envelope.cardProgression", System.StringComparison.Ordinal);
        int refreshCall = source.IndexOf("RefreshPostBattleView(envelope", System.StringComparison.Ordinal);

        Assert.GreaterOrEqual(feedbackCall, 0, "Feedback call is missing.");
        Assert.GreaterOrEqual(refreshCall, 0, "Post-battle refresh call is missing.");
        Assert.Greater(feedbackCall, refreshCall, "XP feedback must run after the post-battle view refresh so rebuilds do not destroy the XP effect.");
    }

    private static string ReadProjectFile(params string[] relativeParts)
    {
        string fullPath = Path.Combine(Application.dataPath, Path.Combine(relativeParts[1..]));
        Assert.IsTrue(File.Exists(fullPath), $"Missing project file: {fullPath}");
        return File.ReadAllText(fullPath);
    }

    private static int InvokeRequiredXpForLevel(MethodInfo method, int level)
    {
        return (int)method.Invoke(null, new object[] { level });
    }

    private static int CountOccurrences(string source, string expected)
    {
        int count = 0;
        int index = 0;

        while ((index = source.IndexOf(expected, index, System.StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += expected.Length;
        }

        return count;
    }
}
