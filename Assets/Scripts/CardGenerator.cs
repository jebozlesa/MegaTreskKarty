using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System.Linq;
using UnityEngine.SceneManagement;


public class CardGenerator : MonoBehaviour
{
    private string connectionString;

    public Canvas canvas;
    public GameObject cardPrefab;
    public GameObject displayBlock;

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
        Debug.Log("ShowCardOnScreen(" + id + "," + cardName + "," + level + ")");
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

        displayBlock.SetActive(true);

        List<int> pack = new List<int>(themedPacks[packIndex]);
        List<GeneratedCard> generatedCards = new List<GeneratedCard>(); // Zoznam pre uchovanie vygenerovaných objektov kariet

        for (int i = 0; i < 6; i++)
        {
            if (pack.Count == 0)
            {
                Debug.LogWarning("No more unique cards left in the pack");
                break;
            }

            int series = 1;
            if (i == 5) // Ak ešte nie je koniec, počkajte pol sekundy medzi kartami
            {
                series = 2;
            }

            int randomIndex = UnityEngine.Random.Range(0, pack.Count);
            int randomCardID = pack[randomIndex];
            GeneratedCard randomCard = null;

            // Vygenerujeme kartu a získame objekt karty
            yield return StartCoroutine(GenerateCard(randomCardID, card => randomCard = card, series));

            if (randomCard != null)
            {
                generatedCards.Add(randomCard); // Pridajte kartu do zoznamu vygenerovaných kariet
            }
            pack.RemoveAt(randomIndex);

            if (i < 5) // Ak ešte nie je koniec, počkajte pol sekundy medzi kartami
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        displayBlock.SetActive(false);

        Debug.Log("HasCompletedTutorialMarketplace pico: " + PlayerPrefs.GetInt("HasCompletedTutorialMarketplace", 0));


        Debug.Log("Generated cards: " + string.Join(", ", generatedCards.Select(c => c.CardID)));
        yield return StartCoroutine(CreateFirstDeck(generatedCards)); // Vytvorte prvý balíček


        // PlayerPrefs.SetInt("HasCompletedTutorialMarketplace", 1);
        // PlayerPrefs.Save();
    }

    private IEnumerator GenerateCard(int cardId, Action<GeneratedCard> onCardCreated, int series)
    {
        Debug.Log("MegaTresk: " + DateTime.Now.ToString("HH:mm:ss.fff") + "CardGenerator.GenerateCard => cardID: " + cardId);
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT * FROM CardDatabase WHERE StyleID = {cardId}";
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            GeneratedCard card = CreateCardFromDatabase(reader, series);
            string json = ConvertCardToJson(card);

            PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
                {
                    string existingDataJson = GetExistingDataJson(result);
                    Dictionary<string, GeneratedCard> data = AddCardToExistingData(existingDataJson, card);
                    string updatedJson = ConvertUpdatedDataToJson(data);

                    UpdateUserDataInPlayFab(updatedJson);
                }, error => Debug.LogError(error.GenerateErrorReport()));

            onCardCreated?.Invoke(card);

            AudioManager.Instance.PlayCardAcquiredSound();

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

        //randomIndex = 50;  // docasne - vymazat resp. zakomentovat ked netreeba                                         <============  RANDOM INDEX
        yield return StartCoroutine(AddCardById(randomIndex));

        dbConnection.Close();
    }

    public IEnumerator AddCardById(int id)
    {
        Debug.Log("AddCardById(" + id + ")");
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

            AudioManager.Instance.PlayCardAcquiredSound();

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

    private GeneratedCard CreateCardFromDatabase(IDataReader reader, int series = 1)
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
            Charisma = reader.GetInt32(8)
            // Color = Array.ConvertAll(reader.GetString(10).Split(';'), int.Parse),
            // CardPicture = reader.GetString(15)

            // Attack1 = 1,
            // Attack2 = 2,
            // Attack3 = 3,
            // Attack4 = 4
        };

        List<string> characterSeries = GetVisualForCharacter(card.StyleID, series);
        List<int> availableAttacks = GetAttacksForCharacter(card.StyleID);
        List<int> selectedAttacks = SelectRandomAttacks(availableAttacks, 4);

        card.Attack1 = selectedAttacks[0];
        card.Attack2 = selectedAttacks[1];
        card.Attack3 = selectedAttacks[2];
        card.Attack4 = selectedAttacks[3];

        card.Color = Array.ConvertAll(characterSeries[0].Split(';'), int.Parse);
        card.CardPicture = characterSeries[1];

        return card;
    }

    private List<string> GetVisualForCharacter(int styleID, int series)
    {
        List<string> visual = new List<string>();
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        try
        {
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"SELECT Color, Image FROM CardVisuals WHERE CharacterID = {styleID} AND Series = {series}";
            // Debug.Log($"SQL Query: {dbCommand.CommandText}");
            IDataReader reader = dbCommand.ExecuteReader();

            while (reader.Read())
            {
                // Add Color to the list
                visual.Add(reader.GetString(0));
                // Add Image to the list
                visual.Add(reader.GetString(1));
            }
            reader.Close();
            dbCommand.Dispose();
        }
        finally
        {
            dbConnection.Close();
        }

        return visual;
    }

    private List<int> GetAttacksForCharacter(int styleID)
    {
        List<int> attacks = new List<int>();
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        try
        {
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText = $"SELECT AttackID FROM CharacterAttacks WHERE CharacterID = {styleID}";
            Debug.Log($"SELECT AttackID FROM CharacterAttacks WHERE CharacterID = {styleID}");
            IDataReader reader = dbCommand.ExecuteReader();

            while (reader.Read())
            {
                attacks.Add(reader.GetInt32(0));
            }

            reader.Close();
            dbCommand.Dispose();
        }
        finally
        {
            dbConnection.Close();
        }

        return attacks;
    }


    private List<int> SelectRandomAttacks(List<int> attacks, int count)
    {
        List<int> selectedAttacks = new List<int>();
        System.Random random = new System.Random();

        while (selectedAttacks.Count < count)
        {
            int randomIndex = random.Next(attacks.Count);
            int selectedAttack = attacks[randomIndex];

            if (!selectedAttacks.Contains(selectedAttack))
            {
                selectedAttacks.Add(selectedAttack);
            }
        }

        return selectedAttacks;
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
            if (existingCards != null && existingCards.cards != null)
            {
                foreach (GeneratedCard existingCard in existingCards.cards)
                {
                    data.Add(existingCard.CardID, existingCard);
                }
            }
        }
        data[card.CardID] = card; // Aktualizujte alebo pridajte novú kartu

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
        Debug.Log("Updating JSON card ");
        var updateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerCards", updatedJson } }
        };
        Debug.Log("Updated JSON card: " + updatedJson);
        PlayFabClientAPI.UpdateUserData(updateRequest, updateResult => Debug.Log("User data updated successfully"), error => Debug.LogError(error.GenerateErrorReport()));
    }

    public IEnumerator CreateFirstDeck(List<GeneratedCard> generatedCards)
    {
        Debug.Log("Skontrolujeme, či je potrebné vytvoriť prvý balíček...");

        var checkDeckRequest = new GetUserDataRequest { Keys = new List<string> { "PlayerDecks" } };
        bool isRequestComplete = false;
        bool deckExists = false;

        PlayFabClientAPI.GetUserData(checkDeckRequest, result =>
        {
            if (result.Data != null && result.Data.ContainsKey("PlayerDecks") && !string.IsNullOrEmpty(result.Data["PlayerDecks"].Value))
            {
                Debug.Log("Balíček už existuje. Vytvorenie balíčka preskočíme.");
                deckExists = true;
            }
            isRequestComplete = true;
        }, error =>
        {
            Debug.LogError(error.GenerateErrorReport());
            isRequestComplete = true;
        });

        yield return new WaitUntil(() => isRequestComplete);

        if (!deckExists)
        {
            yield return StartCoroutine(CreateAndSaveDeck("First Deck", generatedCards));
            SceneManager.LoadScene("Cards");
        }
    }

    public IEnumerator CreateAndSaveDeck(string deckName, List<GeneratedCard> cards)
    {
        Deck newDeck = new Deck
        {
            DeckID = Guid.NewGuid().ToString(),
            DeckName = deckName,
            Card1 = cards[0].CardID,
            Card2 = cards[1].CardID,
            Card3 = cards[2].CardID,
            Card4 = cards[3].CardID,
            Card5 = cards[4].CardID
        };

        bool isRequestComplete = false;
        bool isDeckSaved = false;

        // Získanie existujúcich údajov o balíčkoch
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            string existingDataJson = result.Data != null && result.Data.ContainsKey("PlayerDecks") ? result.Data["PlayerDecks"].Value : "{}";
            string json = ConvertDeckToJson(newDeck, existingDataJson);

            var updateUserDataRequest = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { "PlayerDecks", json } }
            };

            // Aktualizácia údajov o balíčkoch
            PlayFabClientAPI.UpdateUserData(updateUserDataRequest, updateResult =>
            {
                Debug.Log("Nový balíček bol úspešne uložený.");
                isDeckSaved = true;
            }, error =>
            {
                Debug.LogError("Chyba pri ukladaní nového balíčka: " + error.GenerateErrorReport());
            });

            isRequestComplete = true;
        }, error =>
        {
            Debug.LogError("Chyba pri získavaní existujúcich údajov o balíčkoch: " + error.GenerateErrorReport());
            isRequestComplete = true;
        });

        // Čakanie na dokončenie oboch požiadaviek
        yield return new WaitUntil(() => isRequestComplete && isDeckSaved);
    }


    private string ConvertDeckToJson(Deck newDeck, string existingDataJson)
    {
        DeckListWrapper existingDecks = JsonUtility.FromJson<DeckListWrapper>(existingDataJson);
        if (existingDecks == null)
        {
            existingDecks = new DeckListWrapper();
            existingDecks.Decks = new List<Deck>();
        }

        existingDecks.Decks.Add(newDeck);

        return JsonUtility.ToJson(existingDecks);
    }


    // private string ConvertDeckToJson(Deck deck)
    // {
    //     return JsonUtility.ToJson(deck);
    // }

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
