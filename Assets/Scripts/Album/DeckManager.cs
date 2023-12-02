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
            if (result.Data.ContainsKey("PlayerDecks") && result.Data.ContainsKey("PlayerCards"))
            {
                string deckDataJson = result.Data["PlayerDecks"].Value;
                DeckListWrapper deckList = JsonUtility.FromJson<DeckListWrapper>(deckDataJson);

                string playerCardsJson = result.Data["PlayerCards"].Value;
                CardListWrapper playerCards = JsonUtility.FromJson<CardListWrapper>(playerCardsJson);

                foreach (Deck deck in deckList.Decks)
                {
                    CreateCardInDeck(deck.Card1, playerCards);
                    CreateCardInDeck(deck.Card2, playerCards);
                    CreateCardInDeck(deck.Card3, playerCards);
                    CreateCardInDeck(deck.Card4, playerCards);
                    CreateCardInDeck(deck.Card5, playerCards);
                }
            }
            else
            {
                Debug.LogWarning("No rows found in the PlayerDecks or PlayerCards table.");
            }
        }, error => Debug.LogError(error.GenerateErrorReport()));
    }

    private void CreateCardInDeck(string cardID, CardListWrapper playerCards)
    {
        foreach (GeneratedCard existingCard in playerCards.cards)
        {
            if (existingCard.CardID == cardID)
            {
                GameObject novaKarta = Instantiate(cardPrefab, transform);
                novaKarta.GetComponent<Card>().cardId = existingCard.CardID;
                novaKarta.GetComponent<Card>().styleId = existingCard.StyleID;
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
                novaKarta.GetComponent<Card>().story = LoadStory(existingCard.StyleID);
                novaKarta.GetComponent<Card>().zoomedCardHolder = zoomedCardHolder;
                novaKarta.GetComponent<Card>().transform.SetParent(deckPanel.transform);
                novaKarta.GetComponent<Card>().transform.localScale = Vector3.one;
                novaKarta.GetComponent<Card>().deckPanel = deckPanel;
                novaKarta.GetComponent<Card>().deckCard = true;
                novaKarta.GetComponent<Card>().deckManager = this;
            }
        }
    }

    public void AddCardToHand(string cardID)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), result =>
        {
            string existingDataJson = "{}";
            if (result.Data.ContainsKey("PlayerCards"))
            {
                existingDataJson = result.Data["PlayerCards"].Value;
            }
            Debug.Log(existingDataJson);

            if (!string.IsNullOrEmpty(existingDataJson))
            {
                CardListWrapper existingCards = JsonUtility.FromJson<CardListWrapper>(existingDataJson);
                foreach (GeneratedCard existingCard in existingCards.cards)
                {
                    if (existingCard.CardID == cardID)
                    {
                        GameObject novaKarta = Instantiate(cardPrefab, transform);
                        novaKarta.GetComponent<Card>().cardId = existingCard.CardID;
                        novaKarta.GetComponent<Card>().styleId = existingCard.StyleID;
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


}
