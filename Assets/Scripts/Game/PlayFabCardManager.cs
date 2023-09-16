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
       // Login();
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


    public void UpdateCardData(int styleID, Dictionary<string, string> updates, UpdateCallback callback)
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " PlayFabCardManager.UpdateCardData => START " + styleID);

        GetUserDataRequest getRequest = new GetUserDataRequest
        {
            Keys = new List<string> { "PlayerCards" }
        };

        PlayFabClientAPI.GetUserData(getRequest, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerCards"))
            {
                Debug.Log("Data from PlayFab: " + result.Data["PlayerCards"].Value);
                var cardsData = JsonUtility.FromJson<CardsContainer>(result.Data["PlayerCards"].Value);
                Debug.Log("Number of cards: " + cardsData.cards.Count);

                foreach (var card in cardsData.cards)
                {
                    if (card.StyleID == styleID)
                    {
                        // Aktualizujte hodnoty karty dle potřeby
                        foreach (var update in updates)
                        {
                            Debug.Log("Checking card with StyleID: " + card.StyleID);

                            var field = card.GetType().GetField(update.Key);
                            var property = card.GetType().GetProperty(update.Key);

                            if (field != null)
                            {
                                Debug.Log($"Updating {update.Key} from {field.GetValue(card)} to {update.Value}");
                                field.SetValue(card, Convert.ChangeType(update.Value, field.FieldType));
                                Debug.Log($"Updated {update.Key} to {field.GetValue(card)}");
                            }
                            else if (property != null)
                            {
                                Debug.Log($"Updating {update.Key} from {property.GetValue(card)} to {update.Value}");
                                property.SetValue(card, Convert.ChangeType(update.Value, property.PropertyType));
                                Debug.Log($"Updated {update.Key} to {property.GetValue(card)}");
                            }
                            else
                            {
                                Debug.Log("Neither field nor property found for " + update.Key);
                            }

                        }
                        break;
                    }
                }

                // Serializujte aktualizovaný seznam karet zpět do formátu JSON
                string updatedData = JsonUtility.ToJson(cardsData);
                Debug.Log(updatedData);

                UpdateUserDataRequest updateRequest = new UpdateUserDataRequest
                {
                    Data = new Dictionary<string, string> { { "PlayerCards", updatedData } }
                };

                PlayFabClientAPI.UpdateUserData(updateRequest, updateResult =>
                {
                    callback?.Invoke(true);
                }, error =>
                {
                    Debug.LogError(error.GenerateErrorReport());
                    callback?.Invoke(false);
                });
            }
            else
            {
                Debug.LogError("Daco tu chyba");
                callback?.Invoke(false);
            }
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
