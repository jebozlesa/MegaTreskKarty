using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerHandManager : MonoBehaviour
{
    // Reference to the multiplayer service
    public MultiplayerService multiplayerService;
    public GameObject kartaPrefab;
    public GameObject playerBoard;
    public AttackDescriptions attackDescriptions;
    public FightSystemMultiplayer fightSystem;
    public ServerFunctionsManager serverFunctionsManager;
    private string myPlayerId;
    private string roomCode;
    private bool cardsCreated = false;


    public void CreateCards(string myPlayerId, string roomCode)
    {
        Debug.Log($"[CreateCards] Called with myPlayerId={myPlayerId}, roomCode={roomCode}");

        serverFunctionsManager.GetRoomPlayersInfo(roomCode, result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                try
                {
                    JObject functionResult = JObject.Parse(result.FunctionResult.ToString());
                    if (functionResult["room"] != null)
                    {
                        var room = functionResult["room"];
                        Debug.Log($"[CreateCards] Room object: {room}");

                        if (room["playerDecks"] != null && room["playerDecks"][myPlayerId] != null && room["playerDecks"][myPlayerId]["cards"] != null)
                        {
                            JArray cardsArray = room["playerDecks"][myPlayerId]["cards"] as JArray;
                            Debug.Log($"[CreateCards] Found cards array, count={cardsArray?.Count ?? 0}");
                            CreateMultiplayerHandFromRoom(cardsArray);
                        }
                        else
                        {
                            Debug.LogWarning("[CreateCards] Deck or cards not found for this player yet.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[CreateCards] Room object missing in function result.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[CreateCards] Error parsing room info for cards: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("[CreateCards] No result from GetRoomPlayersInfo for cards");
            }
        });
    }

    public void CreateCardsFromDecks(string myPlayerId, string roomCode)
    {
        Debug.Log($"[CreateCardsFromDecks] Called with myPlayerId={myPlayerId}, roomCode={roomCode}");

        serverFunctionsManager.GetRoomDecks(roomCode, result =>
        {
            if (result != null && result.FunctionResult != null)
            {
                try
                {
                    var functionResult = Newtonsoft.Json.Linq.JObject.Parse(result.FunctionResult.ToString());
                    Debug.Log($"[CreateCardsFromDecks] FunctionResult: {functionResult}");

                    if (functionResult["playerDecks"] != null && functionResult["playerDecks"][myPlayerId] != null && functionResult["playerDecks"][myPlayerId]["cards"] != null)
                    {
                        var cardsArray = functionResult["playerDecks"][myPlayerId]["cards"] as Newtonsoft.Json.Linq.JArray;
                        Debug.Log($"[CreateCardsFromDecks] Found cards array, count={cardsArray?.Count ?? 0}");
                        CreateMultiplayerHandFromRoom(cardsArray);
                    }
                    else
                    {
                        Debug.LogWarning("[CreateCardsFromDecks] Deck or cards not found for this player yet.");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[CreateCardsFromDecks] Error parsing deck info: " + e.Message);
                }
            }
            else
            {
                Debug.LogError("[CreateCardsFromDecks] No result from GetRoomDecks");
            }
        });
    }

    void CreateMultiplayerHandFromRoom(JArray cardsArray)
    {
        Debug.Log($"[CreateMultiplayerHandFromRoom] Creating cards, array count={cardsArray?.Count ?? 0}");

        Player player = fightSystem.player;
        GameObject playerGO = fightSystem.hrac;
        if (player == null || playerGO == null)
        {
            Debug.LogError("[CreateMultiplayerHandFromRoom] Player or playerGO is null!");
            return;
        }

        foreach (JObject cardData in cardsArray)
        {
            Debug.Log($"[CreateMultiplayerHandFromRoom] Creating card: {cardData["PersonName"]}");
            GeneratedCard card = new GeneratedCard
            {
                CardID = cardData["CardID"]?.ToString(),
                StyleID = cardData["StyleID"]?.ToObject<int>() ?? 0,
                PersonName = cardData["PersonName"]?.ToString(),
                Health = cardData["MaxHealth"]?.ToObject<int>() ?? cardData["Health"]?.ToObject<int>() ?? 0,
                Color = cardData["Color"]?.ToObject<List<int>>()?.ToArray() ?? new int[] { 255, 255, 255 },
                Level = cardData["Level"]?.ToObject<int>() ?? 1,
                CardPicture = cardData["CardPicture"]?.ToString(),
                Attack1 = cardData["Attack1"]?.ToObject<int>() ?? 0,
                Attack2 = cardData["Attack2"]?.ToObject<int>() ?? 0,
                Attack3 = cardData["Attack3"]?.ToObject<int>() ?? 0,
                Attack4 = cardData["Attack4"]?.ToObject<int>() ?? 0
                // ...dopln ďalšie polia podľa potreby
            };
            CreateCardInGame(card, playerGO, player);
        }
    }

    public void CreateCardInGame(GeneratedCard cardData, GameObject playerGO, Player player)
    {
        GameObject novaKarta = Instantiate(kartaPrefab, playerGO.transform);

        // Assigning the card properties from the PlayFab data
        novaKarta.GetComponent<Kard>().cardId = cardData.CardID;
        novaKarta.GetComponent<Kard>().styleId = cardData.StyleID;
        novaKarta.GetComponent<Kard>().cardName = cardData.PersonName;
        novaKarta.GetComponent<Kard>().health = cardData.Health;
        novaKarta.GetComponent<Kard>().strength = cardData.Strength;
        novaKarta.GetComponent<Kard>().speed = cardData.Speed;
        novaKarta.GetComponent<Kard>().attack = cardData.Attack;
        novaKarta.GetComponent<Kard>().defense = cardData.Defense;
        novaKarta.GetComponent<Kard>().knowledge = cardData.Knowledge;
        novaKarta.GetComponent<Kard>().charisma = cardData.Charisma;
        Color32 cardColor = new Color32((byte)cardData.Color[0], (byte)cardData.Color[1], (byte)cardData.Color[2], 255);
        novaKarta.GetComponent<Kard>().color = cardColor;
        novaKarta.GetComponent<Kard>().level = cardData.Level;
        novaKarta.GetComponent<Kard>().experience = cardData.Experience;
        novaKarta.GetComponent<Kard>().attack1 = cardData.Attack1;
        novaKarta.GetComponent<Kard>().attack2 = cardData.Attack2;
        novaKarta.GetComponent<Kard>().attack3 = cardData.Attack3;
        novaKarta.GetComponent<Kard>().attack4 = cardData.Attack4;
        novaKarta.GetComponent<Kard>().image = cardData.CardPicture;

        // Additional properties from the original function
        novaKarta.GetComponent<Kard>().battleArea = playerBoard;
        novaKarta.GetComponent<Kard>().countAttack1 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), cardData.Attack1);
        novaKarta.GetComponent<Kard>().countAttack2 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), cardData.Attack2);
        novaKarta.GetComponent<Kard>().countAttack3 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), cardData.Attack3);
        novaKarta.GetComponent<Kard>().countAttack4 = attackDescriptions.LoadAttackCount(novaKarta.GetComponent<Kard>(), cardData.Attack4);

        player.AddCardToHand(novaKarta.GetComponent<Kard>());
    }

}
