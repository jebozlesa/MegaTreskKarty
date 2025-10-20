using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using PlayFab.CloudScriptModels;
using Newtonsoft.Json;

/// <summary>
/// Načítava počty útokov zo servera a zobrazuje ich v UI
/// </summary>
public class AttackCountLoader : MonoBehaviour
{
    public TMP_Text button1CountText;
    public TMP_Text button2CountText;
    public TMP_Text button3CountText;
    public TMP_Text button4CountText;

    public ServerFunctionsManager serverFunctionsManager;

    private bool isLoading = false;

    /// <summary>
    /// Načíta počty útokov pre danú kartu zo servera
    /// </summary>
    /// <param name="card">Karta, ktorej útoky sa majú spočítať</param>
    /// <param name="onComplete">Callback po úspešnom načítaní (voliteľný)</param>
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
        Debug.Log($"[AttackCountLoader] Loading attack counts for card: {card.cardName}");

        // Add timeout protection - reset isLoading after 15 seconds
        StartCoroutine(ResetLoadingStateAfterTimeout());

        // Priprav parametre pre serverovú funkciu
        var cardData = new CardStatsForCalculation
        {
            attack1 = card.attack1,
            attack2 = card.attack2,
            attack3 = card.attack3,
            attack4 = card.attack4,
            strength = card.strength,
            defense = card.defense,
            attack = card.attack,
            knowledge = card.knowledge,
            charisma = card.charisma,
            speed = card.speed
        };

        // Zavolaj serverovú funkciu
        serverFunctionsManager.CalculateAttackCounts(cardData, result =>
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
                // Parsuj výsledok
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

                // Zobraz počty v UI
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
    /// Zobrazí počty útokov v UI
    /// </summary>
    private void DisplayAttackCounts(AttackCountsResult counts)
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
    /// Vymaže všetky počty útokov z UI
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
/// Dáta karty potrebné pre výpočet útokov
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
/// Výsledok výpočtu počtov útokov zo servera
/// </summary>
[Serializable]
public class AttackCountsResult
{
    public int count1;
    public int count2;
    public int count3;
    public int count4;
}
