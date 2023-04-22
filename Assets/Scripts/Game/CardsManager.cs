using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CardsManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform cardsParent;

    private List<Card> cards = new List<Card>();

    void Start()
    {
        //LoadCards();
        //CreateCards();
    }

    // Táto funkcia bude načítať údaje o kartách zo súboru CardsData.txt
 /*   public void LoadCards()
    {
        // Pripoj textový súbor k objektu s týmto skriptom
        string filePath = Path.Combine(Application.dataPath, "files", "CardsData.txt");

        // Načítaj údaje zo súboru
        if (File.Exists(filePath))
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Vytvor kartu z načítaných údajov a pridaj ju do zoznamu kariet
                    Card card = CreateCardFromData(line);
                    cards.Add(card);
                }
            }
        }
        else
        {
            Debug.LogWarning("Súbor neexistuje.");
        }
    }

    // Táto funkcia vytvorí objekty kariet na obrazovke
    public void CreateCards()
    {
        foreach (Card card in cards)
        {
            GameObject newCardObject = Instantiate(cardPrefab, cardsParent);
            cardDisplay.card = card;
        }
    }

    // Táto funkcia vytvorí kartu z údajov v textovom súbore
    private Card CreateCardFromData(string cardData)
    {
        // Rozdel údaje na jednotlivé časti
        string[] data = cardData.Split(',');

        // Vytvor novú kartu z údajov
        Card card = new Card();
        card.name = data[0];
        card.attack = int.Parse(data[1]);
        card.defense = int.Parse(data[2]);
        card.type = (CardType)Enum.Parse(typeof(CardType), data[3]);
        card.description = data[4];

        return card;
    }*/
}
