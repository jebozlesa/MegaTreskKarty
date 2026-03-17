using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab.CloudScriptModels;
using Newtonsoft.Json;

/// <summary>
/// Nacitava pocty utokov zo servera a zobrazuje ich v UI
/// </summary>
public class AttackCountLoader : MonoBehaviour
{
    public TMP_Text button1CountText;
    public TMP_Text button2CountText;
    public TMP_Text button3CountText;
    public TMP_Text button4CountText;

    public ServerFunctionsManager serverFunctionsManager;
    public FightSystemMultiplayer fightSystem; // [OK] V8: Reference to get roomCode & playerId

    private bool isLoading = false;

    /// <summary>
    /// Nacita pocty utokov pre danu kartu zo servera
    /// </summary>
    /// <param name="card">Karta, ktorej utoky sa maju spocitat</param>
    /// <param name="onComplete">Callback po uspesnom nacitani (volitelny)</param>
    public void LoadAttackCounts(Kard card, Action<AttackCountsResult> onComplete = null)
    {
        if (card == null)
        {
            Debug.LogWarning("[AttackCountLoader] Card is null");
            ClearAttackCounts();
            onComplete?.Invoke(null);
            return;
        }

        if (serverFunctionsManager == null)
        {
            Debug.LogError("[AttackCountLoader] ServerFunctionsManager reference is missing");
            onComplete?.Invoke(null);
            return;
        }

        if (isLoading)
        {
            Debug.LogWarning("[AttackCountLoader] Already loading attack counts");
            return;
        }

        isLoading = true;
        Debug.LogWarning($"[AttackCountLoader] Loading attack counts for card: {card.cardName} (cardId: {card.cardId})");

        // Add timeout protection - reset isLoading after 15 seconds
        StartCoroutine(ResetLoadingStateAfterTimeout());

        // [OK] V8: Volaj getAttackCounts (citaj z DB), NIE calculateAttackCounts!
        // Server ma attackCounts uz ulozene v room.attackCounts[playerId][cardId]
        if (fightSystem == null)
        {
            Debug.LogError("[AttackCountLoader] FightSystemMultiplayer reference missing!");
            isLoading = false;
            ClearAttackCounts();
            onComplete?.Invoke(null);
            return;
        }

        string roomCode = fightSystem.roomCode;
        string playerId = fightSystem.myPlayerId;

        if (string.IsNullOrEmpty(roomCode) || string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("[AttackCountLoader] Missing roomCode or playerId!");
            isLoading = false;
            ClearAttackCounts();
            onComplete?.Invoke(null);
            return;
        }

        // Zavolaj getAttackCounts (READ from DB)
        serverFunctionsManager.GetAttackCounts(roomCode, playerId, card.cardId, result =>
        {
            isLoading = false;

            if (result == null || result.FunctionResult == null)
            {
                Debug.LogError("[AttackCountLoader] Failed to load attack counts from server");
                ClearAttackCounts();
                onComplete?.Invoke(null);
                return;
            }

            try
            {
                // Parsuj vysledok
                var jsonResult = JsonConvert.SerializeObject(result.FunctionResult);
                var attackCounts = JsonConvert.DeserializeObject<AttackCountsResult>(jsonResult);

                if (attackCounts == null)
                {
                    Debug.LogError("[AttackCountLoader] Failed to parse attack counts result");
                    ClearAttackCounts();
                    onComplete?.Invoke(null);
                    return;
                }

                Debug.Log($"[AttackCountLoader] Received attack counts: {attackCounts.count1}, {attackCounts.count2}, {attackCounts.count3}, {attackCounts.count4}");

                // Zobraz pocty v UI
                DisplayAttackCounts(attackCounts);

                onComplete?.Invoke(attackCounts);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AttackCountLoader] Error parsing attack counts: {ex.Message}");
                ClearAttackCounts();
                onComplete?.Invoke(null);
            }
        });
    }

    /// <summary>
    /// Zobrazi pocty utokov v UI
    /// </summary>
    private void DisplayAttackCounts(AttackCountsResult counts)
    {
        UpdateCountTexts(counts);
    }
    
    /// <summary>
    /// Public metoda pre update count textov (pouzitelna z inych tried)
    /// </summary>
    public void UpdateCountTexts(AttackCountsResult counts)
    {
        if (button1CountText != null)
        {
            button1CountText.text = counts.count1 > 0 ? counts.count1.ToString() : "";
        }

        if (button2CountText != null)
        {
            button2CountText.text = counts.count2 > 0 ? counts.count2.ToString() : "";
        }

        if (button3CountText != null)
        {
            button3CountText.text = counts.count3 > 0 ? counts.count3.ToString() : "";
        }

        if (button4CountText != null)
        {
            button4CountText.text = counts.count4 > 0 ? counts.count4.ToString() : "";
        }
    }

    /// <summary>
    /// Vymaze vsetky pocty utokov z UI
    /// </summary>
    public void ClearAttackCounts()
    {
        if (button1CountText != null) button1CountText.text = "";
        if (button2CountText != null) button2CountText.text = "";
        if (button3CountText != null) button3CountText.text = "";
        if (button4CountText != null) button4CountText.text = "";
    }

    /// <summary>
    /// Timeout protection - resets isLoading flag after 15 seconds
    /// </summary>
    private System.Collections.IEnumerator ResetLoadingStateAfterTimeout()
    {
        yield return new WaitForSeconds(15f);
        
        if (isLoading)
        {
            Debug.LogWarning("[AttackCountLoader] Timeout reached, resetting loading state");
            isLoading = false;
        }
    }
}

/// <summary>
/// [ERR] DEPRECATED: Data karty potrebne pre vypocet utokov
/// V8: Server uz automaticky pocita counts pri setSelectedCard
/// Tato trieda sa uz nepouziva, zostava len kvoli kompatibilite
/// </summary>
[Serializable]
public class CardStatsForCalculation
{
    public int attack1;
    public int attack2;
    public int attack3;
    public int attack4;
    public int strength;
    public int defense;
    public int attack;
    public int knowledge;
    public int charisma;
    public int speed;
}

/// <summary>
/// Vysledok vypoctu poctov utokov zo servera
/// </summary>
[Serializable]
public class AttackCountsResult
{
    public int count1;
    public int count2;
    public int count3;
    public int count4;
}
