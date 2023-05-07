using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;

public class CardGenerator : MonoBehaviour
{
    private string connectionString;

    public Canvas canvas;
    public GameObject cardPrefab;

    private void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
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
        dbCommand.CommandText = "SELECT StyleID FROM CardDatabase";
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
        new List<int> { 5, 8, 1, 11, 12, 15, 16, 20 }, // Balíček 1
        new List<int> { 2, 3, 4, 9, 10, 13, 14, 17, 18, 19 }, // Balíček 2
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

        for (int i = 0; i < 5; i++)
        {
            if (pack.Count == 0)
            {
                Debug.LogWarning("No more unique cards left in the pack");
                break;
            }

            int randomIndex = UnityEngine.Random.Range(0, pack.Count);
            int randomCardID = pack[randomIndex];
            yield return StartCoroutine(AddCardById(randomCardID)); // Zmenené na korutinu

            pack.RemoveAt(randomIndex);

            if (i < 4) // Ak ešte nie je koniec, počkajte pol sekundy medzi kartami
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }


    public IEnumerator AddRandomCard()
{
    IDbConnection dbConnection = new SqliteConnection(connectionString);
    dbConnection.Open();

    IDbCommand dbCommand = dbConnection.CreateCommand();
    dbCommand.CommandText = "SELECT COUNT(*) FROM CardDatabase";
    int cardCount = int.Parse(dbCommand.ExecuteScalar().ToString());

    int randomIndex = UnityEngine.Random.Range(1, cardCount + 1);
    dbCommand.Dispose();

    randomIndex = 29;  // docasne - vymazat resp. zakomentovat ked netreeba
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
            IDbCommand insertCommand = dbConnection.CreateCommand();
            insertCommand.CommandText = "INSERT INTO PlayerCards (StyleID, PersonName, Level, Experience, Health, Strength, Speed, Attack, Defense, Knowledge, Charisma, Color, Attack1, Attack2, Attack3, Attack4, CardPicture) VALUES (@StyleID, @PersonName, @Level, @Experience, @Health, @Strength, @Speed, @Attack, @Defense, @Knowledge, @Charisma, @Color, @Attack1, @Attack2, @Attack3, @Attack4, @CardPicture)";

            insertCommand.Parameters.Add(new SqliteParameter("@StyleID", reader.GetInt32(0)));
            insertCommand.Parameters.Add(new SqliteParameter("@PersonName", reader.GetString(1)));
            insertCommand.Parameters.Add(new SqliteParameter("@Level", 1));
            insertCommand.Parameters.Add(new SqliteParameter("@Experience", Convert.ToInt32(0)));
            insertCommand.Parameters.Add(new SqliteParameter("@Health", reader.GetInt32(2)));
            insertCommand.Parameters.Add(new SqliteParameter("@Strength", reader.GetInt32(3)));
            insertCommand.Parameters.Add(new SqliteParameter("@Speed", reader.GetInt32(4)));
            insertCommand.Parameters.Add(new SqliteParameter("@Attack", reader.GetInt32(5)));
            insertCommand.Parameters.Add(new SqliteParameter("@Defense", reader.GetInt32(6)));
            insertCommand.Parameters.Add(new SqliteParameter("@Knowledge", reader.GetInt32(7)));
            insertCommand.Parameters.Add(new SqliteParameter("@Charisma", reader.GetInt32(8)));
            insertCommand.Parameters.Add(new SqliteParameter("@Color", reader.GetString(10)));
            insertCommand.Parameters.Add(new SqliteParameter("@Attack1", reader.GetInt32(11)));
            insertCommand.Parameters.Add(new SqliteParameter("@Attack2", reader.GetInt32(12)));
            insertCommand.Parameters.Add(new SqliteParameter("@Attack3", reader.GetInt32(13)));
            insertCommand.Parameters.Add(new SqliteParameter("@Attack4", reader.GetInt32(14)));
            insertCommand.Parameters.Add(new SqliteParameter("@CardPicture", reader.GetString(15)));

            insertCommand.ExecuteNonQuery();

            insertCommand.Dispose();

            string[] colorComponents = reader.GetString(10).Split(';');
            Color32 color = new Color32(byte.Parse(colorComponents[0]), byte.Parse(colorComponents[1]), byte.Parse(colorComponents[2]), 255);

            // Pridajte volanie ShowCardOnScreen po vložení karty do databázy
            yield return StartCoroutine(ShowCardOnScreen(id, reader.GetString(1), reader.GetString(15), color, 1));
        }
        else
        {
            Debug.LogError("Card with the specified ID not found");
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();
    }
}
