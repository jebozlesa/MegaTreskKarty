using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[DefaultExecutionOrder(200)]
public class DisplayCard : MonoBehaviour
{
    public List<Card> displayCard = new List<Card>();
    public int displayID;

    public int id;
    public string cardName;
    public int level;
    public Sprite spriteImage;
    public Image background;

    public TMP_Text nameText;
    public TMP_Text levelText;
    public Image artImage;
    public Color32 color;

    public int damage;
    public int maxHP;
    public int currentHP;

    public TMP_Text cardHP;

    public bool cardBack;
    public static bool staticCardBack;

    public GameObject Hand;
    public int numOfCardsInDeck;


    void Start()
    {
        numOfCardsInDeck = PlayerDeck.deckSize;

        displayCard[0] = CardDatabase.cardList[displayID];

        id = displayCard[0].id;
        cardName = displayCard[0].cardName;
        level = displayCard[0].level;
        spriteImage = displayCard[0].spriteImage;
        color = displayCard[0].color;
        damage = displayCard[0].damage;
        maxHP = displayCard[0].maxHP;
        currentHP = displayCard[0].maxHP;


        background.GetComponent<Image>().color = color;
        nameText.text = cardName;
        nameText.color = color;
        levelText.text = "lvl " + level;
        levelText.color = color;
        artImage.sprite = spriteImage;
    //    cardHP.text = ""+currentHP;
    }

}
