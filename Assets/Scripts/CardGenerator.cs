using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;


public class CardGenerator : MonoBehaviour
{
    private string connectionString;

    public Canvas canvas;
    public GameObject cardPrefab;

    private void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
 //       PlayFabLogin();
    }

    void PlayFabLogin()
    {
        //loadingImage.SetActive(true);
        string username = PlayerPrefs.GetString("username");
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");

        var request = new LoginWithEmailAddressRequest 
            {
                Email = email,
                Password = password
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("Sicko dobre");
    //    loadingImage.SetActive(false);
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("Daco nahovno");
   //     errorImage.SetActive(true);
        Debug.Log(error.GenerateErrorReport());
    }

    public void AddAllCardsFromDatabase()
    {
        StartCoroutine(GetAllCardIdsFromDatabaseCoroutine());
    }

    private IEnumerator GetAllCardIdsFromDatabaseCoroutine()
    {
        List<int> cardIds = new List<int>();

        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT StyleID FROM CardDatabase WHERE Series = 1";
        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            cardIds.Add(reader.GetInt32(0));
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();

        yield return StartCoroutine(AddAllCardsFromDatabaseCoroutine(cardIds));
    }

    private IEnumerator AddAllCardsFromDatabaseCoroutine(List<int> cardIds)
    {
        foreach (int cardId in cardIds)
        {
            yield return StartCoroutine(AddCardById(cardId));
           // yield return new WaitForSeconds(1.7f);
        }
    }

    private IEnumerator ShowCardOnScreen(int id, string cardName, string image, Color32 color, int level)
    {
        Debug.Log("ShowCardOnScreen("+id+","+cardName+","+level+")");
        // Vytvorte inštanciu karty
        GameObject cardInstance = Instantiate(cardPrefab);
        cardInstance.transform.SetParent(canvas.transform, false);

        // Nastavte hodnoty karty
        ShowCard showCard = cardInstance.GetComponent<ShowCard>();
        showCard.cardId = id;
        showCard.cardName = cardName;
        showCard.image = image;
        showCard.color = color;
        showCard.level = level;

        // Zväčšte kartu
        cardInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f); // Nastavte menšiu veľkosť karty
        RectTransform cardRectTransform = cardInstance.GetComponent<RectTransform>();
        cardRectTransform.anchoredPosition = Vector2.zero; // Vycentrujte kartu

        // Zobrazte kartu
        cardInstance.SetActive(true);

        // Počkajte na sekundu
        yield return new WaitForSeconds(1.5f);

        // Skryte kartu
        cardInstance.SetActive(false);

        // Zničte inštanciu karty
        Destroy(cardInstance);
    }

    public List<int>[] themedPacks = new List<int>[]
    {
        new List<int> { 5, 8, 11, 12, 15, 16, 20, 23, 25, 27, 28, 29, 30, 33, 38, 39, 41, 42,44, 45 }, // Balíček 0
        new List<int> { 2, 3, 4, 7, 13, 14, 18, 19, 21, 22, 24, 26, 32, 34, 37, 43 }, // Balíček 1
        new List<int> { 4, 10, 25, 30, 35, 36, 41, 44, 45 }, // Balíček 1
    };

    public void StartGenerateCardPack(int packIndex)
    {
        StartCoroutine(GenerateCardPack(packIndex));
    }

    public IEnumerator GenerateCardPack(int packIndex)
    {
        if (packIndex < 0 || packIndex >= themedPacks.Length)
        {
            Debug.LogError("Invalid pack index");
            yield break;
        }

        List<int> pack = new List<int>(themedPacks[packIndex]);
        List<int> generatedCards = new List<int>(); // Nový zoznam pre uchovanie vygenerovaných kariet

        for (int i = 0; i < 6; i++)
        {
            if (pack.Count == 0)
            {
                Debug.LogWarning("No more unique cards left in the pack");
                break;
            }

            int randomIndex = UnityEngine.Random.Range(0, pack.Count);
            int randomCardID = pack[randomIndex];
            yield return StartCoroutine(AddCardById(randomCardID)); // Zmenené na korutinu

            generatedCards.Add(randomCardID); // Pridajte kartu do zoznamu vygenerovaných kariet
            pack.RemoveAt(randomIndex);

            if (i < 4) // Ak ešte nie je koniec, počkajte pol sekundy medzi kartami
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Po vygenerovaní všetkých kariet vytvorte balíček, ak hráč ešte nedokončil tutoriál
        if (PlayerPrefs.GetInt("HasCompletedTutorial", 0) == 0)
        {
            Debug.Log("Generated cards: " + string.Join(", ", generatedCards));
            yield return StartCoroutine(CreateFirstDeck(generatedCards)); // Vytvorte prvý balíček
        }
    }

    public void AddRandomCard()
    {
        StartCoroutine(AddRandomCardCoroutine());
    }

    public IEnumerator AddRandomCardCoroutine()
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT COUNT(*) FROM CardDatabase WHERE Series = 1";
        int cardCount = int.Parse(dbCommand.ExecuteScalar().ToString());

        int randomIndex = UnityEngine.Random.Range(1, cardCount + 1);
        dbCommand.Dispose();

     //   randomIndex = 46;  // docasne - vymazat resp. zakomentovat ked netreeba                                         <============  RANDOM INDEX
        yield return StartCoroutine(AddCardById(randomIndex)); 

        dbConnection.Close();
    }

    public IEnumerator AddCardById(int id)
    {
        Debug.Log("AddCardById("+id+")");
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT * FROM CardDatabase WHERE StyleID = {id}";
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            GeneratedCard card = CreateCardFromDatabase(reader);
            string json = ConvertCardToJson(card);

            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
            {
                string existingDataJson = GetExistingDataJson(result);
                Dictionary<string, GeneratedCard> data = AddCardToExistingData(existingDataJson, card);
                string updatedJson = ConvertUpdatedDataToJson(data);

                UpdateUserDataInPlayFab(updatedJson);
            }, error => Debug.LogError(error.GenerateErrorReport()));

            yield return StartCoroutine(ShowCardOnScreen(card.StyleID, card.PersonName, card.CardPicture, new Color32((byte)card.Color[0], (byte)card.Color[1], (byte)card.Color[2], 255), card.Level));
        }
        else
        {
            Debug.LogError("Card with the specified ID not found");
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();
    }

    private GeneratedCard CreateCardFromDatabase(IDataReader reader)
    {
        string[] colorComponents = reader.GetString(10).Split(';');
        Color32 color = new Color32(byte.Parse(colorComponents[0]), byte.Parse(colorComponents[1]), byte.Parse(colorComponents[2]), 255);

        GeneratedCard card = new GeneratedCard
        {
            CardID = Guid.NewGuid().ToString(),
            StyleID = reader.GetInt32(0),
            PersonName = reader.GetString(1),
            Level = 1,
            Experience = 0,
            Health = reader.GetInt32(2),
            Strength = reader.GetInt32(3),
            Speed = reader.GetInt32(4),
            Attack = reader.GetInt32(5),
            Defense = reader.GetInt32(6),
            Knowledge = reader.GetInt32(7),
            Charisma = reader.GetInt32(8),
            Color = Array.ConvertAll(reader.GetString(10).Split(';'), int.Parse),
            Attack1 = reader.GetInt32(11),
            Attack2 = reader.GetInt32(12),
            Attack3 = reader.GetInt32(13),
            Attack4 = reader.GetInt32(14),
            CardPicture = reader.GetString(15)
        };

        return card;
    }

    private string ConvertCardToJson(GeneratedCard card)
    {
        return JsonUtility.ToJson(card);
    }

    private string GetExistingDataJson(GetUserDataResult result)
    {
        if (result.Data.ContainsKey("PlayerCards"))
        {
            return result.Data["PlayerCards"].Value;
        }

        return "{}";
    }

    private Dictionary<string, GeneratedCard> AddCardToExistingData(string existingDataJson, GeneratedCard card)
    {
        Dictionary<string, GeneratedCard> data = new Dictionary<string, GeneratedCard>();
        if (!string.IsNullOrEmpty(existingDataJson))
        {
            CardListWrapper existingCards = JsonUtility.FromJson<CardListWrapper>(existingDataJson);
            foreach (GeneratedCard existingCard in existingCards.cards)
            {
                data.Add(existingCard.CardID, existingCard);
            }
        }
        data.Add(card.CardID, card);

        return data;
    }

    private string ConvertUpdatedDataToJson(Dictionary<string, GeneratedCard> data)
    {
        CardListWrapper updatedCards = new CardListWrapper
        {
            cards = new List<GeneratedCard>(data.Values)
        };

        return JsonUtility.ToJson(updatedCards);
    }

    private void UpdateUserDataInPlayFab(string updatedJson)
    {
        var updateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerCards", updatedJson } }
        };
        Debug.Log("Updated JSON card: " + updatedJson);
        PlayFabClientAPI.UpdateUserData(updateRequest, updateResult => Debug.Log("User data updated successfully"), error => Debug.LogError(error.GenerateErrorReport()));
    }

    public IEnumerator CreateFirstDeck(List<int> generatedCards)
    {
        Debug.Log("CreateFirstDeck ====> START");

        // Vytvoríme nový balíček s prvými 5 kartami
        Deck newDeck = new Deck
        {
            DeckID = Guid.NewGuid().ToString(),
            DeckName = "First Deck",
            Card1 = generatedCards[0],
            Card2 = generatedCards[1],
            Card3 = generatedCards[2],
            Card4 = generatedCards[3],
            Card5 = generatedCards[4]
        };

        Debug.Log($"New deck created with ID: {newDeck.DeckID}");
        Debug.Log($"New deck CardIDs: {newDeck.Card1}, {newDeck.Card2}, {newDeck.Card3}, {newDeck.Card4}, {newDeck.Card5}");

        string json = ConvertDeckToJson(newDeck);

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            string existingDataJson = GetExistingDeckDataJson(result);
            Dictionary<string, Deck> data = AddDeckToExistingData(existingDataJson, newDeck);
            string updatedJson = ConvertUpdatedDeckDataToJson(data);

            UpdateDeckDataInPlayFab(updatedJson);
        }, error => Debug.LogError(error.GenerateErrorReport()));

        yield return null;
    }


    private string ConvertDeckToJson(Deck deck)
    {
        return JsonUtility.ToJson(deck);
    }

    private string GetExistingDeckDataJson(GetUserDataResult result)
    {
        if (result.Data.ContainsKey("PlayerDecks"))
        {
            return result.Data["PlayerDecks"].Value;
        }

        return "{}";
    }

    private Dictionary<string, Deck> AddDeckToExistingData(string existingDataJson, Deck deck)
    {
        Dictionary<string, Deck> data = new Dictionary<string, Deck>();
        if (!string.IsNullOrEmpty(existingDataJson))
        {
            DeckListWrapper existingDecks = JsonUtility.FromJson<DeckListWrapper>(existingDataJson);
            foreach (Deck existingDeck in existingDecks.Decks)
            {
                data.Add(existingDeck.DeckID, existingDeck);
            }
        }
        Debug.Log("Adding new deck with ID: " + deck.DeckID);
        data.Add(deck.DeckID, deck);
        Debug.Log("Total decks after adding new deck: " + data.Count);

        return data;
    }

    private string ConvertUpdatedDeckDataToJson(Dictionary<string, Deck> data)
    {
        DeckListWrapper updatedDecks = new DeckListWrapper
        {
            Decks = new List<Deck>(data.Values)
        };

        return JsonUtility.ToJson(updatedDecks);
    }

    private void UpdateDeckDataInPlayFab(string updatedJson)
    {
        var updateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerDecks", updatedJson } }
        };
        Debug.Log("Updated JSON deck: " + updatedJson);
        PlayFabClientAPI.UpdateUserData(updateRequest, updateResult => Debug.Log("User deck data updated successfully"), error => Debug.LogError(error.GenerateErrorReport()));
    }



}
