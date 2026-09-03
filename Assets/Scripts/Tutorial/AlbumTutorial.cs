using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlbumTutorial : MonoBehaviour
{
    public GameObject tutorialPanelHint1;
    public GameObject tutorialPanelHint2;
    public GameObject tutorialPanelEmpty;
    public TutorialService tutorialService;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("AlbumTutorial.Start() ===> START");

        tutorialPanelHint1.SetActive(true);
    }

    public void CloseFirstHint()
    {
        Debug.Log("AlbumTutorial.CloseFirstHint() ===> START");
        tutorialPanelHint1.SetActive(false);
        CompleteStep(TutorialConstants.SeeOwnedCards);
        StartCoroutine(ShowHintAfterDelay(tutorialPanelHint2));
    }

    public void CloseSecondHint()
    {
        Debug.Log("AlbumTutorial.CloseSecondHint() ===> START");
        tutorialPanelHint2.SetActive(false);
        CompleteStep(TutorialConstants.DeckArea);
    }

    public void LoadSceneAlbum()
    {
        Debug.Log("AlbumTutorial.LoadSceneAlbum() ===> START");
        SceneManager.LoadScene("Cards");
    }

    // Coroutine to show the second hint after a delay
    private IEnumerator ShowHintAfterDelay(GameObject hint)
    {
        tutorialPanelEmpty.SetActive(true);
        yield return new WaitForSeconds(1f); // Wait for 1 second
        tutorialPanelEmpty.SetActive(false);
        hint.SetActive(true);
    }

    private async void CompleteStep(string stepId)
    {
        TutorialService service = ResolveTutorialService();
        if (service == null)
        {
            return;
        }

        await service.CompleteCurrentPlayerStepAsync(
            TutorialConstants.LibraryIntro,
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
