using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using Newtonsoft.Json;

public class MarketplaceManager : MonoBehaviour
{
    public GameObject tutorial;
    public GameObject insolvencyPanel;
    public CardGenerator cardGenerator;  // ✅ V11: Kept for ShowCardOnScreen animation
    public TMP_Text loveValue;
    public GameObject blockPanel;
    
    [Header("V11: Server-Side Card Generation")]
    public ServerFunctionsManager serverFunctionsManager;  // ✅ NEW: For calling openCardPack API
    
    [Header("Intro/Shop Screens")]
    public GameObject introScreen;  // Prvý obrázok + dialog
    public GameObject shopScreen;   // Druhý obrázok + Scroll s balíčkami

    void Start()
    {
        if (PlayerPrefs.GetInt("HasCompletedTutorialMarketplace", 0) == 0) { tutorial.SetActive(true); }
        
        // Show intro screen on start
        ShowIntroScreen();
        
        StartCoroutine(GetPlayerCurrencyBalance());
    }
    
    /// <summary>
    /// Zobraz intro screen (prvý obrázok + dialog)
    /// </summary>
    void ShowIntroScreen()
    {
        if (introScreen != null) introScreen.SetActive(true);
        if (shopScreen != null) shopScreen.SetActive(false);
    }
    
    /// <summary>
    /// Prejdi na shop screen (druhý obrázok + Scroll)
    /// Volaj túto metódu z Yes tlačidla cez Inspector
    /// </summary>
    public void ShowShopScreen()
    {
        if (introScreen != null) introScreen.SetActive(false);
        if (shopScreen != null) shopScreen.SetActive(true);
    }

    public void StartGenerateCardPack(int packIndex)
    {
        StartCoroutine(HandleCardPackGeneration(packIndex));
    }

    private IEnumerator HandleCardPackGeneration(int packIndex)
    {
        blockPanel.SetActive(true);
        bool hasEnough = false;

        // Kontrola zostatku meny
        yield return StartCoroutine(CheckCurrencyBalance(5, (hasEnoughBalance) =>
        {
            hasEnough = hasEnoughBalance;
        }));

        if (hasEnough)
        {
            // Odpočítanie meny
            yield return StartCoroutine(SubtractCurrency(5));

            // Aktualizácia zostatku meny
            yield return StartCoroutine(GetPlayerCurrencyBalance());

            // ✅ V11: SERVER-SIDE GENERATION (namiesto local CardGenerator)
            yield return StartCoroutine(OpenCardPackFromServer(packIndex));
        }
        blockPanel.SetActive(false);
    }
    
    /// <summary>
    /// ✅ V11: Volá server na vygenerovanie card packu
    /// Server vráti 6 vygenerovaných kariet, zobrazíme animáciu a uložíme do PlayFab
    /// </summary>
    private IEnumerator OpenCardPackFromServer(int packIndex)
    {
        Debug.LogWarning($"[MarketplaceManager] Opening pack {packIndex} from server...");
        
        bool isComplete = false;
        List<GeneratedCard> generatedCards = null;
        string errorMessage = null;
        
        // Call server
        serverFunctionsManager.OpenCardPack(
            PlayFabSettings.staticPlayer.PlayFabId, 
            packIndex, 
            result =>
            {
                if (result == null || result.FunctionResult == null)
                {
                    Debug.LogError("[MarketplaceManager] Server returned null result");
                    errorMessage = "Server error";
                    isComplete = true;
                    return;
                }
                
                // Deserialize response
                var jsonResponse = JsonConvert.SerializeObject(result.FunctionResult);
                Debug.LogWarning($"[MarketplaceManager] Server response: {jsonResponse}");
                
                var response = JsonConvert.DeserializeObject<OpenCardPackResponse>(jsonResponse);
                
                if (!response.success)
                {
                    Debug.LogError($"[MarketplaceManager] Server error: {response.error}");
                    errorMessage = response.error;
                    isComplete = true;
                    return;
                }
                
                generatedCards = response.cards;
                Debug.LogWarning($"[MarketplaceManager] Received {generatedCards.Count} cards from server");
                isComplete = true;
            }
        );
        
        // Wait for server response
        yield return new WaitUntil(() => isComplete);
        
        if (generatedCards == null || generatedCards.Count == 0)
        {
            Debug.LogError($"[MarketplaceManager] Failed to generate cards: {errorMessage}");
            yield break;
        }
        
        // Show cards with animation (reuse CardGenerator's ShowCardOnScreen)
        foreach (var card in generatedCards)
        {
            Debug.LogWarning($"[MarketplaceManager] Showing card: {card.PersonName} ({card.CardID})");
            
            Color32 cardColor = new Color32(
                (byte)card.Color[0], 
                (byte)card.Color[1], 
                (byte)card.Color[2], 
                255
            );
            
            yield return StartCoroutine(
                cardGenerator.ShowCardOnScreen(
                    card.StyleID, 
                    card.PersonName, 
                    card.CardPicture, 
                    cardColor, 
                    card.Level
                )
            );
            
            // Small delay between cards
            if (generatedCards.IndexOf(card) < generatedCards.Count - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        // Save cards to PlayFab UserData
        yield return StartCoroutine(SaveCardsToPlayFab(generatedCards));
        
        // ✅ First deck creation (if needed)
        UnityEngine.SceneManagement.SceneManager.LoadScene("Cards");
        
        Debug.LogWarning("[MarketplaceManager] Pack opening complete!");
    }
    
    /// <summary>
    /// Save generated cards to PlayFab UserData
    /// </summary>
    private IEnumerator SaveCardsToPlayFab(List<GeneratedCard> newCards)
    {
        Debug.LogWarning($"[MarketplaceManager] Saving {newCards.Count} cards to PlayFab...");
        
        bool isComplete = false;
        
        // Get existing cards
        var getRequest = new GetUserDataRequest();
        
        PlayFabClientAPI.GetUserData(getRequest, result =>
        {
            // Parse existing cards
            string existingJson = result.Data != null && result.Data.ContainsKey("PlayerCards") 
                ? result.Data["PlayerCards"].Value 
                : "{}";
            
            CardListWrapper cardList;
            
            if (string.IsNullOrEmpty(existingJson) || existingJson == "{}")
            {
                cardList = new CardListWrapper { cards = new List<GeneratedCard>() };
            }
            else
            {
                cardList = JsonUtility.FromJson<CardListWrapper>(existingJson);
                if (cardList == null || cardList.cards == null)
                {
                    cardList = new CardListWrapper { cards = new List<GeneratedCard>() };
                }
            }
            
            // Add new cards
            cardList.cards.AddRange(newCards);
            
            // Save back to PlayFab
            string updatedJson = JsonUtility.ToJson(cardList);
            
            var updateRequest = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { "PlayerCards", updatedJson } }
            };
            
            PlayFabClientAPI.UpdateUserData(updateRequest, 
                updateResult => 
                {
                    Debug.LogWarning($"[MarketplaceManager] ✅ Saved {newCards.Count} cards to PlayFab");
                    isComplete = true;
                }, 
                error => 
                {
                    Debug.LogError($"[MarketplaceManager] ❌ Failed to save cards: {error.GenerateErrorReport()}");
                    isComplete = true;
                }
            );
        }, error =>
        {
            Debug.LogError($"[MarketplaceManager] ❌ Failed to get existing cards: {error.GenerateErrorReport()}");
            isComplete = true;
        });
        
        yield return new WaitUntil(() => isComplete);
    }


    public void CloseInsolvencyPanel()
    {
        insolvencyPanel.SetActive(false);
    }

    private IEnumerator GetPlayerCurrencyBalance()
    {
        var request = new GetUserInventoryRequest();
        bool isCompleted = false;

        PlayFabClientAPI.GetUserInventory(request, result =>
        {
            if (result.VirtualCurrency.ContainsKey("SK"))
            {
                Debug.Log("Množstvo meny SK: " + result.VirtualCurrency["SK"]);
                loveValue.text = result.VirtualCurrency["SK"].ToString();
            }
            else
            {
                Debug.Log("Hráč nemá žiadnu menu SK na účte.");
            }
            isCompleted = true;
        }, error =>
        {
            Debug.LogError("Chyba pri získavaní zostatku meny: " + error.GenerateErrorReport());
            isCompleted = true;
        });

        yield return new WaitUntil(() => isCompleted);
    }

    private IEnumerator CheckCurrencyBalance(int amountNeeded, System.Action<bool> callback)
    {
        var request = new GetUserInventoryRequest();
        bool isCompleted = false;
        bool hasEnoughMoney = false;

        PlayFabClientAPI.GetUserInventory(request, result =>
        {
            int playerBalance;
            result.VirtualCurrency.TryGetValue("SK", out playerBalance);

            if (playerBalance >= amountNeeded)
            {
                hasEnoughMoney = true;
                Debug.Log("Hráč ma dostatok love. Aktuálne množstvo: " + playerBalance);
            }
            else
            {
                insolvencyPanel.SetActive(true);
                Debug.Log("Hráč nemá dostatok meny. Potrebné množstvo: " + amountNeeded + ", aktuálne množstvo: " + playerBalance);
            }
            isCompleted = true;
        }, error =>
        {
            Debug.LogError("Chyba pri získavaní zostatku meny: " + error.GenerateErrorReport());
            isCompleted = true;
        });

        yield return new WaitUntil(() => isCompleted);
        callback(hasEnoughMoney);
    }

    private IEnumerator AddCurrency(int amount)
    {
        var request = new AddUserVirtualCurrencyRequest
        {
            Amount = amount,
            VirtualCurrency = "SK"
        };
        bool isCompleted = false;

        PlayFabClientAPI.AddUserVirtualCurrency(request, result =>
        {
            Debug.Log("Úspešne pridaná mena. Nový zostatok: " + result.Balance);
            isCompleted = true;
        }, error =>
        {
            Debug.LogError("Chyba pri pridávaní meny: " + error.GenerateErrorReport());
            isCompleted = true;
        });

        yield return new WaitUntil(() => isCompleted);
    }

    private IEnumerator SubtractCurrency(int amount)
    {
        var request = new SubtractUserVirtualCurrencyRequest
        {
            Amount = amount,
            VirtualCurrency = "SK"
        };
        bool isCompleted = false;

        PlayFabClientAPI.SubtractUserVirtualCurrency(request, result =>
        {
            Debug.Log("Úspešne odpočítaná mena. Nový zostatok: " + result.Balance);
            isCompleted = true;
        }, error =>
        {
            Debug.LogError("Chyba pri odpočítavaní meny: " + error.GenerateErrorReport());
            isCompleted = true;
        });

        yield return new WaitUntil(() => isCompleted);
    }
}
