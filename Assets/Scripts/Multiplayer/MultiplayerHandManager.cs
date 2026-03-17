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



    private readonly Dictionary<string, GeneratedCard> cardDefinitions = new Dictionary<string, GeneratedCard>();


    public void CreateCardsFromDecks(string myPlayerId, string roomCode)
    {
        Debug.Log($"[CreateCardsFromDecks] Called with myPlayerId={myPlayerId}, roomCode={roomCode}");
        
        // Prevent duplicate card creation
        if (cardsCreated)
        {
            Debug.Log("[CreateCardsFromDecks] Cards already created, skipping...");
            return;
        }

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
                    Health = cardData["Health"]?.ToObject<int>() ?? 0,
                    MaxHealth = cardData["MaxHealth"]?.ToObject<int>() ?? cardData["Health"]?.ToObject<int>() ?? 0,
                Strength = cardData["Strength"]?.ToObject<int>() ?? 0,
                Speed = cardData["Speed"]?.ToObject<int>() ?? 0,
                Attack = cardData["Attack"]?.ToObject<int>() ?? 0,
                Defense = cardData["Defense"]?.ToObject<int>() ?? 0,
                Knowledge = cardData["Knowledge"]?.ToObject<int>() ?? 0,
                Charisma = cardData["Charisma"]?.ToObject<int>() ?? 0,
                Experience = cardData["Experience"]?.ToObject<int>() ?? 0,
                Color = cardData["Color"]?.ToObject<List<int>>()?.ToArray() ?? new int[] { 255, 255, 255 },
                Level = cardData["Level"]?.ToObject<int>() ?? 1,
                CardPicture = cardData["CardPicture"]?.ToString(),
                Attack1 = cardData["Attack1"]?.ToObject<int>() ?? 0,
                Attack2 = cardData["Attack2"]?.ToObject<int>() ?? 0,
                Attack3 = cardData["Attack3"]?.ToObject<int>() ?? 0,
                Attack4 = cardData["Attack4"]?.ToObject<int>() ?? 0
                // ...dopln dalsie polia podla potreby
            };
                if (!string.IsNullOrEmpty(card.CardID))
                {
                    cardDefinitions[card.CardID] = card;
                }
            CreateCardInGame(card, playerGO, player);
        }
        
        // Mark cards as created to prevent duplicates
        cardsCreated = true;
        Debug.Log("[CreateMultiplayerHandFromRoom] Cards creation completed, flag set");
    }

    public Kard CreateCardInGame(GeneratedCard cardData, GameObject parentOverride, Player player, bool addToHand = true, bool enableMultiplayerDrag = true, GameObject battleAreaOverride = null)
    {
        GameObject parent = parentOverride != null ? parentOverride : playerBoard;
        if (parent == null)
        {
            Debug.LogError("[MultiplayerHandManager] Cannot instantiate card without a valid parent GameObject.");
            return null;
        }

        GameObject novaKarta = Instantiate(kartaPrefab, parent.transform);
        var kardComponent = novaKarta.GetComponent<Kard>();

        // Assigning the card properties from the PlayFab data
        kardComponent.cardId = cardData.CardID;
        kardComponent.styleId = cardData.StyleID;
        kardComponent.cardName = cardData.PersonName;
        kardComponent.health = cardData.Health;
        kardComponent.maxHealth = cardData.MaxHealth;  // [OK] Initialize maxHealth for HealthBar!
        kardComponent.strength = cardData.Strength;
        kardComponent.speed = cardData.Speed;
        kardComponent.attack = cardData.Attack;
        kardComponent.defense = cardData.Defense;
        kardComponent.knowledge = cardData.Knowledge;
        kardComponent.charisma = cardData.Charisma;
        kardComponent.level = cardData.Level;
        kardComponent.experience = cardData.Experience;
        kardComponent.attack1 = cardData.Attack1;
        kardComponent.attack2 = cardData.Attack2;
        kardComponent.attack3 = cardData.Attack3;
        kardComponent.attack4 = cardData.Attack4;
        kardComponent.image = cardData.CardPicture;

        if (cardData.Color != null && cardData.Color.Length >= 3)
        {
            Color32 cardColor = new Color32((byte)Mathf.Clamp(cardData.Color[0], 0, 255), (byte)Mathf.Clamp(cardData.Color[1], 0, 255), (byte)Mathf.Clamp(cardData.Color[2], 0, 255), 255);
            kardComponent.color = cardColor;
        }

        // Additional properties from the original function
        GameObject battleArea = battleAreaOverride != null ? battleAreaOverride : playerBoard;
        kardComponent.battleArea = battleArea;
        kardComponent.countAttack1 = attackDescriptions.LoadAttackCount(kardComponent, cardData.Attack1);
        kardComponent.countAttack2 = attackDescriptions.LoadAttackCount(kardComponent, cardData.Attack2);
        kardComponent.countAttack3 = attackDescriptions.LoadAttackCount(kardComponent, cardData.Attack3);
        kardComponent.countAttack4 = attackDescriptions.LoadAttackCount(kardComponent, cardData.Attack4);

        if (addToHand && player != null)
        {
            player.AddCardToHand(kardComponent);
        }

        var legacyDrag = novaKarta.GetComponent<DragKard>();
        if (legacyDrag != null)
        {
            legacyDrag.enabled = false;
        }

        if (enableMultiplayerDrag)
        {
            var multiplayerDrag = novaKarta.GetComponent<MultiplayerCardDrag>();
            if (multiplayerDrag == null)
            {
                multiplayerDrag = novaKarta.AddComponent<MultiplayerCardDrag>();
            }
            multiplayerDrag.Initialize(fightSystem);
        }
        else
        {
            var multiplayerDrag = novaKarta.GetComponent<MultiplayerCardDrag>();
            if (multiplayerDrag != null)
            {
                Destroy(multiplayerDrag);
            }
        }

        return kardComponent;
    }

    public void ResetCardsState()
    {
        cardsCreated = false;
        cardDefinitions.Clear();
        Debug.Log("[MultiplayerHandManager] Cards state reset");
    }

        public GeneratedCard GetCardDefinition(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                return null;
            }

            cardDefinitions.TryGetValue(cardId, out var definition);
            return definition;
        }

}
