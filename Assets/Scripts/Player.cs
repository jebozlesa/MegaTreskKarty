using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{

    public TMP_Text dialogText;

    public List<Kard> hand = new List<Kard>();
    public List<Kard> cardsInGame = new List<Kard>();
    public bool isEnemy = false;

    public Kard cardInGame;

    public void PlayCard(Kard card, GameObject playerBoard)
    {
        // Presuňte kartu z ruky do hry
        hand.Remove(card);
        //cardsInGame.Add(card);
        cardInGame = card;

        card.transform.SetParent(playerBoard.transform);        
        card.transform.localPosition = new Vector3(0f, 0f, 0f);

        GameObject cardBack = card.gameObject.transform.Find("CardBack").gameObject;
        cardBack.SetActive(false);
        
    }

    public void ReturnCardToHand(Kard card, Vector3 originalHandPosition)
    {
        // Presuňte kartu z bojového priestoru do ruky
        cardInGame = null;
        hand.Add(card);

        // Zmena rodičovského objektu a pozície karty
        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one;
        card.transform.localPosition = originalHandPosition;
    }

    public void RemoveCardFromBoard(Kard card)
    {
        dialogText.text = card.cardName + " failed";

        cardsInGame.Remove(card);
        Destroy(card.gameObject);
    }


    public void AddCardToHand(Kard card)
    {
        hand.Add(card);
        card.transform.SetParent(transform);
        card.transform.localScale = Vector3.one;

        GameObject cardBack = card.gameObject.transform.Find("CardBack").gameObject;
        if (!isEnemy)
        {
            cardBack.SetActive(false);
        }
    }

    public void RemoveCardFromHand(Kard card)
    {
        hand.Remove(card);
        Destroy(card.gameObject);
    }

}
