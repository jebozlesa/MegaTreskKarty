using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GlobalExceptionHandler : MonoBehaviour
{
    public GameObject errorDialog; // Predpokladá sa, že toto je vaše modálne okno s chybovou správou
    public Text errorMessageText; // Textová komponenta v modálnom okne na zobrazenie chybového textu

    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Uistite sa, že tento objekt zostáva medzi scénami
        Application.logMessageReceived += HandleException;
        errorDialog.SetActive(false); // Skryť chybové okno na začiatku
    }

    void HandleException(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Exception)
        {
            // Zobrazte modálne okno s chybovou správou
            errorMessageText.text = logString; // Nastavte chybovú správu na zobrazenie
            errorDialog.SetActive(true); // Zobrazte chybové okno
        }
    }

    public void OnErrorDialogClose()
    {
        errorDialog.SetActive(false);
        LoadLoginScene(); // Návrat na prihlasovaciu scénu alebo reštartujte aplikáciu podľa vášho výberu
    }

    void LoadLoginScene()
    {
        SceneManager.LoadScene("Login"); // Názov vašej prihlasovacej scény
    }

    // Voliteľná metóda pre reštartovanie aplikácie
    public void RestartApplication()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reštartuje aktuálnu scénu
    }
}
