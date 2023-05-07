using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public GameObject deckPanel;
    public GameObject zoomedCardHolder;
    public string connectionString;

    void Start()
    {
        connectionString = $"URI=file:{Database.Instance.GetDatabasePath()}";
        LoadDeckCards();
    }
    

    public bool IsCardNameInDeck(string cardNameToCheck)
    {
        foreach (Transform cardTransform in deckPanel.transform)
        {
            Card card = cardTransform.GetComponent<Card>();
            if (card.cardName == cardNameToCheck)
            {
                return true;
            }
        }
        return false;
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

    private void LoadDeckCards()
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "SELECT * FROM PlayerDecks WHERE DeckID = 1"; // Load the cards for a specific deck
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read()) // Add this line to check if there is a row
        {
            int[] cardIDs = new int[5];
            for (int i = 0; i < 5; i++)
            {
                cardIDs[i] = reader.GetInt32(i + 2); // Get the CardIDs from the table
            }

            reader.Close();
            dbCommand.Dispose();

            // Load the card information and create the cards
            foreach (int cardID in cardIDs)
            {
                AddCardToHand(cardID);
            }
        }
        else
        {
            Debug.LogWarning("No rows found in the PlayerDecks table for the specified DeckID.");
        }

        dbConnection.Close();
    }

    public void AddCardToHand(int cardID)
    {
        IDbConnection dbConnection = new SqliteConnection(connectionString);
        dbConnection.Open();

        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = $"SELECT * FROM PlayerCards WHERE CardID = {cardID}";
        IDataReader reader = dbCommand.ExecuteReader();

        if (reader.Read())
        {
            GameObject novaKarta = Instantiate(cardPrefab, transform);

            novaKarta.GetComponent<Card>().cardId = cardID;
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
            novaKarta.GetComponent<Card>().transform.SetParent(deckPanel.transform);
            novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;
            novaKarta.GetComponent<Card>().deckPanel = deckPanel;
            novaKarta.GetComponent<Card>().deckCard = true;
            novaKarta.GetComponent<Card>().deckManager = this;
        }

        reader.Close();
        dbCommand.Dispose();
        dbConnection.Close();
    }

}
