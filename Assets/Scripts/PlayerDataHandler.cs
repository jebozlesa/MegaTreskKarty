using UnityEngine;
using System.Data;
using System;
using Mono.Data.Sqlite;

public class PlayerDataHandler : MonoBehaviour
{
    private string connectionString;

    private void Awake()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
    }

    public void AddNewPlayer(string playerName)
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand insertCommand = dbConnection.CreateCommand();
        insertCommand.CommandText = "INSERT INTO PlayerData (PlayerName, Level, Cash, Experience, RRrecord) VALUES (@PlayerName, @Level, @Cash, @Experience, @RRrecord)";

        insertCommand.Parameters.Add(new SqliteParameter("@PlayerName", playerName));
        insertCommand.Parameters.Add(new SqliteParameter("@Level", 1));
        insertCommand.Parameters.Add(new SqliteParameter("@Cash", 0));
        insertCommand.Parameters.Add(new SqliteParameter("@Experience", 0));
        insertCommand.Parameters.Add(new SqliteParameter("@RRrecord", 0));

        insertCommand.ExecuteNonQuery();

        insertCommand.Dispose();
        dbConnection.Close();
    }

    public int GetPlayerIntData(string columnName)
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT {columnName} FROM PlayerData LIMIT 1";
        int result = Convert.ToInt32(dbCommand.ExecuteScalar());

        dbCommand.Dispose();
        dbConnection.Close();

        return result;
    }

    public string GetPlayerStringData(string columnName)
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT {columnName} FROM PlayerData LIMIT 1";
        string result = dbCommand.ExecuteScalar().ToString();

        dbCommand.Dispose();
        dbConnection.Close();

        return result;
    }

    public void UpdatePlayerData(string columnName, object value)
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand updateCommand = dbConnection.CreateCommand();
        updateCommand.CommandText = $"UPDATE PlayerData SET {columnName} = @Value";
        updateCommand.Parameters.Add(new SqliteParameter("@Value", value));

        updateCommand.ExecuteNonQuery();

        updateCommand.Dispose();
        dbConnection.Close();
    }
}
