using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

public class AlbumLoveValue : MonoBehaviour

{
    public TMP_Text loveValue;

    public static AlbumLoveValue Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator GetPlayerCurrencyBalance()
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
}
