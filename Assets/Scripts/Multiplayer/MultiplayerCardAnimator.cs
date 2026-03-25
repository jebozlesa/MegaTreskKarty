using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Spravuje animacie kariet v multiplayer rezime
/// Separation of concerns - FightSystemMultiplayer deleguje na tento komponent
/// </summary>
public class MultiplayerCardAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("VOLITELNE: Prefab pre text animacie (STR, ATT, HP, atd.). Ak nie je nastaveny, pouzije sa Kard.notsureGO")]
    public GameObject effectAnimationPrefab;
    
    [Header("Colors")]
    public Color32 redColor = new Color32(255, 0, 0, 255);     // Damage, stat decrease
    public Color32 greenColor = new Color32(0, 255, 0, 255);   // Heal, stat increase
    public Color32 blueColor = new Color32(0, 0, 255, 255);    // Special effects
    public Color32 yellowColor = new Color32(255, 255, 0, 255); // Level up
    public Color32 purpleColor = new Color32(128, 0, 128, 255); // Experience

    /// <summary>
    /// Animuje damage na karte - shake + cervene HP cisla
    /// </summary>
    /// <param name="card">Karta ktora dostava damage</param>
    /// <param name="damageAmount">Mnozstvo damage</param>
    public IEnumerator AnimateDamage(Kard card, int damageAmount)
    {
        if (card == null)
        {
            Debug.LogError("[MultiplayerCardAnimator] AnimateDamage: card is null!");
            yield break;
        }
        
        if (damageAmount <= 0)
            damageAmount = 1;
        
        Debug.Log($"[MultiplayerCardAnimator] Animating {damageAmount} damage on {card.cardName}");
        
        // Spusti oba efekty sucasne
        var shakeCoroutine = StartCoroutine(ShakeCard(card, (float)damageAmount));
        var effectCoroutine = StartCoroutine(PlayEffectAnimation(card, damageAmount, "HP", redColor));
        
        // Pockaj kym sa oba dokoncia
        yield return shakeCoroutine;
        yield return effectCoroutine;
    }
    
    /// <summary>
    /// Animuje heal na karte - zelene HP cisla
    /// </summary>
    /// <param name="card">Karta ktora sa healuje</param>
    /// <param name="healAmount">Mnozstvo heal</param>
    public IEnumerator AnimateHeal(Kard card, int healAmount)
    {
        if (card == null)
        {
            Debug.LogError("[MultiplayerCardAnimator] AnimateHeal: card is null!");
            yield break;
        }
        
        if (healAmount <= 0)
            healAmount = 1;
        
        Debug.Log($"[MultiplayerCardAnimator] Animating {healAmount} heal on {card.cardName}");
        
        yield return StartCoroutine(PlayEffectAnimation(card, healAmount, "HP", greenColor));
    }
    
    /// <summary>
    /// Animuje zmenu statu - cervene/zelene pismena podla zmeny
    /// </summary>
    /// <param name="card">Karta</param>
    /// <param name="statChange">Zmena statu (+ alebo -)</param>
    /// <param name="statName">Nazov statu (STR, DEF, SPD, ATT, KNO, CHA)</param>
    public IEnumerator AnimateStatChange(Kard card, int statChange, string statName)
    {
        if (card == null)
        {
            Debug.LogError("[MultiplayerCardAnimator] AnimateStatChange: card is null!");
            yield break;
        }
        if (statChange == 0)
            yield break; // Ziadna zmena
        Color32 color = (statChange > 0) ? greenColor : redColor;
        Debug.Log($"[MultiplayerCardAnimator] Animating {statChange} {statName} change on {card.cardName}");
        yield return StartCoroutine(PlayEffectAnimation(card, Mathf.Abs(statChange), statName, color));
    }
    
    /// <summary>
    /// Resetuje poziciu karty na povodne miesto (po battle)
    /// </summary>
    /// <param name="card">Karta na reset</param>
    /// <param name="targetPosition">Cielova pozicia</param>
    /// <param name="targetRotation">Cielova rotacia</param>
    public IEnumerator ResetCardPosition(Kard card, Vector3 targetPosition, Quaternion targetRotation)
    {
        if (card == null)
        {
            Debug.LogError("[MultiplayerCardAnimator] ResetCardPosition: card is null!");
            yield break;
        }
        
        float resetDuration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startPosition = card.transform.position;
        Quaternion startRotation = card.transform.rotation;
        
        while (elapsedTime < resetDuration)
        {
            float t = elapsedTime / resetDuration;
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            card.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Presna finalna pozicia
        card.transform.position = targetPosition;
        card.transform.rotation = targetRotation;
        
        Debug.Log($"[MultiplayerCardAnimator] Reset position for {card.cardName}");
    }
    
    // ========== PRIVATE METHODS ==========
    
    /// <summary>
    /// Shake animacia karty (rovnaka ako v Kard.cs)
    /// </summary>
    private IEnumerator ShakeCard(Kard card, float damageAmount)
    {
        Vector3 originalPosition = card.transform.position;
        Quaternion originalRotation = card.transform.rotation;

        float radius = 20f;
        float angle = 0f;
        float maxAngle = 15f;
        float increment = 0.02f;

        float shakeTime = damageAmount / 20f;
        float currentTime = 0f;

        while (currentTime < shakeTime)
        {
            float x = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = radius * Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 newPosition = originalPosition + new Vector3(x, y, 0f);
            card.transform.position = newPosition;
            card.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-maxAngle, maxAngle));
            angle += increment;
            currentTime += increment;
            yield return new WaitForSeconds(increment);
        }

        // Smooth return to original position
        float resetTime = 0.5f;
        float resetElapsed = 0f;
        while (resetElapsed < resetTime)
        {
            card.transform.position = Vector3.Lerp(card.transform.position, originalPosition, resetElapsed / resetTime);
            card.transform.rotation = Quaternion.Lerp(card.transform.rotation, originalRotation, resetElapsed / resetTime);
            resetElapsed += Time.deltaTime;
            yield return null;
        }

        // Presna finalna pozicia
        card.transform.position = originalPosition;
        card.transform.rotation = originalRotation;
    }
    
    /// <summary>
    /// Animuje text efekt (HP, STR, atd.) letiaci od karty
    /// </summary>
    private IEnumerator PlayEffectAnimation(Kard card, int amount, string text, Color32 color)
    {
        Debug.Log($"[MultiplayerCardAnimator] PlayEffectAnimation start: card={card.cardName}, text={text}, amount={amount}, color={color}");

        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(0.1f);
            Debug.Log($"[MultiplayerCardAnimator] Spawning effect {i + 1}/{amount}: card={card.cardName}, text={text}");
            StartCoroutine(CreateSingleEffectAnimation(card, text, color));
        }
    }
    
    /// <summary>
    /// Vytvori jeden text efekt letiaci od karty
    /// </summary>
    private IEnumerator CreateSingleEffectAnimation(Kard card, string text, Color32 color)
    {
        // [OK] Pokus sa pouzit existujuci Kard.notsureGO ak existuje
        GameObject effectObject = null;
        
        if (card.notsureGO != null && card.notsureText != null)
        {
            // Pouzij originalny system z Kard.cs
            card.notsureText.text = text;
            card.notsureText.color = color;
            effectObject = Instantiate(card.notsureGO, card.transform);
            Debug.Log($"[MultiplayerCardAnimator] CreateSingleEffectAnimation: using card.notsureGO for {card.cardName}, text={text}");
        }
        else if (effectAnimationPrefab != null)
        {
            // Fallback na vlastny prefab
            effectObject = Instantiate(effectAnimationPrefab, card.transform);
            var textComponent = effectObject.GetComponentInChildren<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = text;
                textComponent.color = color;
            }
            Debug.Log($"[MultiplayerCardAnimator] CreateSingleEffectAnimation: using effectAnimationPrefab for {card.cardName}, text={text}");
        }
        else
        {
            Debug.LogWarning($"[MultiplayerCardAnimator] No effect animation prefab assigned for {card.cardName}, text={text}");
            yield break;
        }
        
        if (effectObject == null) yield break;
        
        // Animacia letu
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float radius = 50f;
        Vector2 randomPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
        effectObject.transform.position = (Vector2)card.transform.position + randomPosition;
        
        Vector2 direction = (effectObject.transform.position - card.transform.position).normalized;
        float distance = 50f;
        float elapsedTime = 0f;
        
        Debug.Log($"[MultiplayerCardAnimator] Effect text spawned: card={card.cardName}, text={text}, startPos={effectObject.transform.position}, direction={direction}");
        
        while (elapsedTime < 3f)
        {
            effectObject.transform.position += (Vector3)(direction * distance * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(effectObject);
    }
}

