using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class MarketplaceManager : MonoBehaviour
{

    public GameObject tutorial;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("HasCompletedTutorialMarketplace", 0) == 0) { tutorial.SetActive(true); }
        GetPlayerCurrencyBalance();
    }

    public void GetPlayerCurrencyBalance()
    {
        // Pošlite požiadavku na získanie zostatku virtuálnej meny
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), result =>
        {
            // Prejdite cez všetky meny v inventári a vypíšte množstvo pre zvolenú menu
            if (result.VirtualCurrency.ContainsKey("SK"))
            {
                Debug.Log("Množstvo meny SK: " + result.VirtualCurrency["SK"]);
            }
            else
            {
                Debug.Log("Hráč nemá žiadnu menu SK na účte.");
            }
        }, error =>
        {
            Debug.LogError("Chyba pri získavaní zostatku meny: " + error.GenerateErrorReport());
        });
    }
}
