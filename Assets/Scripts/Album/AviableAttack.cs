using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AviableAttack : MonoBehaviour
{
    public TMP_Text attackName;
    public GameObject attackDetailsPanel; // GameObject na ktorom sa nachádzajú texty Description, Attributes a Special
    public TMP_Text attackDescription;
    public TMP_Text attackAttributes;
    public TMP_Text attackSpecial;
    private bool isDescriptionVisible = false;

    private void Start()
    {
        // Skryte popis útoku na začiatku
        attackDetailsPanel.SetActive(false);
    }

    public void ToggleDescriptionVisibility()
    {
        Debug.Log("Tuknute");
        isDescriptionVisible = !isDescriptionVisible;
        attackDetailsPanel.SetActive(isDescriptionVisible);

        if (isDescriptionVisible)
        {
            // Zobraz popis útoku
            attackDescription.gameObject.SetActive(true);
        }
        else
        {
            // Skry popis útoku
            attackDescription.gameObject.SetActive(false);
        }
    }
}

