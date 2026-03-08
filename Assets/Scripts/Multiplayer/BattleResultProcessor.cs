using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json.Linq;

/// <summary>
/// Stat changes pre jeden útok (buffs na útočníka + debuffs na obrancu)
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
/// Zodpovedný za spracovanie battle výsledkov zo servera
/// Aplikuje HP zmeny, hrá animácie a určuje víťaza
/// V5: BattleResult identifikuje karty cez cardId namiesto player1/player2
/// </summary>
public class BattleResultProcessor : MonoBehaviour
{
    [Header("Dependencies")]
    public FightSystemMultiplayer fightSystem;
    public Attack attackComponent;
    public MultiplayerService multiplayerService;  // ✅ For refreshing selectedCards
    public MultiplayerCardAnimator cardAnimator;    // ✅ NEW: Card animations (damage, stats, shake)
    public MultiplayerKillCounterManager killCounterManager;  // ✅ NEW: Kill counter tracking
    private BattleRoundCoordinator roundCoordinator;
    
    [Header("UI References")]
    public TMP_Text dialogText;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;
    
    private void Start()
    {
        // ⚠️ Validácia required referencií
        if (multiplayerService == null)
        {
            Debug.LogError("[BattleResultProcessor] MultiplayerService not assigned! Please set in Inspector.");
        }
        
        if (cardAnimator == null)
        {
            Debug.LogError("[BattleResultProcessor] MultiplayerCardAnimator not assigned! Card animations will be skipped. Please set in Inspector.");
        }
        
        if (attackComponent == null)
        {
            Debug.LogError("[BattleResultProcessor] ❌ CRITICAL: attackComponent not assigned! Sleep animations and effects will NOT work! Please set in Inspector.");
        }
        roundCoordinator = new BattleRoundCoordinator(fightSystem, multiplayerService, killCounterManager, dialogText);
    }
    
    /// <summary>
    /// Spracuje výsledok battle a spustí animácie
    /// V5: BattleResult identifikuje karty cez cardId, HP sa načíta z selectedCards
    /// </summary>
    public void ProcessBattleResult(Dictionary<string, object> battleResult)
    {
        Debug.LogWarning($"📦 [BATTLE_RESULT] ===== RAW SERVER RESPONSE ===== ");
        foreach (var kvp in battleResult)
        {
            Debug.LogWarning($"📦 [BATTLE_RESULT] {kvp.Key}: {kvp.Value}");
        }
        Debug.LogWarning($"📦 [BATTLE_RESULT] ================================ ");
        
        Debug.Log($"[BattleResultProcessor] Processing battle result (V5)");
        
        // Získaj karty
        Kard myCard = fightSystem.player?.cardInGame;
        Kard enemyCard = fightSystem.enemy?.cardInGame;
        
        if (myCard == null || enemyCard == null)
        {
            Debug.LogError("[BattleResultProcessor] Cannot apply result - cards not found");
            return;
        }
        
        Debug.LogWarning($"🎴 [CARDS] MY: {myCard.cardName} (cardId={myCard.cardId}, HP={myCard.health}/{myCard.maxHealth})");
        Debug.LogWarning($"🎴 [CARDS] ENEMY: {enemyCard.cardName} (cardId={enemyCard.cardId}, HP={enemyCard.health}/{enemyCard.maxHealth})");
        
        // ✅ V11: Parse NEW ID-based response format (firstAttacker, secondAttacker)
        if (!BattleResultParser.TryParse(battleResult, myCard.cardId, out var parsed, out var parseError))
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
        Debug.LogWarning($"[ROLE] FirstAttacker={firstAttackerCardId}, SecondAttacker={secondAttackerCardId}");

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

        Debug.LogWarning($"[MY_STATS] Attacker buffs: ATK={myStatChanges.attackerAttack} STR={myStatChanges.attackerStrength} DEF={myStatChanges.attackerDefense} KNO={myStatChanges.attackerKnowledge} SPD={myStatChanges.attackerSpeed} CHA={myStatChanges.attackerCharisma}");
        Debug.LogWarning($"[MY_STATS] Defender debuffs: ATK={myStatChanges.defenderAttack} STR={myStatChanges.defenderStrength} DEF={myStatChanges.defenderDefense} KNO={myStatChanges.defenderKnowledge} SPD={myStatChanges.defenderSpeed} CHA={myStatChanges.defenderCharisma}");
        Debug.LogWarning($"[ENEMY_STATS] Attacker buffs: ATK={enemyStatChanges.attackerAttack} STR={enemyStatChanges.attackerStrength} DEF={enemyStatChanges.attackerDefense} KNO={enemyStatChanges.attackerKnowledge} SPD={enemyStatChanges.attackerSpeed} CHA={enemyStatChanges.attackerCharisma}");
        Debug.LogWarning($"[ENEMY_STATS] Defender debuffs: ATK={enemyStatChanges.defenderAttack} STR={enemyStatChanges.defenderStrength} DEF={enemyStatChanges.defenderDefense} KNO={enemyStatChanges.defenderKnowledge} SPD={enemyStatChanges.defenderSpeed} CHA={enemyStatChanges.defenderCharisma}");

        Dictionary<string, object> myEffectApplied = parsed.MyEffectApplied;
        Dictionary<string, object> enemyEffectApplied = parsed.EnemyEffectApplied;

        if (myEffectApplied != null)
        {
            Debug.LogWarning($"[EFFECT] MY card APPLIED effect to enemy: type={myEffectApplied["type"]}, duration={myEffectApplied["duration"]}");
        }

        if (enemyEffectApplied != null)
        {
            Debug.LogWarning($"[EFFECT] ENEMY card APPLIED effect to me: type={enemyEffectApplied["type"]}, duration={enemyEffectApplied["duration"]}");
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
            Debug.LogWarning($"[SELF-DAMAGE] ENEMY card takes {enemyAttackerSelfDamage} self-damage!");
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
            string effectName = GetEffectName(myBlockedBy ?? 0);
            Debug.LogWarning($"[BLOCK] MY attack BLOCKED by {effectName}! SelfDamage={mySelfDamage}");
        }
        if (enemyAttackBlocked)
        {
            string effectName = GetEffectName(enemyBlockedBy ?? 0);
            Debug.LogWarning($"[BLOCK] ENEMY attack BLOCKED by {effectName}! SelfDamage={enemySelfDamage}");
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
            Debug.LogWarning($"[ASCETICISM] MY card RECOVERED from Asceticism! Feels blessed again.");
        }
        if (enemyRecovered)
        {
            Debug.LogWarning($"[ASCETICISM] ENEMY card RECOVERED from Asceticism!");
        }

        Debug.LogWarning($"[BattleResultProcessor] MyAttackId={myAttackId}, MyDamageReceived={myDamage}, MyHeal={myHealAmount}, MySelfDamage={mySelfDamage}, MyBleedDamage={myBleedDamage}, MyBleedCount={myBleedDamages.Count}, MyExposureDamage={myExposureDamage}, MyExposureRemoved={myExposureRemoved}, MyAttackerSelfDamage={myAttackerSelfDamage}, MyBlocked={myAttackBlocked}, MyBlockedBy={myBlockedBy}, MyWokeUp={myWokeUp}, MyRecovered={myRecovered}, MyEffects={myEffectsApplied.Count}, MySelfEffects={myAttackerEffects.Count}, EnemyAttackId={enemyAttackId}, EnemyDamageReceived={enemyDamage}, EnemyHeal={enemyHealAmount}, EnemySelfDamage={enemySelfDamage}, EnemyBleedDamage={enemyBleedDamage}, EnemyBleedCount={enemyBleedDamages.Count}, EnemyExposureDamage={enemyExposureDamage}, EnemyExposureRemoved={enemyExposureRemoved}, EnemyAttackerSelfDamage={enemyAttackerSelfDamage}, EnemyBlocked={enemyAttackBlocked}, EnemyBlockedBy={enemyBlockedBy}, EnemyWokeUp={enemyWokeUp}, EnemyRecovered={enemyRecovered}, EnemyEffects={enemyEffectsApplied.Count}, EnemySelfEffects={enemyAttackerEffects.Count}");

        // ? Spusti anim�cie (HP sa updatne postupne!)
        // ? REFRESH selectedCards sa spust� A� PO anim�ci�ch
        // ? V11: firstAttacker replaced with firstAttackerCardId (clear ID-based role)
        // ? V11.1: Added effectsApplied arrays + attackerSelfDamage for AoE/recoil attacks
        // ? V11.2: Added bleedDamages arrays for individual Bleed animations + exposureDamage/exposureRemoved
        // ? V12: Added stat changes structs for clean parameter passing
        // M10: V2 hard mode support (no fallback when timelineV2 is missing/invalid).
        bool timelineHardMode = BattleTimelinePilotPolicy.IsHardModeEnabled();
        if (BattleTimelinePilotPolicy.ShouldRunTimeline(battleResult, parsed, out var timelineMode))
        {
            if (BattleTimelineBuilder.TryBuild(battleResult, out var timelineSteps, out var timelineError))
            {
                Debug.Log($"[TimelinePilot] Enabled for this turn, mode={timelineMode}, steps={timelineSteps.Count}");
                StartCoroutine(PlayTimelinePilotAndRefresh(myCard, enemyCard, myCardId, enemyCardId, timelineSteps));
                return;
            }

            if (timelineHardMode)
            {
                Debug.LogError($"[TimelinePilot][HARD] Timeline build failed (mode={timelineMode}). Legacy fallback is disabled. Error: {timelineError}");
                StartCoroutine(ShowDialog("TimelineV2 error (hard mode). Legacy fallback disabled."));
                return;
            }

            Debug.LogWarning($"[TimelinePilot] Timeline build failed (mode={timelineMode}), falling back to legacy flow: {timelineError}");
        }

        if (timelineHardMode && (timelineMode == "v2_hard_missing" || timelineMode == "v2_no_payload"))
        {
            Debug.LogError($"[TimelinePilot][HARD] Missing timelineV2 payload (mode={timelineMode}). Legacy fallback is disabled.");
            StartCoroutine(ShowDialog("Missing timelineV2 (hard mode). Legacy fallback disabled."));
            return;
        }

        StartCoroutine(PlayBattleAnimationsAndRefresh(myCard, enemyCard, firstAttackerCardId, myCardId, enemyCardId, myAttackId, enemyAttackId, myDamage, enemyDamage, myHealAmount, enemyHealAmount, myStatChanges, enemyStatChanges, myEffectsApplied, enemyEffectsApplied, myAttackerEffects, enemyAttackerEffects, myAttackerSelfDamage, enemyAttackerSelfDamage, myAttackBlocked, enemyAttackBlocked, myWokeUp, enemyWokeUp, myRecovered, enemyRecovered, mySelfDamage, enemySelfDamage, myBlockedBy, enemyBlockedBy, myBleedDamages, enemyBleedDamages, myExposureDamage, myExposureRemoved, enemyExposureDamage, enemyExposureRemoved, myAttackResult, enemyAttackResult));
    }
    
    /// <summary>
    /// Wrapper coroutine - animácie POTOM refresh
    /// V5: Používa cardId na identifikáciu, damage namiesto finalHealth
    /// V8: Pridané attackId pre dynamické animácie
    /// V9: Pridané healAmount pre self-heal animácie + effectApplied pre effect ikony + Sleep blocking + blockedBy field
    /// V10: Pridané recovered/selfDamage pre Asceticism effect
    /// V11.1: Pridané effectsApplied arrays + attackerSelfDamage/attackerEffectsApplied pre AoE/recoil attacks (CarHit)
    /// V11.2: Pridané bleedDamage pre Bleed effect processing
    /// </summary>
    private IEnumerator PlayBattleAnimationsAndRefresh(
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
        string enemyAttackResult)
    {
        // 1. Prehrá animácie (postupný HP update) + effect ikony V SPRÁVNOM PORADÍ
        yield return StartCoroutine(PlayBattleAnimations(myCard, enemyCard, firstAttackerCardId, myCardId, enemyCardId, myAttackId, enemyAttackId, myDamage, enemyDamage, myHealAmount, enemyHealAmount, myStatChanges, enemyStatChanges, myEffectsApplied, enemyEffectsApplied, myAttackerEffects, enemyAttackerEffects, myAttackerSelfDamage, enemyAttackerSelfDamage, myAttackBlocked, enemyAttackBlocked, myWokeUp, enemyWokeUp, myRecovered, enemyRecovered, mySelfDamage, enemySelfDamage, myBlockedBy, enemyBlockedBy, myBleedDamages, enemyBleedDamages, myExposureDamage, myExposureRemoved, enemyExposureDamage, enemyExposureRemoved, myAttackResult, enemyAttackResult));
        
        // 2. ✅ Effect ikony sa zobrazujú UŽ v PlayBattleAnimations (MOVED)
        // Tento kód už nie je potrebný - effects sa zobrazujú v správnom momente počas battle flow
        
        // 3. AŽ PO animáciách refreshni selectedCards z DB (pre buffs/effects)
        yield return StartCoroutine(RefreshCardsFromServer());
    }
    
    /// <summary>
    /// Načíta fresh selectedCards z DB po battle
    /// </summary>

    private IEnumerator PlayTimelinePilotAndRefresh(
        Kard myCard,
        Kard enemyCard,
        string myCardId,
        string enemyCardId,
        List<BattleStep> steps)
    {
        yield return StartCoroutine(PlayTimelinePilot(myCard, enemyCard, myCardId, enemyCardId, steps));
        yield return StartCoroutine(RefreshCardsFromServer());
    }

    private IEnumerator PlayTimelinePilot(
        Kard myCard,
        Kard enemyCard,
        string myCardId,
        string enemyCardId,
        List<BattleStep> steps)
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
                        actor.health -= step.Amount;
                        yield return StartCoroutine(PlaySingleBleedAnimation(actor, step.Amount, isMyActor));
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;

                case BattleStepType.ExposureTick:
                    if (actor != null && step.Amount > 0)
                    {
                        actor.health -= step.Amount;
                        yield return StartCoroutine(PlayExposureAnimation(actor, step.Amount, false, isMyActor));
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;

                case BattleStepType.ExposureRemoved:
                    if (actor != null)
                    {
                        yield return StartCoroutine(PlayExposureAnimation(actor, 0, true, isMyActor));
                        yield return new WaitForSeconds(0.3f);
                    }
                    break;

                case BattleStepType.Recovery:
                    if (actor != null)
                    {
                        yield return StartCoroutine(PlayRecoveryAnimation(actor, isMyActor ? "My card" : "Enemy card"));
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
                        Debug.Log($"[TimelinePilot] Skipping attackId={step.AttackId}, actor={step.ActorCardId}, skipped={step.Skipped}, blocked={step.Blocked}");
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

                    int damage = GetAttackDamageFromTimeline(steps, i, step.ActorCardId, step.TargetCardId);
                    int heal = GetAttackHealFromTimeline(steps, i, step.ActorCardId);

                    yield return StartCoroutine(ExecuteAttackAnimation(actor, target, step.AttackId, damage, heal, isMyActor, 0, step.EffectsApplied, step.AttackResult, step.AttackerEffectsApplied));
                    executedAttacks++;
                    break;

                case BattleStepType.Blocked:
                    if (actor != null)
                    {
                        yield return StartCoroutine(PlayBlockAnimation(actor, step.BlockedBy, isMyActor));
                    }
                    break;

                case BattleStepType.SelfDamage:
                    if (actor != null && step.Amount > 0)
                    {
                        actor.health -= step.Amount;
                        yield return StartCoroutine(PlaySelfDamageAnimation(actor, step.Amount, isMyActor));
                    }
                    break;

                case BattleStepType.EffectApplied:
                    if (target != null)
                    {
                        var effectData = new Dictionary<string, object>
                        {
                            { "type", step.EffectType },
                            { "duration", step.Duration },
                            { "source", step.Source }
                        };
                        yield return StartCoroutine(AddEffectIconOnly(target, effectData, target.cardId == myCardId));
                    }
                    break;

                case BattleStepType.Damage:
                case BattleStepType.Heal:
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
            Vector3 playerBoardPos = fightSystem.playerBoard != null ? fightSystem.playerBoard.transform.position : myCard.transform.position;
            Vector3 enemyBoardPos = fightSystem.enemyBoard != null ? fightSystem.enemyBoard.transform.position : enemyCard.transform.position;

            StartCoroutine(cardAnimator.ResetCardPosition(myCard, playerBoardPos, Quaternion.identity));
            StartCoroutine(cardAnimator.ResetCardPosition(enemyCard, enemyBoardPos, Quaternion.identity));
        }

        yield return new WaitForSeconds(1f);
        CheckBattleOutcome(myCard, enemyCard);
    }
    private static int GetAttackDamageFromTimeline(List<BattleStep> steps, int attackIndex, string attackerCardId, string defenderCardId)
    {
        for (int i = attackIndex + 1; i < steps.Count; i++)
        {
            BattleStep next = steps[i];
            if (next.Type == BattleStepType.Attack)
            {
                break;
            }

            if (next.Type == BattleStepType.Damage &&
                next.Source == "attack" &&
                next.ActorCardId == attackerCardId &&
                next.TargetCardId == defenderCardId)
            {
                return next.Amount;
            }
        }

        return 0;
    }

    private static int GetAttackHealFromTimeline(List<BattleStep> steps, int attackIndex, string attackerCardId)
    {
        for (int i = attackIndex + 1; i < steps.Count; i++)
        {
            BattleStep next = steps[i];
            if (next.Type == BattleStepType.Attack)
            {
                break;
            }

            if (next.Type == BattleStepType.Heal &&
                next.ActorCardId == attackerCardId &&
                next.TargetCardId == attackerCardId)
            {
                return next.Amount;
            }
        }

        return 0;
    }

    private static Kard GetCardById(string cardId, Kard myCard, Kard enemyCard, string myCardId, string enemyCardId)
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
    private IEnumerator RefreshCardsFromServer()
    {
        if (multiplayerService == null)
        {
            Debug.LogWarning("[BattleResultProcessor] MultiplayerService not set - cannot refresh cards");
            yield break;
        }
        
        Debug.Log("[BattleResultProcessor] Refreshing selectedCards from server...");
        
        var task = multiplayerService.GetSelectedCardsAsync(fightSystem.roomCode);
        yield return new WaitUntil(() => task.IsCompleted);
        
        if (task.Result != null && task.Result.Count > 0)
        {
            Debug.Log($"[BattleResultProcessor] Received {task.Result.Count} updated cards from server");
            
            // Aplikuj updatované stats na karty
            foreach (var kvp in task.Result)
            {
                string playerId = kvp.Key;
                var cardData = kvp.Value;
                
                Debug.Log($"[RefreshCards] Processing playerId={playerId}, cardName={cardData.name}, HP={cardData.health}/{cardData.maxHealth}");
                Debug.Log($"[RefreshCards] myPlayerId={fightSystem.myPlayerId}");
                Debug.Log($"[RefreshCards] player.cardInGame={fightSystem.player?.cardInGame?.cardName}, enemy.cardInGame={fightSystem.enemy?.cardInGame?.cardName}");
                
                Kard card = null;
                if (playerId == fightSystem.myPlayerId && fightSystem.player?.cardInGame != null)
                {
                    card = fightSystem.player.cardInGame;
                    Debug.Log($"[RefreshCards] ✅ Mapped to MY card: {card.cardName}");
                }
                else if (playerId != fightSystem.myPlayerId && fightSystem.enemy?.cardInGame != null)
                {
                    card = fightSystem.enemy.cardInGame;
                    Debug.Log($"[RefreshCards] ✅ Mapped to ENEMY card: {card.cardName}");
                }
                
                if (card != null)
                {
                    Debug.LogWarning($"🔄 [REFRESH] Updating {card.cardName} from server:");
                    Debug.LogWarning($"🔄 [REFRESH]   - Current Kard.health: {card.health}/{card.maxHealth}");
                    Debug.LogWarning($"🔄 [REFRESH]   - Server cardData.health: {cardData.health}/{cardData.maxHealth}");
                    
                    // ✅ Zachytaj stat changes pre animácie
                    int oldHealth = card.health;  // ✅ DEBUG: Track health change
                    int oldStrength = card.strength;
                    int oldDefense = card.defense;
                    int oldSpeed = card.speed;
                    int oldKnowledge = card.knowledge;
                    
                    // Aplikuj live stats z servera
                    card.health = cardData.health;
                    card.maxHealth = cardData.maxHealth;  // ✅ Update maxHealth!
                    card.strength = cardData.strength;  // ✅ Buffs/debuffs!
                    card.defense = cardData.defense;
                    card.speed = cardData.speed;
                    card.knowledge = cardData.knowledge;
                    
                    Debug.LogWarning($"🔄 [REFRESH]   - After update Kard.health: {card.health}/{card.maxHealth} (change: {card.health - oldHealth})");
                    
                    // ⚠️ CRITICAL: Sync HP bar after server refresh!
                    if (playerId == fightSystem.myPlayerId)
                    {
                        playerLifeBar.SetHP(card.health);
                        Debug.LogWarning($"🔄 [REFRESH] playerLifeBar.SetHP({card.health}) - MY card synced");
                    }
                    else
                    {
                        enemyLifeBar.SetHP(card.health);
                        Debug.LogWarning($"🔄 [REFRESH] enemyLifeBar.SetHP({card.health}) - ENEMY card synced");
                    }
                    
                    // ✅ Animuj stat changes ak sa zmenili
                    if (cardAnimator != null)
                    {
                        int strChange = card.strength - oldStrength;
                        int defChange = card.defense - oldDefense;
                        int spdChange = card.speed - oldSpeed;
                        int knoChange = card.knowledge - oldKnowledge;
                        
                        if (strChange != 0)
                        {
                            Debug.LogWarning($"🔄 [REFRESH] STR changed: {oldStrength} → {card.strength} ({strChange:+#;-#;0})");
                            StartCoroutine(cardAnimator.AnimateStatChange(card, strChange, "STR"));
                        }
                        if (defChange != 0)
                        {
                            Debug.LogWarning($"🔄 [REFRESH] DEF changed: {oldDefense} → {card.defense} ({defChange:+#;-#;0})");
                            StartCoroutine(cardAnimator.AnimateStatChange(card, defChange, "DEF"));
                        }
                        if (spdChange != 0)
                        {
                            Debug.LogWarning($"🔄 [REFRESH] SPD changed: {oldSpeed} → {card.speed} ({spdChange:+#;-#;0})");
                            StartCoroutine(cardAnimator.AnimateStatChange(card, spdChange, "SPD"));
                        }
                        if (knoChange != 0)
                        {
                            Debug.LogWarning($"🔄 [REFRESH] KNO changed: {oldKnowledge} → {card.knowledge} ({knoChange:+#;-#;0})");
                            StartCoroutine(cardAnimator.AnimateStatChange(card, knoChange, "KNO"));
                        }
                    }
                    
                    // ✅ Aplikuj effects (burn, sleep, atď.)
                    if (cardData.effects != null && cardData.effects.Length > 0)
                    {
                        foreach (var effect in cardData.effects)
                        {
                            Debug.Log($"[BattleResultProcessor] {card.cardName} has effect: {effect.type} (duration: {effect.duration})");
                            
                            // ✅ V10: Pridaj effect ikony ak chýbajú (synchronizácia s DB)
                            int effectType = int.Parse(effect.type.ToString());
                            string effectName = GetEffectName(effectType);
                            if (!string.IsNullOrEmpty(effectName))
                            {
                                // Check if icon already exists
                                bool hasIcon = false;
                                foreach (Transform child in card.effectIconContainer)
                                {
                                    if (child.name.StartsWith(effectName + "Icon"))
                                    {
                                        hasIcon = true;
                                        break;
                                    }
                                }
                                
                                if (!hasIcon)
                                {
                                    Debug.LogWarning($"🔄 [REFRESH] Adding missing {effectName} icon to {card.cardName}");
                                    card.AddEffectIcon(effectName);
                                }
                            }
                        }
                    }
                    else
                    {
                        // ✅ V10: Ak DB nemá žiadne effects, odstráň všetky ikony (cleanup)
                        Debug.LogWarning($"🔄 [REFRESH] {card.cardName} has NO effects in DB - removing all effect icons");
                        
                        // Remove all effect icons
                        List<Transform> iconsToRemove = new List<Transform>();
                        foreach (Transform child in card.effectIconContainer)
                        {
                            iconsToRemove.Add(child);
                        }
                        
                        foreach (Transform icon in iconsToRemove)
                        {
                            Debug.LogWarning($"🔄 [REFRESH] Removing orphaned icon: {icon.name}");
                            Destroy(icon.gameObject);
                        }
                        
                        if (iconsToRemove.Count > 0)
                        {
                            card.RepositionEffectIcons();
                        }
                    }
                    
                    Debug.Log($"[BattleResultProcessor] Updated {card.cardName}: HP={card.health}/{card.maxHealth}, STR={card.strength}, DEF={card.defense}");
                }
            }
        }
    }
    
    /// <summary>
    /// Prehrá battle animácie s postupným HP updateom
    /// V5: Používa cardId na určenie kto útočil prvý
    /// V8: Pridané attackId pre dynamické animácie (reuse Attack.cs metód)
    /// V9: Pridané healAmount pre self-heal animácie + Sleep blocking (blocked/wokeUp flags) + effect ikony v správnom poradí + blockedBy field
    /// V10: Pridané recovered/selfDamage pre Asceticism effect
    /// V11.1: Multiple effects arrays + attackerSelfDamage/attackerEffectsApplied pre AoE/recoil attacks
    /// V11.2: Pridané bleedDamage pre Bleed effect processing (Priority 1)
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
        string enemyAttackResult)
    {
        bool iAttackedFirst = (firstAttackerCardId == myCardId);
        
        Debug.LogWarning($"[PlayBattleAnimations] FirstAttackerCardId={firstAttackerCardId}, MyCardId={myCardId}, IAttackedFirst={iAttackedFirst}");
        Debug.LogWarning($"[PlayBattleAnimations] MyAttackId={myAttackId}, EnemyAttackId={enemyAttackId}");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations == null)
        {
            Debug.LogError("[BattleResultProcessor] AttackAnimations not found!");
            yield break;
        }
        
        if (iAttackedFirst)
        {
            // ✅ JA ÚTOČÍM PRVÝ
            // ✅ PRIORITY 1: Bleed damage FIRST (before blocking/recovery checks)
            // ✅ V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
            if (myBleedDamages != null && myBleedDamages.Count > 0)
            {
                Debug.LogWarning($"🩸 [BLEED] MY card has {myBleedDamages.Count} Bleed effects!");
                for (int i = 0; i < myBleedDamages.Count; i++)
                {
                    int bleedDamage = myBleedDamages[i];
                    Debug.LogWarning($"🩸 [BLEED #{i + 1}] MY card takes {bleedDamage} damage");
                    myCard.health -= bleedDamage;
                    yield return StartCoroutine(PlaySingleBleedAnimation(myCard, bleedDamage, true));
                    if (i < myBleedDamages.Count - 1)
                    {
                        yield return new WaitForSeconds(0.3f);  // Small delay between Bleeds
                    }
                }
            }
            
            // ✅ PRIORITY 1: Exposure damage (same priority as Bleed)
            if (myExposureDamage > 0 || myExposureRemoved)
            {
                Debug.LogWarning($"☢️ [EXPOSURE] MY card - damage={myExposureDamage}, removed={myExposureRemoved}");
                myCard.health -= myExposureDamage;
                yield return StartCoroutine(PlayExposureAnimation(myCard, myExposureDamage, myExposureRemoved, true));
                yield return new WaitForSeconds(0.3f);
            }
            
            // ✅ PRIORITY 3: Skontroluj Asceticism recovery (blocking effect)
            if (myRecovered)
            {
                Debug.LogWarning($"🙏 [ASCETICISM] MY card RECOVERED from Asceticism before attack!");
                yield return StartCoroutine(PlayRecoveryAnimation(myCard, "My card"));
                yield return new WaitForSeconds(0.5f);
            }
            
            // ✅ PRIORITY 3: Skontroluj Sleep blocking/wake-up (blocking effect)
            if (myWokeUp)
            {
                // Zobraz wake-up animáciu pred útokom
                yield return StartCoroutine(PlayWakeUpAnimation(myCard, true));
            }
            
            if (!myAttackBlocked)
            {
                // ✅ Set color to BLUE for entire attack sequence (like singleplayer)
                if (dialogText != null) dialogText.color = Color.blue;
                
                // ✅ Execute attack animation
                yield return StartCoroutine(ExecuteAttackAnimation(myCard, enemyCard, myAttackId, enemyDamage, myHealAmount, true, myAttackerSelfDamage, myEffectsApplied, myAttackResult, myAttackerEffects));
                
                Debug.LogWarning($"📊 [APPLYING_STATS] My turn - attackerKno={myStatChanges.attackerKnowledge}, defenderKno={myStatChanges.defenderKnowledge}");
                
                // ✅ V12: Apply stat changes IMMEDIATELY after attack
                if (myStatChanges.attackerAttack != 0) myCard.HandleAttack(myStatChanges.attackerAttack);
                if (myStatChanges.attackerStrength != 0) myCard.HandleStrength(myStatChanges.attackerStrength);
                if (myStatChanges.attackerDefense != 0) myCard.HandleDefense(myStatChanges.attackerDefense);
                if (myStatChanges.attackerKnowledge != 0) { Debug.LogWarning($"📚 Applying my self-knowledge buff: {myStatChanges.attackerKnowledge}"); myCard.HandleKnowledge(myStatChanges.attackerKnowledge); }
                if (myStatChanges.attackerSpeed != 0) myCard.HandleSpeed(myStatChanges.attackerSpeed);
                if (myStatChanges.attackerCharisma != 0) myCard.HandleCharisma(myStatChanges.attackerCharisma);
                
                if (myStatChanges.defenderAttack != 0) enemyCard.HandleAttack(myStatChanges.defenderAttack);
                if (myStatChanges.defenderStrength != 0) enemyCard.HandleStrength(myStatChanges.defenderStrength);
                if (myStatChanges.defenderDefense != 0) enemyCard.HandleDefense(myStatChanges.defenderDefense);
                if (myStatChanges.defenderKnowledge != 0) { Debug.LogWarning($"📚 Applying defender knowledge change: {myStatChanges.defenderKnowledge} to ENEMY card"); enemyCard.HandleKnowledge(myStatChanges.defenderKnowledge); }
                if (myStatChanges.defenderSpeed != 0) enemyCard.HandleSpeed(myStatChanges.defenderSpeed);
                if (myStatChanges.defenderCharisma != 0) enemyCard.HandleCharisma(myStatChanges.defenderCharisma);
                
                // ✅ V11.1: Display multiple effects on defender
                if (myEffectsApplied != null && myEffectsApplied.Count > 0)
                {
                    yield return StartCoroutine(DisplayMultipleEffects(enemyCard, myEffectsApplied, false));
                }
                
                // ✅ V11.1: Attacker self-damage (CarHit recoil)
                // Skip for CarHit (attackId 7) - already handled in case 7
                if (myAttackerSelfDamage > 0 && myAttackId != 7)
                {
                    Debug.LogWarning($"💥 [SELF-DAMAGE] MY card takes {myAttackerSelfDamage} recoil damage!");
                    myCard.health -= myAttackerSelfDamage;
                    yield return StartCoroutine(PlaySelfDamageAnimation(myCard, myAttackerSelfDamage, true));
                }
                
                // ✅ V11.1: Attacker self-effects (CarHit recoil bleed/sleep)
                if (myAttackerEffects != null && myAttackerEffects.Count > 0)
                {
                    yield return StartCoroutine(DisplayMultipleEffects(myCard, myAttackerEffects, true));
                }
            }
            else
            {
                // ✅ V10: Útok blocked by effect (Sleep, Asceticism, atď.)
                yield return StartCoroutine(PlayBlockAnimation(myCard, myBlockedBy, true));
                
                // ✅ V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                if (myBlockedBy == 2 && mySelfDamage > 0)
                {
                    Debug.LogWarning($"🙏💔 [ASCETICISM] MY card takes {mySelfDamage} self-damage due to blocking!");
                    myCard.health -= mySelfDamage;
                    yield return StartCoroutine(PlaySelfDamageAnimation(myCard, mySelfDamage, true));
                }
            }
            
            // ✅ AK NEPRIATEĽ PREŽIL, JEHO ÚTOK
            if (enemyCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);
                
                // ✅ Set dialog color to RED for enemy attack (like singleplayer)
                if (dialogText != null) dialogText.color = Color.red;
                
                // ✅ PRIORITY 1: Enemy Bleed damage FIRST
                // ✅ V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
                if (enemyBleedDamages != null && enemyBleedDamages.Count > 0)
                {
                    Debug.LogWarning($"🩸 [BLEED] ENEMY card has {enemyBleedDamages.Count} Bleed effects!");
                    for (int i = 0; i < enemyBleedDamages.Count; i++)
                    {
                        int bleedDamage = enemyBleedDamages[i];
                        Debug.LogWarning($"🩸 [BLEED #{i + 1}] ENEMY card takes {bleedDamage} damage");
                        enemyCard.health -= bleedDamage;
                        yield return StartCoroutine(PlaySingleBleedAnimation(enemyCard, bleedDamage, false));
                        if (i < enemyBleedDamages.Count - 1)
                        {
                            yield return new WaitForSeconds(0.3f);  // Small delay between Bleeds
                        }
                    }
                }
                
                // ✅ PRIORITY 1: Exposure damage (same priority as Bleed)
                if (enemyExposureDamage > 0 || enemyExposureRemoved)
                {
                    Debug.LogWarning($"☢️ [EXPOSURE] ENEMY card - damage={enemyExposureDamage}, removed={enemyExposureRemoved}");
                    enemyCard.health -= enemyExposureDamage;
                    yield return StartCoroutine(PlayExposureAnimation(enemyCard, enemyExposureDamage, enemyExposureRemoved, false));
                    yield return new WaitForSeconds(0.3f);
                }
                
                // ✅ PRIORITY 3: Nepriateľ Asceticism recovery check (blocking effect)
                if (enemyRecovered)
                {
                    Debug.LogWarning($"🙏 [ASCETICISM] ENEMY card RECOVERED from Asceticism before counter-attack!");
                    yield return StartCoroutine(PlayRecoveryAnimation(enemyCard, "Enemy card"));
                    yield return new WaitForSeconds(0.5f);
                }
                
                // ✅ PRIORITY 3: Nepriateľ wake-up check (blocking effect)
                if (enemyWokeUp)
                {
                    yield return StartCoroutine(PlayWakeUpAnimation(enemyCard, false));
                }
                
                if (!enemyAttackBlocked)
                {
                    // ✅ FIX: enemyCard útočí myCard → použij myDamage (damage ktorý JA dostanem)
                    yield return StartCoroutine(ExecuteAttackAnimation(enemyCard, myCard, enemyAttackId, myDamage, enemyHealAmount, false, enemyAttackerSelfDamage, enemyEffectsApplied, enemyAttackResult, enemyAttackerEffects));
                    
                    Debug.LogWarning($"📊 [APPLYING_STATS] Enemy turn - attackerKno={enemyStatChanges.attackerKnowledge}, defenderKno={enemyStatChanges.defenderKnowledge}");
                    
                    // ✅ V12: Apply stat changes IMMEDIATELY after attack (POČAS enemy tahu!)
                    // Attacker self-buffs (enemy buffs himself)
                    if (enemyStatChanges.attackerAttack != 0) enemyCard.HandleAttack(enemyStatChanges.attackerAttack);
                    if (enemyStatChanges.attackerStrength != 0) enemyCard.HandleStrength(enemyStatChanges.attackerStrength);
                    if (enemyStatChanges.attackerDefense != 0) enemyCard.HandleDefense(enemyStatChanges.attackerDefense);
                    if (enemyStatChanges.attackerKnowledge != 0) { Debug.LogWarning($"📚 Applying enemy self-knowledge buff: {enemyStatChanges.attackerKnowledge}"); enemyCard.HandleKnowledge(enemyStatChanges.attackerKnowledge); }
                    if (enemyStatChanges.attackerSpeed != 0) enemyCard.HandleSpeed(enemyStatChanges.attackerSpeed);
                    if (enemyStatChanges.attackerCharisma != 0) enemyCard.HandleCharisma(enemyStatChanges.attackerCharisma);
                    
                    // Defender debuffs (enemy debuffs me)
                    if (enemyStatChanges.defenderAttack != 0) myCard.HandleAttack(enemyStatChanges.defenderAttack);
                    if (enemyStatChanges.defenderStrength != 0) myCard.HandleStrength(enemyStatChanges.defenderStrength);
                    if (enemyStatChanges.defenderDefense != 0) myCard.HandleDefense(enemyStatChanges.defenderDefense);
                    if (enemyStatChanges.defenderKnowledge != 0) { Debug.LogWarning($"📚 Applying defender knowledge change: {enemyStatChanges.defenderKnowledge} to MY card"); myCard.HandleKnowledge(enemyStatChanges.defenderKnowledge); }
                    if (enemyStatChanges.defenderSpeed != 0) myCard.HandleSpeed(enemyStatChanges.defenderSpeed);
                    if (enemyStatChanges.defenderCharisma != 0) myCard.HandleCharisma(enemyStatChanges.defenderCharisma);
                    
                    // ✅ V11.1: Display multiple effects on defender (me)
                    if (enemyEffectsApplied != null && enemyEffectsApplied.Count > 0)
                    {
                        yield return StartCoroutine(DisplayMultipleEffects(myCard, enemyEffectsApplied, true));
                    }
                    
                    // ✅ V11.1: Enemy attacker self-damage
                    // Skip for CarHit (attackId 7) - already handled in case 7
                    if (enemyAttackerSelfDamage > 0 && enemyAttackId != 7)
                    {
                        Debug.LogWarning($"💥 [SELF-DAMAGE] ENEMY card takes {enemyAttackerSelfDamage} recoil damage!");
                        enemyCard.health -= enemyAttackerSelfDamage;
                        yield return StartCoroutine(PlaySelfDamageAnimation(enemyCard, enemyAttackerSelfDamage, false));
                    }
                    
                    // ✅ V11.1: Enemy attacker self-effects
                    if (enemyAttackerEffects != null && enemyAttackerEffects.Count > 0)
                    {
                        yield return StartCoroutine(DisplayMultipleEffects(enemyCard, enemyAttackerEffects, false));
                    }
                }
                else
                {
                    // ✅ V10: Útok blocked by effect (Sleep, Asceticism, atď.)
                    yield return StartCoroutine(PlayBlockAnimation(enemyCard, enemyBlockedBy, false));
                    
                    // ✅ V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                    if (enemyBlockedBy == 2 && enemySelfDamage > 0)
                    {
                        Debug.LogWarning($"🙏💔 [ASCETICISM] ENEMY card takes {enemySelfDamage} self-damage due to blocking!");
                        enemyCard.health -= enemySelfDamage;
                        yield return StartCoroutine(PlaySelfDamageAnimation(enemyCard, enemySelfDamage, false));
                    }
                }
            }
        }
        else
        {
            // ✅ NEPRIATEĽ ÚTOČÍ PRVÝ
            // ✅ Set dialog color to RED for enemy attack (like singleplayer)
            if (dialogText != null) dialogText.color = Color.red;
            
            // ✅ PRIORITY 1: Enemy Bleed damage FIRST
            // ✅ V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
            if (enemyBleedDamages != null && enemyBleedDamages.Count > 0)
            {
                Debug.LogWarning($"🩸 [BLEED] ENEMY card has {enemyBleedDamages.Count} Bleed effects!");
                for (int i = 0; i < enemyBleedDamages.Count; i++)
                {
                    int bleedDamage = enemyBleedDamages[i];
                    Debug.LogWarning($"🩸 [BLEED #{i + 1}] ENEMY card takes {bleedDamage} damage");
                    enemyCard.health -= bleedDamage;
                    yield return StartCoroutine(PlaySingleBleedAnimation(enemyCard, bleedDamage, false));
                    if (i < enemyBleedDamages.Count - 1)
                    {
                        yield return new WaitForSeconds(0.3f);  // Small delay between Bleeds
                    }
                }
            }
            
            // ✅ PRIORITY 1: Exposure damage (same priority as Bleed)
            if (enemyExposureDamage > 0 || enemyExposureRemoved)
            {
                Debug.LogWarning($"☢️ [EXPOSURE] ENEMY card - damage={enemyExposureDamage}, removed={enemyExposureRemoved}");
                enemyCard.health -= enemyExposureDamage;
                yield return StartCoroutine(PlayExposureAnimation(enemyCard, enemyExposureDamage, enemyExposureRemoved, false));
                yield return new WaitForSeconds(0.3f);
            }
            
            // ✅ PRIORITY 3: Nepriateľ Asceticism recovery check (blocking effect)
            if (enemyRecovered)
            {
                Debug.LogWarning($"🙏 [ASCETICISM] ENEMY card RECOVERED from Asceticism before attack!");
                yield return StartCoroutine(PlayRecoveryAnimation(enemyCard, "Enemy card"));
                yield return new WaitForSeconds(0.5f);
            }
            
            // ✅ PRIORITY 3: Nepriateľ wake-up check (blocking effect)
            if (enemyWokeUp)
            {
                yield return StartCoroutine(PlayWakeUpAnimation(enemyCard, false));
            }
            
            if (!enemyAttackBlocked)
            {
                // ✅ FIX: enemyCard útočí myCard → použij myDamage (damage ktorý JA dostanem)
                yield return StartCoroutine(ExecuteAttackAnimation(enemyCard, myCard, enemyAttackId, myDamage, enemyHealAmount, false, enemyAttackerSelfDamage, enemyEffectsApplied, enemyAttackResult, enemyAttackerEffects));
                
                Debug.LogWarning($"📊 [APPLYING_STATS] Enemy turn - attackerKno={enemyStatChanges.attackerKnowledge}, defenderKno={enemyStatChanges.defenderKnowledge}");
                
                // ✅ V12: Apply stat changes IMMEDIATELY after attack
                if (enemyStatChanges.attackerAttack != 0) enemyCard.HandleAttack(enemyStatChanges.attackerAttack);
                if (enemyStatChanges.attackerStrength != 0) enemyCard.HandleStrength(enemyStatChanges.attackerStrength);
                if (enemyStatChanges.attackerDefense != 0) enemyCard.HandleDefense(enemyStatChanges.attackerDefense);
                if (enemyStatChanges.attackerKnowledge != 0) { Debug.LogWarning($"📚 Applying enemy self-knowledge buff: {enemyStatChanges.attackerKnowledge}"); enemyCard.HandleKnowledge(enemyStatChanges.attackerKnowledge); }
                if (enemyStatChanges.attackerSpeed != 0) enemyCard.HandleSpeed(enemyStatChanges.attackerSpeed);
                if (enemyStatChanges.attackerCharisma != 0) enemyCard.HandleCharisma(enemyStatChanges.attackerCharisma);
                
                if (enemyStatChanges.defenderAttack != 0) myCard.HandleAttack(enemyStatChanges.defenderAttack);
                if (enemyStatChanges.defenderStrength != 0) myCard.HandleStrength(enemyStatChanges.defenderStrength);
                if (enemyStatChanges.defenderDefense != 0) myCard.HandleDefense(enemyStatChanges.defenderDefense);
                if (enemyStatChanges.defenderKnowledge != 0) { Debug.LogWarning($"📚 Applying defender knowledge change: {enemyStatChanges.defenderKnowledge} to MY card"); myCard.HandleKnowledge(enemyStatChanges.defenderKnowledge); }
                if (enemyStatChanges.defenderSpeed != 0) myCard.HandleSpeed(enemyStatChanges.defenderSpeed);
                if (enemyStatChanges.defenderCharisma != 0) myCard.HandleCharisma(enemyStatChanges.defenderCharisma);
                
                // ✅ V11.1: Display multiple effects on defender (me)
                if (enemyEffectsApplied != null && enemyEffectsApplied.Count > 0)
                {
                    yield return StartCoroutine(DisplayMultipleEffects(myCard, enemyEffectsApplied, true));
                }
                
                // ✅ V11.1: Enemy attacker self-damage
                if (enemyAttackerSelfDamage > 0)
                {
                    Debug.LogWarning($"💥 [SELF-DAMAGE] ENEMY card takes {enemyAttackerSelfDamage} recoil damage!");
                    enemyCard.health -= enemyAttackerSelfDamage;
                    yield return StartCoroutine(PlaySelfDamageAnimation(enemyCard, enemyAttackerSelfDamage, false));
                }
                
                // ✅ V11.1: Enemy attacker self-effects
                if (enemyAttackerEffects != null && enemyAttackerEffects.Count > 0)
                {
                    yield return StartCoroutine(DisplayMultipleEffects(enemyCard, enemyAttackerEffects, false));
                }
            }
            else
            {
                // ✅ V10: Útok blocked by effect (Sleep, Asceticism, atď.)
                yield return StartCoroutine(PlayBlockAnimation(enemyCard, enemyBlockedBy, false));
                
                // ✅ V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                if (enemyBlockedBy == 2 && enemySelfDamage > 0)
                {
                    Debug.LogWarning($"🙏💔 [ASCETICISM] ENEMY card takes {enemySelfDamage} self-damage due to blocking!");
                    enemyCard.health -= enemySelfDamage;
                    yield return StartCoroutine(PlaySelfDamageAnimation(enemyCard, enemySelfDamage, false));
                }
            }
            
            // ✅ AK JA PREŽIJEM, MÔJ ÚTOK
            if (myCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);
                
                // ✅ Set color to BLUE for entire attack sequence (like singleplayer)
                if (dialogText != null) dialogText.color = Color.blue;
                
                // ✅ PRIORITY 1: My Bleed damage FIRST
                // ✅ V11.2: Play INDIVIDUAL Bleed animations for each Bleed effect
                if (myBleedDamages != null && myBleedDamages.Count > 0)
                {
                    Debug.LogWarning($"🩸 [BLEED] MY card has {myBleedDamages.Count} Bleed effects!");
                    for (int i = 0; i < myBleedDamages.Count; i++)
                    {
                        int bleedDamage = myBleedDamages[i];
                        Debug.LogWarning($"🩸 [BLEED #{i + 1}] MY card takes {bleedDamage} damage");
                        myCard.health -= bleedDamage;
                        yield return StartCoroutine(PlaySingleBleedAnimation(myCard, bleedDamage, true));
                        if (i < myBleedDamages.Count - 1)
                        {
                            yield return new WaitForSeconds(0.3f);  // Small delay between Bleeds
                        }
                    }
                }
                
                // ✅ PRIORITY 1: Exposure damage (same priority as Bleed)
                if (myExposureDamage > 0 || myExposureRemoved)
                {
                    Debug.LogWarning($"☢️ [EXPOSURE] MY card - damage={myExposureDamage}, removed={myExposureRemoved}");
                    myCard.health -= myExposureDamage;
                    yield return StartCoroutine(PlayExposureAnimation(myCard, myExposureDamage, myExposureRemoved, true));
                    yield return new WaitForSeconds(0.3f);
                }
                
                // ✅ PRIORITY 3: Môj Asceticism recovery check (blocking effect)
                if (myRecovered)
                {
                    Debug.LogWarning($"🙏 [ASCETICISM] MY card RECOVERED from Asceticism before counter-attack!");
                    yield return StartCoroutine(PlayRecoveryAnimation(myCard, "My card"));
                    yield return new WaitForSeconds(0.5f);
                }
                
                // ✅ PRIORITY 3: Môj wake-up check (blocking effect)
                if (myWokeUp)
                {
                    yield return StartCoroutine(PlayWakeUpAnimation(myCard, true));
                }
                
                if (!myAttackBlocked)
                {
                    // ✅ FIX: myCard útočí enemyCard → použij enemyDamage (damage ktorý ENEMY dostane)
                    yield return StartCoroutine(ExecuteAttackAnimation(myCard, enemyCard, myAttackId, enemyDamage, myHealAmount, true, myAttackerSelfDamage, myEffectsApplied, myAttackResult, myAttackerEffects));
                    
                    Debug.LogWarning($"📊 [APPLYING_STATS] My turn - attackerKno={myStatChanges.attackerKnowledge}, defenderKno={myStatChanges.defenderKnowledge}");
                    
                    // ✅ V12: Apply stat changes IMMEDIATELY after attack (POČAS môjho tahu!)
                    // Attacker self-buffs (I buff myself)
                    if (myStatChanges.attackerAttack != 0) myCard.HandleAttack(myStatChanges.attackerAttack);
                    if (myStatChanges.attackerStrength != 0) myCard.HandleStrength(myStatChanges.attackerStrength);
                    if (myStatChanges.attackerDefense != 0) myCard.HandleDefense(myStatChanges.attackerDefense);
                    if (myStatChanges.attackerKnowledge != 0) { Debug.LogWarning($"📚 Applying my self-knowledge buff: {myStatChanges.attackerKnowledge}"); myCard.HandleKnowledge(myStatChanges.attackerKnowledge); }
                    if (myStatChanges.attackerSpeed != 0) myCard.HandleSpeed(myStatChanges.attackerSpeed);
                    if (myStatChanges.attackerCharisma != 0) myCard.HandleCharisma(myStatChanges.attackerCharisma);
                    
                    // Defender debuffs (I debuff enemy)
                    if (myStatChanges.defenderAttack != 0) enemyCard.HandleAttack(myStatChanges.defenderAttack);
                    if (myStatChanges.defenderStrength != 0) enemyCard.HandleStrength(myStatChanges.defenderStrength);
                    if (myStatChanges.defenderDefense != 0) enemyCard.HandleDefense(myStatChanges.defenderDefense);
                    if (myStatChanges.defenderKnowledge != 0) { Debug.LogWarning($"📚 Applying defender knowledge change: {myStatChanges.defenderKnowledge} to ENEMY card"); enemyCard.HandleKnowledge(myStatChanges.defenderKnowledge); }
                    if (myStatChanges.defenderSpeed != 0) enemyCard.HandleSpeed(myStatChanges.defenderSpeed);
                    if (myStatChanges.defenderCharisma != 0) enemyCard.HandleCharisma(myStatChanges.defenderCharisma);
                    
                    // ✅ V11.1: Display multiple effects on defender (enemy)
                    if (myEffectsApplied != null && myEffectsApplied.Count > 0)
                    {
                        yield return StartCoroutine(DisplayMultipleEffects(enemyCard, myEffectsApplied, false));
                    }
                    
                    // ✅ V11.1: My attacker self-damage
                    // Skip for CarHit (attackId 7) - already handled in case 7
                    if (myAttackerSelfDamage > 0 && myAttackId != 7)
                    {
                        Debug.LogWarning($"💥 [SELF-DAMAGE] MY card takes {myAttackerSelfDamage} recoil damage!");
                        myCard.health -= myAttackerSelfDamage;
                        yield return StartCoroutine(PlaySelfDamageAnimation(myCard, myAttackerSelfDamage, true));
                    }
                    
                    // ✅ V11.1: My attacker self-effects
                    if (myAttackerEffects != null && myAttackerEffects.Count > 0)
                    {
                        yield return StartCoroutine(DisplayMultipleEffects(myCard, myAttackerEffects, true));
                    }
                }
                else
                {
                    // ✅ V10: Útok blocked by effect (Sleep, Asceticism, atď.)
                    yield return StartCoroutine(PlayBlockAnimation(myCard, myBlockedBy, true));
                    
                    // ✅ V10: Ak je blocked Asceticism-om (type 2), aplikuj self-damage
                    if (myBlockedBy == 2 && mySelfDamage > 0)
                    {
                        Debug.LogWarning($"🙏💔 [ASCETICISM] MY card takes {mySelfDamage} self-damage due to blocking!");
                        myCard.health -= mySelfDamage;
                        yield return StartCoroutine(PlaySelfDamageAnimation(myCard, mySelfDamage, true));
                    }
                }
            }
        }
        
        // ✅ Reset dialog color to BLACK after battle (for neutral messages like "Choose your attack")
        if (dialogText != null) dialogText.color = Color.black;
        
        // ✅ V11.2: Immediately show "Preparing next turn..." to mask color transition
        yield return StartCoroutine(ShowDialog("Preparing next turn..."));
        
        // ✅ V5: HP sa updatuje postupne počas animácií, žiadna finálna sync!
        // selectedCards refresh sa volá v PlayBattleAnimationsAndRefresh
        
        // ✅ Reset card positions na správne miesta
        if (cardAnimator != null)
        {
            Vector3 playerBoardPos = fightSystem.playerBoard != null ? fightSystem.playerBoard.transform.position : myCard.transform.position;
            Vector3 enemyBoardPos = fightSystem.enemyBoard != null ? fightSystem.enemyBoard.transform.position : enemyCard.transform.position;
            
            StartCoroutine(cardAnimator.ResetCardPosition(myCard, playerBoardPos, Quaternion.identity));
            StartCoroutine(cardAnimator.ResetCardPosition(enemyCard, enemyBoardPos, Quaternion.identity));
        }
        
        // Skontroluj výsledok
        yield return new WaitForSeconds(1f);
        CheckBattleOutcome(myCard, enemyCard);
    }
    
    /// <summary>
    /// Vykoná animáciu pre konkrétny útok (router pattern)
    /// V8: Podporuje Attack ID 1 (Punch), 2 (Kick), 3 (Heal), ... rozširiteľné
    /// V9: Heal support - self-heal attacks s healAmount + zelená HP animácia
    /// V11.2: CarHit - attackerSelfDamage pre súčasné animácie damage
    /// V12: Modular - každý útok má svoj handler v AttackHandlers/ folder
    /// V14: attackerEffects - effects applied to attacker SELF (backfire Sleep from UpInSmoke)
    /// </summary>
    private IEnumerator ExecuteAttackAnimation(Kard attacker, Kard defender, int attackId, int damage, int healAmount, bool isMyAttack, int attackerSelfDamage = 0, List<Dictionary<string, object>> effectsApplied = null, string attackResult = null, List<Dictionary<string, object>> attackerEffects = null)
    {
        AttackAnimations animations = attackComponent.attackAnimations;
        
        // ✅ Router pattern - delegate to attack handlers
        switch (attackId)
        {
            case 1: // Punch
                yield return Attack1Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog,
                    effectsApplied);
                break;
                
            case 2: // Kick
                yield return Attack2Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog);
                break;
                
            case 3: // Heal
                yield return Attack3Handler.Execute(
                    attacker, defender, healAmount, isMyAttack,
                    animations, playerLifeBar, enemyLifeBar, ShowDialog);
                break;
                
            case 4: // Forgiveness
                yield return Attack4Handler.Execute(
                    attacker, defender, animations, ShowDialog,
                    effectsApplied);
                break;
                
            case 5: // Crusade
                yield return Attack5Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog);
                break;
                
            case 6: // Water To Wine
                yield return Attack6Handler.Execute(
                    attacker, animations, ShowDialog);
                break;
                
            case 7: // CarHit
                yield return Attack7Handler.Execute(
                    attacker, defender, damage, attackerSelfDamage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog,
                    effectsApplied, attackerEffects);
                break;
                
            case 8: // MonkeyWrench
                yield return Attack8Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog, effectsApplied);
                break;
                
            case 9: // Radiation
                yield return Attack9Handler.Execute(
                    attacker, defender, animations, ShowDialog);
                break;
            
            case 10: // Scratch
                yield return Attack10Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog,
                    effectsApplied);
                break;
            
            case 11: // Scientific Lecture
                yield return Attack11Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog,
                    attackResult);  // ✅ Server decides animation, client is dumb renderer
                break;
            
            case 12: // Chi Sau
                yield return Attack12Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog);
                break;
            
            case 13: // One Inch Punch
                yield return Attack13Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog);
                break;
            
            case 14: // Up In Smoke
                yield return Attack14Handler.Execute(
                    attacker, defender, healAmount, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog,
                    attackerEffects);
                break;
                
            // ✅ TODO: Add case 15-123 - just add 3 lines per attack!
            
            default:
                Debug.LogWarning($"[ExecuteAttackAnimation] Unknown attackId={attackId}, using Punch as fallback");
                yield return Attack1Handler.Execute(
                    attacker, defender, damage, isMyAttack,
                    animations, cardAnimator, playerLifeBar, enemyLifeBar, ShowDialog);
                break;
        }
    }
    
    /// <summary>
    /// Pridá len ikonu efektu bez animácie (V9.1: KO animácia sa hrá v ExecuteAttackAnimation)
    /// </summary>
    private IEnumerator AddEffectIconOnly(Kard card, Dictionary<string, object> effectData, bool isMyCard)
    {
        int effectType = int.Parse(effectData["type"].ToString());
        int duration = int.Parse(effectData["duration"].ToString());
        
        Debug.LogWarning($"🎭 [EFFECT_ICON] Adding ICON ONLY on {card.cardName}: type={effectType}, duration={duration}");
        
        // Pridá effect ikonu (reuse Kard.AddEffectIcon)
        string effectName = GetEffectName(effectType);
        if (!string.IsNullOrEmpty(effectName))
        {
            card.AddEffectIcon(effectName);
            card.RepositionEffectIcons();
            Debug.LogWarning($"🎭 [EFFECT_ICON] Added {effectName} icon to {card.cardName}");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// ✅ V11.1: Displays MULTIPLE effect icons on a card (for Bleed stacking, AoE attacks)
    /// Iterates through all effects in the array and displays each one
    /// </summary>
    private IEnumerator DisplayMultipleEffects(Kard card, List<Dictionary<string, object>> effectsArray, bool isMyCard)
    {
        if (effectsArray == null || effectsArray.Count == 0)
        {
            yield break;
        }
        
        Debug.LogWarning($"🎭 [MULTI-EFFECTS] Displaying {effectsArray.Count} effects on {card.cardName}");
        
        foreach (var effect in effectsArray)
        {
            yield return StartCoroutine(DisplayEffectIcon(card, effect, isMyCard));
            yield return new WaitForSeconds(0.3f);  // Slight delay between multiple effects
        }
    }
    
    /// <summary>
    /// Zobrazí effect ikonu na karte (V9: Sleep, Bleed, Burn, Poison...)
    /// VOLÁ SA keď sa NOVÝ effect aplikuje (Turn 1 aplikácie)
    /// DEPRECATED V9.1: Použite AddEffectIconOnly, KO animácia sa hrá v ExecuteAttackAnimation
    /// </summary>
    private IEnumerator DisplayEffectIcon(Kard card, Dictionary<string, object> effectData, bool isMyCard)
    {
        int effectType = int.Parse(effectData["type"].ToString());
        int duration = int.Parse(effectData["duration"].ToString());
        
        Debug.LogWarning($"🎭 [EFFECT_ICON] Adding effect icon to {card.cardName}: type={effectType}, duration={duration}");
        
        // ✅ V14: Initial effect animations moved to Attack Handlers
        // DisplayEffectIcon now ONLY adds icons, NO animations
        
        // ✅ Pridá effect ikonu (reuse Kard.AddEffectIcon)
        string effectName = GetEffectName(effectType);
        if (!string.IsNullOrEmpty(effectName))
        {
            card.AddEffectIcon(effectName);
            card.RepositionEffectIcons();
            Debug.LogWarning($"🎭 [EFFECT_ICON] Added {effectName} icon to {card.cardName}");
        }
        
        yield break;
    }
    
    /// <summary>
    /// Prehrá wake-up animáciu (Sleep duration = 0)
    /// </summary>
    private IEnumerator PlayWakeUpAnimation(Kard card, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        Debug.LogWarning($"⏰ [WAKE_UP] {cardOwner} card ({card.cardName}) is waking up!");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            // Reuse singleplayer wake-up animation
            yield return StartCoroutine(animations.PlaySleepEndAnimation(card.transform));
        }
        
        // ✅ Odstráň Sleep ikonu po prebratí
        string sleepEffectName = GetEffectName(3); // 3 = Sleep
        if (!string.IsNullOrEmpty(sleepEffectName))
        {
            yield return StartCoroutine(card.RemoveEffectIcon(sleepEffectName));
            Debug.LogWarning($"⏰ [WAKE_UP] Removed {sleepEffectName} icon from {card.cardName}");
        }
        
        yield return StartCoroutine(ShowDialog($"{card.cardName} wakes up!"));
    }
    
    /// <summary>
    /// Prehrá recovery animáciu (Asceticism duration = 0)
    /// V10: Pridané pre Asceticism effect
    /// </summary>
    private IEnumerator PlayRecoveryAnimation(Kard card, string cardDescription)
    {
        Debug.LogWarning($"🙏 [RECOVERY] {cardDescription} ({card.cardName}) recovered from Asceticism!");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            Debug.LogWarning($"🙏 [RECOVERY] Playing PlayAscetismEndAnimation...");
            // ✅ Použij Asceticism end animáciu
            yield return StartCoroutine(animations.PlayAscetismEndAnimation(card.transform));
            Debug.LogWarning($"🙏 [RECOVERY] PlayAscetismEndAnimation finished");
        }
        else
        {
            Debug.LogError($"🙏 [RECOVERY] AttackAnimations is NULL!");
        }
        
        // ✅ Odstráň Asceticism ikonu po recovery
        string asceticismEffectName = GetEffectName(2); // 2 = Asceticism
        Debug.LogWarning($"🙏 [RECOVERY] Effect name for type 2: {asceticismEffectName}");
        
        if (!string.IsNullOrEmpty(asceticismEffectName))
        {
            Debug.LogWarning($"🙏 [RECOVERY] Attempting to remove {asceticismEffectName} icon from {card.cardName}...");
            yield return StartCoroutine(card.RemoveEffectIcon(asceticismEffectName));
            Debug.LogWarning($"🙏 [RECOVERY] Removed {asceticismEffectName} icon from {card.cardName}");
        }
        else
        {
            Debug.LogError($"🙏 [RECOVERY] asceticismEffectName is NULL or EMPTY!");
        }
        
        yield return StartCoroutine(ShowDialog($"{card.cardName} feels blessed again!"));
    }
    
    /// <summary>
    /// Prehrá self-damage animáciu (Asceticism blocking penalty)
    /// V10: Pridané pre Asceticism effect
    /// </summary>
    private IEnumerator PlaySelfDamageAnimation(Kard card, int damage, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        Debug.LogWarning($"💔 [SELF_DAMAGE] {cardOwner} card ({card.cardName}) takes {damage} self-damage!");
        
        // ✅ Použij cardAnimator pre damage animáciu
        if (cardAnimator != null && damage > 0)
        {
            yield return StartCoroutine(cardAnimator.AnimateDamage(card, damage));
        }
        
        // ✅ Update HP bar
        if (isMyCard)
        {
            playerLifeBar.SetHP(card.health);
        }
        else
        {
            enemyLifeBar.SetHP(card.health);
        }
        
        yield return StartCoroutine(ShowDialog($"{card.cardName} suffers -{damage} HP!"));
    }
    
    /// <summary>
    /// Prehrá SINGLE Bleed continue animáciu + damage (pre jeden Bleed effect)
    /// V11.2: PRIORITY 1 - Individual Bleed damage processing (separate animations for each Bleed)
    /// </summary>
    private IEnumerator PlaySingleBleedAnimation(Kard card, int damage, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        Debug.LogWarning($"🩸 [SINGLE_BLEED] {cardOwner} card ({card.cardName}) takes {damage} Bleed damage!");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            // Zahrá BleedContinue animáciu (drops = damage amount)
            yield return StartCoroutine(animations.PlayBleedContinueAnimation(card.transform, damage));
        }
        
        // ✅ Použij cardAnimator pre damage animáciu
        if (cardAnimator != null && damage > 0)
        {
            yield return StartCoroutine(cardAnimator.AnimateDamage(card, damage));
        }
        
        // ✅ Update HP bar
        if (isMyCard)
        {
            playerLifeBar.SetHP(card.health);
        }
        else
        {
            enemyLifeBar.SetHP(card.health);
        }
        
        yield return StartCoroutine(ShowDialog($"{card.cardName} is bleeding! -{damage} HP"));
    }
    
    /// <summary>
    /// Prehrá Exposure animáciu - damage alebo removal
    /// V11.2: PRIORITY 1 - Exposure processing (escalating radiation damage)
    /// </summary>
    private IEnumerator PlayExposureAnimation(Kard card, int damage, bool removed, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        
        if (removed)
        {
            // 20% proc - Exposure removed!
            Debug.LogWarning($"☢️ [EXPOSURE] {cardOwner} card ({card.cardName})'s irradiation is gone! (20% removal proc)");
            
            if (animations != null)
            {
                yield return StartCoroutine(animations.PlayExposureEndAnimation(card.transform));
            }
            
            // ✅ Odstráň Exposure ikonu
            string exposureEffectName = GetEffectName(4); // 4 = Exposure
            if (!string.IsNullOrEmpty(exposureEffectName))
            {
                yield return StartCoroutine(card.RemoveEffectIcon(exposureEffectName));
                Debug.LogWarning($"☢️ [EXPOSURE] Removed {exposureEffectName} icon from {card.cardName}");
            }
            
            yield return StartCoroutine(ShowDialog($"{card.cardName}'s irradiation is gone"));
        }
        else if (damage > 0)
        {
            // 80% proc - Take escalating damage
            Debug.LogWarning($"☢️ [EXPOSURE] {cardOwner} card ({card.cardName}) is irradiated! -{damage} HP");
            
            if (animations != null)
            {
                yield return StartCoroutine(animations.PlayExposureAnimation(card.transform));
            }
            
            // ✅ Použij cardAnimator pre damage animáciu
            if (cardAnimator != null)
            {
                yield return StartCoroutine(cardAnimator.AnimateDamage(card, damage));
            }
            
            // ✅ Update HP bar
            if (isMyCard)
            {
                playerLifeBar.SetHP(card.health);
            }
            else
            {
                enemyLifeBar.SetHP(card.health);
            }
            
            yield return StartCoroutine(ShowDialog($"{card.cardName} is irradiated"));
        }
    }
    
    /// <summary>
    /// Prehrá blocking animáciu podľa typu effectu
    /// VOLÁ SA keď karta má aktívny blocking effect (Sleep, Stun, Freeze, atď.)
    /// </summary>
    private IEnumerator PlayBlockAnimation(Kard card, int? blockedBy, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        string effectName = blockedBy.HasValue ? GetEffectName(blockedBy.Value) : "unknown effect";
        Debug.LogWarning($"🛡️ [BLOCK] {cardOwner} card ({card.cardName}) blocked by effect type {blockedBy} ({effectName})!");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations == null)
        {
            Debug.LogError("[BattleResultProcessor] AttackAnimations not found!");
            yield break;
        }
        
        // ✅ Prehrá animáciu podľa numeric effect type ID
        switch (blockedBy)
        {
            case 2: // ASCETICISM
                Debug.LogWarning($"🙏 [ASCETICISM_ONGOING] Playing HURT ITSELF animation (ongoing Asceticism blocking)");
                yield return StartCoroutine(animations.PlayConfusionHurtItselfAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} practizes asceticism..."));
                break;
                
            case 3: // SLEEP
                Debug.LogWarning($"?? [SLEEP_ONGOING] Playing SLEEP animation (ongoing Sleep, not initial)");
                yield return StartCoroutine(animations.PlaySleepAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} is sleeping..."));
                break;
            case 27: // KNOCKOUT -> ongoing flow behaves like sleep
                Debug.LogWarning($"?? [KNOCKOUT_ONGOING_AS_SLEEP] Playing SLEEP animation (ongoing Knockout)");
                yield return StartCoroutine(animations.PlaySleepAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} is sleeping..."));
                break;
                
            case 5: // STUN (example)
                // TODO: Implementovať stun animáciu
                // yield return StartCoroutine(animations.PlayStunAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} is stunned!"));
                break;
                
            case 6: // FREEZE (example)
                // TODO: Implementovať freeze animáciu
                // yield return StartCoroutine(animations.PlayFreezeAnimation(card.transform));
                yield return StartCoroutine(ShowDialog($"{card.cardName} is frozen!"));
                break;
                
            default:
                Debug.LogWarning($"[PlayBlockAnimation] Unknown effect type {blockedBy}, using default message");
                yield return StartCoroutine(ShowDialog($"{card.cardName} cannot attack!"));
                break;
        }
    }
    
    /// <summary>
    /// DEPRECATED: Replaced by PlayBlockAnimation(blockedBy)
    /// Prehrá Sleep blocking animáciu (útok blocked, duration decremented)
    /// VOLÁ SA keď karta UŽ MÁ Sleep a útok je blokovaný (Turn 2+)
    /// </summary>
    [System.Obsolete("Use PlayBlockAnimation(card, blockedBy, isMyCard) instead")]
    private IEnumerator PlaySleepBlockAnimation(Kard card, bool isMyCard)
    {
        string cardOwner = isMyCard ? "MY" : "ENEMY";
        Debug.LogWarning($"💤 [SLEEP_BLOCK] {cardOwner} card ({card.cardName}) is sleeping, attack blocked!");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            // ✅ Ovečka animácia (karta UŽ spí, nie prvá aplikácia)
            Debug.LogWarning($"🐑 [SLEEP_ONGOING] Playing SLEEP animation (ongoing Sleep, not initial)");
            yield return StartCoroutine(animations.PlaySleepAnimation(card.transform));
        }
        
        yield return StartCoroutine(ShowDialog($"{card.cardName} is sleeping..."));
    }
    
    /// <summary>
    /// Vráti názov efektu pre effect type ID (používa Kard.GetEffectNameById logiku)
    /// </summary>
    private string GetEffectName(int effectType)
    {
        switch (effectType)
        {
            case 1: return "Bleed";
            case 2: return "Asceticism";
            case 3: return "Sleep";
            case 27: return "Sleep"; // Knockout currently reuses Sleep icon
            case 4: return "Exposure";
            case 5: return "Siege";
            case 6: return "Fury";
            case 7: return "Famine";
            case 8: return "Electricity";
            case 9: return "Tether";
            case 10: return "Starving";
            case 11: return "ScientificLecture";
            case 12: return "Blockade";
            case 13: return "Depression";
            case 14: return "ArtInspiration";
            case 15: return "Autoportrait";
            case 16: return "Burn";
            case 17: return "Confusion";
            case 18: return "Satellite";
            case 19: return "Fear";
            case 20: return "Horns";
            case 21: return "Calm";
            case 22: return "Reloading";
            case 23: return "Trident";
            case 24: return "Poison";
            case 26: return "Curse";
            default:
                Debug.LogWarning($"[GetEffectName] Unknown effect type: {effectType}");
                return null;
        }
    }
    
    /// <summary>
    /// Vráti názov útoku pre attackId (pre dialog text)
    /// </summary>
    private string GetAttackName(int attackId)
    {
        switch (attackId)
        {
            case 1: return "Punch";
            case 2: return "Kick";
            case 3: return "Heal";
            case 4: return "Forgiveness";
            case 5: return "Crusade";
            case 6: return "Water To Wine";
            case 7: return "Car Hit";
            case 8: return "Monkey Wrench";
            case 9: return "Radiation";
            case 10: return "Scratch";
            case 11: return "Scientific Lecture";
            case 12: return "Chi Sau";
            case 13: return "One Inch Punch";
            case 14: return "Up In Smoke";
            // ✅ TODO: Rozšíriť pre všetky útoky
            default: return $"Attack#{attackId}";
        }
    }
    
    /// <summary>
    /// Zobrazí dialóg
    /// </summary>
    private IEnumerator ShowDialog(string message)
    {
        if (dialogText != null)
        {
            dialogText.text = message;
            // Color is set BEFORE attack sequence (like singleplayer), not per-message
        }
        yield return new WaitForSeconds(1.5f);
    }
    
    /// <summary>
    /// Určí či som player1 alebo player2
    /// </summary>
    private bool DetermineIfIAmPlayer1()
    {
        // TODO: Implementovať správnu logiku podľa roomPlayers
        return true; // Placeholder
    }
    
    /// <summary>
    /// Skontroluje výsledok boja a určí víťaza
    /// </summary>
    private void CheckBattleOutcome(Kard myCard, Kard enemyCard)
    {
        if (roundCoordinator == null)
        {
            roundCoordinator = new BattleRoundCoordinator(fightSystem, multiplayerService, killCounterManager, dialogText);
        }

        bool myCardDead = myCard.health <= 0;
        bool enemyCardDead = enemyCard.health <= 0;

        if (myCardDead && enemyCardDead)
        {
            if (dialogText != null) dialogText.text = "Both cards destroyed!";
            Debug.Log("[BattleResultProcessor] Both cards died - checking remaining cards");
            StartCoroutine(roundCoordinator.HandleBothCardsDeath(myCard, enemyCard));
        }
        else if (myCardDead)
        {
            Debug.Log("[BattleResultProcessor] My card died - checking if I have more cards");
            StartCoroutine(roundCoordinator.HandlePlayerCardDeath(myCard));
        }
        else if (enemyCardDead)
        {
            if (dialogText != null) dialogText.text = "Enemy card destroyed!";
            Debug.Log("[BattleResultProcessor] Enemy card died");
            StartCoroutine(roundCoordinator.HandleEnemyCardDeath(enemyCard));
        }
        else
        {
            Debug.Log("[BattleResultProcessor] Battle continues - preparing next turn");
            StartCoroutine(roundCoordinator.PrepareNextTurn());
        }
    }
    
    /// <summary>
    /// Pripraví ďalší turn - ready check systém + reset UI
    /// </summary>
    private IEnumerator HandleCardDeath(Kard deadCard, bool isMyCard)
    {
        if (deadCard == null)
        {
            Debug.LogWarning("[BattleResultProcessor] HandleCardDeath called with null card!");
            yield break;
        }
        
        string cardId = deadCard.cardId;
        string cardName = deadCard.cardName;
        
        Debug.Log($"[BattleResultProcessor] 💀 Card died: {cardName} (ID: {cardId}, isMyCard: {isMyCard})");
        
        // 1. ✅ Animácia smrti (voliteľné - fade out, shake, atď.)
        yield return new WaitForSeconds(1f); // Krátka pauza pre dramatický efekt
        
        // 2. ✅ Vymaž kartu z boardu (UI)
        Player owner = isMyCard ? fightSystem.player : fightSystem.enemy;
        if (owner != null)
        {
            Debug.Log($"[BattleResultProcessor] Removing {cardName} from {(isMyCard ? "player" : "enemy")} board");
            owner.RemoveCardFromBoard(deadCard);
        }
        else
        {
            Debug.LogWarning($"[BattleResultProcessor] Owner not found for card {cardName}!");
            // Fallback: zničíme GameObject priamo
            Destroy(deadCard.gameObject);
        }
        
        // 3. ✅ Vymaž kartu zo servera (selectedCards)
        var serverFunctions = fightSystem.serverFunctionsManager;
        if (serverFunctions == null)
        {
            Debug.LogError("[BattleResultProcessor] ServerFunctionsManager not found! Cannot clear dead card from server.");
            yield break;
        }
        
        string roomCode = multiplayerService?.RoomCode;
        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogError("[BattleResultProcessor] RoomCode is null/empty! Cannot clear dead card from server.");
            yield break;
        }
        
        Debug.Log($"[BattleResultProcessor] Calling server to clear dead card - roomCode: {roomCode}, cardId: {cardId}");
        
        bool serverCallCompleted = false;
        bool serverCallSuccess = false;
        
        serverFunctions.ClearDeadCard(roomCode, cardId, (result) =>
        {
            serverCallCompleted = true;
            
            if (result != null && result.FunctionResult != null)
            {
                // ✅ Server vracia JObject, nie Dictionary!
                var jObject = result.FunctionResult as Newtonsoft.Json.Linq.JObject;
                if (jObject != null && jObject["success"] != null)
                {
                    serverCallSuccess = jObject["success"].ToObject<bool>();
                    
                    if (serverCallSuccess)
                    {
                        Debug.Log($"[BattleResultProcessor] ✅ Dead card cleared from server: {cardName} (ID: {cardId})");
                    }
                    else
                    {
                        string errorMsg = jObject["error"]?.ToString() ?? "Unknown error";
                        Debug.LogError($"[BattleResultProcessor] ❌ Server failed to clear dead card: {errorMsg}");
                    }
                }
                else
                {
                    Debug.LogWarning("[BattleResultProcessor] ⚠️ Unexpected response format - assuming success");
                    serverCallSuccess = true; // Assume success if can't parse (server returned success in logs)
                }
            }
            else
            {
                Debug.LogError("[BattleResultProcessor] ❌ Server call returned null result!");
            }
        });
        
        // Počkaj na server response (max 5s)
        float timeout = 5f;
        float elapsed = 0f;
        while (!serverCallCompleted && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (!serverCallCompleted)
        {
            Debug.LogError($"[BattleResultProcessor] ⏱️ Server call timeout after {timeout}s - dead card may still be in selectedCards!");
        }
        else if (!serverCallSuccess)
        {
            Debug.LogWarning("[BattleResultProcessor] Server call completed but failed - check server logs");
        }
        
        Debug.Log($"[BattleResultProcessor] Card death handling complete for {cardName}");
    }
    
    /// <summary>
    /// Handler pre smrť player karty - skontroluje či má ďalšie karty
    /// </summary>
    private IEnumerator HandlePlayerCardDeath(Kard myCard)
    {
        // ✅ HNEĎ zaznamenaj kill (PRED HandleCardDeath ktorý môže failnúť)
        if (killCounterManager != null)
        {
            Debug.Log("[BattleResultProcessor] 💀 Player card died - incrementing enemy kill count");
            killCounterManager.OnEnemyKilledPlayerCard();
        }
        else
        {
            Debug.LogWarning("[BattleResultProcessor] KillCounterManager not assigned!");
        }
        
        // 1. Vymaž kartu (board + server)
        yield return StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
        
        // 2. Skontroluj či má player ďalšie karty v ruke
        Player player = fightSystem?.player;
        if (player == null)
        {
            Debug.LogError("[BattleResultProcessor] Player reference is null!");
            fightSystem.state = FightStateMultiplayer.LOST;
            yield break;
        }
        
        int remainingCards = player.hand.Count;
        Debug.Log($"[BattleResultProcessor] Player has {remainingCards} cards remaining in hand");
        
        if (remainingCards > 0)
        {
            // ✅ Má karty → PLAYERDEATH state (výber novej karty)
            dialogText.text = "Choose new fighter!";
            fightSystem.state = FightStateMultiplayer.PLAYERDEATH;
            
            Debug.Log("[BattleResultProcessor] 🔓 Unlocking hand for new card selection");
            
            // Unlock hand pre výber novej karty
            var boardManager = fightSystem.multiplayerBoardManager;
            if (boardManager != null)
            {
                boardManager.UnlockPlayerHand();
            }
            else
            {
                Debug.LogError("[BattleResultProcessor] MultiplayerBoardManager not found - cannot unlock hand!");
            }
            
            // Existujúci systém HandleCardSelectedAsync sa postará o:
            // - Submit novej karty do selectedCards
            // - Wait for opponent (ak aj on vyberie novú)
            // - Reveal cards + pokračovanie battle
        }
        else
        {
            // ❌ Žiadne karty → definitívna prehra
            dialogText.text = "You Lost! No cards left!";
            fightSystem.state = FightStateMultiplayer.LOST;
            Debug.Log("[BattleResultProcessor] Player lost - no cards remaining");
        }
    }
    
    /// <summary>
    /// Handler pre smrť enemy karty - čakáme na výber novej enemy karty
    /// </summary>
    private IEnumerator HandleEnemyCardDeath(Kard enemyCard)
    {
        // ✅ HNEĎ zaznamenaj kill (PRED HandleCardDeath ktorý môže failnúť)
        if (killCounterManager != null)
        {
            Debug.Log("[BattleResultProcessor] 💀 Enemy card died - incrementing player kill count");
            killCounterManager.OnPlayerKilledEnemyCard();
        }
        else
        {
            Debug.LogWarning("[BattleResultProcessor] KillCounterManager not assigned!");
        }
        
        // 1. Vymaž kartu (board + server)
        yield return StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));
        
        // 2. ✅ REUSE MultiplayerBoardManager polling + reveal systém (KISS principle!)
        dialogText.text = "Opponent choosing new fighter...";
        Debug.Log("[BattleResultProcessor] Waiting for opponent to select new card...");
        
        var boardManager = fightSystem.multiplayerBoardManager;
        if (boardManager == null)
        {
            Debug.LogError("[BattleResultProcessor] MultiplayerBoardManager not found!");
            dialogText.text = "You Won!"; // Fallback
            fightSystem.state = FightStateMultiplayer.WON;
            yield break;
        }
        
        // ✅ REUSE: Reset opponent card flag (aby WaitForOpponentSelectionAsync fungoval znova)
        boardManager.opponentCardRevealed = false;
        
        // ✅ REUSE: Zavolaj existujúcu metódu (async → coroutine wrapper)
        var waitTask = boardManager.WaitForOpponentSelectionAsync();
        yield return new WaitUntil(() => waitTask.IsCompleted);
        
        // ✅ REUSE: Reveal opponent card (existujúca metóda)
        boardManager.RevealCards();
        
        // Hotovo - battle pokračuje
        Debug.Log("[BattleResultProcessor] Enemy card revealed! Battle continues.");
        
        // ⏳ Počkaj chvíľu aby sa GUI mohlo updatovať
        yield return new WaitForSeconds(0.5f);
        
        // ✅ CRITICAL: Vyčisti starý battleResult zo servera (inak server vráti battle s MŔTVOU kartou!)
        var serverFunctions = fightSystem.serverFunctionsManager;
        if (serverFunctions != null)
        {
            Debug.Log("[BattleResultProcessor] Clearing old battle result from server...");
            
            bool clearCompleted = false;
            bool clearSuccess = false;
            
            serverFunctions.ClearBattleData(fightSystem.roomCode, fightSystem.myPlayerId, result =>
            {
                clearCompleted = true;
                clearSuccess = result != null && (result.FunctionResult as Newtonsoft.Json.Linq.JObject)?["success"]?.ToObject<bool>() == true;
                
                if (clearSuccess)
                {
                    Debug.Log("[BattleResultProcessor] ✅ Old battle result cleared successfully!");
                }
                else
                {
                    Debug.LogWarning("[BattleResultProcessor] ⚠️ Failed to clear battle result - may cause issues!");
                }
            });
            
            // Počkaj na server response (max 5s - môže byť pomalý)
            float waitTime = 0f;
            while (!clearCompleted && waitTime < 5f)
            {
                yield return new WaitForSeconds(0.1f);
                waitTime += 0.1f;
            }
            
            if (!clearCompleted)
            {
                Debug.LogWarning("[BattleResultProcessor] ⚠️ ClearBattleData timeout after 5s - continuing anyway");
            }
        }
        else
        {
            Debug.LogError("[BattleResultProcessor] ServerFunctionsManager not found!");
        }
        
        // ✅ Nastav state na TURN (RevealCards() nemusí to urobiť ak fightSystem field je null)
        fightSystem.state = FightStateMultiplayer.TURN;
        Debug.Log($"[BattleResultProcessor] State set to TURN. Current state: {fightSystem.state}");
        
        // Reload attack counts pre novú kartu
        var myCard = fightSystem.player.cardInGame;
        if (myCard != null && fightSystem.attackCountLoader != null)
        {
            Debug.Log($"[BattleResultProcessor] Reloading attack counts for {myCard.cardName}");
            fightSystem.LoadAttackCounts(myCard);
        }
    }
    
    // ✅ REMOVED: WaitForEnemyNewCard() - duplicitný kód
    // ✅ REMOVED: RevealEnemyNewCard() - duplicitný kód
    // Teraz reusujeme MultiplayerBoardManager.WaitForOpponentSelectionAsync() + RevealCards()
    
    /// <summary>
    /// Handler pre smrť oboch kariet simultánne
    /// </summary>
    private IEnumerator HandleBothCardsDeath(Kard myCard, Kard enemyCard)
    {
        // ✅ HNEĎ zaznamenaj obe kills (PRED HandleCardDeath ktorý môže failnúť)
        if (killCounterManager != null)
        {
            Debug.Log("[BattleResultProcessor] 💀💀 Both cards died - incrementing both kill counts");
            killCounterManager.OnEnemyKilledPlayerCard(); // Enemy zabil player kartu
            killCounterManager.OnPlayerKilledEnemyCard(); // Player zabil enemy kartu
        }
        else
        {
            Debug.LogWarning("[BattleResultProcessor] KillCounterManager not assigned!");
        }
        
        // 1. Vymaž obe karty (board + server)
        yield return StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
        yield return StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));
        
        // 2. Skontroluj či player má ďalšie karty
        Player player = fightSystem?.player;
        if (player == null)
        {
            dialogText.text = "Draw!";
            fightSystem.state = FightStateMultiplayer.WON; // alebo DRAW state
            yield break;
        }
        
        int remainingCards = player.hand.Count;
        Debug.Log($"[BattleResultProcessor] Both died - Player has {remainingCards} cards remaining");
        
        if (remainingCards > 0)
        {
            // ✅ Má karty → PLAYERDEATH state
            dialogText.text = "Both destroyed! Choose new fighter!";
            fightSystem.state = FightStateMultiplayer.PLAYERDEATH;
            
            Debug.Log("[BattleResultProcessor] 🔓 Unlocking hand after mutual destruction");
            
            var boardManager = fightSystem.multiplayerBoardManager;
            if (boardManager != null)
            {
                boardManager.UnlockPlayerHand();
            }
            else
            {
                Debug.LogError("[BattleResultProcessor] MultiplayerBoardManager not found in both cards death!");
            }
        }
        else
        {
            // ❌ Žiadne karty → Draw
            dialogText.text = "Draw! No cards left!";
            fightSystem.state = FightStateMultiplayer.WON; // alebo DRAW state
            Debug.Log("[BattleResultProcessor] Draw - both players out of cards");
        }
    }
}



