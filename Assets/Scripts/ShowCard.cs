using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShowCard : MonoBehaviour
{
    public int cardId;
    public string cardName;

    public string image;
    public Color32 color;
    public int level;


    public TMP_Text nameText;
    public TMP_Text levelText;
    

    public Image cardImage;
    public Sprite cardSprite;

    public Image background;

    public GameObject cardPrefab;

    public TMP_Text labelText;


    private void Start()
    {


        nameText.text = cardName;
        levelText.text = "lvl " + level;
        cardImage.sprite = Resources.Load<Sprite>(image);
        background.GetComponent<Image>().color = color;
        nameText.color = color;
        levelText.color = color;

        labelText.color = color;
        
    }

    
}
