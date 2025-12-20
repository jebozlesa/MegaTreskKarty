using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json.Linq;

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
        
        // ✅ V5: Parsuj attacks object (indexované podľa cardId)
        if (!battleResult.ContainsKey("attacks"))
        {
            Debug.LogError("[BattleResultProcessor] Missing 'attacks' in battleResult!");
            return;
        }
        
        var attacksJson = battleResult["attacks"].ToString();
        var attacks = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
            .DeserializeObject<Dictionary<string, object>>(attacksJson);
        
        string firstAttacker = battleResult["firstAttacker"].ToString();
        
        // ✅ Nájdi damage pre moju kartu a nepriateľa pomocou cardId
        string myCardId = myCard.cardId;
        string enemyCardId = enemyCard.cardId;
        
        Debug.Log($"[BattleResultProcessor] MyCardId={myCardId}, EnemyCardId={enemyCardId}");
        Debug.Log($"[BattleResultProcessor] FirstAttacker={firstAttacker}");
        
        if (!attacks.ContainsKey(myCardId) || !attacks.ContainsKey(enemyCardId))
        {
            Debug.LogError($"[BattleResultProcessor] Missing attack data for cards! attacks keys: {string.Join(", ", attacks.Keys)}");
            return;
        }
        
        var myAttackData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
            .DeserializeObject<Dictionary<string, object>>(attacks[myCardId].ToString());
        var enemyAttackData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
            .DeserializeObject<Dictionary<string, object>>(attacks[enemyCardId].ToString());
        
        int myDamage = int.Parse(myAttackData["damage"].ToString());
        int enemyDamage = int.Parse(enemyAttackData["damage"].ToString());
        
        // ✅ NEW: Získaj attackId pre správne animácie
        int myAttackId = myAttackData.ContainsKey("attackId") ? int.Parse(myAttackData["attackId"].ToString()) : 1;
        int enemyAttackId = enemyAttackData.ContainsKey("attackId") ? int.Parse(enemyAttackData["attackId"].ToString()) : 1;
        
        // ✅ V9: Získaj healAmount pre self-heal útoky (Attack ID 3, atď.)
        int myHealAmount = myAttackData.ContainsKey("healAmount") ? int.Parse(myAttackData["healAmount"].ToString()) : 0;
        int enemyHealAmount = enemyAttackData.ContainsKey("healAmount") ? int.Parse(enemyAttackData["healAmount"].ToString()) : 0;
        
        // ✅ V9: Získaj effectApplied (nový effect pridaný tento turn)
        Dictionary<string, object> myEffectApplied = null;
        Dictionary<string, object> enemyEffectApplied = null;
        
        if (myAttackData.ContainsKey("effectApplied") && myAttackData["effectApplied"] != null)
        {
            myEffectApplied = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                .DeserializeObject<Dictionary<string, object>>(myAttackData["effectApplied"].ToString());
            Debug.LogWarning($"🎭 [EFFECT] MY card APPLIED effect to enemy: type={myEffectApplied["type"]}, duration={myEffectApplied["duration"]}");
        }
        
        if (enemyAttackData.ContainsKey("effectApplied") && enemyAttackData["effectApplied"] != null)
        {
            enemyEffectApplied = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                .DeserializeObject<Dictionary<string, object>>(enemyAttackData["effectApplied"].ToString());
            Debug.LogWarning($"🎭 [EFFECT] ENEMY card APPLIED effect to me: type={enemyEffectApplied["type"]}, duration={enemyEffectApplied["duration"]}");
        }
        
        // ✅ V9: Skontroluj blocked/wokeUp flags (Sleep blocking system)
        bool myAttackBlocked = myAttackData.ContainsKey("blocked") && bool.Parse(myAttackData["blocked"].ToString());
        bool enemyAttackBlocked = enemyAttackData.ContainsKey("blocked") && bool.Parse(enemyAttackData["blocked"].ToString());
        bool myWokeUp = myAttackData.ContainsKey("wokeUp") && bool.Parse(myAttackData["wokeUp"].ToString());
        bool enemyWokeUp = enemyAttackData.ContainsKey("wokeUp") && bool.Parse(enemyAttackData["wokeUp"].ToString());
        
        // ✅ V10: Skontroluj recovered flag (Asceticism recovery)
        bool myRecovered = myAttackData.ContainsKey("recovered") && bool.Parse(myAttackData["recovered"].ToString());
        bool enemyRecovered = enemyAttackData.ContainsKey("recovered") && bool.Parse(enemyAttackData["recovered"].ToString());
        
        // ✅ V10: Získaj selfDamage (Asceticism self-damage)
        int mySelfDamage = myAttackData.ContainsKey("selfDamage") ? int.Parse(myAttackData["selfDamage"].ToString()) : 0;
        int enemySelfDamage = enemyAttackData.ContainsKey("selfDamage") ? int.Parse(enemyAttackData["selfDamage"].ToString()) : 0;
        
        // ✅ Získaj blockedBy field (typ effectu ktorý blokuje útok - numeric effect type ID)
        int? myBlockedBy = (myAttackData.ContainsKey("blockedBy") && myAttackData["blockedBy"] != null) 
            ? (int?)int.Parse(myAttackData["blockedBy"].ToString()) 
            : null;
        int? enemyBlockedBy = (enemyAttackData.ContainsKey("blockedBy") && enemyAttackData["blockedBy"] != null) 
            ? (int?)int.Parse(enemyAttackData["blockedBy"].ToString()) 
            : null;
        
        if (myAttackBlocked)
        {
            string effectName = GetEffectName(myBlockedBy ?? 0);
            Debug.LogWarning($"🛡️ [BLOCK] MY attack BLOCKED by {effectName}! SelfDamage={mySelfDamage}");
        }
        if (enemyAttackBlocked)
        {
            string effectName = GetEffectName(enemyBlockedBy ?? 0);
            Debug.LogWarning($"🛡️ [BLOCK] ENEMY attack BLOCKED by {effectName}! SelfDamage={enemySelfDamage}");
        }
        if (myWokeUp)
        {
            Debug.LogWarning($"⏰ [SLEEP] MY card WOKE UP from Sleep! Attack executed.");
        }
        if (enemyWokeUp)
        {
            Debug.LogWarning($"⏰ [SLEEP] ENEMY card WOKE UP from Sleep!");
        }
        if (myRecovered)
        {
            Debug.LogWarning($"🙏 [ASCETICISM] MY card RECOVERED from Asceticism! Feels blessed again.");
        }
        if (enemyRecovered)
        {
            Debug.LogWarning($"🙏 [ASCETICISM] ENEMY card RECOVERED from Asceticism!");
        }
        
        Debug.LogWarning($"[BattleResultProcessor] MyAttackId={myAttackId}, MyDamage={myDamage}, MyHeal={myHealAmount}, MySelfDamage={mySelfDamage}, MyBlocked={myAttackBlocked}, MyBlockedBy={myBlockedBy}, MyWokeUp={myWokeUp}, MyRecovered={myRecovered}, EnemyAttackId={enemyAttackId}, EnemyDamage={enemyDamage}, EnemyHeal={enemyHealAmount}, EnemySelfDamage={enemySelfDamage}, EnemyBlocked={enemyAttackBlocked}, EnemyBlockedBy={enemyBlockedBy}, EnemyWokeUp={enemyWokeUp}, EnemyRecovered={enemyRecovered}");
        
        // ✅ Spusti animácie (HP sa updatne postupne!)
        // ✅ REFRESH selectedCards sa spustí AŽ PO animáciách
        StartCoroutine(PlayBattleAnimationsAndRefresh(myCard, enemyCard, firstAttacker, myCardId, enemyCardId, myAttackId, enemyAttackId, myDamage, enemyDamage, myHealAmount, enemyHealAmount, myEffectApplied, enemyEffectApplied, myAttackBlocked, enemyAttackBlocked, myWokeUp, enemyWokeUp, myRecovered, enemyRecovered, mySelfDamage, enemySelfDamage, myBlockedBy, enemyBlockedBy));
    }
    
    /// <summary>
    /// Wrapper coroutine - animácie POTOM refresh
    /// V5: Používa cardId na identifikáciu, damage namiesto finalHealth
    /// V8: Pridané attackId pre dynamické animácie
    /// V9: Pridané healAmount pre self-heal animácie + effectApplied pre effect ikony + Sleep blocking + blockedBy field
    /// V10: Pridané recovered/selfDamage pre Asceticism effect
    /// </summary>
    private IEnumerator PlayBattleAnimationsAndRefresh(Kard myCard, Kard enemyCard, string firstAttacker, string myCardId, string enemyCardId, int myAttackId, int enemyAttackId, int myDamage, int enemyDamage, int myHealAmount, int enemyHealAmount, Dictionary<string, object> myEffectApplied, Dictionary<string, object> enemyEffectApplied, bool myAttackBlocked, bool enemyAttackBlocked, bool myWokeUp, bool enemyWokeUp, bool myRecovered, bool enemyRecovered, int mySelfDamage, int enemySelfDamage, int? myBlockedBy, int? enemyBlockedBy)
    {
        // 1. Prehrá animácie (postupný HP update) + effect ikony V SPRÁVNOM PORADÍ
        yield return StartCoroutine(PlayBattleAnimations(myCard, enemyCard, firstAttacker, myCardId, enemyCardId, myAttackId, enemyAttackId, myDamage, enemyDamage, myHealAmount, enemyHealAmount, myAttackBlocked, enemyAttackBlocked, myWokeUp, enemyWokeUp, myRecovered, enemyRecovered, mySelfDamage, enemySelfDamage, myEffectApplied, enemyEffectApplied, myBlockedBy, enemyBlockedBy));
        
        // 2. ✅ Effect ikony sa zobrazujú UŽ v PlayBattleAnimations (MOVED)
        // Tento kód už nie je potrebný - effects sa zobrazujú v správnom momente počas battle flow
        
        // 3. AŽ PO animáciách refreshni selectedCards z DB (pre buffs/effects)
        yield return StartCoroutine(RefreshCardsFromServer());
    }
    
    /// <summary>
    /// Načíta fresh selectedCards z DB po battle
    /// </summary>
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
                            card.GetComponent<Kard>()?.GetType().GetMethod("RepositionEffectIcons", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(card, null);
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
    /// </summary>
    private IEnumerator PlayBattleAnimations(Kard myCard, Kard enemyCard, string firstAttacker, string myCardId, string enemyCardId, int myAttackId, int enemyAttackId, int myDamage, int enemyDamage, int myHealAmount, int enemyHealAmount, bool myAttackBlocked, bool enemyAttackBlocked, bool myWokeUp, bool enemyWokeUp, bool myRecovered, bool enemyRecovered, int mySelfDamage, int enemySelfDamage, Dictionary<string, object> myEffectApplied, Dictionary<string, object> enemyEffectApplied, int? myBlockedBy, int? enemyBlockedBy)
    {
        bool iAttackedFirst = (firstAttacker == myCardId);
        
        Debug.LogWarning($"[PlayBattleAnimations] FirstAttacker={firstAttacker}, MyCardId={myCardId}, IAttackedFirst={iAttackedFirst}");
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
            // ✅ V10: Skontroluj Asceticism recovery
            if (myRecovered)
            {
                Debug.LogWarning($"🙏 [ASCETICISM] MY card RECOVERED from Asceticism before attack!");
                yield return StartCoroutine(PlayRecoveryAnimation(myCard, "My card"));
                yield return new WaitForSeconds(0.5f);
            }
            
            // ✅ V9: Skontroluj Sleep blocking/wake-up
            if (myWokeUp)
            {
                // Zobraz wake-up animáciu pred útokom
                yield return StartCoroutine(PlayWakeUpAnimation(myCard, true));
            }
            
            if (!myAttackBlocked)
            {
                // Útok sa vykoná normálne
                // ✅ FIX: myCard útočí enemyCard → použij enemyDamage (damage ktorý ENEMY dostane)
                yield return StartCoroutine(ExecuteAttackAnimation(myCard, enemyCard, myAttackId, enemyDamage, myHealAmount, true));
                
                // ✅ V9: Ak JA útočím, použijem myEffectApplied (effect ktorý JA aplikujem NA enemy)
                if (myEffectApplied != null)
                {
                    yield return StartCoroutine(DisplayEffectIcon(enemyCard, myEffectApplied, false));
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
                
                // ✅ V10: Nepriateľ Asceticism recovery check
                if (enemyRecovered)
                {
                    Debug.LogWarning($"🙏 [ASCETICISM] ENEMY card RECOVERED from Asceticism before counter-attack!");
                    yield return StartCoroutine(PlayRecoveryAnimation(enemyCard, "Enemy card"));
                    yield return new WaitForSeconds(0.5f);
                }
                
                // ✅ V9: Nepriateľ wake-up check
                if (enemyWokeUp)
                {
                    yield return StartCoroutine(PlayWakeUpAnimation(enemyCard, false));
                }
                
                if (!enemyAttackBlocked)
                {
                    // ✅ FIX: enemyCard útočí myCard → použij myDamage (damage ktorý JA dostanem)
                    yield return StartCoroutine(ExecuteAttackAnimation(enemyCard, myCard, enemyAttackId, myDamage, enemyHealAmount, false));
                    
                    // ✅ V9: Ak enemy útočník aplikoval effect NA MŇA (defendera), zobraz effect ikonu
                    // enemyEffectApplied = effect z enemyAttackData (enemy je útočník)
                    if (enemyEffectApplied != null)
                    {
                        yield return StartCoroutine(DisplayEffectIcon(myCard, enemyEffectApplied, true));
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
            // ✅ V10: Nepriateľ Asceticism recovery check
            if (enemyRecovered)
            {
                Debug.LogWarning($"🙏 [ASCETICISM] ENEMY card RECOVERED from Asceticism before attack!");
                yield return StartCoroutine(PlayRecoveryAnimation(enemyCard, "Enemy card"));
                yield return new WaitForSeconds(0.5f);
            }
            
            // ✅ V9: Nepriateľ wake-up check
            if (enemyWokeUp)
            {
                yield return StartCoroutine(PlayWakeUpAnimation(enemyCard, false));
            }
            
            if (!enemyAttackBlocked)
            {
                // ✅ FIX: enemyCard útočí myCard → použij myDamage (damage ktorý JA dostanem)
                yield return StartCoroutine(ExecuteAttackAnimation(enemyCard, myCard, enemyAttackId, myDamage, enemyHealAmount, false));
                
                // ✅ V9: Ak enemy útočník aplikoval effect NA MŇA (defendera), zobraz effect ikonu
                if (enemyEffectApplied != null)
                {
                    yield return StartCoroutine(DisplayEffectIcon(myCard, enemyEffectApplied, true));
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
                
                // ✅ V10: Môj Asceticism recovery check
                if (myRecovered)
                {
                    Debug.LogWarning($"🙏 [ASCETICISM] MY card RECOVERED from Asceticism before counter-attack!");
                    yield return StartCoroutine(PlayRecoveryAnimation(myCard, "My card"));
                    yield return new WaitForSeconds(0.5f);
                }
                
                // ✅ V9: Môj wake-up check
                if (myWokeUp)
                {
                    yield return StartCoroutine(PlayWakeUpAnimation(myCard, true));
                }
                
                if (!myAttackBlocked)
                {
                    // ✅ FIX: myCard útočí enemyCard → použij enemyDamage (damage ktorý ENEMY dostane)
                    yield return StartCoroutine(ExecuteAttackAnimation(myCard, enemyCard, myAttackId, enemyDamage, myHealAmount, true));
                    
                    // ✅ V9: Keď JA kontratujem, použijem myEffectApplied (effect ktorý JA aplikujem NA enemy)
                    if (myEffectApplied != null)
                    {
                        yield return StartCoroutine(DisplayEffectIcon(enemyCard, myEffectApplied, false));
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
    /// Vykoná animáciu pre konkrétny útok (reuse Attack.cs metód)
    /// V8: Podporuje Attack ID 1 (Punch), 2 (Kick), 3 (Heal), ... rozširiteľné
    /// V9: Heal support - self-heal attacks s healAmount + zelená HP animácia
    /// </summary>
    private IEnumerator ExecuteAttackAnimation(Kard attacker, Kard defender, int attackId, int damage, int healAmount, bool isMyAttack)
    {
        string attackName = GetAttackName(attackId);
        AttackAnimations animations = attackComponent.attackAnimations;
        
        // ✅ Zobraz správu o útoku
        yield return StartCoroutine(ShowDialog($"{attacker.cardName} uses {attackName}!"));
        
        // ✅ Prehrá animáciu podľa attackId
        switch (attackId)
        {
            case 1: // Punch
                yield return StartCoroutine(animations.PlayPunchAnimation(attacker.transform, defender.transform));
                break;
                
            case 2: // Kick
                yield return StartCoroutine(animations.PlayKickAnimation(attacker.transform, defender.transform));
                break;
                
            case 3: // Heal (self-heal animation)
                yield return StartCoroutine(animations.PlayHealAnimation(attacker.transform));
                break;
                
            case 4: // Forgiveness (self-animation on attacker)
                yield return StartCoroutine(animations.PlayForgivenessAnimation(attacker.transform));
                break;
                
            // ✅ TODO: Pridaj case 5, 6, 7... pre ďalšie útoky
            
            default:
                Debug.LogWarning($"[ExecuteAttackAnimation] Unknown attackId={attackId}, using Punch animation");
                yield return StartCoroutine(animations.PlayPunchAnimation(attacker.transform, defender.transform));
                break;
        }
        
        // ✅ V9: Special handling pre self-heal útoky (Attack ID 3 = Heal)
        if (attackId == 3)
        {
            // ✅ Heal - zavolaj Kard.Heal() metódu (trigger zelená HP animácia!)
            if (healAmount > 0)
            {
                Debug.LogWarning($"🩹 [HEAL] {attacker.cardName} heals for {healAmount} HP! (Before: {attacker.health}/{attacker.maxHealth})");
                attacker.Heal(healAmount);  // ✅ Trigger zelená HP animácia + heal sound
                Debug.LogWarning($"🩹 [HEAL] {attacker.cardName} after Heal(): HP={attacker.health}/{attacker.maxHealth}");
                yield return StartCoroutine(ShowDialog($"{attacker.cardName} healed {healAmount} HP!"));
                
                // ✅ DEBUG: Manually sync HP bar after heal
                Debug.LogWarning($"🩹 [HEAL] Updating HP bar for {(isMyAttack ? "MY" : "ENEMY")} card");
                if (isMyAttack)
                {
                    playerLifeBar.SetHP(attacker.health);
                    Debug.LogWarning($"🩹 [HEAL] playerLifeBar.SetHP({attacker.health}) called");
                }
                else
                {
                    enemyLifeBar.SetHP(attacker.health);
                    Debug.LogWarning($"🩹 [HEAL] enemyLifeBar.SetHP({attacker.health}) called");
                }
            }
            else
            {
                Debug.LogWarning($"[ExecuteAttackAnimation] ⚠️ healAmount=0! Server didn't return healAmount!");
                yield return StartCoroutine(ShowDialog($"{attacker.cardName} healed!"));
            }
        }
        else if (attackId == 4)
        {
            // ✅ V10: Forgiveness - NO damage, len attack stat debuff + Asceticism effect
            // Peaceful attack - "forgives your heresy"
            Debug.LogWarning($"🙏 [FORGIVENESS] {attacker.cardName} forgives {defender.cardName}! Attack debuff=-1");
            
            // ✅ Attack stat debuff (-1 ATT visual effect) - NO HP damage!
            defender.HandleAttack(-1);
            
            yield return StartCoroutine(ShowDialog($"{attacker.cardName} forgives your heresy!"));
        }
        else
        {
            // ✅ Damage útoky (Punch, Kick, atď.)
            Debug.LogWarning($"💥 [DAMAGE] {attacker.cardName} attacks {defender.cardName} for {damage} damage! (Defender HP before: {defender.health}/{defender.maxHealth})");
            defender.health -= damage;
            if (defender.health < 0) defender.health = 0;
            Debug.LogWarning($"💥 [DAMAGE] {defender.cardName} after damage: HP={defender.health}/{defender.maxHealth}");
            
            if (cardAnimator != null && damage > 0)
            {
                yield return StartCoroutine(cardAnimator.AnimateDamage(defender, damage));
            }
            
            // ✅ Update HP bar (môj alebo nepriateľov)
            Debug.LogWarning($"💥 [DAMAGE] Updating HP bar for {(isMyAttack ? "ENEMY" : "MY")} card");
            if (isMyAttack)
            {
                enemyLifeBar.SetHP(defender.health);
                Debug.LogWarning($"💥 [DAMAGE] enemyLifeBar.SetHP({defender.health}) called");
            }
            else
            {
                playerLifeBar.SetHP(defender.health);
                Debug.LogWarning($"💥 [DAMAGE] playerLifeBar.SetHP({defender.health}) called");
            }
            
            yield return StartCoroutine(ShowDialog($"Hit! {damage} damage!"));
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
            Debug.LogWarning($"🎭 [EFFECT_ICON] Added {effectName} icon to {card.cardName}");
        }
        
        yield return null;
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
        
        Debug.LogWarning($"🎭 [EFFECT_ICON] Displaying NEW effect on {card.cardName}: type={effectType}, duration={duration}");
        
        // ✅ Prehrá INITIAL effect animation (knockout pre Sleep, blood spray pre Bleed, etc.)
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations != null)
        {
            switch (effectType)
            {
                case 2: // Asceticism - INITIAL application (prayer/holy effect)
                    Debug.LogWarning($"🙏 [ASCETICISM_INIT] Playing ASCETICISM START animation (initial Asceticism application)");
                    yield return StartCoroutine(animations.PlayAscetismStartAnimation(card.transform));
                    yield return StartCoroutine(ShowDialog($"{card.cardName} feels doomed!"));
                    break;
                    
                case 3: // Sleep - INITIAL application (hviezdičky/knockout)
                    Debug.LogWarning($"⭐ [SLEEP_INIT] Playing KNOCKOUT animation (initial Sleep application)");
                    yield return StartCoroutine(animations.PlayKnockoutAnimation(card.transform));
                    yield return StartCoroutine(ShowDialog($"{card.cardName} falls asleep!"));
                    break;
                    
                case 1: // Bleed (future)
                    // yield return StartCoroutine(animations.PlayBleedStartAnimation(card.transform));
                    // yield return StartCoroutine(ShowDialog($"{card.cardName} is bleeding!"));
                    break;
                    
                // TODO: Pridaj ďalšie effect typy (Burn=16, Poison=24, etc.)
            }
        }
        
        // ✅ Pridá effect ikonu (reuse Kard.AddEffectIcon)
        string effectName = GetEffectName(effectType);
        if (!string.IsNullOrEmpty(effectName))
        {
            card.AddEffectIcon(effectName);
            Debug.LogWarning($"🎭 [EFFECT_ICON] Added {effectName} icon to {card.cardName}");
        }
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
                Debug.LogWarning($"🐑 [SLEEP_ONGOING] Playing SLEEP animation (ongoing Sleep, not initial)");
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
            case 4: return "Exposure";
            case 5: return "Siege";
            case 6: return "Fury";
            case 7: return "Famine";
            case 8: return "Electricity";
            case 9: return "Tether";
            case 10: return "Starving";
            case 11: return "Envelop";
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
        bool myCardDead = myCard.health <= 0;
        bool enemyCardDead = enemyCard.health <= 0;
        
        if (myCardDead && enemyCardDead)
        {
            // ✅ Obe karty zomreli - Draw alebo PLAYERDEATH pre oboch
            dialogText.text = "Both cards destroyed!";
            Debug.Log("[BattleResultProcessor] Both cards died - checking remaining cards");
            
            StartCoroutine(HandleBothCardsDeath(myCard, enemyCard));
        }
        else if (myCardDead)
        {
            // ✅ Len moja karta zomrela
            Debug.Log("[BattleResultProcessor] My card died - checking if I have more cards");
            
            StartCoroutine(HandlePlayerCardDeath(myCard));
        }
        else if (enemyCardDead)
        {
            // ✅ Len enemy karta zomrela
            dialogText.text = "Enemy card destroyed!";
            Debug.Log("[BattleResultProcessor] Enemy card died");
            
            StartCoroutine(HandleEnemyCardDeath(enemyCard));
        }
        else
        {
            // ✅ Obe karty žijú - pokračuj v battle
            Debug.Log("[BattleResultProcessor] Battle continues - preparing next turn");
            
            StartCoroutine(PrepareNextTurn());
        }
    }
    
    /// <summary>
    /// Pripraví ďalší turn - ready check systém + reset UI
    /// </summary>
    private IEnumerator PrepareNextTurn()
    {
        // ✅ Žiadna pauza - priama plynulosť!
        
        // ✅ Označ sa ako ready pre ďalší turn
        yield return StartCoroutine(MarkReadyForNextTurn());
        
        // ✅ Reset UI pre ďalší attack selection
        ResetAttackSelectionUI();
        
        dialogText.text = MultiplayerUI.MSG_CHOOSE_ATTACK;
    }
    
    /// <summary>
    /// Označí hráča ako ready pre ďalší turn a čaká na opponent
    /// </summary>
    private IEnumerator MarkReadyForNextTurn()
    {
        var serverFunctions = fightSystem.serverFunctionsManager;
        if (serverFunctions == null)
        {
            Debug.LogError("[BattleResultProcessor] ServerFunctionsManager not found!");
            yield break;
        }
        
        Debug.Log("[BattleResultProcessor] Marking ready for next turn...");
        
        bool isCompleted = false;
        bool bothReady = false;
        
        // Pošli ready signál na server
        serverFunctions.MarkReadyForNextTurn(fightSystem.roomCode, fightSystem.myPlayerId, result => {
            if (result?.FunctionResult != null)
            {
                var resultData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                    .DeserializeObject<Dictionary<string, object>>(result.FunctionResult.ToString());
                
                if (resultData.ContainsKey("bothPlayersReady"))
                {
                    bothReady = (bool)resultData["bothPlayersReady"];
                    Debug.Log($"[BattleResultProcessor] Ready check result: bothReady={bothReady}");
                }
            }
            isCompleted = true;
        });
        
        yield return new WaitUntil(() => isCompleted);
        
        if (bothReady)
        {
            Debug.Log("[BattleResultProcessor] Both players ready immediately - no polling needed!");
            // Immediately proceed to next turn
            yield break; // Use yield break instead of return in IEnumerator
        }
        else
        {
            dialogText.text = "Waiting for opponent to be ready...";
            
            // Polling kým nie sú obaja ready
            yield return StartCoroutine(PollForNextTurnReady());
        }
    }
    
    /// <summary>
    /// Polling - čaká kým nie sú obaja hráči ready pre ďalší turn
    /// </summary>
    private IEnumerator PollForNextTurnReady()
    {
        var serverFunctions = fightSystem.serverFunctionsManager;
        int pollAttempts = 0;
        const int MAX_POLL_ATTEMPTS = 30; // 30 sekúnd timeout
        
        while (pollAttempts < MAX_POLL_ATTEMPTS)
        {
            yield return new WaitForSeconds(1f);
            pollAttempts++;
            
            bool isCompleted = false;
            bool bothReady = false;
            
            // Check ready status
            serverFunctions.CheckNextTurnReady(fightSystem.roomCode, result => {
                if (result?.FunctionResult != null)
                {
                    var resultData = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer)
                        .DeserializeObject<Dictionary<string, object>>(result.FunctionResult.ToString());
                    
                    if (resultData.ContainsKey("bothPlayersReady"))
                    {
                        bothReady = (bool)resultData["bothPlayersReady"];
                    }
                }
                isCompleted = true;
            });
            
            yield return new WaitUntil(() => isCompleted);
            
            if (bothReady)
            {
                Debug.Log("[BattleResultProcessor] Both players ready after polling!");
                break;
            }
        }
        
        if (pollAttempts >= MAX_POLL_ATTEMPTS)
        {
            Debug.LogError("[BattleResultProcessor] Timeout waiting for opponent to be ready");
            dialogText.text = "Opponent disconnected?";
        }
    }
    
    /// <summary>
    /// Reset attack selection UI pre ďalší turn
    /// </summary>
    private void ResetAttackSelectionUI()
    {
        var attackSelectionManager = fightSystem.attackSelectionManager;
        if (attackSelectionManager != null)
        {
            attackSelectionManager.ResetSelection();
            Debug.Log("[BattleResultProcessor] Attack selection UI reset for next turn");
        }
        else
        {
            Debug.LogWarning("[BattleResultProcessor] AttackSelectionManager not found!");
        }
        
        // ✅ Re-enable attack selection pre aktuálnu kartu
        Kard myCard = fightSystem.player?.cardInGame;
        if (myCard != null && fightSystem.attackCountLoader != null)
        {
            // Znovu načítaj attack counts (server už ich decrementoval v executeBattle)
            fightSystem.LoadAttackCounts(myCard);
        }
    }
    
    /// <summary>
    /// Vymaže mŕtvu kartu z boardu a zo servera (selectedCards)
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
