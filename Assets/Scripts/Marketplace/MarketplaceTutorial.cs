using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketplaceTutorial : MonoBehaviour
{

    public GameObject tutorialPanel;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ClosePanel()
    {
        tutorialPanel.SetActive(false);
    }
}
