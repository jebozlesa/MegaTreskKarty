using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;
using System.Data;
using TMPro;

public class LoadCampaign : MonoBehaviour
{
    public Image backgroundImage;  // Pripojte tu vašu UI Image alebo Sprite Renderer
    private string connectionString;
    public List<Button> missionButtons;


    
    void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        LoadBackgroundImage();
        LoadMissionNames();
    }

    void LoadMissionNames()
    {
        IDbConnection dbConnection = (IDbConnection)new SqliteConnection(connectionString);
        dbConnection.Open(); 

        IDbCommand dbCommand = dbConnection.CreateCommand();
        string sqlQuery = "SELECT * FROM Campaigns WHERE CampaignID = 1"; 
        dbCommand.CommandText = sqlQuery;

        IDataReader reader = dbCommand.ExecuteReader();
        reader.Read();
        
        for (int i = 0; i < 10; i++)
        {
            int missionID = reader.GetInt32(i+3); // i+3 to skip first 3 columns (CampaignID, CampaignName, Background)
            
            string missionNameQuery = "SELECT MissionName FROM Missions WHERE MissionID=" + missionID;
            IDbCommand missionCommand = dbConnection.CreateCommand();
            missionCommand.CommandText = missionNameQuery;
            IDataReader missionReader = missionCommand.ExecuteReader();
            missionReader.Read();
            string missionName = missionReader.GetString(0);
            
            missionButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = missionName;
            
            missionReader.Close();
            missionReader = null;
            missionCommand.Dispose();
            missionCommand = null;
        }

        reader.Close();
        reader = null;
        dbCommand.Dispose();
        dbCommand = null;
        dbConnection.Close();
        dbConnection = null;
    }


    void LoadBackgroundImage()
    {
        //string connection = "URI=file:" + Application.dataPath + "/MyDatabase.db"; //Replace this with your connection string
        IDbConnection dbConnection = (IDbConnection)new SqliteConnection(connectionString);
        dbConnection.Open(); 

        IDbCommand dbCommand = dbConnection.CreateCommand();
        string sqlQuery = "SELECT Background FROM Campaigns WHERE CampaignID = 1"; //Replace with your query
        dbCommand.CommandText = sqlQuery;

        IDataReader reader = dbCommand.ExecuteReader();
        while (reader.Read())
        {
            string backgroundName = reader.GetString(0);
            Sprite newSprite = Resources.Load<Sprite>("Campaign/" + backgroundName);
            if (newSprite)
            {
                backgroundImage.sprite = newSprite;
            }
            else
            {
                Debug.LogError("Sprite not found", this);
            }
        }
        
        reader.Close();
        reader = null;
        dbCommand.Dispose();
        dbCommand = null;
        dbConnection.Close();
        dbConnection = null;
    }
}
