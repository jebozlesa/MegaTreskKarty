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
        var context = new BattleTimelinePlaybackContext
        {
            CoroutineHost = this,
            PlayerCard = myCard,
            EnemyCard = enemyCard,
            PlayerCardId = myCardId,
            EnemyCardId = enemyCardId,
            AttackComponent = attackComponent,
            CardAnimator = cardAnimator,
            PlayerLifeBar = playerLifeBar,
            EnemyLifeBar = enemyLifeBar,
            DialogText = dialogText,
            PlayerBoard = fightSystem != null ? fightSystem.playerBoard : null,
            EnemyBoard = fightSystem != null ? fightSystem.enemyBoard : null,
            ResetCardsAfterPlayback = true,
            CompletionDialog = "Preparing next turn...",
            DialogDelaySeconds = 1.5f,
            TickDelaySeconds = 0.3f,
            InterAttackDelaySeconds = 0.5f,
            FinalDelaySeconds = 1f,
        };

        yield return BattleTimelinePlayback.Play(context, steps);
        CheckBattleOutcome(myCard, enemyCard);
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
    /// <summary>
    /// VykonA animAciu pre konkrAtny Astok (router pattern)
    /// V8: Podporuje Attack ID 1 (Punch), 2 (Kick), 3 (Heal), ... rozLiriteAlnA
    /// V9: Heal support - self-heal attacks s healAmount + zelenA HP animAcia
    /// V11.2: CarHit - attackerSelfDamage pre sAsATasnA animAcie damage
    /// V12: Modular - kaLldA Astok mA svoj handler v AttackHandlers/ folder
    /// V14: attackerEffects - effects applied to attacker SELF (backfire Sleep from UpInSmoke)
    /// </summary>
}


































