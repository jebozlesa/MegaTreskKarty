using UnityEngine;
using UnityEngine.UI;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

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
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            if (result.Data.ContainsKey("PlayerDecks"))
            {
                string deckDataJson = result.Data["PlayerDecks"].Value;
                DeckListWrapper deckList = JsonUtility.FromJson<DeckListWrapper>(deckDataJson);

                foreach (Deck deck in deckList.Decks)
                {
                    AddCardToHand(deck.Card1);
                    AddCardToHand(deck.Card2);
                    AddCardToHand(deck.Card3);
                    AddCardToHand(deck.Card4);
                    AddCardToHand(deck.Card5);
                }
            }
            else
            {
                Debug.LogWarning("No rows found in the PlayerDecks table for the specified DeckID.");
            }
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }


    // private void LoadDeckCards()
    // {
    //     IDbConnection dbConnection = new SqliteConnection(connectionString);
    //     dbConnection.Open();

    //     IDbCommand dbCommand = dbConnection.CreateCommand();
    //     dbCommand.CommandText = "SELECT * FROM PlayerDecks WHERE DeckID = 1"; // Load the cards for a specific deck
    //     IDataReader reader = dbCommand.ExecuteReader();

    //     if (reader.Read()) // Add this line to check if there is a row
    //     {
    //         int[] cardIDs = new int[5];
    //         for (int i = 0; i < 5; i++)
    //         {
    //             cardIDs[i] = reader.GetInt32(i + 2); // Get the CardIDs from the table
    //         }

    //         reader.Close();
    //         dbCommand.Dispose();

    //         // Load the card information and create the cards
    //         foreach (int cardID in cardIDs)
    //         {
    //             AddCardToHand(cardID);
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("No rows found in the PlayerDecks table for the specified DeckID.");
    //     }

    //     dbConnection.Close();
    // }

    public void AddCardToHand(int cardStyleID)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            string existingDataJson = "{}";
            if (result.Data.ContainsKey("PlayerCards"))
            {
                existingDataJson = result.Data["PlayerCards"].Value;
            }

            if (!string.IsNullOrEmpty(existingDataJson))
            {
                CardListWrapper existingCards = JsonUtility.FromJson<CardListWrapper>(existingDataJson);
                foreach (GeneratedCard existingCard in existingCards.cards)
                {
                    if (existingCard.StyleID == cardStyleID)
                    {
                        GameObject novaKarta = Instantiate(cardPrefab, transform);
                        novaKarta.GetComponent<Card>().cardId = existingCard.StyleID;
                        novaKarta.GetComponent<Card>().cardName = existingCard.PersonName;
                        novaKarta.GetComponent<Card>().health = existingCard.Health;
                        novaKarta.GetComponent<Card>().strength = existingCard.Strength;
                        novaKarta.GetComponent<Card>().speed = existingCard.Speed;
                        novaKarta.GetComponent<Card>().attack = existingCard.Attack;
                        novaKarta.GetComponent<Card>().defense = existingCard.Defense;
                        novaKarta.GetComponent<Card>().knowledge = existingCard.Knowledge;
                        novaKarta.GetComponent<Card>().charisma = existingCard.Charisma;
                        novaKarta.GetComponent<Card>().image = existingCard.CardPicture;
                        Color32 cardColor = new Color32((byte)existingCard.Color[0], (byte)existingCard.Color[1], (byte)existingCard.Color[2], 255);
                        novaKarta.GetComponent<Card>().color = cardColor;
                        novaKarta.GetComponent<Card>().level = existingCard.Level;
                        novaKarta.GetComponent<Card>().attack1 = existingCard.Attack1;
                        novaKarta.GetComponent<Card>().attack2 = existingCard.Attack2;
                        novaKarta.GetComponent<Card>().attack3 = existingCard.Attack3;
                        novaKarta.GetComponent<Card>().attack4 = existingCard.Attack4;
                        // novaKarta.GetComponent<Card>().story = LoadStory(existingCard.StyleID);
                        novaKarta.GetComponent<Card>().zoomedCardHolder = zoomedCardHolder;
                        novaKarta.GetComponent<Card>().transform.SetParent(deckPanel.transform);
                        novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;
                        novaKarta.GetComponent<Card>().deckPanel = deckPanel;
                        novaKarta.GetComponent<Card>().deckCard = true;
                        novaKarta.GetComponent<Card>().deckManager = this;
                    }
                }
            }
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }



    // public void AddCardToHand(int cardID)
    // {
    //     IDbConnection dbConnection = new SqliteConnection(connectionString);
    //     dbConnection.Open();

    //     IDbCommand dbCommand = dbConnection.CreateCommand();
    //     dbCommand.CommandText = $"SELECT * FROM PlayerCards WHERE CardID = {cardID}";
    //     IDataReader reader = dbCommand.ExecuteReader();

    //     if (reader.Read())
    //     {
    //         GameObject novaKarta = Instantiate(cardPrefab, transform);

    //         novaKarta.GetComponent<Card>().cardId = cardID;
    //         novaKarta.GetComponent<Card>().cardName = reader.GetString(2);
    //         novaKarta.GetComponent<Card>().health = reader.GetInt32(5);
    //         novaKarta.GetComponent<Card>().strength = reader.GetInt32(6);
    //         novaKarta.GetComponent<Card>().speed = reader.GetInt32(7);
    //         novaKarta.GetComponent<Card>().attack = reader.GetInt32(8);
    //         novaKarta.GetComponent<Card>().defense = reader.GetInt32(9);
    //         novaKarta.GetComponent<Card>().knowledge = reader.GetInt32(10);
    //         novaKarta.GetComponent<Card>().charisma = reader.GetInt32(11);
            
    //         novaKarta.GetComponent<Card>().image = reader.GetString(18);

    //         string[] farbaKarty = reader.GetString(13).Split(';');
    //         Color32 cardColor = new Color32(byte.Parse(farbaKarty[0]), byte.Parse(farbaKarty[1]), byte.Parse(farbaKarty[2]), 255);
    //         novaKarta.GetComponent<Card>().color = cardColor;

    //         novaKarta.GetComponent<Card>().level = reader.GetInt32(3);
    //         novaKarta.GetComponent<Card>().attack1 = reader.GetInt32(14);
    //         novaKarta.GetComponent<Card>().attack2 = reader.GetInt32(15);
    //         novaKarta.GetComponent<Card>().attack3 = reader.GetInt32(16);
    //         novaKarta.GetComponent<Card>().attack4 = reader.GetInt32(17);


            // novaKarta.GetComponent<Card>().story = LoadStory(reader.GetInt32(1));
    //         novaKarta.GetComponent<Card>().zoomedCardHolder = zoomedCardHolder;
    //         novaKarta.GetComponent<Card>().transform.SetParent(deckPanel.transform);
    //         novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;
    //         novaKarta.GetComponent<Card>().deckPanel = deckPanel;
    //         novaKarta.GetComponent<Card>().deckCard = true;
    //         novaKarta.GetComponent<Card>().deckManager = this;
    //     }

    //     reader.Close();
    //     dbCommand.Dispose();
    //     dbConnection.Close();
    // }

}
