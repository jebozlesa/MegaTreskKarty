using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class Database : MonoBehaviour
{
    public static Database Instance;

    private string databasePath;

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
        }
        InitializeDatabase();
    }

    void Start()
    {
        
    }

    private void InitializeDatabase()
{
    string sourceFilePath = Path.Combine(Application.streamingAssetsPath, "MyDatabase.db");
    string destinationFilePath = Path.Combine(Application.persistentDataPath, "MyDatabase.db");

    if (!File.Exists(destinationFilePath))
    {
        if (sourceFilePath.Contains("://") || sourceFilePath.Contains(":///"))
        {
            // Android
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(sourceFilePath);
            unityWebRequest.SendWebRequest();
            while (!unityWebRequest.isDone) { }
            File.WriteAllBytes(destinationFilePath, unityWebRequest.downloadHandler.data);
        }
        else
        {
            // Other platforms
            File.Copy(sourceFilePath, destinationFilePath, true);
        }
    }
}

    public string GetDatabasePath()
{
    return Path.Combine(Application.persistentDataPath, "MyDatabase.db");
}
}
