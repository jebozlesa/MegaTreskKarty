using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
//[System.Serializable]

public class Card:MonoBehaviour
{
    public List<Card> displayCard = new List<Card>();
    public int displayID;
    
    public int id;
    public string cardName;
    public int level;
    public Sprite spriteImage;
    public Image background;

    public int damage;
    public int maxHP;
    public int currentHP;

    public TMP_Text nameText;
    public TMP_Text levelText;
    public Image artImage;
    public Color32 color;
    

    public Card(int Id,string CardName,int Level, Sprite SpriteImage, Color32 Color, int Damage, int MaxHP)
    {

        id = Id;
        cardName = CardName;
        level = Level;
        spriteImage = SpriteImage;
        color = Color;
        damage = Damage;
        maxHP = MaxHP;

    }
}
