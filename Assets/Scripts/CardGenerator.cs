using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Mono.Data.Sqlite;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
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

        var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
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

    public IEnumerator ShowCardOnScreen(
        int id,
        string cardName,
        string image,
        Color32 color,
        int level
    )
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
        new List<int>
        {
            5,
            8,
            11,
            12,
            15,
            16,
            20,
            23,
            25,
            27,
            28,
            29,
            30,
            33,
            38,
            39,
            41,
            42,
            44,
            45,
        }, // Balíček 0
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

            // ✅ V11: Všetky karty z balíčka môžu byť hocaká séria (nie Series 1 - tá je len pre AI)
            // Náhodne vyberieme sériu 2 alebo vyššiu (podľa dostupnosti v CardVisuals tabuľke)
            int series = GetRandomAvailableSeries();

            int randomIndex = UnityEngine.Random.Range(0, pack.Count);
            int randomCardID = pack[randomIndex];
            GeneratedCard randomCard = null;

            // Vygenerujeme kartu a získame objekt karty
            yield return StartCoroutine(
                GenerateCard(randomCardID, card => randomCard = card, series)
            );

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

        Debug.Log(
            "HasCompletedTutorialMarketplace pico: "
                + PlayerPrefs.GetInt("HasCompletedTutorialMarketplace", 0)
        );

        Debug.Log("Generated cards: " + string.Join(", ", generatedCards.Select(c => c.CardID)));
        SceneManager.LoadScene("Cards");

        // PlayerPrefs.SetInt("HasCompletedTutorialMarketplace", 1);
        // PlayerPrefs.Save();
    }

    private IEnumerator GenerateCard(int cardId, Action<GeneratedCard> onCardCreated, int series)
    {
        Debug.Log(
            "MegaTresk: "
                + DateTime.Now.ToString("HH:mm:ss.fff")
                + "CardGenerator.GenerateCard => cardID: "
                + cardId
        );
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT * FROM CardDatabase WHERE StyleID = {cardId}";
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            GeneratedCard card = CreateCardFromDatabase(reader, series);
            string json = ConvertCardToJson(card);

            PlayFabClientAPI.GetUserData(
                new GetUserDataRequest(),
                result =>
                {
                    string existingDataJson = GetExistingDataJson(result);
                    Dictionary<string, GeneratedCard> data = AddCardToExistingData(
                        existingDataJson,
                        card
                    );
                    string updatedJson = ConvertUpdatedDataToJson(data);

                    UpdateUserDataInPlayFab(updatedJson);
                },
                error => Debug.LogError(error.GenerateErrorReport())
            );

            onCardCreated?.Invoke(card);

            AudioManager.Instance.PlayCardAcquiredSound();

            yield return StartCoroutine(
                ShowCardOnScreen(
                    card.StyleID,
                    card.PersonName,
                    card.CardPicture,
                    new Color32((byte)card.Color[0], (byte)card.Color[1], (byte)card.Color[2], 255),
                    card.Level
                )
            );
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
        // ✅ V11: Royal Battle rewards môžu byť hocaká séria (nie Series 1 - tá je len pre AI)
        dbCommand.CommandText = "SELECT COUNT(*) FROM CardDatabase";
        int cardCount = int.Parse(dbCommand.ExecuteScalar().ToString());

        int randomIndex = UnityEngine.Random.Range(1, cardCount + 1);
        dbCommand.Dispose();

        int series = GetRandomAvailableSeries();
        yield return StartCoroutine(AddCardById(randomIndex, series));

        dbConnection.Close();
    }

    public IEnumerator AddCardById(int id, int series = 1)
    {
        Debug.Log("AddCardById(" + id + ")");
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT * FROM CardDatabase WHERE StyleID = {id}";
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            GeneratedCard card = CreateCardFromDatabase(reader, series);
            string json = ConvertCardToJson(card);

            PlayFabClientAPI.GetUserData(
                new GetUserDataRequest(),
                result =>
                {
                    string existingDataJson = GetExistingDataJson(result);
                    Dictionary<string, GeneratedCard> data = AddCardToExistingData(
                        existingDataJson,
                        card
                    );
                    string updatedJson = ConvertUpdatedDataToJson(data);
                    UpdateUserDataInPlayFab(updatedJson);
                },
                error => Debug.LogError(error.GenerateErrorReport())
            );

            AudioManager.Instance.PlayCardAcquiredSound();

            yield return StartCoroutine(
                ShowCardOnScreen(
                    card.StyleID,
                    card.PersonName,
                    card.CardPicture,
                    new Color32((byte)card.Color[0], (byte)card.Color[1], (byte)card.Color[2], 255),
                    card.Level
                )
            );
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
        Color32 color = new Color32(
            byte.Parse(colorComponents[0]),
            byte.Parse(colorComponents[1]),
            byte.Parse(colorComponents[2]),
            255
        );

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
            dbCommand.CommandText =
                $"SELECT Color, Image FROM CardVisuals WHERE CharacterID = {styleID} AND Series = {series}";
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

    /// <summary>
    /// ✅ V11: Vráti náhodné číslo série (okrem Series 1, ktorá je rezervovaná pre AI)
    /// Ak máš v databáze Series 2, 3, 4... táto funkcia náhodne vyberie jednu z nich.
    /// Ak máš len Series 2, vždy vráti 2.
    /// </summary>
    private int GetRandomAvailableSeries()
    {
        // Pre jednoduchosť momentálne vrátime Series 2
        // Ak v budúcnosti pridáš Series 3, 4, 5... môžeš zmeniť na náhodný výber
        return 2;

        // BUDÚCE ROZŠÍRENIE (ak budeš mať viac sérií):
        // int[] availableSeries = new int[] { 2, 3, 4 }; // Definuj aké série máš
        // return availableSeries[UnityEngine.Random.Range(0, availableSeries.Length)];
    }

    private List<int> GetAttacksForCharacter(int styleID)
    {
        List<int> attacks = new List<int>();
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        try
        {
            IDbCommand dbCommand = dbConnection.CreateCommand();
            dbCommand.CommandText =
                $"SELECT AttackID FROM CharacterAttacks WHERE CharacterID = {styleID}";
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

    private Dictionary<string, GeneratedCard> AddCardToExistingData(
        string existingDataJson,
        GeneratedCard card
    )
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
            cards = new List<GeneratedCard>(data.Values),
        };

        return JsonUtility.ToJson(updatedCards);
    }

    private void UpdateUserDataInPlayFab(string updatedJson)
    {
        Debug.Log("Updating JSON card ");
        var updateRequest = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerCards", updatedJson } },
        };
        Debug.Log("Updated JSON card: " + updatedJson);
        PlayFabClientAPI.UpdateUserData(
            updateRequest,
            updateResult => Debug.Log("User data updated successfully"),
            error => Debug.LogError(error.GenerateErrorReport())
        );
    }

}
