using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class Album : MonoBehaviour
{

    public GameObject kartaPrefab;
    public GameObject album;
    public GameObject zoomedCardHolder;
    public AttackDescriptions attackDescriptions;
    public GameObject content;

    private string connectionString;

    public GameObject deckPanel;

    void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";

        deckPanel.SetActive(false);

        StartCoroutine(VytvorKarty());
    }

    string LoadStory(int cardID)
    {
        string cardStory = "";
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM CardStories WHERE StyleID = " + cardID;
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            cardStory = reader.GetString(2);
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();

        return cardStory;
    }

    private IEnumerator VytvorKarty()
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM PlayerCards";
        IDataReader reader = dbCommand.ExecuteReader();

        while (reader.Read())
        {
            GameObject novaKarta = Instantiate(kartaPrefab, transform);

            novaKarta.GetComponent<Card>().cardName = reader.GetString(2);
            novaKarta.GetComponent<Card>().health = reader.GetInt32(5);
            novaKarta.GetComponent<Card>().strength = reader.GetInt32(6);
            novaKarta.GetComponent<Card>().speed = reader.GetInt32(7);
            novaKarta.GetComponent<Card>().attack = reader.GetInt32(8);
            novaKarta.GetComponent<Card>().defense = reader.GetInt32(9);
            novaKarta.GetComponent<Card>().knowledge = reader.GetInt32(10);
            novaKarta.GetComponent<Card>().charisma = reader.GetInt32(11);
            
            novaKarta.GetComponent<Card>().image = reader.GetString(18);

            string[] farbaKarty = reader.GetString(13).Split(';');
            Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);
            novaKarta.GetComponent<Card>().color = cardColor;

            novaKarta.GetComponent<Card>().level = reader.GetInt32(3);
            novaKarta.GetComponent<Card>().attack1 = reader.GetInt32(14);
            novaKarta.GetComponent<Card>().attack2 = reader.GetInt32(15);
            novaKarta.GetComponent<Card>().attack3 = reader.GetInt32(16);
            novaKarta.GetComponent<Card>().attack4 = reader.GetInt32(17);


            novaKarta.GetComponent<Card>().story = LoadStory(reader.GetInt32(1));

            novaKarta.GetComponent<Card>().zoomedCardHolder = zoomedCardHolder;

            novaKarta.GetComponent<Card>().transform.SetParent(content.transform);
            novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;

            novaKarta.GetComponent<Card>().Initialize(deckPanel);

            yield return new WaitForSeconds(0.05f);
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();
    }

}
