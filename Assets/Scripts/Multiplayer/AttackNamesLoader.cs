using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Nacitava a zobrazuje nazvy utokov vybranej karty
/// </summary>
public class AttackNamesLoader : MonoBehaviour
{
    public TMP_Text button1Text;
    public TMP_Text button2Text;
    public TMP_Text button3Text;
    public TMP_Text button4Text;

    public Button button1;
    public Button button2;
    public Button button3;
    public Button button4;

    public AttackDescriptions attackDescriptions;

    /// <summary>
    /// Nacita a zobrazi nazvy utokov pre danu kartu
    /// </summary>
    /// <param name="card">Karta, ktorej utoky sa maju zobrazit</param>
    public void LoadAttackNames(Kard card)
    {
        if (card == null)
        {
            Debug.LogWarning("[AttackNamesLoader] Card is null");
            ClearAttackNames();
            return;
        }

        if (attackDescriptions == null)
        {
            Debug.LogError("[AttackNamesLoader] AttackDescriptions reference is missing");
            return;
        }

        // Nacitaj nazov pre kazdy utok
        LoadAttackName(card.attack1, button1Text, button1);
        LoadAttackName(card.attack2, button2Text, button2);
        LoadAttackName(card.attack3, button3Text, button3);
        LoadAttackName(card.attack4, button4Text, button4);

        Debug.Log($"[AttackNamesLoader] Loaded attack names for card: {card.cardName}");
    }

    /// <summary>
    /// Nacita nazov konkretneho utoku
    /// </summary>
    private void LoadAttackName(int attackId, TMP_Text buttonText, Button button)
    {
        if (attackId == 0 || buttonText == null)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
            return;
        }

        // Ziskaj nazov utoku z AttackDescriptions
        string attackName = attackDescriptions.GetAttackName(attackId);
        
        if (!string.IsNullOrEmpty(attackName))
        {
            buttonText.text = attackName;
            if (button != null)
            {
                button.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning($"[AttackNamesLoader] Unknown attack ID: {attackId}");
            buttonText.text = "Unknown";
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Vymaze vsetky nazvy utokov
    /// </summary>
    public void ClearAttackNames()
    {
        if (button1Text != null) button1Text.text = "";
        if (button2Text != null) button2Text.text = "";
        if (button3Text != null) button3Text.text = "";
        if (button4Text != null) button4Text.text = "";

        if (button1 != null) button1.gameObject.SetActive(false);
        if (button2 != null) button2.gameObject.SetActive(false);
        if (button3 != null) button3.gameObject.SetActive(false);
        if (button4 != null) button4.gameObject.SetActive(false);
    }
}
