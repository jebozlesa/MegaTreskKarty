using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public GameObject deckPanel;
    public GameObject createDeckPrompt;
    public GameObject zoomedCardHolder;
    public LibraryDeckController libraryDeckController;
    public string connectionString;

    private LibraryContextDto currentContext;
    private LibraryDeckDto currentDeck;

    void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
    }

    public void RenderDeck(
        LibraryDeckDto deck,
        List<GeneratedCard> allCards,
        LibraryContextDto context
    )
    {
        currentContext = context;
        currentDeck = deck;

        if (deckPanel == null)
        {
            Debug.LogError("[DeckManager] deckPanel is not assigned");
            return;
        }

        foreach (Transform child in deckPanel.transform)
        {
            Destroy(child.gameObject);
        }

        if (createDeckPrompt != null)
        {
            createDeckPrompt.SetActive(deck == null);
        }

        if (deck?.cardIds == null || allCards == null)
        {
            Debug.LogWarning($"[DeckManager] No active deck to render: context={context?.contextId}");
            return;
        }

        Dictionary<string, GeneratedCard> cardsById = new Dictionary<string, GeneratedCard>();
        foreach (GeneratedCard card in allCards)
        {
            if (!string.IsNullOrWhiteSpace(card.CardID))
            {
                cardsById[card.CardID] = card;
            }
        }

        foreach (string cardId in deck.cardIds)
        {
            if (cardsById.TryGetValue(cardId, out GeneratedCard card))
            {
                CreateCardInDeck(card);
            }
            else
            {
                Debug.LogWarning(
                    $"[DeckManager] Deck references missing card: context={context?.contextId}, deck={deck.deckId}, cardId={cardId}"
                );
            }
        }

        Debug.LogWarning(
            $"[DeckManager] Deck rendered: context={context?.contextId}, deck={deck.deckId}, cards={deck.cardIds.Count}"
        );
    }

    public bool IsCardNameInDeck(string cardNameToCheck)
    {
        foreach (Transform cardTransform in deckPanel.transform)
        {
            Card card = cardTransform.GetComponent<Card>();
            if (card != null && card.cardName == cardNameToCheck)
            {
                return true;
            }
        }
        return false;
    }

    public bool IsCardUsedInAnyKnownDeck(string cardId)
    {
        return libraryDeckController != null && libraryDeckController.IsCardUsedInAnyKnownDeck(cardId);
    }

    public async Task<bool> SwapWithSelectedCardAsync(Card clickedDeckCard, Card selectedCard)
    {
        if (libraryDeckController == null)
        {
            Debug.LogError("[DeckManager] libraryDeckController is not assigned");
            return false;
        }

        if (clickedDeckCard == null || selectedCard == null)
        {
            Debug.LogWarning("[DeckManager] Cannot swap: clicked or selected card is null");
            return false;
        }

        if (clickedDeckCard.cardId == selectedCard.cardId)
        {
            Debug.LogWarning("[DeckManager] Card with the same id is already in the deck.");
            return false;
        }

        if (IsCardNameInDeck(selectedCard.cardName) && clickedDeckCard.cardName != selectedCard.cardName)
        {
            Debug.LogWarning("[DeckManager] Card with the same name already exists in the deck.");
            return false;
        }

        if (currentContext == null || currentDeck == null)
        {
            Debug.LogWarning("[DeckManager] Cannot swap before a library deck is rendered");
            return false;
        }

        if (!selectedCard.deckCard)
        {
            selectedCard.ZoomOut();
        }

        Debug.LogWarning(
            $"[DeckManager] Swap requested: context={currentContext.contextId}, deck={currentDeck.deckId}, old={clickedDeckCard.cardId}, new={selectedCard.cardId}"
        );

        return await libraryDeckController.SwapActiveDeckCardAsync(clickedDeckCard.cardId, selectedCard.cardId);
    }

    private void CreateCardInDeck(GeneratedCard card)
    {
        GameObject novaKarta = Instantiate(cardPrefab, deckPanel.transform);
        Card deckCard = novaKarta.GetComponent<Card>();

        deckCard.cardId = card.CardID;
        deckCard.styleId = card.StyleID;
        deckCard.cardName = card.PersonName;
        deckCard.health = card.Health;
        deckCard.strength = card.Strength;
        deckCard.speed = card.Speed;
        deckCard.attack = card.Attack;
        deckCard.defense = card.Defense;
        deckCard.knowledge = card.Knowledge;
        deckCard.charisma = card.Charisma;
        deckCard.image = card.CardPicture;
        deckCard.color = BuildColor(card.Color);
        deckCard.level = card.Level;
        deckCard.experience = card.Experience;
        deckCard.attack1 = card.Attack1;
        deckCard.attack2 = card.Attack2;
        deckCard.attack3 = card.Attack3;
        deckCard.attack4 = card.Attack4;
        deckCard.story = LoadStory(card.StyleID);
        deckCard.zoomedCardHolder = zoomedCardHolder;
        deckCard.transform.localScale = Vector3.one;
        deckCard.deckPanel = deckPanel;
        deckCard.deckCard = true;
        deckCard.deckManager = this;
    }

    private static Color32 BuildColor(int[] color)
    {
        if (color == null || color.Length < 3)
        {
            return new Color32(255, 255, 255, 255);
        }

        return new Color32((byte)color[0], (byte)color[1], (byte)color[2], 255);
    }

    string LoadStory(int cardID)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        }

        string cardStory = "";
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM CardStories WHERE StyleID = " + cardID;
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            cardStory = reader.GetString(2);
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();

        return cardStory;
    }
}
