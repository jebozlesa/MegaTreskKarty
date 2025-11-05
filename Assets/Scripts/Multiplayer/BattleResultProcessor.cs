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
    }
    
    /// <summary>
    /// Spracuje výsledok battle a spustí animácie
    /// V5: BattleResult identifikuje karty cez cardId, HP sa načíta z selectedCards
    /// </summary>
    public void ProcessBattleResult(Dictionary<string, object> battleResult)
    {
        Debug.Log($"[BattleResultProcessor] Processing battle result (V5)");
        
        // Získaj karty
        Kard myCard = fightSystem.player?.cardInGame;
        Kard enemyCard = fightSystem.enemy?.cardInGame;
        
        if (myCard == null || enemyCard == null)
        {
            Debug.LogError("[BattleResultProcessor] Cannot apply result - cards not found");
            return;
        }
        
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
        
        Debug.Log($"[BattleResultProcessor] MyDamage={myDamage}, EnemyDamage={enemyDamage}");
        
        // ✅ Spusti animácie (HP sa updatne postupne!)
        // ✅ REFRESH selectedCards sa spustí AŽ PO animáciách
        StartCoroutine(PlayBattleAnimationsAndRefresh(myCard, enemyCard, firstAttacker, myCardId, enemyCardId, myDamage, enemyDamage));
    }
    
    /// <summary>
    /// Wrapper coroutine - animácie POTOM refresh
    /// V5: Používa cardId na identifikáciu, damage namiesto finalHealth
    /// </summary>
    private IEnumerator PlayBattleAnimationsAndRefresh(Kard myCard, Kard enemyCard, string firstAttacker, string myCardId, string enemyCardId, int myDamage, int enemyDamage)
    {
        // 1. Prehrá animácie (postupný HP update)
        yield return StartCoroutine(PlayBattleAnimations(myCard, enemyCard, firstAttacker, myCardId, enemyCardId, myDamage, enemyDamage));
        
        // 2. AŽ PO animáciách refreshni selectedCards z DB (pre buffs/effects)
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
                    // ✅ Zachytaj stat changes pre animácie
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
                    
                    // ✅ Animuj stat changes ak sa zmenili
                    if (cardAnimator != null)
                    {
                        int strChange = card.strength - oldStrength;
                        int defChange = card.defense - oldDefense;
                        int spdChange = card.speed - oldSpeed;
                        int knoChange = card.knowledge - oldKnowledge;
                        
                        if (strChange != 0)
                            StartCoroutine(cardAnimator.AnimateStatChange(card, strChange, "STR"));
                        if (defChange != 0)
                            StartCoroutine(cardAnimator.AnimateStatChange(card, defChange, "DEF"));
                        if (spdChange != 0)
                            StartCoroutine(cardAnimator.AnimateStatChange(card, spdChange, "SPD"));
                        if (knoChange != 0)
                            StartCoroutine(cardAnimator.AnimateStatChange(card, knoChange, "KNO"));
                    }
                    
                    // ✅ Aplikuj effects (burn, sleep, atď.)
                    if (cardData.effects != null && cardData.effects.Length > 0)
                    {
                        foreach (var effect in cardData.effects)
                        {
                            Debug.Log($"[BattleResultProcessor] {card.cardName} has effect: {effect.type} (duration: {effect.duration})");
                            
                            // TODO: Aplikuj visual effects (fire animation pre burn, ZZZ pre sleep, atď.)
                            // TODO: Aplikuj gameplay effects cez Effects.cs system
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
    /// </summary>
    private IEnumerator PlayBattleAnimations(Kard myCard, Kard enemyCard, string firstAttacker, string myCardId, string enemyCardId, int myDamage, int enemyDamage)
    {
        bool iAttackedFirst = (firstAttacker == myCardId);
        
        Debug.Log($"[PlayBattleAnimations] FirstAttacker={firstAttacker}, MyCardId={myCardId}, IAttackedFirst={iAttackedFirst}");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations == null)
        {
            Debug.LogError("[BattleResultProcessor] AttackAnimations not found!");
            yield break;
        }
        
        if (iAttackedFirst)
        {
            // ✅ JA ÚTOČÍM PRVÝ
            yield return StartCoroutine(ShowDialog($"{myCard.cardName} uses Punch!"));
            yield return StartCoroutine(animations.PlayPunchAnimation(myCard.transform, enemyCard.transform));
            
            // ✅ APLIKUJ DAMAGE NA NEPRIATEĽA S ANIMÁCIOU
            enemyCard.health -= myDamage;
            if (enemyCard.health < 0) enemyCard.health = 0;
            
            if (cardAnimator != null && myDamage > 0)
            {
                yield return StartCoroutine(cardAnimator.AnimateDamage(enemyCard, myDamage));
            }
            enemyLifeBar.SetHP(enemyCard.health);
            
            yield return StartCoroutine(ShowDialog($"Hit! {myDamage} damage!"));
            
            // ✅ AK NEPRIATEĽ PREŽIL, JEHO ÚTOK
            if (enemyCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ShowDialog($"{enemyCard.cardName} uses Punch!"));
                yield return StartCoroutine(animations.PlayPunchAnimation(enemyCard.transform, myCard.transform));
                
                // ✅ APLIKUJ DAMAGE NA MŇA S ANIMÁCIOU
                myCard.health -= enemyDamage;
                if (myCard.health < 0) myCard.health = 0;
                
                if (cardAnimator != null && enemyDamage > 0)
                {
                    yield return StartCoroutine(cardAnimator.AnimateDamage(myCard, enemyDamage));
                }
                playerLifeBar.SetHP(myCard.health);
                
                yield return StartCoroutine(ShowDialog($"Hit! {enemyDamage} damage!"));
            }
        }
        else
        {
            // ✅ NEPRIATEĽ ÚTOČÍ PRVÝ
            yield return StartCoroutine(ShowDialog($"{enemyCard.cardName} uses Punch!"));
            yield return StartCoroutine(animations.PlayPunchAnimation(enemyCard.transform, myCard.transform));
            
            // ✅ APLIKUJ DAMAGE NA MŇA S ANIMÁCIOU
            myCard.health -= enemyDamage;
            if (myCard.health < 0) myCard.health = 0;
            
            if (cardAnimator != null && enemyDamage > 0)
            {
                yield return StartCoroutine(cardAnimator.AnimateDamage(myCard, enemyDamage));
            }
            playerLifeBar.SetHP(myCard.health);
            
            yield return StartCoroutine(ShowDialog($"Hit! {enemyDamage} damage!"));
            
            // ✅ AK JA PREŽIJEM, MÔJ ÚTOK
            if (myCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ShowDialog($"{myCard.cardName} uses Punch!"));
                yield return StartCoroutine(animations.PlayPunchAnimation(myCard.transform, enemyCard.transform));
                
                // ✅ APLIKUJ DAMAGE NA NEPRIATEĽA S ANIMÁCIOU
                enemyCard.health -= myDamage;
                if (enemyCard.health < 0) enemyCard.health = 0;
                
                if (cardAnimator != null && myDamage > 0)
                {
                    yield return StartCoroutine(cardAnimator.AnimateDamage(enemyCard, myDamage));
                }
                enemyLifeBar.SetHP(enemyCard.health);
                
                yield return StartCoroutine(ShowDialog($"Hit! {myDamage} damage!"));
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
