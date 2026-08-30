using System.Collections.Generic;
using System.IO;
using System;
using NUnit.Framework;
using UnityEngine;

public class BattleSharedDamagePlaybackTests
{
    [Test]
    public void BattleTimelinePlayback_DoesNotContainSharedDamageFallbackAfterHandlerExecution()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("yield return AttackRegistry.ExecuteOrFallback(attackId, attackContext);", source);
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
    public void BattleTimelinePlayback_UsesSharedStatPlaybackService()
    {
        string timelineSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string statSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleStatPlayback.cs");

        StringAssert.Contains("BattleStatPlayback.PlayTimelineStatChange(", timelineSource);
        StringAssert.Contains("public static IEnumerator PlayCardStatChanges(", statSource);
        Assert.IsFalse(timelineSource.Contains("GetStatApplier("));
        Assert.IsFalse(timelineSource.Contains("BattleStatApplier"));
    }

    [Test]
    public void BattleTimelinePlayback_UsesSharedEffectPlaybackService()
    {
        string timelineSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string effectSource = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");

        StringAssert.Contains("BattleEffectPlayback.AddEffectIconOnly(", timelineSource);
        StringAssert.Contains("BattleEffectPlayback.RemoveEffectIconOnly(", timelineSource);
        StringAssert.Contains("public static IEnumerator DisplayMultipleEffects(", effectSource);
        StringAssert.Contains("public static string GetEffectName(", effectSource);
        Assert.IsFalse(timelineSource.Contains("GetEffectVisuals("));
        Assert.IsFalse(timelineSource.Contains("BattleEffectVisuals"));
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
    public void SharedBattleEffectPlayback_DoesNotExposeOngoingMarkerEffectsAsIcons()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");
        StringAssert.Contains("case 20:", source);
        StringAssert.Contains("case 22:", source);
        StringAssert.Contains("case 23:", source);
        StringAssert.Contains("Online ongoing maneuvers are rendered from timeline ongoing-action steps", source);
        Assert.IsFalse(source.Contains("return \"Reloading\";"));
        Assert.IsFalse(source.Contains("return \"Trident\";"));
    }


    [Test]
    public void BattleTimelinePlayback_DotTickHandlersOwnTheirDamagePlayback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        StringAssert.Contains("case BattleStepType.BleedTick:", source);
        StringAssert.Contains("case BattleStepType.BurnTick:", source);
        StringAssert.Contains("case BattleStepType.ExposureTick:", source);
        StringAssert.Contains("PlaySingleBleedAnimation(context, actor, step.Amount, isPlayerActor)", source);
        StringAssert.Contains("PlayBurnAnimation(context, actor, step.Amount, isPlayerActor)", source);
        StringAssert.Contains("PlayExposureAnimation(context, actor, step.Amount, isPlayerActor)", source);
    }

    [Test]
    public void BattleTimelinePlayback_LogsEffectTickAnimationLifecycle()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        StringAssert.Contains("private static void LogEffectTickAnimation(", source);
        StringAssert.Contains("[EFFECT_TICK_ANIM]", source);
        StringAssert.Contains("LogEffectTickAnimation(\"start\", \"BleedTick\"", source);
        StringAssert.Contains("LogEffectTickAnimation(\"start\", \"BurnTick\"", source);
        StringAssert.Contains("LogEffectTickAnimation(\"start\", \"ExposureTick\"", source);
        StringAssert.Contains("LogEffectTickAnimation(\"start\", \"SatelliteTick\"", source);
        StringAssert.Contains("LogEffectTickAnimation(\"missing-animations\"", source);
        StringAssert.Contains("LogEffectTickAnimation(\"finish\"", source);
    }

    [Test]
    public void BattleTimelinePlayback_GenericDamageBranch_DoesNotReplayDotTickDamage()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
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

    [Test]
    public void MultiplayerCardAnimator_FloatingEffectAnimationsUseDetachedVisibleParent()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerCardAnimator.cs");

        StringAssert.Contains("Vector3 cardPosition = card.transform.position;", source);
        StringAssert.Contains("GameObject notsureTemplate = card.notsureGO;", source);
        StringAssert.Contains("ResolveDetachedEffectParent(card)", source);
        StringAssert.Contains("card.GetComponentInParent<Canvas>()", source);
        StringAssert.Contains("card.transform.parent", source);
        StringAssert.Contains("effectObject = Instantiate(notsureTemplate, effectParent, false);", source);
        StringAssert.Contains("effectObject = Instantiate(effectAnimationPrefab, effectParent, false);", source);
        Assert.IsFalse(source.Contains("Transform effectParent = transform;"));
        Assert.IsFalse(source.Contains("Instantiate(card.notsureGO, card.transform)"));
        Assert.IsFalse(source.Contains("Instantiate(effectAnimationPrefab, card.transform)"));
        Assert.IsFalse(source.Contains("(Vector2)card.transform.position + randomPosition"));
        Assert.IsFalse(source.Contains(" - card.transform.position"));
    }

    [Test]
    public void RoyalRumbleBattlePlayback_UsesMultiplayerBlockEffectAnimations()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("PlayBlockAnimation(context, actor, step.BlockedBy)", source);
        StringAssert.Contains("animations.PlayConfusionHurtItselfAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlaySleepAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlayElectricityAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlayTetherAnimation(card.transform)", source);
        StringAssert.Contains("animations.PlayBlocadeWaitAnimation(card.transform)", source);
        Assert.IsFalse(source.Contains("attack was blocked"));
    }

    [Test]
    public void RoyalRumbleBattlePlayback_UsesSharedEffectApplicationAndSelfRemoval()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("BattleEffectPlayback.AddEffectIconOnly(", source);
        StringAssert.Contains("BattleEffectPlayback.RemoveEffectIconOnly(", source);
        Assert.IsFalse(source.Contains("SyncEffectIcons(defender, attackerResult.effectsApplied)"));
        Assert.IsFalse(source.Contains("SyncEffectIcons(attacker, attackerResult.attackerEffectsApplied)"));
    }

    [Test]
    public void BattleResultDto_PreservesTimelineV2ForSharedPlayback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleContracts", "BattleContractDtos.cs");
        string builder = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelineBuilder.cs");

        StringAssert.Contains("public Dictionary<string, object> timelineV2;", source);
        StringAssert.Contains("TryBuild(BattleResultDto battleResult", builder);
    }

    [Test]
    public void RoyalRumbleBattlePlayback_UsesTimelineV2InsteadOfLegacyEffectFields()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattlePlayback.cs");

        StringAssert.Contains("BattleTimelineBuilder.TryBuild(envelope.battleResult", source);
        StringAssert.Contains("BattleTimelinePlayback.Play(", source);
        Assert.IsFalse(source.Contains("PlayPreAttackPassiveEffectsAsync"));
        Assert.IsFalse(source.Contains("attackerResult.bleedDamage"));
        Assert.IsFalse(source.Contains("attackerResult.exposureDamage"));
        Assert.IsFalse(source.Contains("attackerResult.exposureRemoved"));
    }

    [Test]
    public void SharedTimelinePlayback_CoversEffectLifecycleSteps()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");

        StringAssert.Contains("case BattleStepType.BleedTick:", source);
        StringAssert.Contains("case BattleStepType.BurnTick:", source);
        StringAssert.Contains("case BattleStepType.ExposureTick:", source);
        StringAssert.Contains("case BattleStepType.ExposureRemoved:", source);
        StringAssert.Contains("case BattleStepType.EffectApplied:", source);
        StringAssert.Contains("case BattleStepType.EffectRemoved:", source);
        StringAssert.Contains("case BattleStepType.Blocked:", source);
        StringAssert.Contains("case BattleStepType.OngoingActionProgress:", source);
        StringAssert.Contains("case BattleStepType.OngoingActionResolved:", source);
    }

    [Test]
    public void SharedTimelinePlayback_RemovesEndedEffectIconsAtTimelineStep()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleTimeline", "BattleTimelinePlayback.cs");
        string effects = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleEffectPlayback.cs");

        StringAssert.Contains("PlayWakeUpAnimation(context, actor, step.EffectType)", source);
        StringAssert.Contains("RemoveEndedEffectIcon(context, card, effectType, 3)", source);
        StringAssert.Contains("RemoveEndedEffectIcon(context, card, 2)", source);
        StringAssert.Contains("RemoveEndedEffectIcon(context, actor, 4)", source);
        StringAssert.Contains("RemoveEndedEffectIcon(context, target, step.EffectType)", source);
        StringAssert.Contains("animations.PlaySleepEndAnimation(card.transform)", effects);
        StringAssert.Contains("animations.PlayAscetismEndAnimation(card.transform)", effects);
        StringAssert.Contains("animations.PlayExposureEndAnimation(card.transform)", effects);
    }

    [Test]
    public void MultiplayerTimelinePilot_UsesSharedTimelinePlayback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Multiplayer", "BattleResultProcessor.cs");

        StringAssert.Contains("new BattleTimelinePlaybackContext", source);
        StringAssert.Contains("BattleTimelinePlayback.Play(context, steps)", source);
    }

    [Test]
    public void RoyalRumbleSnapshotEffectSync_DetachesRemovedIconsBeforeRepositioning()
    {
        string shell = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string coordinator = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattleCoordinator.cs");

        StringAssert.Contains("DestroyEffectIconBeforeReposition(icon);", shell);
        StringAssert.Contains("DestroyEffectIconBeforeReposition(icon);", coordinator);
        StringAssert.Contains("icon.SetParent(null, false);", shell);
        StringAssert.Contains("icon.SetParent(null, false);", coordinator);
    }

    [Test]
    public void RoyalRumbleShellRebuild_RendersReplacementHandOnlyOnce()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string rebuildMethod = ExtractMethodBlock(source, "private void RebuildShellView()");

        StringAssert.Contains("RestorePlayerActiveOrHand();", rebuildMethod);
        Assert.IsFalse(
            rebuildMethod.Contains("RenderPlayerHand();"),
            "RebuildShellView must not render the player hand directly; RestorePlayerActiveOrHand owns that decision."
        );
    }

    [Test]
    public void CampaignOnlineShell_RendersAndCleansEnemyHand()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");
        string renderMethod = ExtractMethodBlock(source, "private void RenderCampaignView()");
        string clearMethod = ExtractMethodBlock(source, "private void ClearRenderedCards()");

        StringAssert.Contains("private readonly List<Kard> renderedEnemyHandCards", source);
        StringAssert.Contains("RenderEnemyHand(selectedEnemy?.cardId);", renderMethod);
        StringAssert.Contains("private void RenderEnemyHand(string excludeCardId)", source);
        StringAssert.Contains("foreach (Kard card in renderedEnemyHandCards)", clearMethod);
        StringAssert.Contains("renderedEnemyHandCards.Clear();", clearMethod);
    }

    [Test]
    public void CampaignOnlineShell_UsesExplicitSceneReferencesWithoutRuntimeListenerReplacement()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");

        StringAssert.Contains("public bool HasRequiredSceneReferences(out string error)", source);
        StringAssert.Contains("public void ConfigureForMission(string configuredCampaignId, int configuredMissionId)", source);
        Assert.IsFalse(source.Contains("ReplaceSceneButtonListenersForOnlineCampaign"));
        Assert.IsFalse(source.Contains("FindButtonByName("));
        Assert.IsFalse(source.Contains("FindPlayers("));
        Assert.IsFalse(source.Contains("GameObject.Find("));
        Assert.IsFalse(source.Contains("gameObject.AddComponent<CampaignOnlineService>()"));
        Assert.IsFalse(source.Contains("gameObject.AddComponent<CampaignOnlineBattlePlayback>()"));
    }

    [Test]
    public void SingleplayerBattleSceneRouter_OwnsModeSelectionAndSharedInput()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Game", "SingleplayerBattleSceneRouter.cs");

        StringAssert.Contains("GameParameters.MissionID > 0", source);
        StringAssert.Contains("campaignShell.ConfigureForMission(", source);
        StringAssert.Contains("public void AttackButton(int attackSlot)", source);
        StringAssert.Contains("public void ConfirmAttackButton()", source);
        StringAssert.Contains("campaignShell?.AttackButton(attackSlot)", source);
        StringAssert.Contains("royalRumbleShell?.AttackButton(attackSlot)", source);
    }

    [Test]
    public void GameScene_RoutesAllSharedBattleInputThroughSingleplayerRouter()
    {
        string source = ReadProjectFile("Assets", "Scenes", "Game.unity");

        StringAssert.Contains("m_Name: SingleplayerBattleSceneRouter", source);
        StringAssert.Contains("m_Name: CampaignOnline", source);
        Assert.AreEqual(
            5,
            CountOccurrences(source, "m_TargetAssemblyTypeName: SingleplayerBattleSceneRouter, Assembly-CSharp"),
            "Four attack events and one confirm event must target the scene router."
        );
        Assert.IsFalse(source.Contains("m_TargetAssemblyTypeName: RoyalRumbleShellController, Assembly-CSharp"));
        Assert.IsFalse(source.Contains("m_TargetAssemblyTypeName: RoyalRumbleBattleCoordinator, Assembly-CSharp"));
    }

    [Test]
    public void LocalCampaignBattleAndProgressAuthority_IsRemoved()
    {
        string campaignDirectory = Path.Combine(Application.dataPath, "Scripts", "Campaign");
        string onlineDirectory = Path.Combine(campaignDirectory, "Online");

        Assert.IsFalse(File.Exists(Path.Combine(onlineDirectory, "CampaignOnlineGameBootstrap.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(campaignDirectory, "FightSystemCampaign.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(campaignDirectory, "CampaignManager.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(Application.dataPath, "Scripts", "FightSystem.cs")));
    }

    [Test]
    public void LegacyFightSystemAndDragKardRuntime_IsRemoved()
    {
        string scriptsDirectory = Path.Combine(Application.dataPath, "Scripts");
        string multiplayerScene = ReadProjectFile("Assets", "Scenes", "Multiplayer.unity");
        string cardPrefab = ReadProjectFile("Assets", "Prefabs", "Kard.prefab");
        string attackDescriptions = ReadProjectFile("Assets", "Scripts", "AttackDescriptions.cs");
        string rrShell = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string rrCoordinator = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattleCoordinator.cs");
        string campaignShell = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");
        string multiplayerHand = ReadProjectFile("Assets", "Scripts", "Multiplayer", "MultiplayerHandManager.cs");

        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "FightSystem.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "FightSystem.cs.meta")));
        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "DragKard.cs")));
        Assert.IsFalse(File.Exists(Path.Combine(scriptsDirectory, "DragKard.cs.meta")));

        Assert.IsFalse(multiplayerScene.Contains("FightSystem, Assembly-CSharp"));
        Assert.IsFalse(multiplayerScene.Contains("5e54471e2c9ac384396981d6824289d5"));
        Assert.IsFalse(cardPrefab.Contains("4892ce4360c797f419cc643828d8ec67"));
        Assert.IsFalse(attackDescriptions.Contains("FightSystem"));
        Assert.IsFalse(rrShell.Contains("DragKard"));
        Assert.IsFalse(rrCoordinator.Contains("DragKard"));
        Assert.IsFalse(campaignShell.Contains("DragKard"));
        Assert.IsFalse(multiplayerHand.Contains("DragKard"));
    }

    [Test]
    public void CampaignOnlineShell_RefreshesReplacementHandDraggabilityAfterBusyStateClears()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "Online", "CampaignOnlineShellController.cs");
        string updateMethod = ExtractMethodBlock(source, "private void UpdateAttackButtons()");
        string refreshMethod = ExtractMethodBlock(source, "private void RefreshPlayerHandDraggability()");

        StringAssert.Contains("RefreshPlayerHandDraggability();", updateMethod);
        StringAssert.Contains("foreach (Kard card in renderedPlayerHandCards)", refreshMethod);
        StringAssert.Contains("card.isDragable = CanDragCard(card);", refreshMethod);
    }

    [Test]
    public void CampaignMissionManager_LoadsServerAuthoritativeProgressWithoutLocalFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "Campaign", "MissionManager.cs");

        StringAssert.Contains("private async void Start()", source);
        Assert.IsFalse(source.Contains("private async void OnEnable()"));
        StringAssert.Contains("campaignService.GetProgressAsync(playerId, campaignId)", source);
        StringAssert.Contains("ApplyLevelAccess(envelope.progress.highestUnlockedMissionId);", source);
        StringAssert.Contains("campaignContentRoot", source);
        StringAssert.Contains("SetCampaignContentVisible(false);", source);
        StringAssert.Contains("SceneLoadingOverlay.SetMessage(\"LOADING...\");", source);
        StringAssert.Contains("SceneLoadingOverlay.Show();", source);
        StringAssert.Contains("SceneLoadingOverlay.Hide();", source);
        StringAssert.Contains("SetCampaignContentVisible(true);", source);
        Assert.IsFalse(source.Contains("CampaignManager.Instance.LoadCampaignData"));
    }

    [Test]
    public void RoyalRumbleShell_DoesNotMaintainLocalOngoingFallback()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");

        Assert.IsFalse(
            source.Contains("localPendingOngoingAction"),
            "RR shell must not keep local pending ongoing state; server session ongoingActions are authoritative."
        );
        Assert.IsFalse(
            source.Contains("UpdateLocalPendingOngoingFallback"),
            "RR shell must not infer ongoing state from battle results as a fallback."
        );
        Assert.IsFalse(
            source.Contains("TryCreateStartedOngoingFallback"),
            "RR shell must not synthesize started ongoing actions locally."
        );
        Assert.IsFalse(
            source.Contains("Created local pending ongoing fallback"),
            "RR shell must not log or create local ongoing fallback state."
        );
    }

    [Test]
    public void RoyalRumbleShell_PendingOngoingReadsCanonicalSessionStateWithoutAdapterComponent()
    {
        string source = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string pendingMethod = ExtractMethodBlock(source, "private PendingOngoingActionTurnData GetPendingOngoingAction()");

        StringAssert.Contains("RoyalRumbleTurnAdapter.CreatePendingOngoingAction(selected)", pendingMethod);
        Assert.IsFalse(
            pendingMethod.Contains("turnAdapter != null"),
            "RR shell pending ongoing detection must not depend on an optional scene adapter component."
        );
    }

    [Test]
    public void RoyalRumbleClients_DoNotSelectEnemyCardLocallyAsFallback()
    {
        string shell = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleShellController.cs");
        string coordinator = ReadProjectFile("Assets", "Scripts", "RoyalRumble", "RoyalRumbleBattleCoordinator.cs");

        Assert.IsFalse(
            shell.Contains("Falling back locally"),
            "RR shell must not locally choose an enemy card when server active enemy state is missing."
        );
        Assert.IsFalse(
            coordinator.Contains("Falling back locally"),
            "RR coordinator must not locally choose an enemy card when server active enemy state is missing."
        );
        Assert.IsFalse(
            shell.Contains("SetActiveEnemyCardId("),
            "RR shell must not mutate active enemy state locally."
        );
        Assert.IsFalse(
            coordinator.Contains("SetActiveEnemyCardId("),
            "RR coordinator must not mutate active enemy state locally."
        );
    }

    private static string ReadProjectFile(params string[] relativeParts)
    {
        var allParts = new List<string> { Application.dataPath };
        allParts.AddRange(relativeParts[1..]);
        string fullPath = Path.Combine(allParts.ToArray());
        Assert.IsTrue(File.Exists(fullPath), $"Missing project file: {fullPath}");
        return File.ReadAllText(fullPath);
    }

    private static string ExtractMethodBlock(string source, string signature)
    {
        int start = source.IndexOf(signature, StringComparison.Ordinal);
        Assert.GreaterOrEqual(start, 0, $"Missing method signature: {signature}");

        int bodyStart = source.IndexOf('{', start);
        Assert.GreaterOrEqual(bodyStart, 0, $"Missing method body for: {signature}");

        int depth = 0;
        for (int i = bodyStart; i < source.Length; i++)
        {
            if (source[i] == '{')
            {
                depth++;
            }
            else if (source[i] == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return source.Substring(start, i - start + 1);
                }
            }
        }

        Assert.Fail($"Unterminated method body for: {signature}");
        return string.Empty;
    }

    private static int CountOccurrences(string source, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}
