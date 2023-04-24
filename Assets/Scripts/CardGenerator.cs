using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardGenerator : MonoBehaviour
{
    private void AddRandomCard()
    {
        // Load card database
        List<string> cardDatabase = new List<string>(File.ReadAllLines(Application.dataPath + "/Files/CardDatabase.txt"));

        // Select a random card from the card database
        int randomIndex = Random.Range(0, cardDatabase.Count);
        string[] randomCardData = cardDatabase[randomIndex].Split(',');

        // Add a new parameter with value 0 at the end of the card data
        List<string> updatedCardData = new List<string>(randomCardData);
        updatedCardData.Add("0");

        // Create a new card entry for PlayerCards.txt
        string newCardEntry = string.Join(",", updatedCardData);

        // Load player cards
        List<string> playerCards = new List<string>(File.ReadAllLines(Application.dataPath + "/Files/PlayerCards.txt"));

        // Add the new card entry to player cards
        playerCards.Add(newCardEntry);

        // Save the updated player cards back to PlayerCards.txt
        File.WriteAllLines(Application.dataPath + "/Files/PlayerCards.txt", playerCards);
    }

    private void AddCardById(int id)
    {
        // Load card database
        List<string> cardDatabase = new List<string>(File.ReadAllLines(Application.dataPath + "/Files/CardDatabase.txt"));

        string[] cardData = null;

        // Find the card with the specified ID in the card database
        for (int i = 0; i < cardDatabase.Count; i++)
        {
            string[] currentCardData = cardDatabase[i].Split(',');
            if (int.Parse(currentCardData[0]) == id)
            {
                cardData = currentCardData;
                break;
            }
        }

        if (cardData == null)
        {
            Debug.LogError("Card with the specified ID not found");
            return;
        }

        // Add a new parameter with value 0 at the end of the card data
        List<string> updatedCardData = new List<string>(cardData);
        updatedCardData.Add("0");

        // Create a new card entry for PlayerCards.txt
        string newCardEntry = string.Join(",", updatedCardData);

        // Load player cards
        List<string> playerCards = new List<string>(File.ReadAllLines(Application.dataPath + "/Files/PlayerCards.txt"));

        // Add the new card entry to player cards
        playerCards.Add(newCardEntry);

        // Save the updated player cards back to PlayerCards.txt
        File.WriteAllLines(Application.dataPath + "/Files/PlayerCards.txt", playerCards);
    }

}
