using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayFabCardManager : MonoBehaviour
{
    public delegate void CardDataCallback(Dictionary<string, object> cardData);
    public delegate void UpdateCallback(bool success);

    void Awake()
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " PlayFabCardManager.Awake => START");
        Login();
    }

    void Login()
    {
//        loadingImage.SetActive(true);
        string username = PlayerPrefs.GetString("username");
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");

        var request = new LoginWithEmailAddressRequest 
            {
                Email = email,
                Password = password
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);
//        loadingImage.SetActive(false);
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("PlayFabCardManager = Sicko dobre");
  //      loadingImage.SetActive(false);
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("PlayFabCardManager = Daco nahovno");
  //      errorImage.SetActive(true);
        Debug.Log(error.GenerateErrorReport());
    }

    public void GetCardData(int cardId, CardDataCallback callback)
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " PlayFabCardManager.GetCardData => START " + cardId);
        GetUserDataRequest request = new GetUserDataRequest
        {
            Keys = new List<string> { "PlayerCards" }  // Změna klíče na "PlayerCards"
        };

        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerCards"))  // Zkontrolujte, zda data obsahují klíč "PlayerCards"
            {
                var cardsData = JsonUtility.FromJson<CardsContainer>(result.Data["PlayerCards"].Value);  // Deserializujte data s klíčem "PlayerCards"
                foreach (var card in cardsData.cards)
                {
                    if (card.StyleID == cardId)
                    {
                        callback?.Invoke(ConvertCardDataToDictionary(card));
                        return;
                    }
                }
            }
            callback?.Invoke(null);
        }, error =>
        {
            Debug.LogError(error.GenerateErrorReport());
            callback?.Invoke(null);
        });
    }


    public Dictionary<string, object> ConvertCardDataToDictionary(CardData cardData)
    {
        Dictionary<string, object> cardDataDict = new Dictionary<string, object>
        {
            {"CardID", cardData.CardID},
            {"StyleID", cardData.StyleID},
            {"PersonName", cardData.PersonName},
            {"Level", cardData.Level},
            {"Experience", cardData.Experience},
            {"Health", cardData.Health},
            {"Strength", cardData.Strength},
            {"Speed", cardData.Speed},
            {"Attack", cardData.Attack},
            {"Defense", cardData.Defense},
            {"Knowledge", cardData.Knowledge},
            {"Charisma", cardData.Charisma},
            {"Color", cardData.Color},
            {"Attack1", cardData.Attack1},
            {"Attack2", cardData.Attack2},
            {"Attack3", cardData.Attack3},
            {"Attack4", cardData.Attack4},
            {"CardPicture", cardData.CardPicture}
        };

        return cardDataDict;
    }


    public void UpdateCardData(string cardId, Dictionary<string, string> updates, UpdateCallback callback)
    {
        UpdateUserDataRequest request = new UpdateUserDataRequest
        {
            Data = updates
        };

        PlayFabClientAPI.UpdateUserData(request, result =>
        {
            callback?.Invoke(true);
        }, error =>
        {
            Debug.LogError(error.GenerateErrorReport());
            callback?.Invoke(false);
        });
    }

    [System.Serializable]
    public class CardsContainer
    {
        public List<CardData> cards;
    }

    [System.Serializable]
    public class CardData
    {
        public string CardID;
        public int StyleID;
        public string PersonName;
        public int Level;
        public int Experience;
        public int Health;
        public int Strength;
        public int Speed;
        public int Attack;
        public int Defense;
        public int Knowledge;
        public int Charisma;
        public List<int> Color;
        public int Attack1;
        public int Attack2;
        public int Attack3;
        public int Attack4;
        public string CardPicture;
    }
}
