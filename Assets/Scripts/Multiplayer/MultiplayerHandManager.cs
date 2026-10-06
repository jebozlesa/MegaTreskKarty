using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private int preparationGeneration;
    private TaskCompletionSource<bool> preparation;
    private readonly List<Kard> preparedCards = new List<Kard>();
    private readonly Dictionary<string, GeneratedCard> cardDefinitions = new Dictionary<string, GeneratedCard>();


    public Task PrepareStartupHandAsync(string myPlayerId, string roomCode)
    {
        if (preparation != null) return preparation.Task;
        int generation = ++preparationGeneration;
        preparation = new TaskCompletionSource<bool>();
        var completion = preparation;
        serverFunctionsManager.GetRoomDecks(roomCode, myPlayerId, result =>
        {
            if (!OwnsPreparation(generation)) { completion.TrySetCanceled(); return; }
            try
            {
                object raw = result?.FunctionResult;
                JObject response = raw is string text ? JObject.Parse(text) : raw == null ? null : JObject.FromObject(raw);
                var state = BattleContractMapper.ParseMatchStateResult(response);
                if (response?["success"]?.Value<bool>() != true || state?.roomCode != roomCode
                    || state.viewerPlayerId != myPlayerId || state.status != "active")
                    throw new System.InvalidOperationException("Startup deck read was not confirmed");
                JArray cards = response["playerDecks"]?[myPlayerId]?["cards"] as JArray;
                ValidateStartupCardIds(cards?.Select(card => (string)card["CardID"]).ToArray());
                CreateMultiplayerHandFromRoom(cards);
                StartCoroutine(FinishPreparation(generation, completion));
            }
            catch (System.Exception error) { completion.TrySetException(error); }
        });
        return completion.Task;
    }

    private static void ValidateStartupCardIds(string[] ids)
    {
        if (ids == null || ids.Length != 5 || ids.Any(string.IsNullOrEmpty) || ids.Distinct().Count() != 5)
            throw new System.InvalidOperationException("Startup requires five distinct cards");
    }

    private bool OwnsPreparation(int generation) => this != null && isActiveAndEnabled
        && generation == preparationGeneration && multiplayerService != null && multiplayerService.CanContinueStartup;

    private IEnumerator FinishPreparation(int generation, TaskCompletionSource<bool> completion)
    {
        // Allow active Kard components to run Start before acknowledging local readiness.
        yield return null;
        if (!OwnsPreparation(generation)) { completion.TrySetCanceled(); yield break; }
        bool complete = preparedCards.Count == 5 && preparedCards.All(card => card != null
            && card.gameObject.activeInHierarchy && fightSystem.player.hand.Contains(card)
            && card.cardImage != null && card.cardImage.sprite != null);
        if (!complete) completion.TrySetException(new System.InvalidOperationException("Five startup card visuals were not initialized"));
        else
        {
            Debug.LogWarning("[MultiplayerHandManager] Startup hand prepared: cards=5");
            completion.TrySetResult(true);
        }
    }

    public void CancelPreparation()
    {
        preparationGeneration++;
        preparation?.TrySetCanceled();
        foreach (var card in preparedCards)
        {
            if (card == null) continue;
            fightSystem?.player?.hand.Remove(card);
            Destroy(card.gameObject);
        }
        preparedCards.Clear();
        cardDefinitions.Clear();
    }

    private void OnDestroy()
    {
        preparationGeneration++;
        preparation?.TrySetCanceled();
    }

    void CreateMultiplayerHandFromRoom(JArray cardsArray)
    {
        Debug.Log($"[CreateMultiplayerHandFromRoom] Creating cards, array count={cardsArray?.Count ?? 0}");

        Player player = fightSystem.player;
        GameObject playerGO = fightSystem.hrac;
        if (player == null || playerGO == null)
        {
            throw new System.InvalidOperationException("Startup player/hand parent is missing");
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
            var created = CreateCardInGame(card, playerGO, player);
            if (created == null) throw new System.InvalidOperationException("Startup card creation failed");
            preparedCards.Add(created);
        }
        
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
