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
        }
        else if (myCard.health <= 0)
        {
            dialogText.text = "You Lost!";
            fightSystem.state = FightStateMultiplayer.LOST;
            Debug.Log("[BattleResultProcessor] Player lost");
        }
        else if (enemyCard.health <= 0)
        {
            dialogText.text = "You Won!";
            fightSystem.state = FightStateMultiplayer.WON;
            Debug.Log("[BattleResultProcessor] Player won");
        }
        else
        {
            dialogText.text = "Next turn!";
            Debug.Log("[BattleResultProcessor] Battle continues - preparing next turn");
            // TODO: Priprav ďalší turn (reset attack selection UI)
        }
    }
}
