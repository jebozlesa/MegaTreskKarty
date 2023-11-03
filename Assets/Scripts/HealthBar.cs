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
        this.hp = Mathf.Clamp(hp, 0, maxHp); // Zabezpečíme, aby hp nebolo mimo rozsah

        // Aktualizujeme množstvo zdravia
        health.fillAmount = this.hp / maxHp;

        // Interpolácia farby medzi zelenou (1), žltou (0.5) a červenou (0)
        Color green = Color.green;
        Color yellow = Color.yellow;
        Color red = Color.red;

        float healthPercent = this.hp / maxHp;

        // Interpolujeme medzi zelenou a žltou alebo medzi žltou a červenou na základe percenta zdravia
        if (healthPercent > 0.5f)
        {
            // Zdravie medzi 50% a 100%, interpolujeme medzi zelenou a žltou
            health.color = Color.Lerp(yellow, green, (healthPercent - 0.5f) * 2);
        }
        else
        {
            // Zdravie medzi 0% a 50%, interpolujeme medzi žltou a červenou
            health.color = Color.Lerp(red, yellow, healthPercent * 2);
        }

        // hpText.text = this.hp + "/" + maxHp; // Ak chcete zobrazovať textové informácie o zdraví
    }

}
