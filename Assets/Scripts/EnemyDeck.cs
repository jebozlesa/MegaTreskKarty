using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(101)]
public class EnemyDeck : MonoBehaviour
{
    public List<Card> container = new List<Card>();
    public int x;
    public static int deckSize;
    public List<Card> deck = new List<Card>();
    public static List<Card> staticDeck = new List<Card>();

    public GameObject cardInDeck1;

    public GameObject[] Clones;

    //public GameObject CardToGamePlayer;
    //public GameObject PlayerBattlefield;

    public GameObject CardToGameEnemy;
    public GameObject EnemyBattlefield;

    void Start()
    {
        x = 0;
        deckSize = 40;

        Debug.Log("EnemyDeck initialization KOKOT!");

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
    }

    IEnumerator StartGame()
    {
        
        yield return new WaitForSeconds(6);
        //Instantiate(CardToGamePlayer, transform.position, transform.rotation);
        //Instantiate(CardToGameEnemy, transform.position, transform.rotation);
    }


}
