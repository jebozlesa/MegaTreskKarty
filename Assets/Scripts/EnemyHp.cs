using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHp : MonoBehaviour
{
    public static float maxHp;
    public static float staticHp;
    public float hp;
    public Image health;
    public TMP_Text hpText;


    // Start is called before the first frame update
    void Start()
    {
        maxHp = 10;
        staticHp = 8;    
    }

    // Update is called once per frame
    void Update()
    {
        hp = staticHp;
        health.fillAmount = hp/maxHp;

        if(hp >= maxHp)
        {
            hp = maxHp;
        }

        hpText.text = hp + "HP";
    }
}
