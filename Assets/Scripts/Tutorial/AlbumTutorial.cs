using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AlbumTutorial : MonoBehaviour
{
    public GameObject tutorialPanelHint1;
    public GameObject tutorialPanelHint2;
    public GameObject tutorialPanelEmpty;

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
        StartCoroutine(ShowHintAfterDelay(tutorialPanelHint2));
    }

    public void CloseSecondHint()
    {
        Debug.Log("AlbumTutorial.CloseSecondHint() ===> START");
        tutorialPanelHint2.SetActive(false);
        PlayerPrefs.SetInt("HasCompletedTutorialAlbum", 1);
        PlayerPrefs.Save();
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
}
