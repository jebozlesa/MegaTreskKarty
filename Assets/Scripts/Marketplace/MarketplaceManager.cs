using System.Collections;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class MarketplaceManager : MonoBehaviour
{
    public GameObject tutorial;
    public GameObject insolvencyPanel;
    public CardGenerator cardGenerator;
    public TMP_Text loveValue;

    public GameObject blockPanel;

    void Start()
    {
        if (PlayerPrefs.GetInt("HasCompletedTutorialMarketplace", 0) == 0) { tutorial.SetActive(true); }
        StartCoroutine(GetPlayerCurrencyBalance());
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

            // // Aktualizácia zostatku meny
            yield return StartCoroutine(GetPlayerCurrencyBalance());

            // // Generovanie balíčka kariet
            yield return StartCoroutine(cardGenerator.GenerateCardPack(packIndex));
        }
        blockPanel.SetActive(false);
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
