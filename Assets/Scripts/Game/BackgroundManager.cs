using System.Data;
using UnityEngine;
using UnityEngine.UI;
using Mono.Data.Sqlite;

public class BackgroundManager : MonoBehaviour
{

    public Image backgroundImage;
    private string connectionString;


    // Start is called before the first frame update
    void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        SetBackgroundImage(GameParameters.MissionID);
    }

    void SetBackgroundImage(int missionID)
    {
        IDbConnection dbConnection = (IDbConnection)new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT BackgroundName FROM Missions WHERE MissionID = {missionID}";
        IDataReader reader = dbCommand.ExecuteReader();

        if(reader.Read())
        {
            string backgroundName = reader.GetString(0);

            // Load the background image from the Resources folder
            Sprite backgroundSprite = Resources.Load<Sprite>("Campaign/" + backgroundName);

            // Set the background image
            backgroundImage.sprite = backgroundSprite;
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();
    }
}
