using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(100)]
public class PlayerDeck : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public int x;
    public static int deckSize;
    public List<Card> deck = new List<Card>();
    public static List<Card> staticDeck = new List<Card>();

    public GameObject cardInDeck1;

    public GameObject CardToHand;
    public GameObject[] Clones;
    public GameObject Hand;

    public GameObject CardToGamePlayer;
    public GameObject PlayerBattlefield;

   // public GameObject CardToGameEnemy;
   // public GameObject EnemyBattlefield;

    void Start()
    {
        x = 0;
        deckSize = 40;

        Debug.Log("PlayerDeck initialization KOKOT!");

        for (int i=0;i<deckSize;i++)
        {
            x = Random.Range(1,72);
            deck[i] = CardDatabase.cardList[x];
        }

        StartCoroutine(StartGame());
    }

    // Update is called once per frame
    void Update()
    {

        staticDeck = deck;

        /*if(deckSize<5)//aby mizli karty, asi netreba
        {
            cardInDeck1.SetActive(false);
        }
        if(deckSize<4)
        {
            cardInDeck2.SetActive(false);
        }
        if(deckSize<3)
        {
            cardInDeck3.SetActive(false);
        }
        if(deckSize<2)
        {
            cardInDeck4.SetActive(false);
        }*/

        if(TurnSystem.startTurn == true)
        {
            StartCoroutine(Draw(1));
            TurnSystem.startTurn = false;
        }
    }

    IEnumerator StartGame()
    {
        for(int i=0;i<5;i++)
        {
            yield return new WaitForSeconds(1);
            //audioSource.PlayOneShot(draw,1f);
            Instantiate(CardToHand, transform.position, transform.rotation);
        }
        yield return new WaitForSeconds(1);
        //Instantiate(CardToGamePlayer, transform.position, transform.rotation);
        //Instantiate(CardToGameEnemy, transform.position, transform.rotation);
    }

    public void Shuffle()
    {
        for (int i=0;i<deckSize;i++)
        {
            container[0] = deck[i];
            int randomIndex = Random.Range(i,deckSize);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = container[0];
        }
    }

    IEnumerator Draw(int x)
    {
        for(int i=0;i<x;i++)
        {
            yield return new WaitForSeconds(1);
            Instantiate(CardToHand, transform.position, transform.rotation);
        }
    }
}
