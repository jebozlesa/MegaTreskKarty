using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class BattleSharedDamagePlaybackTests
{
    [Test]
    public void BattleResultProcessor_DoesNotContainSharedDamageFallbackAfterHandlerExecution()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");

        StringAssert.Contains("yield return AttackRegistry.ExecuteOrFallback(attackId, context);", source);
        Assert.IsFalse(source.Contains("defender.health == defenderHealthBeforeAttack"));
        Assert.IsFalse(source.Contains("[AttackPlayback] Applying shared damage fallback"));
        Assert.IsFalse(source.Contains("private IEnumerator PlayTimelineDamageAnimation("));
    }

    [Test]
    public void StandardDamageHandlers_UseBattleValuePlayback()
    {
        string[] helperHandlers =
        {
            "Attack1Handler.cs",
            "Attack2Handler.cs",
            "Attack5Handler.cs",
            "Attack8Handler.cs",
            "Attack10Handler.cs",
            "Attack12Handler.cs",
            "Attack13Handler.cs",
            "Attack16Handler.cs",
            "Attack17Handler.cs",
            "Attack18Handler.cs",
            "Attack19Handler.cs",
            "Attack20Handler.cs",
            "Attack23Handler.cs",
            "Attack24Handler.cs",
            "Attack25Handler.cs",
            "Attack28Handler.cs",
            "Attack32Handler.cs",
            "Attack34Handler.cs",
            "Attack36Handler.cs",
            "Attack45Handler.cs",
            "Attack46Handler.cs",
        };

        foreach (string fileName in helperHandlers)
        {
            string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "AttackHandlers", fileName);
            StringAssert.Contains("BattleValuePlayback.PlayDamage(", source, $"Expected shared damage helper in {fileName}");
            StringAssert.Contains("!isMyAttack", source, $"Defender damage should target the opposite health bar in {fileName}");
            Assert.IsFalse(source.Contains("defender.health -= damage;"), $"Manual defender damage should be removed from {fileName}");
        }
    }

    [Test]
    public void OnlyExplicitSpecialCaseHandlers_UseManualParallelDamagePlayback()
    {
        string handlersDir = Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "AttackHandlers");
        string[] allHandlerFiles = Directory.GetFiles(handlersDir, "Attack*Handler.cs");

        var manualDamageHandlers = new List<string>();
        foreach (string path in allHandlerFiles)
        {
            string source = File.ReadAllText(path);
            if (source.Contains("AnimateDamage(defender, damage)") || source.Contains("AnimateDamage(attacker, attackerSelfDamage)"))
            {
                manualDamageHandlers.Add(Path.GetFileName(path));
            }
        }

        CollectionAssert.AreEquivalent(
            new[] { "Attack7Handler.cs", "Attack41Handler.cs" },
            manualDamageHandlers
        );
    }

    [Test]
    public void SharedBattleValuePlayback_OwnsHealthMutationAndUiSync()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleValuePlayback.cs");
        StringAssert.Contains("public static IEnumerator PlayDamage(", source);
        StringAssert.Contains("public static IEnumerator PlayHeal(", source);
        StringAssert.Contains("public static void ApplyDamage(", source);
        StringAssert.Contains("public static void ApplyHeal(", source);
        StringAssert.Contains("public static void SyncHealthBar(", source);
    }

    [Test]
    public void SharedBattleStatPlayback_OwnsStatMutationAndAnimation()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleStatPlayback.cs");
        StringAssert.Contains("public static IEnumerator PlayCardStatChanges(", source);
        StringAssert.Contains("public static IEnumerator PlayTimelineStatChange(", source);
        StringAssert.Contains("cardAnimator.AnimateStatChange(card, change, statName)", source);
    }

    [Test]
    public void BattleResultProcessor_UsesSharedStatPlaybackService()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("BattleStatPlayback.PlayTimelineStatChange(", source);
        StringAssert.Contains("BattleStatPlayback.PlayCardStatChanges(", source);
        Assert.IsFalse(source.Contains("GetStatApplier("));
        Assert.IsFalse(source.Contains("BattleStatApplier"));
    }

    [Test]
    public void BattleResultProcessor_UsesSharedEffectPlaybackService()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("BattleEffectPlayback.AddEffectIconOnly(", source);
        StringAssert.Contains("BattleEffectPlayback.RemoveEffectIconOnly(", source);
        StringAssert.Contains("BattleEffectPlayback.DisplayMultipleEffects(", source);
        StringAssert.Contains("BattleEffectPlayback.GetEffectName(", source);
        Assert.IsFalse(source.Contains("GetEffectVisuals("));
        Assert.IsFalse(source.Contains("BattleEffectVisuals"));
    }

    [Test]
    public void SharedBattleEffectPlayback_OwnsEffectIconAndEndAnimations()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");
        StringAssert.Contains("public static IEnumerator AddEffectIconOnly(", source);
        StringAssert.Contains("public static IEnumerator RemoveEffectIconOnly(", source);
        StringAssert.Contains("public static IEnumerator DisplayMultipleEffects(", source);
        StringAssert.Contains("public static string GetEffectName(", source);
        StringAssert.Contains("PlayFamineEndAnimation(card.transform)", source);
    }


    [Test]
    public void BattleResultProcessor_DotTickHandlersOwnTheirDamagePlayback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("case BattleStepType.BleedTick:", source);
        StringAssert.Contains("case BattleStepType.BurnTick:", source);
        StringAssert.Contains("case BattleStepType.ExposureTick:", source);
        StringAssert.Contains("PlaySingleBleedAnimation(actor, step.Amount, isMyActor)", source);
        StringAssert.Contains("PlayBurnAnimation(actor, step.Amount, isMyActor)", source);
        StringAssert.Contains("PlayExposureAnimation(actor, step.Amount, false, isMyActor)", source);
    }

    [Test]
    public void BattleResultProcessor_GenericDamageBranch_DoesNotReplayDotTickDamage()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");
        StringAssert.Contains("bool isTickDamage =", source);
        StringAssert.Contains("string.Equals(step.Source, \"bleed\", StringComparison.OrdinalIgnoreCase)", source);
        StringAssert.Contains("string.Equals(step.Source, \"burn\", StringComparison.OrdinalIgnoreCase)", source);
        StringAssert.Contains("string.Equals(step.Source, \"exposure\", StringComparison.OrdinalIgnoreCase)", source);
        StringAssert.Contains("!isTickDamage", source);
    }

    [Test]
    public void BattleSubmitter_PollsViaGetBattleStatusNotDummyAttackZero()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleSubmitter.cs");
        StringAssert.Contains("serverFunctionsManager.GetBattleStatus(", source);
        Assert.IsFalse(source.Contains("attackId = 0"));
        Assert.IsFalse(source.Contains("attackSlot = 0"));
        Assert.IsFalse(source.Contains("dummySubmission"));
    }

    [Test]
    public void ServerFunctionsManager_ExposesGetBattleStatus()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Networking", "ServerFunctionsManager.cs");
        StringAssert.Contains("public void GetBattleStatus(", source);
        StringAssert.Contains("CallFunctionWithRetry(\"getBattleStatus\"", source);
    }

    [Test]
    public void MultiplayerCleanupManager_ResolvesServerFunctionsManagerLazily()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerCleanupManager.cs");
        StringAssert.Contains("ResolveServerFunctionsManager();", source);
        StringAssert.Contains("FindFirstObjectByType<ServerFunctionsManager>(FindObjectsInactive.Include)", source);
    }

    [Test]
    public void BattleSubmitter_DoesNotUseLegacyBattlePollingFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleSubmitter.cs");
        Assert.IsFalse(source.Contains("PollForBattleResultLegacy("));
        Assert.IsFalse(source.Contains("falling back to legacy battle polling"));
        Assert.IsFalse(source.Contains("switching to legacy GetBattleStatus polling"));
    }

    [Test]
    public void BattleRoundCoordinator_DoesNotUseLegacyNextTurnPollingFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleRoundCoordinator.cs");
        Assert.IsFalse(source.Contains("PollForNextTurnReadyLegacy("));
        Assert.IsFalse(source.Contains("switching to legacy CheckNextTurnReady polling"));
    }

    [Test]
    public void MultiplayerBoardManager_DoesNotFallbackToSelectedCardsPolling()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerBoardManager.cs");
        Assert.IsFalse(source.Contains("WaitForOpponentSelectionLegacyAsync"));
        Assert.IsFalse(source.Contains("falling back to getSelectedCards polling"));
    }

    [Test]
    public void MultiplayerService_EmergencyLeaveRoom_DoesNotPassNullCallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerService.cs");
        Assert.IsFalse(source.Contains("LeaveRoom(myPlayerId, null)"));
        StringAssert.Contains("LeaveRoom(myPlayerId);", source);
    }

    [Test]
    public void MultiplayerLobbyScene_DoesNotContainObsoleteMissingMatchmakingComponent()
    {
        string source = ReadProjectFile("Assets", "Scenes", "MultiplayerLobby.unity");
        Assert.IsFalse(source.Contains("guid: 5a48533f13544dc47b7d808bc555a419"));
        Assert.IsFalse(source.Contains("component: {fileID: 1347824464}"));
    }

    [Test]
    public void LegacyPlaceholderScripts_AreNotPresent()
    {
        string[] legacyPaths =
        {
            Path.Combine(Application.dataPath, "Scripts", "MultiplayerLobby.cs"),
            Path.Combine(Application.dataPath, "Scripts", "MultiplayerLobbyUI.cs"),
            Path.Combine(Application.dataPath, "Scripts", "MultiplayerMatchmakingUI.cs"),
            Path.Combine(Application.dataPath, "Scripts", "ServerFunctionsManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "MultiplayerNetworkManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "MultiplayerPlayerManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Multiplayer", "MultiplayerRoomManager.cs"),
            Path.Combine(Application.dataPath, "Scripts", "Album", "Animations", "Animations.cs"),
        };

        foreach (string path in legacyPaths)
        {
            Assert.IsFalse(File.Exists(path), $"Legacy placeholder script should not exist: {path}");
            Assert.IsFalse(File.Exists(path + ".meta"), $"Legacy placeholder meta should not exist: {path}.meta");
        }
    }

    private static string ReadProjectFile(params string[] relativeParts)
    {
        var allParts = new List<string> { Application.dataPath };
        allParts.AddRange(relativeParts[1..]);
        string fullPath = Path.Combine(allParts.ToArray());
        Assert.IsTrue(File.Exists(fullPath), $"Missing project file: {fullPath}");
        return File.ReadAllText(fullPath);
    }
}
