using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MarketplaceTutorial : MonoBehaviour
{

    public GameObject tutorialPanelHint1;
    public GameObject tutorialPanelHint2;
    public GameObject tutorialPanelEmpty;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("MarketplaceTutorial.Start() ===> START");
        // Check if the player has completed the tutorial

        tutorialPanelHint1.SetActive(true);

    }

    public void CloseFirstHint()
    {
        Debug.Log("MarketplaceTutorial.CloseFirstHint() ===> START");
        tutorialPanelHint1.SetActive(false);
        StartCoroutine(ShowSecondHintAfterDelay());
    }

    public void CloseSecondHint()
    {
        Debug.Log("MarketplaceTutorial.CloseSecondHint() ===> START");
        tutorialPanelHint2.SetActive(false);
        PlayerPrefs.SetInt("HasCompletedTutorialMarketplace", 1);
        PlayerPrefs.Save();
    }

    public void LoadSceneAlbum()
    {
        Debug.Log("MarketplaceTutorial.LoadSceneAlbum() ===> START");
        SceneManager.LoadScene("Cards");
    }

    // Coroutine to show the second hint after a delay
    private IEnumerator ShowSecondHintAfterDelay()
    {
        tutorialPanelEmpty.SetActive(true);
        yield return new WaitForSeconds(1f); // Wait for 1 second
        tutorialPanelEmpty.SetActive(false);
        tutorialPanelHint2.SetActive(true);
    }
}