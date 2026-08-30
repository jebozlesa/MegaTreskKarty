using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MarketplaceManager : MonoBehaviour
{
    public GameObject tutorial;
    public GameObject insolvencyPanel;
    public CardGenerator cardGenerator;
    public TMP_Text loveValue;
    public GameObject blockPanel;

    [Header("Server-Owned Pack Purchase")]
    public ServerFunctionsManager serverFunctionsManager;

    [Header("Intro/Shop Screens")]
    public GameObject introScreen;
    public GameObject shopScreen;

    private bool isPurchasingPack;

    private void Start()
    {
        if (PlayerPrefs.GetInt("HasCompletedTutorialMarketplace", 0) == 0 && tutorial != null)
        {
            tutorial.SetActive(true);
        }

        ShowIntroScreen();
        StartCoroutine(GetPlayerCurrencyBalance());
    }

    private void ShowIntroScreen()
    {
        if (introScreen != null) introScreen.SetActive(true);
        if (shopScreen != null) shopScreen.SetActive(false);
    }

    public void ShowShopScreen()
    {
        if (introScreen != null) introScreen.SetActive(false);
        if (shopScreen != null) shopScreen.SetActive(true);
    }

    public void StartGenerateCardPack(int packIndex)
    {
        if (isPurchasingPack)
        {
            Debug.LogWarning($"[MarketplaceManager] Pack purchase ignored while busy: packIndex={packIndex}");
            return;
        }

        StartCoroutine(HandleCardPackPurchase(packIndex));
    }

    private IEnumerator HandleCardPackPurchase(int packIndex)
    {
        isPurchasingPack = true;
        SetBlockPanelVisible(true);

        yield return StartCoroutine(PurchaseCardPackFromServer(packIndex));

        SetBlockPanelVisible(false);
        isPurchasingPack = false;
    }

    private IEnumerator PurchaseCardPackFromServer(int packIndex)
    {
        string playerId = PlayFabSettings.staticPlayer?.PlayFabId;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            Debug.LogError("[MarketplaceManager] Cannot purchase pack: missing PlayFab player id.");
            yield break;
        }

        if (serverFunctionsManager == null)
        {
            Debug.LogError("[MarketplaceManager] Cannot purchase pack: ServerFunctionsManager is not assigned.");
            yield break;
        }

        if (cardGenerator == null)
        {
            Debug.LogError("[MarketplaceManager] Cannot purchase pack: CardGenerator animation reference is not assigned.");
            yield break;
        }

        string requestId = Guid.NewGuid().ToString();
        bool isComplete = false;
        OpenCardPackResponse response = null;
        string errorMessage = null;

        Debug.LogWarning(
            $"[MarketplaceManager] Pack purchase requested: player={playerId}, packIndex={packIndex}, requestId={requestId}"
        );

        serverFunctionsManager.PurchaseCardPack(
            playerId,
            packIndex,
            requestId,
            result =>
            {
                if (result == null || result.FunctionResult == null)
                {
                    errorMessage = "Server returned no result";
                    isComplete = true;
                    return;
                }

                try
                {
                    string jsonResponse = JsonConvert.SerializeObject(result.FunctionResult);
                    response = JsonConvert.DeserializeObject<OpenCardPackResponse>(jsonResponse);
                }
                catch (Exception exception)
                {
                    errorMessage = $"Invalid server response: {exception.Message}";
                }
                finally
                {
                    isComplete = true;
                }
            }
        );

        yield return new WaitUntil(() => isComplete);

        if (response == null)
        {
            Debug.LogError($"[MarketplaceManager] Pack purchase failed: {errorMessage}");
            yield break;
        }

        Debug.LogWarning(
            $"[MarketplaceManager] Pack purchase response: success={response.success}, "
                + $"stage={response.stage}, error={response.error}, packIndex={response.packIndex}, "
                + $"requestId={response.requestId}, price={response.price} {response.currencyCode}, "
                + $"cards={response.cards?.Count ?? 0}, firstDeckEnsured={response.firstDeckEnsured}, "
                + $"requiresManualReview={response.requiresManualReview}"
        );

        if (!response.success)
        {
            if (response.error == "insufficient_currency" && insolvencyPanel != null)
            {
                insolvencyPanel.SetActive(true);
            }

            yield break;
        }

        if (response.cards == null || response.cards.Count == 0)
        {
            Debug.LogError("[MarketplaceManager] Pack purchase returned no cards.");
            yield break;
        }

        yield return StartCoroutine(AnimateCards(response.cards));
        yield return StartCoroutine(GetPlayerCurrencyBalance());

        Debug.LogWarning(
            $"[MarketplaceManager] Pack opening complete: packIndex={response.packIndex}, "
                + $"requestId={response.requestId}, cards={response.cards.Count}"
        );
        SceneManager.LoadScene("Cards");
    }

    private IEnumerator AnimateCards(List<GeneratedCard> generatedCards)
    {
        for (int index = 0; index < generatedCards.Count; index++)
        {
            GeneratedCard card = generatedCards[index];
            Debug.LogWarning($"[MarketplaceManager] Showing purchased card: {card.PersonName} ({card.CardID})");

            yield return StartCoroutine(
                cardGenerator.ShowCardOnScreen(
                    card.StyleID,
                    card.PersonName,
                    card.CardPicture,
                    ResolveCardColor(card),
                    card.Level
                )
            );

            if (index < generatedCards.Count - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private static Color32 ResolveCardColor(GeneratedCard card)
    {
        if (card?.Color == null || card.Color.Length < 3)
        {
            return new Color32(255, 255, 255, 255);
        }

        return new Color32(
            (byte)Mathf.Clamp(card.Color[0], 0, 255),
            (byte)Mathf.Clamp(card.Color[1], 0, 255),
            (byte)Mathf.Clamp(card.Color[2], 0, 255),
            255
        );
    }

    public void CloseInsolvencyPanel()
    {
        if (insolvencyPanel != null)
        {
            insolvencyPanel.SetActive(false);
        }
    }

    private IEnumerator GetPlayerCurrencyBalance()
    {
        var request = new GetUserInventoryRequest();
        bool isCompleted = false;

        PlayFabClientAPI.GetUserInventory(
            request,
            result =>
            {
                if (result.VirtualCurrency != null && result.VirtualCurrency.ContainsKey("SK"))
                {
                    if (loveValue != null)
                    {
                        loveValue.text = result.VirtualCurrency["SK"].ToString();
                    }
                }
                else if (loveValue != null)
                {
                    loveValue.text = "0";
                }

                isCompleted = true;
            },
            error =>
            {
                Debug.LogError("Chyba pri ziskavani zostatku meny: " + error.GenerateErrorReport());
                isCompleted = true;
            }
        );

        yield return new WaitUntil(() => isCompleted);
    }

    private void SetBlockPanelVisible(bool visible)
    {
        if (blockPanel != null)
        {
            blockPanel.SetActive(visible);
        }
    }
}
