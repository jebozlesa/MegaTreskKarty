using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;

public class CampaignManager : MonoBehaviour
{
    public static CampaignManager Instance { get; private set; }
    private string connectionString;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
    }

    public void IncreaseMission(string campaignName)
    {
        var campaignData = LoadCampaignData(campaignName);
        int newLevel = campaignData.campaignLevel;
        int newMission = campaignData.mission + 1;

        if (newMission > 10)
        {
            newMission = 1;
            newLevel++;
        }

        UpdateCampaignData(campaignName, newLevel, newMission);
    }

    public (int campaignLevel, int mission) LoadCampaignData(string campaignName)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = $"SELECT {campaignName}Lvl, {campaignName}mission FROM PlayerMissions";
                dbCmd.CommandText = sqlQuery;
                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int campaignLevel = reader.GetInt32(0);
                        int mission = reader.GetInt32(1);
                        return (campaignLevel, mission);
                    }
                    else
                    {
                        Debug.LogError("No campaign data found");
                        return (0, 0);
                    }
                }
            }
        }
    }

    private void UpdateCampaignData(string campaignName, int newLevel, int newMission)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = $"UPDATE PlayerMissions SET {campaignName}Lvl = @newLevel, {campaignName}mission = @newMission";
                dbCmd.CommandText = sqlQuery;
                dbCmd.Parameters.Add(new SqliteParameter("@newLevel", newLevel));
                dbCmd.Parameters.Add(new SqliteParameter("@newMission", newMission));
                dbCmd.ExecuteNonQuery();
            }
        }
    }
}
