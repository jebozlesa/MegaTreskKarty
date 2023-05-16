using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class AviableAttack : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int attackId;
    public int originalAttackId;
    public TMP_Text attackName;
    public GameObject attackDetailsPanel; // GameObject na ktorom sa nachádzajú texty Description, Attributes a Special
    public TMP_Text attackDescription;
    public TMP_Text attackAttributes;
    public TMP_Text attackSpecial;

    public Image backgroundImage;  // Komponent Image z prefabu
    public Color normalColor = Color.white;  // Farba pri nevýbere
    public Color selectedColor = Color.yellow;  // Farba pri výbere
    private bool isSelected = false;

    public Card card;

    private void Start()
    {
        // Skryte popis útoku na začiatku
        attackDetailsPanel.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Zobraz popis útoku
        attackDetailsPanel.SetActive(true);
        attackDescription.gameObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Skry popis útoku
        attackDetailsPanel.SetActive(false);
        attackDescription.gameObject.SetActive(false);

        if (!isSelected) 
        {
            card.DeselectAllAttacks();  // Deselect all other attacks
            card.SelectAttack(attackId, originalAttackId);
            SetSelected(true);
        } 
        else 
        {
            card.SelectAttack(-1, -1);  // Deselect this attack
            SetSelected(false);
        }
    }

    public void SetSelected(bool selected) 
    {
        isSelected = selected;
        if (selected) 
        {
            backgroundImage.color = selectedColor;
        } 
        else 
        {
            backgroundImage.color = normalColor;
        }
    }
}
