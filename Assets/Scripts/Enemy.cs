using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public List<Kard> hand = new List<Kard>();

    public void PlayCard(Kard card, GameObject playerBoard)
    {
        // Presuňte kartu z ruky do hry
        hand.Remove(card);
        card.transform.SetParent(playerBoard.transform);
        
        //GameObject kartaHra = Instantiate(kartaPrefab, board.transform);

        // Ak je na ploche iná karta, odstráňte ju
      /*  Kard existingCard = FindObjectOfType<Kard>();
        if (existingCard != null)
        {
            Destroy(existingCard.gameObject);
        }*/
        
        // Umiestnite kartu na plochu
        card.transform.localPosition = new Vector3(0f, 0f, 0f);
        
        // Vykreslite kartu v prednej časti, aby bola viditeľná
        //card.GetComponent<Canvas>().sortingOrder = 1;
    }

    public void AddCardToHand(Kard card)
    {
        Debug.Log("PRIDAVAM KARTU");
            hand.Add(card);
            Debug.Log("PRIDAVAM KARTU  " + card.cardName);
            card.transform.SetParent(transform);
            card.transform.localScale = Vector3.one;
            //ArrangeCardsInHand();
    }

    public void RemoveCardFromHand(Kard card)
    {
        hand.Remove(card);
        Destroy(card.gameObject);
        ArrangeCardsInHand();
    }

    public void ArrangeCardsInHand()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            Kard card = hand[i];
            card.transform.localPosition = new Vector3(i * 150f - 450f, -250f, 0f);
            card.transform.SetSiblingIndex(i);
        }
    }
}
