using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketplaceManager : MonoBehaviour
{

    public GameObject tutorial;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.GetInt("HasCompletedTutorialMarketplace", 0) == 0)  {   tutorial.SetActive(true);   }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
