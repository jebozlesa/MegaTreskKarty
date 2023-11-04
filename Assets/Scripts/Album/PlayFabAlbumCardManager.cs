using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using System;
using static PlayFabCardManager;

public class PlayFabAlbumCardManager : MonoBehaviour
{
    public delegate void CardDataCallback(Dictionary<string, object> cardData);
    public delegate void UpdateCallback(bool success);

    public void UpdateCardAttack(string cardID, int attackID, int newAttackValue, Action<bool> callback)
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + " PlayFabAlbumCardManager.UpdateCardAttack => START " + cardID);
        Debug.Log($"UpdateCardAttack: {newAttackValue} -> {attackID}");

        var getRequest = new GetUserDataRequest
        {
            Keys = new List<string> { "PlayerCards" }
        };

        PlayFabClientAPI.GetUserData(getRequest, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerCards"))
            {
                var cardsData = JsonUtility.FromJson<PlayFabCardManager.CardsContainer>(result.Data["PlayerCards"].Value);

                foreach (var card in cardsData.cards)
                {
                    if (card.CardID == cardID)
                    {
                        // Priradenie novej hodnoty k správnemu útoku podľa ID
                        bool attackUpdated = false;
                        if (card.Attack1 == attackID)
                        {
                            card.Attack1 = newAttackValue;
                            attackUpdated = true;
                        }
                        else if (card.Attack2 == attackID)
                        {
                            card.Attack2 = newAttackValue;
                            attackUpdated = true;
                        }
                        else if (card.Attack3 == attackID)
                        {
                            card.Attack3 = newAttackValue;
                            attackUpdated = true;
                        }
                        else if (card.Attack4 == attackID)
                        {
                            card.Attack4 = newAttackValue;
                            attackUpdated = true;
                        }

                        // Ak bolo ID útoku nájdené a aktualizované
                        if (attackUpdated)
                        {
                            string updatedData = JsonUtility.ToJson(cardsData);

                            var updateRequest = new UpdateUserDataRequest
                            {
                                Data = new Dictionary<string, string> { { "PlayerCards", updatedData } }
                            };

                            PlayFabClientAPI.UpdateUserData(updateRequest, updateResult =>
                            {
                                Debug.Log("Útok karty bol úspešne aktualizovaný");
                                callback?.Invoke(true);
                            },
                            error =>
                            {
                                Debug.LogError(error.GenerateErrorReport());
                                callback?.Invoke(false);
                            });
                        }
                        else
                        {
                            Debug.LogError("Nesprávne ID útoku");
                            callback?.Invoke(false);
                        }

                        return;
                    }
                }
                Debug.LogError("Karta s ID nenalezena");
                callback?.Invoke(false);
            }
            else
            {
                Debug.LogError("Data nenalezena v PlayFab");
                callback?.Invoke(false);
            }
        },
        error =>
        {
            Debug.LogError(error.GenerateErrorReport());
            callback?.Invoke(false);
        });
    }

}
