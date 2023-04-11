using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{

    float maxHp;
    public static float staticHp;
    public float hp;
    public Image health;
    public Image healthBorder;
    //public TMP_Text hpText;


    public void SetBar(Kard card)
    {
        maxHp = card.health;
        SetHP(maxHp);
    }

    // Update is called once per frame
    public void SetHP(float hp) 
    {
        health.fillAmount = hp/maxHp;
        healthBorder.fillAmount = hp/maxHp;

        if(hp >= maxHp)
        {
            hp = maxHp;
        }

        //hpText.text = hp + "/" + maxHp;
    }
}
