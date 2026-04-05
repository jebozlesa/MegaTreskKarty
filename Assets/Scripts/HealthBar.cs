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
    // public TMP_Text hpText;

    public void SetBar(Kard card)
    {
        // Use maxHealth if available (multiplayer), otherwise use current health (singleplayer).
        maxHp = card.maxHealth > 0 ? card.maxHealth : card.health;
        SetHP(card.health);
    }

    public void SetHP(float hp)
    {
        float oldHp = this.hp;
        this.hp = Mathf.Clamp(hp, 0, maxHp); // Keep HP within a valid range.

        Debug.LogWarning(
            $"[HP_BAR] {gameObject.name}.SetHP({hp}) -> Clamped: {this.hp}/{maxHp} (fillAmount: {this.hp / maxHp:F2}) [Change: {this.hp - oldHp:+#.#;-#.#;0}]"
        );

        health.fillAmount = this.hp / maxHp;

        Color green = Color.green;
        Color yellow = Color.yellow;
        Color red = Color.red;

        float healthPercent = this.hp / maxHp;

        if (healthPercent > 0.5f)
        {
            health.color = Color.Lerp(yellow, green, (healthPercent - 0.5f) * 2);
        }
        else
        {
            health.color = Color.Lerp(red, yellow, healthPercent * 2);
        }

        // hpText.text = this.hp + "/" + maxHp;
    }
}
