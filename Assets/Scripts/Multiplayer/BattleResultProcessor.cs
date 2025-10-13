using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Zodpovedný za spracovanie battle výsledkov zo servera
/// Aplikuje HP zmeny, hrá animácie a určuje víťaza
/// </summary>
public class BattleResultProcessor : MonoBehaviour
{
    [Header("Dependencies")]
    public FightSystemMultiplayer fightSystem;
    public Attack attackComponent;
    public MultiplayerService multiplayerService;  // ✅ For refreshing selectedCards
    
    [Header("UI References")]
    public TMP_Text dialogText;
    public HealthBar playerLifeBar;
    public HealthBar enemyLifeBar;
    
    /// <summary>
    /// Spracuje výsledok battle a spustí animácie
    /// </summary>
    public void ProcessBattleResult(Dictionary<string, object> battleResult)
    {
        Debug.Log($"[BattleResultProcessor] Processing battle result");
        
        // Parsuj výsledky
        int player1Health = int.Parse(battleResult["player1Health"].ToString());
        int player2Health = int.Parse(battleResult["player2Health"].ToString());
        int player1Damage = int.Parse(battleResult["player1Damage"].ToString());
        int player2Damage = int.Parse(battleResult["player2Damage"].ToString());
        string firstAttacker = battleResult["firstAttacker"].ToString();
        
        // Získaj karty
        Kard myCard = fightSystem.player?.cardInGame;
        Kard enemyCard = fightSystem.enemy?.cardInGame;
        
        if (myCard == null || enemyCard == null)
        {
            Debug.LogError("[BattleResultProcessor] Cannot apply result - cards not found");
            return;
        }
        
        // Urči ktorý hráč som ja
        bool iAmPlayer1 = DetermineIfIAmPlayer1();
        
        // Aplikuj HP zmeny
        if (iAmPlayer1)
        {
            myCard.health = player1Health;
            enemyCard.health = player2Health;
        }
        else
        {
            myCard.health = player2Health;
            enemyCard.health = player1Health;
        }
        
        // Aktualizuj health bary
        playerLifeBar.SetHP(myCard.health);
        enemyLifeBar.SetHP(enemyCard.health);
        
        // ✅ REFRESH selectedCards z DB (načíta live stats, effects, atď.)
        StartCoroutine(RefreshCardsFromServer());
        
        // Spusti animácie
        StartCoroutine(PlayBattleAnimations(myCard, enemyCard, firstAttacker, player1Damage, player2Damage, iAmPlayer1));
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
                
                Kard card = null;
                if (playerId == fightSystem.myPlayerId && fightSystem.player?.cardInGame != null)
                {
                    card = fightSystem.player.cardInGame;
                }
                else if (playerId != fightSystem.myPlayerId && fightSystem.enemy?.cardInGame != null)
                {
                    card = fightSystem.enemy.cardInGame;
                }
                
                if (card != null)
                {
                    // Aplikuj live stats z servera
                    card.health = cardData.health;
                    card.strength = cardData.strength;  // ✅ Buffs/debuffs!
                    card.defense = cardData.defense;
                    card.speed = cardData.speed;
                    card.knowledge = cardData.knowledge;
                    
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
                    
                    Debug.Log($"[BattleResultProcessor] Updated {card.cardName}: HP={card.health}/{cardData.maxHealth}, STR={card.strength}, DEF={card.defense}");
                }
            }
        }
    }
    
    /// <summary>
    /// Prehrá battle animácie
    /// </summary>
    private IEnumerator PlayBattleAnimations(Kard myCard, Kard enemyCard, string firstAttacker, int p1Damage, int p2Damage, bool iAmPlayer1)
    {
        bool iAttackedFirst = (iAmPlayer1 && firstAttacker == "player1") || (!iAmPlayer1 && firstAttacker == "player2");
        
        AttackAnimations animations = attackComponent?.attackAnimations;
        if (animations == null)
        {
            Debug.LogError("[BattleResultProcessor] AttackAnimations not found!");
            yield break;
        }
        
        if (iAttackedFirst)
        {
            // Ja útočím prvý
            yield return StartCoroutine(ShowDialog($"{myCard.cardName} uses Punch!"));
            yield return StartCoroutine(animations.PlayPunchAnimation(myCard.transform, enemyCard.transform));
            yield return StartCoroutine(ShowDialog($"Hit! {(iAmPlayer1 ? p1Damage : p2Damage)} damage!"));
            
            // Ak nepriateľ prežil, jeho útok
            if (enemyCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ShowDialog($"{enemyCard.cardName} uses Punch!"));
                yield return StartCoroutine(animations.PlayPunchAnimation(enemyCard.transform, myCard.transform));
                yield return StartCoroutine(ShowDialog($"Hit! {(iAmPlayer1 ? p2Damage : p1Damage)} damage!"));
            }
        }
        else
        {
            // Nepriateľ útočí prvý
            yield return StartCoroutine(ShowDialog($"{enemyCard.cardName} uses Punch!"));
            yield return StartCoroutine(animations.PlayPunchAnimation(enemyCard.transform, myCard.transform));
            yield return StartCoroutine(ShowDialog($"Hit! {(iAmPlayer1 ? p2Damage : p1Damage)} damage!"));
            
            // Ak ja prežijem, môj útok
            if (myCard.health > 0)
            {
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(ShowDialog($"{myCard.cardName} uses Punch!"));
                yield return StartCoroutine(animations.PlayPunchAnimation(myCard.transform, enemyCard.transform));
                yield return StartCoroutine(ShowDialog($"Hit! {(iAmPlayer1 ? p1Damage : p2Damage)} damage!"));
            }
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
