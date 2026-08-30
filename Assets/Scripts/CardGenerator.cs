using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class CardGenerator : MonoBehaviour
{
    private string connectionString;

    public Canvas canvas;
    public GameObject cardPrefab;
    public GameObject displayBlock;

    private void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        // PlayFabLogin();
    }

    private void PlayFabLogin()
    {
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");

        var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);
    }

    private void OnSuccess(LoginResult result)
    {
        Debug.Log("Sicko dobre");
    }

    private void OnError(PlayFabError error)
    {
        Debug.Log("Daco nahovno");
        Debug.Log(error.GenerateErrorReport());
    }

    public void AddAllCardsFromDatabase()
    {
        StartCoroutine(GetAllCardIdsFromDatabaseCoroutine());
    }

    private IEnumerator GetAllCardIdsFromDatabaseCoroutine()
    {
        List<int> cardIds = new List<int>();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = "SELECT StyleID FROM CardDatabase WHERE Series = 1";

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cardIds.Add(reader.GetInt32(0));
                    }
                }
            }
        }

        yield return StartCoroutine(AddAllCardsFromDatabaseCoroutine(cardIds));
    }

    private IEnumerator AddAllCardsFromDatabaseCoroutine(List<int> cardIds)
    {
        foreach (int cardId in cardIds)
        {
            yield return StartCoroutine(AddCardById(cardId));
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

        GameObject cardInstance = Instantiate(cardPrefab);
        cardInstance.transform.SetParent(canvas.transform, false);

        ShowCard showCard = cardInstance.GetComponent<ShowCard>();
        showCard.cardId = id;
        showCard.cardName = cardName;
        showCard.image = image;
        showCard.color = color;
        showCard.level = level;

        cardInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        RectTransform cardRectTransform = cardInstance.GetComponent<RectTransform>();
        cardRectTransform.anchoredPosition = Vector2.zero;

        cardInstance.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        cardInstance.SetActive(false);
        Destroy(cardInstance);
    }

    public void AddRandomCard()
    {
        StartCoroutine(AddRandomCardCoroutine());
    }

    public IEnumerator AddRandomCardCoroutine()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = "SELECT COUNT(*) FROM CardDatabase";
                int cardCount = int.Parse(dbCommand.ExecuteScalar().ToString());

                int randomIndex = UnityEngine.Random.Range(1, cardCount + 1);
                int series = GetRandomAvailableSeries();
                yield return StartCoroutine(AddCardById(randomIndex, series));
            }
        }
    }

    public IEnumerator AddCardById(int id, int series = 1)
    {
        Debug.Log("AddCardById(" + id + ")");

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = $"SELECT * FROM CardDatabase WHERE StyleID = {id}";

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        GeneratedCard card = CreateCardFromDatabase(reader, series);

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
                                new Color32(
                                    (byte)card.Color[0],
                                    (byte)card.Color[1],
                                    (byte)card.Color[2],
                                    255
                                ),
                                card.Level
                            )
                        );
                    }
                    else
                    {
                        Debug.LogError("Card with the specified ID not found");
                    }
                }
            }
        }
    }

    private GeneratedCard CreateCardFromDatabase(IDataReader reader, int series = 1)
    {
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

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText =
                    $"SELECT Color, Image FROM CardVisuals WHERE CharacterID = {styleID} AND Series = {series}";

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        visual.Add(reader.GetString(0));
                        visual.Add(reader.GetString(1));
                    }
                }
            }
        }

        return visual;
    }

    private int GetRandomAvailableSeries()
    {
        return 2;
    }

    private List<int> GetAttacksForCharacter(int styleID)
    {
        List<int> attacks = new List<int>();

        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText =
                    $"SELECT AttackID FROM CharacterAttacks WHERE CharacterID = {styleID}";
                Debug.Log($"SELECT AttackID FROM CharacterAttacks WHERE CharacterID = {styleID}");

                using (IDataReader reader = dbCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        attacks.Add(reader.GetInt32(0));
                    }
                }
            }
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
            if (existingCards?.cards != null)
            {
                foreach (GeneratedCard existingCard in existingCards.cards)
                {
                    data.Add(existingCard.CardID, existingCard);
                }
            }
        }

        data[card.CardID] = card;
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
