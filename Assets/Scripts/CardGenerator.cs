using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;

public class CardGenerator : MonoBehaviour
{
    private string connectionString;

    private void Awake()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
    }

    public List<int>[] themedPacks = new List<int>[]
    {
        new List<int> { 5, 8, 1, 11, 12, 15, 16, 20 }, // Balíček 1
        new List<int> { 2, 3, 4, 9, 10, 13, 14, 17, 18, 19 }, // Balíček 2
    };

    public void GenerateCardPack(int packIndex)
    {
        if (packIndex < 0 || packIndex >= themedPacks.Length)
        {
            Debug.LogError("Invalid pack index");
            return;
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
            AddCardById(randomCardID);

            pack.RemoveAt(randomIndex);
        }
    }

    public void AddRandomCard()
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT COUNT(*) FROM CardDatabase";
        int cardCount = int.Parse(dbCommand.ExecuteScalar().ToString());

        int randomIndex = UnityEngine.Random.Range(1, cardCount + 1);
        dbCommand.Dispose();

        AddCardById(randomIndex);

        dbConnection.Close();
    }

    public void AddCardById(int id)
    {
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
