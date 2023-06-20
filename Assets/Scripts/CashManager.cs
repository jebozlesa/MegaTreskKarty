using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;

public class CashManager : MonoBehaviour
{
    public Text cashText; // UI Text element to display cash
    private int cash; // Cash value
    private string connectionString;

    void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        cash = GetCashFromDB();
        UpdateCashDisplay();
    }

    public int GetCashFromDB()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT Cash FROM PlayerData WHERE PlayerId = 1"; 
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return reader.GetInt32(0);
                    }
                    dbConnection.Close();
                    reader.Close();
                }
            }
        }
        return 0;
    }

    public void UpdateCashDisplay()
    {
        cashText.text = "Cash: " + cash.ToString();
    }

    public void ChangeCash(int amount)
    {
        cash += amount;
        UpdateCashInDB();
        UpdateCashDisplay();
    }

    public void UpdateCashInDB()
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = $"UPDATE PlayerData SET Cash = {cash} WHERE PlayerId = 1"; 
                dbCmd.CommandText = sqlQuery;
                dbCmd.ExecuteScalar();
                dbConnection.Close();
            }
        }
    }

    public bool HasEnoughCash(int amount)
    {
        return cash >= amount;
    }
}
