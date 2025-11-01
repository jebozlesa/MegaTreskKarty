using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

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
        if (myCard.health <= 0 && enemyCard.health <= 0)
        {
            dialogText.text = "Draw!";
            fightSystem.state = FightStateMultiplayer.WON; // alebo nový state DRAW
            Debug.Log("[BattleResultProcessor] Battle ended in a draw");
            
            // ✅ Obe karty mŕtve - vymaž obe
            StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
            StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));
        }
        else if (myCard.health <= 0)
        {
            dialogText.text = "You Lost!";
            fightSystem.state = FightStateMultiplayer.LOST;
            Debug.Log("[BattleResultProcessor] Player lost");
            
            // ✅ Moja karta zomrela
            StartCoroutine(HandleCardDeath(myCard, isMyCard: true));
        }
        else if (enemyCard.health <= 0)
        {
            dialogText.text = "You Won!";
            fightSystem.state = FightStateMultiplayer.WON;
            Debug.Log("[BattleResultProcessor] Player won");
            
            // ✅ Enemy karta zomrela
            StartCoroutine(HandleCardDeath(enemyCard, isMyCard: false));
        }
        else
        {
            dialogText.text = "Next turn!";
            Debug.Log("[BattleResultProcessor] Battle continues - preparing next turn");
            
            // ✅ Priprav ďalší turn s ready check systémom
            StartCoroutine(PrepareNextTurn());
        }
    }
    
    /// <summary>
    /// Pripraví ďalší turn - ready check systém + reset UI
    /// </summary>
    private IEnumerator PrepareNextTurn()
    {
        yield return new WaitForSeconds(2f); // Chvíľa pauzy po "Next turn!" 
        
        dialogText.text = "Preparing next turn...";
        
        // ✅ Označ sa ako ready pre ďalší turn
        yield return StartCoroutine(MarkReadyForNextTurn());
        
        // ✅ Reset UI pre ďalší attack selection
        ResetAttackSelectionUI();
        
        dialogText.text = "Choose an attack";
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
            // Znovu načítaj attack counts (možno sa zmenili)
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
                var resultDict = result.FunctionResult as Dictionary<string, object>;
                if (resultDict != null && resultDict.ContainsKey("success"))
                {
                    serverCallSuccess = (bool)resultDict["success"];
                    
                    if (serverCallSuccess)
                    {
                        Debug.Log($"[BattleResultProcessor] ✅ Dead card cleared from server: {cardName} (ID: {cardId})");
                    }
                    else
                    {
                        string errorMsg = resultDict.ContainsKey("error") ? resultDict["error"].ToString() : "Unknown error";
                        Debug.LogError($"[BattleResultProcessor] ❌ Server failed to clear dead card: {errorMsg}");
                    }
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
}
