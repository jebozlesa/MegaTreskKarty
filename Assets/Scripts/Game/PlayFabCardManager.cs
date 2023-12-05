using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayFabCardManager : MonoBehaviour
{
    public delegate void CardDataCallback(Dictionary<string, object> cardData);
    public delegate void UpdateCallback(bool success);

    public void GetCardData(string cardId, CardDataCallback callback)
    {
        if (!PlayFabManagerLogin.IsLoggedIn)
        {
            Debug.LogError("PlayFabCardManager.GetCardData ==> Hráč nie je prihlásený.");
            return;
        }

        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " PlayFabCardManager.GetCardData => START " + cardId);
        GetUserDataRequest request = new GetUserDataRequest
        {
            Keys = new List<string> { "PlayerCards" }
        };

        PlayFabClientAPI.GetUserData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerCards"))
            {
                var cardsData = JsonUtility.FromJson<CardsContainer>(result.Data["PlayerCards"].Value);
                foreach (var card in cardsData.cards)
                {
                    if (card.CardID == cardId)
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

    public void UpdateCardData(string cardID, Dictionary<string, string> updates, UpdateCallback callback)
    {
        if (!PlayFabManagerLogin.IsLoggedIn)
        {
            Debug.LogError("PlayFabCardManager.UpdateCardData ==> Hráč nie je prihlásený.");
            return;
        }

        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " PlayFabCardManager.UpdateCardData => START " + cardID);

        GetUserDataRequest getRequest = new GetUserDataRequest
        {
            Keys = new List<string> { "PlayerCards" }
        };

        PlayFabClientAPI.GetUserData(getRequest, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerCards"))
            {
                var cardsData = JsonUtility.FromJson<CardsContainer>(result.Data["PlayerCards"].Value);

                foreach (var card in cardsData.cards)
                {
                    if (card.CardID == cardID)
                    {
                        foreach (var update in updates)
                        {
                            var field = card.GetType().GetField(update.Key);
                            var property = card.GetType().GetProperty(update.Key);

                            if (field != null)
                            {
                                field.SetValue(card, Convert.ChangeType(update.Value, field.FieldType));
                            }
                            else if (property != null)
                            {
                                property.SetValue(card, Convert.ChangeType(update.Value, property.PropertyType));
                            }
                        }
                        break;
                    }
                }

                string updatedData = JsonUtility.ToJson(cardsData);

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
