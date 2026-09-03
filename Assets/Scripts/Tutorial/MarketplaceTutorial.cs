using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MarketplaceTutorial : MonoBehaviour
{
    public GameObject tutorialPanelHint1;
    public GameObject tutorialPanelHint2;
    public GameObject tutorialPanelEmpty;
    public TutorialService tutorialService;

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
        CompleteStep(TutorialConstants.OpenMarketplace);
        StartCoroutine(ShowSecondHintAfterDelay());
    }

    public void CloseSecondHint()
    {
        Debug.Log("MarketplaceTutorial.CloseSecondHint() ===> START");
        tutorialPanelHint2.SetActive(false);
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

    private async void CompleteStep(string stepId)
    {
        TutorialService service = ResolveTutorialService();
        if (service == null)
        {
            return;
        }

        await service.CompleteCurrentPlayerStepAsync(
            TutorialConstants.MarketplaceFirstPack,
            stepId
        );
    }

    private TutorialService ResolveTutorialService()
    {
        if (tutorialService != null)
        {
            return tutorialService;
        }

        tutorialService = FindFirstObjectByType<TutorialService>(FindObjectsInactive.Include);
        return tutorialService;
    }
}
