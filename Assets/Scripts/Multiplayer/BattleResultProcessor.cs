using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

/// <summary>
/// Stat changes pre jeden Astok (buffs na AstoATnAka + debuffs na obrancu)
/// </summary>
public struct AttackStatChanges
{
    // Self-buffs on attacker (e.g. WaterToWine)
    public int attackerAttack;
    public int attackerStrength;
    public int attackerDefense;
    public int attackerKnowledge;
    public int attackerSpeed;
    public int attackerCharisma;

    // Debuffs on defender (e.g. Crusade, ScientificLecture)
    public int defenderAttack;
    public int defenderStrength;
    public int defenderDefense;
    public int defenderKnowledge;
    public int defenderSpeed;
    public int defenderCharisma;

    public static AttackStatChanges Zero => new AttackStatChanges();
}

/// <summary>
/// ZodpovednA za spracovanie battle vAsledkov zo servera
/// Aplikuje HP zmeny, hrA animAcie a urATuje vALAaza
/// V5: BattleResult identifikuje karty cez cardId namiesto player1/player2
/// </summary>
public partial class BattleResultProcessor : MonoBehaviour
{
    private static bool VerboseBattleLogs => false;

    [Header("Dependencies")]
    public FightSystemMultiplayer fightSystem;
    public Attack attackComponent;
    public MultiplayerService multiplayerService; // For refreshing selectedCards
    public MultiplayerCardAnimator cardAnimator; // NEW: Card animations (damage, stats, shake)
    public MultiplayerKillCounterManager killCounterManager; // NEW: Kill counter tracking
    private BattleRoundCoordinator roundCoordinator;
    

    [Header("UI References")]
    public TMP_Text dialogText;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;

    private static void LogVerboseBattle(string message)
    {
        if (VerboseBattleLogs)
        {
            Debug.Log(message);
        }
    }

    private static void LogVerboseBattleWarning(string message)
    {
        if (VerboseBattleLogs)
        {
            Debug.LogWarning(message);
        }
    }

    private void Start()
    {
        // ValidAcia required referenciA
        if (multiplayerService == null)
        {
            Debug.LogError(
                "[BattleResultProcessor] MultiplayerService not assigned! Please set in Inspector."
            );
        }

        if (cardAnimator == null)
        {
            Debug.LogError(
                "[BattleResultProcessor] MultiplayerCardAnimator not assigned! Card animations will be skipped. Please set in Inspector."
            );
        }

        if (attackComponent == null)
        {
            Debug.LogError(
                "[BattleResultProcessor] CRITICAL: attackComponent not assigned! Sleep animations and effects will NOT work! Please set in Inspector."
            );
        }
        roundCoordinator = new BattleRoundCoordinator(
            fightSystem,
            multiplayerService,
            killCounterManager,
            dialogText
        );
    }

    /// <summary>
    /// Spracuje vAsledok battle a spustA animAcie
    /// V5: BattleResult identifikuje karty cez cardId, HP sa naATAta z selectedCards
    /// </summary>
    public void ProcessBattleResult(Dictionary<string, object> battleResult)
    {
        LogVerboseBattleWarning("[BATTLE_RESULT] ===== RAW SERVER RESPONSE ===== ");
        foreach (var kvp in battleResult)
        {
            LogVerboseBattleWarning($"[BATTLE_RESULT] {kvp.Key}: {kvp.Value}");
        }
        LogVerboseBattleWarning("[BATTLE_RESULT] ================================ ");

        Debug.Log($"[BattleResultProcessor] Processing battle result (V5)");

        // ZAskaj karty
        Kard myCard = fightSystem.player?.cardInGame;
        Kard enemyCard = fightSystem.enemy?.cardInGame;

        if (myCard == null || enemyCard == null)
        {
            Debug.LogError("[BattleResultProcessor] Cannot apply result - cards not found");
            return;
        }

        Debug.LogWarning(
            $"[CARDS] MY: {myCard.cardName} (cardId={myCard.cardId}, HP={myCard.health}/{myCard.maxHealth})"
        );
        Debug.LogWarning(
            $"[CARDS] ENEMY: {enemyCard.cardName} (cardId={enemyCard.cardId}, HP={enemyCard.health}/{enemyCard.maxHealth})"
        );

        // V11: Parse NEW ID-based response format (firstAttacker, secondAttacker)
        if (
            !BattleResultParser.TryParse(
                battleResult,
                myCard.cardId,
                out var parsed,
                out var parseError
            )
        )
        {
            Debug.LogError($"[BattleResultProcessor] Parse failed: {parseError}");
            return;
        }

        string firstAttackerCardId = parsed.FirstAttackerCardId;
        string secondAttackerCardId = parsed.SecondAttackerCardId;

        string myCardId = myCard.cardId;
        string enemyCardId = enemyCard.cardId;

        bool iAmFirstAttacker = parsed.IAmFirstAttacker;

        Debug.LogWarning($"[ROLE] I am {(iAmFirstAttacker ? "FIRST" : "SECOND")} attacker");
        Debug.LogWarning(
            $"[ROLE] FirstAttacker={firstAttackerCardId}, SecondAttacker={secondAttackerCardId}"
        );

        int myDamage = parsed.MyDamage;
        int enemyDamage = parsed.EnemyDamage;

        int myAttackId = parsed.MyAttackId;
        int enemyAttackId = parsed.EnemyAttackId;

        int myHealAmount = parsed.MyHealAmount;
        int enemyHealAmount = parsed.EnemyHealAmount;

        string myAttackResult = parsed.MyAttackResult;
        string enemyAttackResult = parsed.EnemyAttackResult;

        AttackStatChanges myStatChanges = parsed.MyStatChanges;
        AttackStatChanges enemyStatChanges = parsed.EnemyStatChanges;

        Debug.LogWarning(
            $"[MY_STATS] Attacker buffs: ATK={myStatChanges.attackerAttack} STR={myStatChanges.attackerStrength} DEF={myStatChanges.attackerDefense} KNO={myStatChanges.attackerKnowledge} SPD={myStatChanges.attackerSpeed} CHA={myStatChanges.attackerCharisma}"
        );
        Debug.LogWarning(
            $"[MY_STATS] Defender debuffs: ATK={myStatChanges.defenderAttack} STR={myStatChanges.defenderStrength} DEF={myStatChanges.defenderDefense} KNO={myStatChanges.defenderKnowledge} SPD={myStatChanges.defenderSpeed} CHA={myStatChanges.defenderCharisma}"
        );
        Debug.LogWarning(
            $"[ENEMY_STATS] Attacker buffs: ATK={enemyStatChanges.attackerAttack} STR={enemyStatChanges.attackerStrength} DEF={enemyStatChanges.attackerDefense} KNO={enemyStatChanges.attackerKnowledge} SPD={enemyStatChanges.attackerSpeed} CHA={enemyStatChanges.attackerCharisma}"
        );
        Debug.LogWarning(
            $"[ENEMY_STATS] Defender debuffs: ATK={enemyStatChanges.defenderAttack} STR={enemyStatChanges.defenderStrength} DEF={enemyStatChanges.defenderDefense} KNO={enemyStatChanges.defenderKnowledge} SPD={enemyStatChanges.defenderSpeed} CHA={enemyStatChanges.defenderCharisma}"
        );

        Dictionary<string, object> myEffectApplied = parsed.MyEffectApplied;
        Dictionary<string, object> enemyEffectApplied = parsed.EnemyEffectApplied;

        if (myEffectApplied != null)
        {
            Debug.LogWarning(
                $"[EFFECT] MY card APPLIED effect to enemy: type={myEffectApplied["type"]}, duration={myEffectApplied["duration"]}"
            );
        }

        if (enemyEffectApplied != null)
        {
            Debug.LogWarning(
                $"[EFFECT] ENEMY card APPLIED effect to me: type={enemyEffectApplied["type"]}, duration={enemyEffectApplied["duration"]}"
            );
        }

        List<Dictionary<string, object>> myEffectsApplied = parsed.MyEffectsApplied;
        List<Dictionary<string, object>> enemyEffectsApplied = parsed.EnemyEffectsApplied;

        List<Dictionary<string, object>> myAttackerEffects = parsed.MyAttackerEffects;
        List<Dictionary<string, object>> enemyAttackerEffects = parsed.EnemyAttackerEffects;

        int myAttackerSelfDamage = parsed.MyAttackerSelfDamage;
        int enemyAttackerSelfDamage = parsed.EnemyAttackerSelfDamage;

        if (myAttackerSelfDamage > 0)
        {
            Debug.LogWarning($"[SELF-DAMAGE] MY card takes {myAttackerSelfDamage} self-damage!");
        }
        if (enemyAttackerSelfDamage > 0)
        {
            Debug.LogWarning(
                $"[SELF-DAMAGE] ENEMY card takes {enemyAttackerSelfDamage} self-damage!"
            );
        }

        bool myAttackBlocked = parsed.MyAttackBlocked;
        bool enemyAttackBlocked = parsed.EnemyAttackBlocked;
        bool myWokeUp = parsed.MyWokeUp;
        bool enemyWokeUp = parsed.EnemyWokeUp;

        bool myRecovered = parsed.MyRecovered;
        bool enemyRecovered = parsed.EnemyRecovered;

        int mySelfDamage = parsed.MySelfDamage;
        int enemySelfDamage = parsed.EnemySelfDamage;

        int myBleedDamage = parsed.MyBleedDamage;
        int enemyBleedDamage = parsed.EnemyBleedDamage;

        List<int> myBleedDamages = parsed.MyBleedDamages;
        List<int> enemyBleedDamages = parsed.EnemyBleedDamages;

        int myExposureDamage = parsed.MyExposureDamage;
        int enemyExposureDamage = parsed.EnemyExposureDamage;

        bool myExposureRemoved = parsed.MyExposureRemoved;
        bool enemyExposureRemoved = parsed.EnemyExposureRemoved;

        int? myBlockedBy = parsed.MyBlockedBy;
        int? enemyBlockedBy = parsed.EnemyBlockedBy;

        if (myAttackBlocked)
        {
            string effectName = BattleEffectPlayback.GetEffectName(myBlockedBy ?? 0);
            Debug.LogWarning(
                $"[BLOCK] MY attack BLOCKED by {effectName}! SelfDamage={mySelfDamage}"
            );
        }
        if (enemyAttackBlocked)
        {
            string effectName = BattleEffectPlayback.GetEffectName(enemyBlockedBy ?? 0);
            Debug.LogWarning(
                $"[BLOCK] ENEMY attack BLOCKED by {effectName}! SelfDamage={enemySelfDamage}"
            );
        }
        if (myWokeUp)
        {
            Debug.LogWarning($"[SLEEP] MY card WOKE UP from Sleep! Attack executed.");
        }
        if (enemyWokeUp)
        {
            Debug.LogWarning($"[SLEEP] ENEMY card WOKE UP from Sleep!");
        }
        if (myRecovered)
        {
            Debug.LogWarning(
                $"[ASCETICISM] MY card RECOVERED from Asceticism! Feels blessed again."
            );
        }
        if (enemyRecovered)
        {
            Debug.LogWarning($"[ASCETICISM] ENEMY card RECOVERED from Asceticism!");
        }

        Debug.LogWarning(
            $"[BattleResultProcessor] MyAttackId={myAttackId}, MyDamageReceived={myDamage}, MyHeal={myHealAmount}, MySelfDamage={mySelfDamage}, MyBleedDamage={myBleedDamage}, MyBleedCount={myBleedDamages.Count}, MyExposureDamage={myExposureDamage}, MyExposureRemoved={myExposureRemoved}, MyAttackerSelfDamage={myAttackerSelfDamage}, MyBlocked={myAttackBlocked}, MyBlockedBy={myBlockedBy}, MyWokeUp={myWokeUp}, MyRecovered={myRecovered}, MyEffects={myEffectsApplied.Count}, MySelfEffects={myAttackerEffects.Count}, EnemyAttackId={enemyAttackId}, EnemyDamageReceived={enemyDamage}, EnemyHeal={enemyHealAmount}, EnemySelfDamage={enemySelfDamage}, EnemyBleedDamage={enemyBleedDamage}, EnemyBleedCount={enemyBleedDamages.Count}, EnemyExposureDamage={enemyExposureDamage}, EnemyExposureRemoved={enemyExposureRemoved}, EnemyAttackerSelfDamage={enemyAttackerSelfDamage}, EnemyBlocked={enemyAttackBlocked}, EnemyBlockedBy={enemyBlockedBy}, EnemyWokeUp={enemyWokeUp}, EnemyRecovered={enemyRecovered}, EnemyEffects={enemyEffectsApplied.Count}, EnemySelfEffects={enemyAttackerEffects.Count}"
        );

        // ? Spusti animacie (HP sa updatne postupne!)
        // ? REFRESH selectedCards sa spusti AZ PO animaciach
        // ? V11: firstAttacker replaced with firstAttackerCardId (clear ID-based role)
        // ? V11.1: Added effectsApplied arrays + attackerSelfDamage for AoE/recoil attacks
        // ? V11.2: Added bleedDamages arrays for individual Bleed animations + exposureDamage/exposureRemoved
        // ? V12: Added stat changes structs for clean parameter passing
        // M10: TimelineV2 only. Missing or invalid timeline payload must fail fast.
        if (BattleTimelinePilotPolicy.ShouldRunTimeline(battleResult, parsed, out var timelineMode))
        {
            if (
                BattleTimelineBuilder.TryBuild(
                    battleResult,
                    out var timelineSteps,
                    out var timelineError
                )
            )
            {
                Debug.Log(
                    $"[TimelinePilot] Enabled for this turn, mode={timelineMode}, steps={timelineSteps.Count}"
                );
                StartCoroutine(
                    PlayTimelinePilotAndRefresh(
                        myCard,
                        enemyCard,
                        myCardId,
                        enemyCardId,
                        timelineSteps
                    )
                );
                return;
            }

            Debug.LogError(
                $"[TimelinePilot] TimelineV2 build failed (mode={timelineMode}). Legacy playback is disabled. Error: {timelineError}"
            );
            StartCoroutine(ShowDialog("TimelineV2 build error. Legacy playback disabled."));
            return;
        }

        Debug.LogError(
            $"[TimelinePilot] TimelineV2 unavailable (mode={timelineMode}). Legacy playback is disabled."
        );
        StartCoroutine(
            ShowDialog($"TimelineV2 unavailable ({timelineMode}). Legacy playback disabled.")
        );
        return;
    }

    private IEnumerator PlayTimelinePilotAndRefresh(
        Kard myCard,
        Kard enemyCard,
        string myCardId,
        string enemyCardId,
        List<BattleStep> steps
    )
    {
        yield return StartCoroutine(
            PlayTimelinePilot(myCard, enemyCard, myCardId, enemyCardId, steps)
        );
        yield return StartCoroutine(RefreshCardsFromServer());
    }

    private IEnumerator PlayTimelinePilot(
        Kard myCard,
        Kard enemyCard,
        string myCardId,
        string enemyCardId,
        List<BattleStep> steps
    )
    {
        int executedAttacks = 0;

        for (int i = 0; i < steps.Count; i++)
        {
            BattleStep step = steps[i];
            if (step == null)
            {
                continue;
            }

            Kard actor = GetCardById(step.ActorCardId, myCard, enemyCard, myCardId, enemyCardId);
            Kard target = GetCardById(step.TargetCardId, myCard, enemyCard, myCardId, enemyCardId);
            bool isMyActor = actor != null && actor.cardId == myCardId;

            switch (step.Type)
            {
                case BattleStepType.BleedTick:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return StartCoroutine(
                            PlaySingleBleedAnimation(actor, step.Amount, isMyActor)
                        );
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;

                case BattleStepType.BurnTick:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return StartCoroutine(
                            PlayBurnAnimation(actor, step.Amount, isMyActor)
                        );
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;

                case BattleStepType.ExposureTick:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return StartCoroutine(
                            PlayExposureAnimation(actor, step.Amount, false, isMyActor)
                        );
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;

                case BattleStepType.ExposureRemoved:
                    if (actor != null)
                    {
                        yield return StartCoroutine(
                            PlayExposureAnimation(actor, 0, true, isMyActor)
                        );
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;

                case BattleStepType.Recovery:
                    if (actor != null)
                    {
                        yield return StartCoroutine(
                            PlayRecoveryAnimation(actor, isMyActor ? "My card" : "Enemy card")
                        );
                        yield return new WaitForSeconds(0.5f);
                    }
                    break;

                case BattleStepType.WakeUp:
                    if (actor != null)
                    {
                        yield return StartCoroutine(PlayWakeUpAnimation(actor, isMyActor));
                    }
                    break;

                case BattleStepType.Attack:
                    if (actor == null || target == null)
                    {
                        break;
                    }

                    if (step.Skipped || step.Blocked)
                    {
                        Debug.Log(
                            $"[TimelinePilot] Skipping attackId={step.AttackId}, actor={step.ActorCardId}, skipped={step.Skipped}, blocked={step.Blocked}"
                        );
                        break;
                    }

                    if (actor.health <= 0 || target.health <= 0)
                    {
                        break;
                    }

                    if (executedAttacks > 0)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }

                    if (dialogText != null)
                    {
                        dialogText.color = isMyActor ? Color.blue : Color.red;
                    }

                    int damage = GetAttackDamageFromTimeline(
                        steps,
                        i,
                        step.ActorCardId,
                        step.TargetCardId
                    );
                    int heal = GetAttackHealFromTimeline(steps, i, step.ActorCardId);

                    yield return StartCoroutine(
                        ExecuteAttackAnimation(
                            actor,
                            target,
                            step.AttackId,
                            damage,
                            heal,
                            isMyActor,
                            0,
                            step.EffectsApplied,
                            step.AttackResult,
                            step.AttackerEffectsApplied
                        )
                    );
                    executedAttacks++;
                    break;

                case BattleStepType.StatChange:
                    if (target != null && !string.IsNullOrEmpty(step.StatName) && step.Amount != 0)
                    {
                        Debug.Log(
                            $"[TimelinePilot] Applying stat change: target={target.cardName}, stat={step.StatName}, amount={step.Amount}"
                        );
                        yield return StartCoroutine(
                            BattleStatPlayback.PlayTimelineStatChange(
                                target,
                                step.Amount,
                                step.StatName,
                                cardAnimator
                            )
                        );
                    }
                    break;

                case BattleStepType.Blocked:
                    if (actor != null)
                    {
                        yield return StartCoroutine(
                            PlayBlockAnimation(actor, step.BlockedBy, isMyActor)
                        );
                    }
                    break;

                case BattleStepType.SelfDamage:
                    if (actor != null && step.Amount > 0)
                    {
                        yield return StartCoroutine(
                            PlaySelfDamageAnimation(actor, step.Amount, isMyActor)
                        );
                    }
                    break;

                case BattleStepType.EffectApplied:
                    if (target != null)
                    {
                        var effectData = new Dictionary<string, object>
                        {
                            { "type", step.EffectType },
                            { "duration", step.Duration },
                            { "source", step.Source },
                        };
                        yield return StartCoroutine(
                            BattleEffectPlayback.AddEffectIconOnly(target, effectData, target.cardId == myCardId)
                        );
                    }
                    break;

                case BattleStepType.EffectRemoved:
                    if (target != null)
                    {
                        yield return StartCoroutine(
                            BattleEffectPlayback.RemoveEffectIconOnly(
                                target,
                                step.EffectType,
                                target.cardId == myCardId,
                                attackComponent
                            )
                        );
                        if (step.EffectType == 7)
                        {
                            yield return StartCoroutine(ShowDialog($"{target.cardName} is starving no more"));
                        }
                        yield return new WaitForSeconds(0.2f);
                    }
                    break;

                case BattleStepType.SatelliteTick:
                    if (actor != null)
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null)
                        {
                            yield return StartCoroutine(
                                animations.PlaySatelliteAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }

                        yield return new WaitForSeconds(0.2f);
                    }
                    break;

                case BattleStepType.OngoingActionStarted:
                    Debug.Log(
                        $"[TimelinePilot] Ongoing action started: type={step.ActionType}, actor={step.ActorCardId}, target={step.TargetCardId}, turnsRemaining={step.TurnsRemaining}"
                    );
                    break;

                case BattleStepType.OngoingActionProgress:
                    if (string.Equals(step.ActionType, "siege", StringComparison.OrdinalIgnoreCase))
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlaySiegeContinueAnimation(actor.transform, target.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "doubleEnvelopment",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayDoubleEnvelopmentWaitAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "artInspiration",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayArtInspirationWaitAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "autoportrait",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayAutoportraitAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "buffaloHorns",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayBuffaloHornsContinueAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "flintlockPistol",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayFlintlockPistolLoadingAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "trident",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayTridentAimAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    break;

                case BattleStepType.OngoingActionResolved:
                    if (string.Equals(step.ActionType, "siege", StringComparison.OrdinalIgnoreCase))
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlaySiegeEndAnimation(target.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "doubleEnvelopment",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayDoubleEnvelopAttackAnimation(target.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "artInspiration",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayArtInspirationEndAnimation(actor.transform)
                            );
                            yield return StartCoroutine(
                                ShowDialog($"{actor.cardName} finished his creation")
                            );
                        }

                        if (animations != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayArtInspirationEndEnemyAnimation(target.transform)
                            );
                            yield return StartCoroutine(
                                ShowDialog($"{target.cardName} is impressed by masterpiece")
                            );
                        }

                        if (animations != null && actor != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayArtInspirationEndAttackAnimation(actor.transform, target.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "autoportrait",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayAutoportraitFinishAnimation(actor.transform)
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "buffaloHorns",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayBuffaloHornsEndAnimation(
                                    actor.transform,
                                    target.transform
                                )
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "flintlockPistol",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        bool hit = string.Equals(
                            step.AttackResult,
                            "hit",
                            StringComparison.OrdinalIgnoreCase
                        );
                        if (animations != null && actor != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayFlintlockPistolShotAnimation(
                                    actor.transform,
                                    target.transform,
                                    hit
                                )
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    else if (
                        string.Equals(
                            step.ActionType,
                            "trident",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null && actor != null && target != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayTridentHitAnimation(
                                    actor.transform,
                                    target.transform
                                )
                            );
                        }

                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    break;

                case BattleStepType.OngoingActionCancelled:
                    if (!string.IsNullOrEmpty(step.Note))
                    {
                        yield return StartCoroutine(ShowDialog(step.Note));
                    }
                    break;

                case BattleStepType.Damage:
                    if (target != null)
                    {
                        bool isTickDamage =
                            string.Equals(step.Source, "bleed", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(step.Source, "burn", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(step.Source, "exposure", StringComparison.OrdinalIgnoreCase);

                        if (
                            !isTickDamage
                            && !string.Equals(step.Source, "attack", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            bool isMyTarget = target.cardId == myCardId;
                            yield return StartCoroutine(
                                BattleValuePlayback.PlayDamage(
                                    target,
                                    step.Amount,
                                    isMyTarget,
                                    cardAnimator,
                                    playerLifeBar,
                                    enemyLifeBar
                                )
                            );
                        }
                    }
                    break;

                case BattleStepType.Heal:
                    if (
                        target != null
                        && string.Equals(step.Source, "famine", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        bool isMyTarget = target.cardId == myCardId;
                        AttackAnimations animations = attackComponent?.attackAnimations;
                        if (animations != null)
                        {
                            yield return StartCoroutine(
                                animations.PlayFamineContinueAnimation(target.transform)
                            );
                        }
                        yield return StartCoroutine(
                            BattleValuePlayback.PlayHeal(
                                target,
                                step.Amount,
                                isMyTarget,
                                cardAnimator,
                                playerLifeBar,
                                enemyLifeBar
                            )
                        );
                        if (!string.IsNullOrEmpty(step.Note))
                        {
                            yield return StartCoroutine(ShowDialog(step.Note));
                        }
                    }
                    break;
                case BattleStepType.Death:
                default:
                    break;
            }
        }

        if (dialogText != null)
        {
            dialogText.color = Color.black;
        }

        yield return StartCoroutine(ShowDialog("Preparing next turn..."));

        if (cardAnimator != null)
        {
            Vector3 playerBoardPos =
                fightSystem.playerBoard != null
                    ? fightSystem.playerBoard.transform.position
                    : myCard.transform.position;
            Vector3 enemyBoardPos =
                fightSystem.enemyBoard != null
                    ? fightSystem.enemyBoard.transform.position
                    : enemyCard.transform.position;

            StartCoroutine(
                cardAnimator.ResetCardPosition(myCard, playerBoardPos, Quaternion.identity)
            );
            StartCoroutine(
                cardAnimator.ResetCardPosition(enemyCard, enemyBoardPos, Quaternion.identity)
            );
        }

        yield return new WaitForSeconds(1f);
        CheckBattleOutcome(myCard, enemyCard);
    }

    private static int GetAttackDamageFromTimeline(
        List<BattleStep> steps,
        int attackIndex,
        string attackerCardId,
        string defenderCardId
    )
    {
        for (int i = attackIndex + 1; i < steps.Count; i++)
        {
            BattleStep next = steps[i];
            if (next.Type == BattleStepType.Attack)
            {
                break;
            }

            if (
                next.Type == BattleStepType.Damage
                && next.Source == "attack"
                && next.ActorCardId == attackerCardId
                && next.TargetCardId == defenderCardId
            )
            {
                return next.Amount;
            }
        }

        return 0;
    }

    private static int GetAttackHealFromTimeline(
        List<BattleStep> steps,
        int attackIndex,
        string attackerCardId
    )
    {
        for (int i = attackIndex + 1; i < steps.Count; i++)
        {
            BattleStep next = steps[i];
            if (next.Type == BattleStepType.Attack)
            {
                break;
            }

            if (
                next.Type == BattleStepType.Heal
                && next.ActorCardId == attackerCardId
                && next.TargetCardId == attackerCardId
            )
            {
                return next.Amount;
            }
        }

        return 0;
    }

    private static Kard GetCardById(
        string cardId,
        Kard myCard,
        Kard enemyCard,
        string myCardId,
        string enemyCardId
    )
    {
        if (cardId == myCardId)
        {
            return myCard;
        }

        if (cardId == enemyCardId)
        {
            return enemyCard;
        }

        return null;
    }

    /// <summary>
    /// PrehrA battle animAcie s postupnAm HP updateom
    /// V5: PouLlAva cardId na urATenie kto AstoATil prvA
    /// V8: PridanA attackId pre dynamickA animAcie (reuse Attack.cs metAd)
    /// V9: PridanA healAmount pre self-heal animAcie + Sleep blocking (blocked/wokeUp flags) + effect ikony v sprAvnom poradA + blockedBy field
    /// V10: PridanA recovered/selfDamage pre Asceticism effect
    /// V11.1: Multiple effects arrays + attackerSelfDamage/attackerEffectsApplied pre AoE/recoil attacks
    /// V11.2: PridanA bleedDamage pre Bleed effect processing (Priority 1)
    /// </summary>
    private IEnumerator PlayBattleAnimations(
        Kard myCard,
        Kard enemyCard,
        string firstAttackerCardId,
        string myCardId,
        string enemyCardId,
        int myAttackId,
        int enemyAttackId,
        int myDamage,
        int enemyDamage,
        int myHealAmount,
        int enemyHealAmount,
        AttackStatChanges myStatChanges,
        AttackStatChanges enemyStatChanges,
        List<Dictionary<string, object>> myEffectsApplied,
        List<Dictionary<string, object>> enemyEffectsApplied,
        List<Dictionary<string, object>> myAttackerEffects,
        List<Dictionary<string, object>> enemyAttackerEffects,
        int myAttackerSelfDamage,
        int enemyAttackerSelfDamage,
        bool myAttackBlocked,
        bool enemyAttackBlocked,
        bool myWokeUp,
        bool enemyWokeUp,
        bool myRecovered,
        bool enemyRecovered,
        int mySelfDamage,
        int enemySelfDamage,
        int? myBlockedBy,
        int? enemyBlockedBy,
        List<int> myBleedDamages,
        List<int> enemyBleedDamages,
        int myExposureDamage,
        bool myExposureRemoved,
        int enemyExposureDamage,
        bool enemyExposureRemoved,
        string myAttackResult,
        string enemyAttackResult
    )
    {
        bool iAttackedFirst = (firstAttackerCardId == myCardId);

        Debug.LogWarning(
            $"[PlayBattleAnimations] FirstAttackerCardId={firstAttackerCardId}, MyCardId={myCardId}, IAttackedFirst={iAttackedFirst}"
        );
        Debug.LogWarning(
            $"[PlayBattleAnimations] MyAttackId={myAttackId}, EnemyAttackId={enemyAttackId}"
        );

        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations == null)
        {
            Debug.LogError("[BattleResultProcessor] AttackAnimations not found!");
            yield break;
        }

        if (iAttackedFirst)
        {
            // JA AsTOASATM PRVAt
            // PRIORITY 1: Bleed damage FIRST (before blocking/recovery checks)
            // V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
            if (myBleedDamages != null && myBleedDamages.Count > 0)
            {
                Debug.LogWarning($"[BLEED] MY card has {myBleedDamages.Count} Bleed effects!");
                for (int i = 0; i < myBleedDamages.Count; i++)
                {
                    int bleedDamage = myBleedDamages[i];
                    Debug.LogWarning($"[BLEED #{i + 1}] MY card takes {bleedDamage} damage");
                    yield return StartCoroutine(
                        PlaySingleBleedAnimation(myCard, bleedDamage, true)
                    );
                    if (i < myBleedDamages.Count - 1)
                    {
                        yield return new WaitForSeconds(0.3f); // Small delay between Bleeds
                    }
                }
            }

            // PRIORITY 1: Exposure damage (same priority as Bleed)
            if (myExposureDamage > 0 || myExposureRemoved)
            {
                Debug.LogWarning(
                    $"adZ [EXPOSURE] MY card - damage={myExposureDamage}, removed={myExposureRemoved}"
                );
                yield return StartCoroutine(
                    PlayExposureAnimation(myCard, myExposureDamage, myExposureRemoved, true)
                );
                yield return new WaitForSeconds(0.3f);
            }

            // PRIORITY 3: Skontroluj Asceticism recovery (blocking effect)
            if (myRecovered)
            {
                Debug.LogWarning($"[ASCETICISM] MY card RECOVERED from Asceticism before attack!");
                yield return StartCoroutine(PlayRecoveryAnimation(myCard, "My card"));
                yield return new WaitForSeconds(0.5f);
            }

            // PRIORITY 3: Skontroluj Sleep blocking/wake-up (blocking effect)
            if (myWokeUp)
            {
                // Zobraz wake-up animAciu pred Astokom
                yield return StartCoroutine(PlayWakeUpAnimation(myCard, true));
            }

            if (!myAttackBlocked)
            {
                // Set color to BLUE for entire attack sequence (like singleplayer)
                if (dialogText != null)
                    dialogText.color = Color.blue;

                // Execute attack animation
                yield return StartCoroutine(
                    ExecuteAttackAnimation(
                        myCard,
                        enemyCard,
                        myAttackId,
                        enemyDamage,
                        myHealAmount,
                        true,
                        myAttackerSelfDamage,
                        myEffectsApplied,
                        myAttackResult,
                        myAttackerEffects
                    )
                );

                Debug.LogWarning(
                    $"[APPLYING_STATS] My turn - attackerKno={myStatChanges.attackerKnowledge}, defenderKno={myStatChanges.defenderKnowledge}"
                );

                // [OK] V12: Apply and animate stat changes during battle playback
                yield return StartCoroutine(
                    BattleStatPlayback.PlayCardStatChanges(
                        myCard,
                        myStatChanges.attackerAttack,
                        myStatChanges.attackerStrength,
                        myStatChanges.attackerDefense,
                        myStatChanges.attackerKnowledge,
                        myStatChanges.attackerSpeed,
                        myStatChanges.attackerCharisma,
                        cardAnimator
                    )
                );

                yield return StartCoroutine(
                    BattleStatPlayback.PlayCardStatChanges(
                        enemyCard,
                        myStatChanges.defenderAttack,
                        myStatChanges.defenderStrength,
                        myStatChanges.defenderDefense,
                        myStatChanges.defenderKnowledge,
                        myStatChanges.defenderSpeed,
                        myStatChanges.defenderCharisma,
                        cardAnimator
                    )
                );

                // V11.1: Display multiple effects on defender
                if (myEffectsApplied != null && myEffectsApplied.Count > 0)
                {
                    yield return StartCoroutine(
                        BattleEffectPlayback.DisplayMultipleEffects(enemyCard, myEffectsApplied, false)
                    );
                }

                // V11.1: Attacker self-damage (CarHit recoil)
                // Skip for CarHit (attackId 7) - already handled in case 7
                if (myAttackerSelfDamage > 0 && myAttackId != 7)
                {
                    Debug.LogWarning(
                        $"zA [SELF-DAMAGE] MY card takes {myAttackerSelfDamage} recoil damage!"
                    );
                    yield return StartCoroutine(
                        PlaySelfDamageAnimation(myCard, myAttackerSelfDamage, true)
                    );
                }

                // V11.1: Attacker self-effects (CarHit recoil bleed/sleep)
                if (myAttackerEffects != null && myAttackerEffects.Count > 0)
                {
                    yield return StartCoroutine(
                        BattleEffectPlayback.DisplayMultipleEffects(myCard, myAttackerEffects, true)
                    );
                }
            }
            else
            {
                // V10: Astok blocked by effect (Sleep, Asceticism, atAZ.)
                yield return StartCoroutine(PlayBlockAnimation(myCard, myBlockedBy, true));

                // V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                if (myBlockedBy == 2 && mySelfDamage > 0)
                {
                    Debug.LogWarning(
                        $"[ASCETICISM] MY card takes {mySelfDamage} self-damage due to blocking!"
                    );
                    yield return StartCoroutine(
                        PlaySelfDamageAnimation(myCard, mySelfDamage, true)
                    );
                }
            }

            // AK NEPRIATEA PRELIL, JEHO AsTOK
            if (enemyCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);

                // Set dialog color to RED for enemy attack (like singleplayer)
                if (dialogText != null)
                    dialogText.color = Color.red;

                // PRIORITY 1: Enemy Bleed damage FIRST
                // V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
                if (enemyBleedDamages != null && enemyBleedDamages.Count > 0)
                {
                    Debug.LogWarning(
                        $"[BLEED] ENEMY card has {enemyBleedDamages.Count} Bleed effects!"
                    );
                    for (int i = 0; i < enemyBleedDamages.Count; i++)
                    {
                        int bleedDamage = enemyBleedDamages[i];
                        Debug.LogWarning($"[BLEED #{i + 1}] ENEMY card takes {bleedDamage} damage");
                        yield return StartCoroutine(
                            PlaySingleBleedAnimation(enemyCard, bleedDamage, false)
                        );
                        if (i < enemyBleedDamages.Count - 1)
                        {
                            yield return new WaitForSeconds(0.3f); // Small delay between Bleeds
                        }
                    }
                }

                // PRIORITY 1: Exposure damage (same priority as Bleed)
                if (enemyExposureDamage > 0 || enemyExposureRemoved)
                {
                    Debug.LogWarning(
                        $"adZ [EXPOSURE] ENEMY card - damage={enemyExposureDamage}, removed={enemyExposureRemoved}"
                    );
                    yield return StartCoroutine(
                        PlayExposureAnimation(
                            enemyCard,
                            enemyExposureDamage,
                            enemyExposureRemoved,
                            false
                        )
                    );
                    yield return new WaitForSeconds(0.3f);
                }

                // PRIORITY 3: NepriateAl Asceticism recovery check (blocking effect)
                if (enemyRecovered)
                {
                    Debug.LogWarning(
                        $"[ASCETICISM] ENEMY card RECOVERED from Asceticism before counter-attack!"
                    );
                    yield return StartCoroutine(PlayRecoveryAnimation(enemyCard, "Enemy card"));
                    yield return new WaitForSeconds(0.5f);
                }

                // PRIORITY 3: NepriateAl wake-up check (blocking effect)
                if (enemyWokeUp)
                {
                    yield return StartCoroutine(PlayWakeUpAnimation(enemyCard, false));
                }

                if (!enemyAttackBlocked)
                {
                    // FIX: enemyCard AstoATA myCard a pouLlij myDamage (damage ktorA JA dostanem)
                    yield return StartCoroutine(
                        ExecuteAttackAnimation(
                            enemyCard,
                            myCard,
                            enemyAttackId,
                            myDamage,
                            enemyHealAmount,
                            false,
                            enemyAttackerSelfDamage,
                            enemyEffectsApplied,
                            enemyAttackResult,
                            enemyAttackerEffects
                        )
                    );

                    Debug.LogWarning(
                        $"[APPLYING_STATS] Enemy turn - attackerKno={enemyStatChanges.attackerKnowledge}, defenderKno={enemyStatChanges.defenderKnowledge}"
                    );

                    // [OK] V12: Apply and animate stat changes during battle playback
                    yield return StartCoroutine(
                        BattleStatPlayback.PlayCardStatChanges(
                            enemyCard,
                            enemyStatChanges.attackerAttack,
                            enemyStatChanges.attackerStrength,
                            enemyStatChanges.attackerDefense,
                            enemyStatChanges.attackerKnowledge,
                            enemyStatChanges.attackerSpeed,
                            enemyStatChanges.attackerCharisma,
                            cardAnimator
                        )
                    );

                    yield return StartCoroutine(
                        BattleStatPlayback.PlayCardStatChanges(
                            myCard,
                            enemyStatChanges.defenderAttack,
                            enemyStatChanges.defenderStrength,
                            enemyStatChanges.defenderDefense,
                            enemyStatChanges.defenderKnowledge,
                            enemyStatChanges.defenderSpeed,
                            enemyStatChanges.defenderCharisma,
                            cardAnimator
                        )
                    );

                    // V11.1: Display multiple effects on defender (me)
                    if (enemyEffectsApplied != null && enemyEffectsApplied.Count > 0)
                    {
                        yield return StartCoroutine(
                            BattleEffectPlayback.DisplayMultipleEffects(myCard, enemyEffectsApplied, true)
                        );
                    }

                    // V11.1: Enemy attacker self-damage
                    // Skip for CarHit (attackId 7) - already handled in case 7
                    if (enemyAttackerSelfDamage > 0 && enemyAttackId != 7)
                    {
                        Debug.LogWarning(
                            $"zA [SELF-DAMAGE] ENEMY card takes {enemyAttackerSelfDamage} recoil damage!"
                        );
                        yield return StartCoroutine(
                            PlaySelfDamageAnimation(enemyCard, enemyAttackerSelfDamage, false)
                        );
                    }

                    // V11.1: Enemy attacker self-effects
                    if (enemyAttackerEffects != null && enemyAttackerEffects.Count > 0)
                    {
                        yield return StartCoroutine(
                            BattleEffectPlayback.DisplayMultipleEffects(enemyCard, enemyAttackerEffects, false)
                        );
                    }
                }
                else
                {
                    // V10: Astok blocked by effect (Sleep, Asceticism, atAZ.)
                    yield return StartCoroutine(
                        PlayBlockAnimation(enemyCard, enemyBlockedBy, false)
                    );

                    // V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                    if (enemyBlockedBy == 2 && enemySelfDamage > 0)
                    {
                        Debug.LogWarning(
                            $"[ASCETICISM] ENEMY card takes {enemySelfDamage} self-damage due to blocking!"
                        );
                        yield return StartCoroutine(
                            PlaySelfDamageAnimation(enemyCard, enemySelfDamage, false)
                        );
                    }
                }
            }
        }
        else
        {
            // NEPRIATEA AsTOASAT PRVAt
            // Set dialog color to RED for enemy attack (like singleplayer)
            if (dialogText != null)
                dialogText.color = Color.red;

            // PRIORITY 1: Enemy Bleed damage FIRST
            // V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
            if (enemyBleedDamages != null && enemyBleedDamages.Count > 0)
            {
                Debug.LogWarning(
                    $"[BLEED] ENEMY card has {enemyBleedDamages.Count} Bleed effects!"
                );
                for (int i = 0; i < enemyBleedDamages.Count; i++)
                {
                    int bleedDamage = enemyBleedDamages[i];
                    Debug.LogWarning($"[BLEED #{i + 1}] ENEMY card takes {bleedDamage} damage");
                    yield return StartCoroutine(
                        PlaySingleBleedAnimation(enemyCard, bleedDamage, false)
                    );
                    if (i < enemyBleedDamages.Count - 1)
                    {
                        yield return new WaitForSeconds(0.3f); // Small delay between Bleeds
                    }
                }
            }

            // PRIORITY 1: Exposure damage (same priority as Bleed)
            if (enemyExposureDamage > 0 || enemyExposureRemoved)
            {
                Debug.LogWarning(
                    $"adZ [EXPOSURE] ENEMY card - damage={enemyExposureDamage}, removed={enemyExposureRemoved}"
                );
                yield return StartCoroutine(
                    PlayExposureAnimation(
                        enemyCard,
                        enemyExposureDamage,
                        enemyExposureRemoved,
                        false
                    )
                );
                yield return new WaitForSeconds(0.3f);
            }

            // PRIORITY 3: NepriateAl Asceticism recovery check (blocking effect)
            if (enemyRecovered)
            {
                Debug.LogWarning(
                    $"[ASCETICISM] ENEMY card RECOVERED from Asceticism before attack!"
                );
                yield return StartCoroutine(PlayRecoveryAnimation(enemyCard, "Enemy card"));
                yield return new WaitForSeconds(0.5f);
            }

            // PRIORITY 3: NepriateAl wake-up check (blocking effect)
            if (enemyWokeUp)
            {
                yield return StartCoroutine(PlayWakeUpAnimation(enemyCard, false));
            }

            if (!enemyAttackBlocked)
            {
                // FIX: enemyCard AstoATA myCard a pouLlij myDamage (damage ktorA JA dostanem)
                yield return StartCoroutine(
                    ExecuteAttackAnimation(
                        enemyCard,
                        myCard,
                        enemyAttackId,
                        myDamage,
                        enemyHealAmount,
                        false,
                        enemyAttackerSelfDamage,
                        enemyEffectsApplied,
                        enemyAttackResult,
                        enemyAttackerEffects
                    )
                );

                Debug.LogWarning(
                    $"[APPLYING_STATS] Enemy turn - attackerKno={enemyStatChanges.attackerKnowledge}, defenderKno={enemyStatChanges.defenderKnowledge}"
                );

                // [OK] V12: Apply and animate stat changes during battle playback
                yield return StartCoroutine(
                    BattleStatPlayback.PlayCardStatChanges(
                        enemyCard,
                        enemyStatChanges.attackerAttack,
                        enemyStatChanges.attackerStrength,
                        enemyStatChanges.attackerDefense,
                        enemyStatChanges.attackerKnowledge,
                        enemyStatChanges.attackerSpeed,
                        enemyStatChanges.attackerCharisma,
                        cardAnimator
                    )
                );

                yield return StartCoroutine(
                    BattleStatPlayback.PlayCardStatChanges(
                        myCard,
                        enemyStatChanges.defenderAttack,
                        enemyStatChanges.defenderStrength,
                        enemyStatChanges.defenderDefense,
                        enemyStatChanges.defenderKnowledge,
                        enemyStatChanges.defenderSpeed,
                        enemyStatChanges.defenderCharisma,
                        cardAnimator
                    )
                );

                // V11.1: Display multiple effects on defender (me)
                if (enemyEffectsApplied != null && enemyEffectsApplied.Count > 0)
                {
                    yield return StartCoroutine(
                        BattleEffectPlayback.DisplayMultipleEffects(myCard, enemyEffectsApplied, true)
                    );
                }

                // V11.1: Enemy attacker self-damage
                if (enemyAttackerSelfDamage > 0)
                {
                    Debug.LogWarning(
                        $"zA [SELF-DAMAGE] ENEMY card takes {enemyAttackerSelfDamage} recoil damage!"
                    );
                    yield return StartCoroutine(
                        PlaySelfDamageAnimation(enemyCard, enemyAttackerSelfDamage, false)
                    );
                }

                // V11.1: Enemy attacker self-effects
                if (enemyAttackerEffects != null && enemyAttackerEffects.Count > 0)
                {
                    yield return StartCoroutine(
                        BattleEffectPlayback.DisplayMultipleEffects(enemyCard, enemyAttackerEffects, false)
                    );
                }
            }
            else
            {
                // V10: Astok blocked by effect (Sleep, Asceticism, atAZ.)
                yield return StartCoroutine(PlayBlockAnimation(enemyCard, enemyBlockedBy, false));

                // V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                if (enemyBlockedBy == 2 && enemySelfDamage > 0)
                {
                    Debug.LogWarning(
                        $"[ASCETICISM] ENEMY card takes {enemySelfDamage} self-damage due to blocking!"
                    );
                    yield return StartCoroutine(
                        PlaySelfDamageAnimation(enemyCard, enemySelfDamage, false)
                    );
                }
            }

            // AK JA PRELIJEM, MAJ AsTOK
            if (myCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);

                // Set color to BLUE for entire attack sequence (like singleplayer)
                if (dialogText != null)
                    dialogText.color = Color.blue;

                // PRIORITY 1: My Bleed damage FIRST
                // V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
                if (myBleedDamages != null && myBleedDamages.Count > 0)
                {
                    Debug.LogWarning($"[BLEED] MY card has {myBleedDamages.Count} Bleed effects!");
                    for (int i = 0; i < myBleedDamages.Count; i++)
                    {
                        int bleedDamage = myBleedDamages[i];
                        Debug.LogWarning($"[BLEED #{i + 1}] MY card takes {bleedDamage} damage");
                        yield return StartCoroutine(
                            PlaySingleBleedAnimation(myCard, bleedDamage, true)
                        );
                        if (i < myBleedDamages.Count - 1)
                        {
                            yield return new WaitForSeconds(0.3f); // Small delay between Bleeds
                        }
                    }
                }

                // PRIORITY 1: Exposure damage (same priority as Bleed)
                if (myExposureDamage > 0 || myExposureRemoved)
                {
                    Debug.LogWarning(
                        $"adZ [EXPOSURE] MY card - damage={myExposureDamage}, removed={myExposureRemoved}"
                    );
                    yield return StartCoroutine(
                        PlayExposureAnimation(myCard, myExposureDamage, myExposureRemoved, true)
                    );
                    yield return new WaitForSeconds(0.3f);
                }

                // PRIORITY 3: MAj Asceticism recovery check (blocking effect)
                if (myRecovered)
                {
                    Debug.LogWarning(
                        $"[ASCETICISM] MY card RECOVERED from Asceticism before counter-attack!"
                    );
                    yield return StartCoroutine(PlayRecoveryAnimation(myCard, "My card"));
                    yield return new WaitForSeconds(0.5f);
                }

                // PRIORITY 3: MAj wake-up check (blocking effect)
                if (myWokeUp)
                {
                    yield return StartCoroutine(PlayWakeUpAnimation(myCard, true));
                }

                if (!myAttackBlocked)
                {
                    // FIX: myCard AstoATA enemyCard a pouLlij enemyDamage (damage ktorA ENEMY dostane)
                    yield return StartCoroutine(
                        ExecuteAttackAnimation(
                            myCard,
                            enemyCard,
                            myAttackId,
                            enemyDamage,
                            myHealAmount,
                            true,
                            myAttackerSelfDamage,
                            myEffectsApplied,
                            myAttackResult,
                            myAttackerEffects
                        )
                    );

                    Debug.LogWarning(
                        $"[APPLYING_STATS] My turn - attackerKno={myStatChanges.attackerKnowledge}, defenderKno={myStatChanges.defenderKnowledge}"
                    );

                    // [OK] V12: Apply and animate stat changes during battle playback
                    yield return StartCoroutine(
                        BattleStatPlayback.PlayCardStatChanges(
                            myCard,
                            myStatChanges.attackerAttack,
                            myStatChanges.attackerStrength,
                            myStatChanges.attackerDefense,
                            myStatChanges.attackerKnowledge,
                            myStatChanges.attackerSpeed,
                            myStatChanges.attackerCharisma,
                            cardAnimator
                        )
                    );

                    yield return StartCoroutine(
                        BattleStatPlayback.PlayCardStatChanges(
                            enemyCard,
                            myStatChanges.defenderAttack,
                            myStatChanges.defenderStrength,
                            myStatChanges.defenderDefense,
                            myStatChanges.defenderKnowledge,
                            myStatChanges.defenderSpeed,
                            myStatChanges.defenderCharisma,
                            cardAnimator
                        )
                    );

                    // V11.1: Display multiple effects on defender (enemy)
                    if (myEffectsApplied != null && myEffectsApplied.Count > 0)
                    {
                        yield return StartCoroutine(
                            BattleEffectPlayback.DisplayMultipleEffects(enemyCard, myEffectsApplied, false)
                        );
                    }

                    // V11.1: My attacker self-damage
                    // Skip for CarHit (attackId 7) - already handled in case 7
                    if (myAttackerSelfDamage > 0 && myAttackId != 7)
                    {
                        Debug.LogWarning(
                            $"zA [SELF-DAMAGE] MY card takes {myAttackerSelfDamage} recoil damage!"
                        );
                        yield return StartCoroutine(
                            PlaySelfDamageAnimation(myCard, myAttackerSelfDamage, true)
                        );
                    }

                    // V11.1: My attacker self-effects
                    if (myAttackerEffects != null && myAttackerEffects.Count > 0)
                    {
                        yield return StartCoroutine(
                            BattleEffectPlayback.DisplayMultipleEffects(myCard, myAttackerEffects, true)
                        );
                    }
                }
                else
                {
                    // V10: Astok blocked by effect (Sleep, Asceticism, atAZ.)
                    yield return StartCoroutine(PlayBlockAnimation(myCard, myBlockedBy, true));

                    // V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                    if (myBlockedBy == 2 && mySelfDamage > 0)
                    {
                        Debug.LogWarning(
                            $"[ASCETICISM] MY card takes {mySelfDamage} self-damage due to blocking!"
                        );
                        yield return StartCoroutine(
                            PlaySelfDamageAnimation(myCard, mySelfDamage, true)
                        );
                    }
                }
            }
        }

        // Reset dialog color to BLACK after battle (for neutral messages like "Choose your attack")
        if (dialogText != null)
            dialogText.color = Color.black;

        // V11.2: Immediately show "Preparing next turn..." to mask color transition
        yield return StartCoroutine(ShowDialog("Preparing next turn..."));

        // V5: HP sa updatuje postupne poATas animAciA, Lliadna finAlna sync!
        // selectedCards refresh sa volA v PlayBattleAnimationsAndRefresh

        // Reset card positions na sprAvne miesta
        if (cardAnimator != null)
        {
            Vector3 playerBoardPos =
                fightSystem.playerBoard != null
                    ? fightSystem.playerBoard.transform.position
                    : myCard.transform.position;
            Vector3 enemyBoardPos =
                fightSystem.enemyBoard != null
                    ? fightSystem.enemyBoard.transform.position
                    : enemyCard.transform.position;

            StartCoroutine(
                cardAnimator.ResetCardPosition(myCard, playerBoardPos, Quaternion.identity)
            );
            StartCoroutine(
                cardAnimator.ResetCardPosition(enemyCard, enemyBoardPos, Quaternion.identity)
            );
        }

        // Skontroluj vAsledok
        yield return new WaitForSeconds(1f);
        CheckBattleOutcome(myCard, enemyCard);
    }

    /// <summary>
    /// VykonA animAciu pre konkrAtny Astok (router pattern)
    /// V8: Podporuje Attack ID 1 (Punch), 2 (Kick), 3 (Heal), ... rozLiriteAlnA
    /// V9: Heal support - self-heal attacks s healAmount + zelenA HP animAcia
    /// V11.2: CarHit - attackerSelfDamage pre sAsATasnA animAcie damage
    /// V12: Modular - kaLldA Astok mA svoj handler v AttackHandlers/ folder
    /// V14: attackerEffects - effects applied to attacker SELF (backfire Sleep from UpInSmoke)
    /// </summary>
    private IEnumerator ExecuteAttackAnimation(
        Kard attacker,
        Kard defender,
        int attackId,
        int damage,
        int healAmount,
        bool isMyAttack,
        int attackerSelfDamage = 0,
        List<Dictionary<string, object>> effectsApplied = null,
        string attackResult = null,
        List<Dictionary<string, object>> attackerEffects = null
    )
    {
        AttackAnimations animations = attackComponent.attackAnimations;
        AttackExecutionContext context = new AttackExecutionContext(
            attacker,
            defender,
            damage,
            healAmount,
            isMyAttack,
            attackerSelfDamage,
            animations,
            cardAnimator,
            playerLifeBar,
            enemyLifeBar,
            ShowDialog,
            effectsApplied,
            attackResult,
            attackerEffects
        );

        yield return AttackRegistry.ExecuteOrFallback(attackId, context);
    }
}


































