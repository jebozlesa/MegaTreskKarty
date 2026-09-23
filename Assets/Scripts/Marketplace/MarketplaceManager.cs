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
    private const string MainCurrencyCode = "SK";
    private const int DefaultPackPrice = 5;

    [Serializable]
    public class ClientPackPrice
    {
        public int packIndex;
        public int price = DefaultPackPrice;
    }

    public GameObject insolvencyPanel;
    public CardGenerator cardGenerator;
    public TMP_Text loveValue;

    [Header("Server-Owned Pack Purchase")]
    public ServerFunctionsManager serverFunctionsManager;
    public TutorialService tutorialService;
    public MarketplaceTutorialController marketplaceTutorialController;
    public GameObject purchaseConfirmationPanel;
    public GameObject backButton;
    public string librarySceneName = "Cards";
    public List<ClientPackPrice> clientPackPrices = new List<ClientPackPrice>
    {
        new ClientPackPrice { packIndex = 0, price = DefaultPackPrice },
        new ClientPackPrice { packIndex = 1, price = DefaultPackPrice },
        new ClientPackPrice { packIndex = 2, price = DefaultPackPrice },
        new ClientPackPrice { packIndex = 3, price = DefaultPackPrice },
        new ClientPackPrice { packIndex = 4, price = DefaultPackPrice },
        new ClientPackPrice { packIndex = 5, price = DefaultPackPrice },
        new ClientPackPrice { packIndex = 6, price = DefaultPackPrice },
    };

    [Header("Intro/Shop Screens")]
    public GameObject introScreen;
    public GameObject shopScreen;

    private bool isPurchasingPack;
    private int pendingPackIndex = -1;
    private int currentCurrencyBalance;
    private bool hasCurrencyBalance;
    private TutorialStateResponse tutorialState;
    private bool firstPackTutorialActive;
    private bool keepLoadingOverlayVisible;

    private void Awake()
    {
        marketplaceTutorialController?.Hide();
    }

    private IEnumerator Start()
    {
        SetPurchaseConfirmationVisible(false);
        SetMarketplaceContentVisible(showIntro: false, showShop: false);
        SetBackBlocked(true);
        SceneLoadingOverlay.SetMessage("LOADING...");
        SceneLoadingOverlay.Show();

        if (tutorialService == null || marketplaceTutorialController == null)
        {
            FailClosed("Marketplace tutorial references are not assigned.");
            yield break;
        }

        if (!marketplaceTutorialController.Configure(tutorialService))
        {
            FailClosed("Marketplace tutorial references are not assigned.");
            yield break;
        }
        yield return StartCoroutine(LoadMarketplaceTutorialState());
        if (tutorialState == null || !tutorialState.success)
        {
            yield break;
        }

        ApplyResolvedTutorialPresentation(tutorialState);
        yield return StartCoroutine(GetPlayerCurrencyBalance());

        marketplaceTutorialController.Begin(tutorialState);
        SceneLoadingOverlay.Hide();

        Debug.LogWarning(
            $"[MarketplaceManager] Bootstrap complete: firstPackActive={firstPackTutorialActive}, "
                + $"route={tutorialState.recommendedRoute}, stateResolved=true"
        );
    }

    public void ShowShopScreen()
    {
        SetMarketplaceContentVisible(showIntro: false, showShop: true);
    }

    public void StartGenerateCardPack(int packIndex)
    {
        if (firstPackTutorialActive
            && (marketplaceTutorialController == null || !marketplaceTutorialController.CanSelectPack))
        {
            Debug.LogWarning(
                $"[MarketplaceManager] Tutorial pack selection ignored before instruction acknowledgement: packIndex={packIndex}"
            );
            return;
        }

        if (isPurchasingPack)
        {
            Debug.LogWarning($"[MarketplaceManager] Pack purchase ignored while busy: packIndex={packIndex}");
            return;
        }

        if (pendingPackIndex >= 0 && IsPurchaseConfirmationVisible())
        {
            Debug.LogWarning(
                $"[MarketplaceManager] Pack purchase ignored while confirmation is open: "
                    + $"currentPackIndex={pendingPackIndex}, requestedPackIndex={packIndex}"
            );
            return;
        }

        if (insolvencyPanel != null)
        {
            insolvencyPanel.SetActive(false);
        }

        if (!HasEnoughClientCurrencyForPack(packIndex))
        {
            int packPrice = GetClientPackPrice(packIndex);
            Debug.LogWarning(
                $"[MarketplaceManager] Pack purchase blocked by client balance check: "
                    + $"packIndex={packIndex}, balance={currentCurrencyBalance} {MainCurrencyCode}, "
                    + $"price={packPrice} {MainCurrencyCode}"
            );

            if (insolvencyPanel != null)
            {
                insolvencyPanel.SetActive(true);
            }

            return;
        }

        pendingPackIndex = packIndex;
        Debug.LogWarning($"[MarketplaceManager] Pack purchase confirmation requested: packIndex={packIndex}");

        if (purchaseConfirmationPanel == null)
        {
            Debug.LogError("[MarketplaceManager] Cannot confirm pack purchase: purchaseConfirmationPanel is not assigned.");
            pendingPackIndex = -1;
            return;
        }

        SetPurchaseConfirmationVisible(true);
    }

    public void ConfirmPendingPackPurchase()
    {
        if (isPurchasingPack)
        {
            Debug.LogWarning("[MarketplaceManager] Pack purchase confirmation ignored while busy.");
            return;
        }

        if (pendingPackIndex < 0)
        {
            Debug.LogWarning("[MarketplaceManager] Pack purchase confirmation ignored: no pending pack.");
            SetPurchaseConfirmationVisible(false);
            return;
        }

        int packIndex = pendingPackIndex;
        bool openLibraryAfterPurchase = ShouldOpenLibraryAfterPurchase();
        pendingPackIndex = -1;
        SetPurchaseConfirmationVisible(false);

        Debug.LogWarning(
            $"[MarketplaceManager] Pack purchase confirmed: packIndex={packIndex}, "
                + $"openLibraryAfterPurchase={openLibraryAfterPurchase}"
        );

        StartCoroutine(HandleCardPackPurchase(packIndex, openLibraryAfterPurchase));
    }

    public void CancelPendingPackPurchase()
    {
        Debug.LogWarning($"[MarketplaceManager] Pack purchase cancelled: packIndex={pendingPackIndex}");
        pendingPackIndex = -1;
        SetPurchaseConfirmationVisible(false);
    }

    private IEnumerator HandleCardPackPurchase(int packIndex, bool openLibraryAfterPurchase)
    {
        isPurchasingPack = true;
        keepLoadingOverlayVisible = false;
        SceneLoadingOverlay.SetMessage("BUYING...");
        SceneLoadingOverlay.Show();

        yield return StartCoroutine(PurchaseCardPackFromServer(packIndex, openLibraryAfterPurchase));

        if (!keepLoadingOverlayVisible)
        {
            SceneLoadingOverlay.Hide();
        }
        isPurchasingPack = false;
    }

    private IEnumerator PurchaseCardPackFromServer(int packIndex, bool openLibraryAfterPurchase)
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

        SceneLoadingOverlay.Hide();
        yield return StartCoroutine(AnimateCards(response.cards));
        yield return StartCoroutine(GetPlayerCurrencyBalance());

        Debug.LogWarning(
            $"[MarketplaceManager] Pack opening complete: packIndex={response.packIndex}, "
                + $"requestId={response.requestId}, cards={response.cards.Count}"
        );

        if (openLibraryAfterPurchase)
        {
            SceneLoadingOverlay.SetMessage("VERIFYING...");
            SceneLoadingOverlay.Show();

            System.Threading.Tasks.Task<TutorialStateResponse> stateTask =
                tutorialService.RefreshCurrentPlayerStateAsync();
            yield return new WaitUntil(() => stateTask.IsCompleted);

            tutorialState = stateTask.Result;
            bool tutorialPurchaseVerified = tutorialState != null
                && tutorialState.success
                && tutorialState.gates?.safeCardDeckState == true
                && tutorialState.gates.needsFirstPack == false;
            if (!tutorialPurchaseVerified)
            {
                keepLoadingOverlayVisible = true;
                FailClosed(
                    $"First pack verification failed: stage={tutorialState?.stage}, error={tutorialState?.error}"
                );
                yield break;
            }

            firstPackTutorialActive = false;
            marketplaceTutorialController.Begin(tutorialState);
            SetBackBlocked(false);
            Debug.LogWarning(
                $"[MarketplaceManager] Tutorial pack purchase complete; opening library scene '{librarySceneName}'."
            );
            SceneLoadingOverlay.Hide();
            SceneManager.LoadScene(librarySceneName);
        }
        else
        {
            Debug.LogWarning("[MarketplaceManager] Pack purchase complete; staying in Marketplace.");
        }
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
                if (result.VirtualCurrency != null && result.VirtualCurrency.ContainsKey(MainCurrencyCode))
                {
                    currentCurrencyBalance = result.VirtualCurrency[MainCurrencyCode];
                    hasCurrencyBalance = true;

                    if (loveValue != null)
                    {
                        loveValue.text = currentCurrencyBalance.ToString();
                    }
                }
                else
                {
                    currentCurrencyBalance = 0;
                    hasCurrencyBalance = true;

                    if (loveValue != null)
                    {
                        loveValue.text = "0";
                    }
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

    private void SetPurchaseConfirmationVisible(bool visible)
    {
        if (purchaseConfirmationPanel != null)
        {
            purchaseConfirmationPanel.SetActive(visible);
        }
    }

    private bool ShouldOpenLibraryAfterPurchase()
    {
        return firstPackTutorialActive || tutorialState?.gates?.needsFirstPack == true;
    }

    private bool IsPurchaseConfirmationVisible()
    {
        return purchaseConfirmationPanel != null && purchaseConfirmationPanel.activeInHierarchy;
    }

    private bool HasEnoughClientCurrencyForPack(int packIndex)
    {
        return !hasCurrencyBalance || currentCurrencyBalance >= GetClientPackPrice(packIndex);
    }

    private int GetClientPackPrice(int packIndex)
    {
        if (clientPackPrices != null)
        {
            ClientPackPrice packPrice = clientPackPrices.Find(entry => entry != null && entry.packIndex == packIndex);
            if (packPrice != null && packPrice.price > 0)
            {
                return packPrice.price;
            }
        }

        return DefaultPackPrice;
    }

    private IEnumerator LoadMarketplaceTutorialState()
    {
        System.Threading.Tasks.Task<TutorialStateResponse> stateTask =
            tutorialService.GetCurrentPlayerStateAsync();
        if (!stateTask.IsCompleted)
        {
            yield return new WaitUntil(() => stateTask.IsCompleted);
        }

        tutorialState = stateTask.Result;
        if (tutorialState == null || !tutorialState.success)
        {
            FailClosed(
                $"Tutorial state load failed: stage={tutorialState?.stage}, error={tutorialState?.error}"
            );
            yield break;
        }
    }

    private void ApplyResolvedTutorialPresentation(TutorialStateResponse state)
    {
        firstPackTutorialActive = state?.gates?.needsFirstPack == true;
        SetBackBlocked(firstPackTutorialActive);
        SetMarketplaceContentVisible(
            showIntro: !firstPackTutorialActive,
            showShop: firstPackTutorialActive
        );
    }

    private void SetMarketplaceContentVisible(bool showIntro, bool showShop)
    {
        if (introScreen != null) introScreen.SetActive(showIntro);
        if (shopScreen != null) shopScreen.SetActive(showShop);
    }

    private void FailClosed(string reason)
    {
        SetMarketplaceContentVisible(showIntro: false, showShop: false);
        SetBackBlocked(true);
        marketplaceTutorialController?.Hide();
        SceneLoadingOverlay.SetMessage("CONNECTION ERROR");
        SceneLoadingOverlay.Show();
        Debug.LogError($"[MarketplaceManager] {reason}");
    }

    private void SetBackBlocked(bool blocked)
    {
        if (backButton != null)
        {
            backButton.SetActive(!blocked);
        }
    }
}
