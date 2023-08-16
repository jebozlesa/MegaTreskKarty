using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardTutorial : MonoBehaviour
{

    public static CardTutorial instance;

    public GameObject tutorialPanelHint1;
    public GameObject tutorialPanelHint2;
    public GameObject tutorialPanelHint3;
    public GameObject tutorialPanelHint4;
    public GameObject tutorialPanelHint5;
    public GameObject tutorialPanelHint6;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false); // Deaktivuje tento GameObject hneď po inicializácii
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("CardTutorial.Start() ===> START");

        tutorialPanelHint1.SetActive(true);
        tutorialPanelHint2.SetActive(false);
        tutorialPanelHint3.SetActive(false);
        tutorialPanelHint4.SetActive(false);
        tutorialPanelHint5.SetActive(false);
        tutorialPanelHint6.SetActive(false);

    }

    private GameObject InstantiateAndSetup(GameObject prefab)
    {
        
        GameObject instance = Instantiate(prefab, transform);
   //     instance.transform.localPosition = Vector3.zero;
        return instance;
    }

    public void CloseFirstHint()
    {
        Debug.Log("CardTutorial.CloseFirstHint() ===> START");
        tutorialPanelHint1.SetActive(false);
        StartCoroutine(ShowHintAfterDelay(tutorialPanelHint2, 0f));
    }

    public void CloseSecondHint()
    {
        Debug.Log("CardTutorial.CloseSecondHint() ===> START");
        tutorialPanelHint2.SetActive(false);
        StartCoroutine(ShowHintAfterDelay(tutorialPanelHint3, 0f));
    }

    public void CloseThirdHint()
    {
        Debug.Log("CardTutorial.CloseThirdHint() ===> START");
        tutorialPanelHint3.SetActive(false);
        StartCoroutine(ShowHintAfterDelay(tutorialPanelHint4, 5f));
    }

    public void CloseFourthHint()
    {
        Debug.Log("CardTutorial.CloseFourthHint() ===> START");
        tutorialPanelHint4.SetActive(false);
        StartCoroutine(ShowHintAfterDelay(tutorialPanelHint5, 0f));
    }

    public void CloseFifthHint()
    {
        Debug.Log("CardTutorial.CloseFifthHint() ===> START");
        tutorialPanelHint5.SetActive(false);
        StartCoroutine(ShowHintAfterDelay(tutorialPanelHint6, 3f));
    }

    public void CloseSixthHint()
    {
        Debug.Log("CardTutorial.CloseSixthHint() ===> START");
        tutorialPanelHint6.SetActive(false);
        PlayerPrefs.SetInt("HasCompletedTutorialCard", 1);
        PlayerPrefs.Save();
    }

    // Coroutine to show the second hint after a delay
    private IEnumerator ShowHintAfterDelay(GameObject hint, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds); 
        hint.SetActive(true);
    }

}
