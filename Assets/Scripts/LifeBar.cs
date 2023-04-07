using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LifeBar : MonoBehaviour
{

    public TMP_Text cardHP;

    // Update is called once per frame
    public void SetBar(Card card)
    {
        cardHP.text = "" + card.maxHP;
    }

    public void SetHP(int hp) 
    {
        cardHP.text = "" + hp;
    }
}
