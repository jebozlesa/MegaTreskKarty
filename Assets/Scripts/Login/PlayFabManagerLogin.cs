using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayFabManagerLogin : MonoBehaviour
{
    public GameObject loadingImage;
    public GameObject messageGandhiBubble;
    public GameObject messageStalinBubble;
    public GameObject messageEinsteinBubble;
    public GameObject controlPanel;
    public TMP_Text messageGandhiText;
    public TMP_Text messageStalinText;
    public TMP_Text messageEinsteinText;
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    // Start is called before the first frame update
    void Start()
    {
        loadingImage.SetActive(true);

        // Načítanie prihlasovacích údajov
        string username = PlayerPrefs.GetString("username");
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");

        Debug.Log("Username: " + username);
        Debug.Log("Email: " + email);
        Debug.Log("Password: " + password);

        // Ak sú prihlasovacie údaje uložené, prihlás užívateľa
        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            var request = new LoginWithEmailAddressRequest 
            {
                Email = email,
                Password = password
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
        }
        else
        {
            loadingImage.SetActive(false);
            messageGandhiBubble.SetActive(true);
            messageStalinBubble.SetActive(true);
            controlPanel.SetActive(true);
        }
    }

    public void RegisterButton()
    {
        messageGandhiBubble.SetActive(false);
        messageStalinBubble.SetActive(false);
        controlPanel.SetActive(false);

        loadingImage.SetActive(true);

        if (passwordInput.text.Length < 6)
        {
            messageGandhiText.text = "Password too short!";
            return;
        }

        var request = new RegisterPlayFabUserRequest 
        {
            Email = emailInput.text,
            Username = usernameInput.text,
            Password = passwordInput.text,
            RequireBothUsernameAndEmail = false
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        loadingImage.SetActive(false);

        messageEinsteinBubble.SetActive(true);
        messageEinsteinText.text = "Welcome " + usernameInput.text;

        PlayerPrefs.SetString("username", usernameInput.text);
        PlayerPrefs.SetString("email", emailInput.text);
        PlayerPrefs.SetString("password", passwordInput.text);
        PlayerPrefs.Save();

        // Nastavenie DisplayName na užívateľské meno
        UpdateUserTitleDisplayName(usernameInput.text);

        StartCoroutine(LoadMainSceneAfterDelay(2));
    }

    void UpdateUserTitleDisplayName(string displayName)
    {
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdated, OnError);
    }

    void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("DisplayName updated successfully");
    }


    public void LoginButton()
    {
        messageGandhiBubble.SetActive(false);
        messageStalinBubble.SetActive(false);
        controlPanel.SetActive(false);

        loadingImage.SetActive(true);

        var request = new LoginWithEmailAddressRequest 
        {
            Email = emailInput.text,
            Password = passwordInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        PlayerPrefs.SetString("email", emailInput.text);
        PlayerPrefs.SetString("password", passwordInput.text);

        // Get the username
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest { PlayFabId = result.PlayFabId }, resultAccountInfo =>
        {
            string username = resultAccountInfo.AccountInfo.Username;
            PlayerPrefs.SetString("username", username);

            loadingImage.SetActive(false);
            Debug.Log("Sicko dobre");
            messageEinsteinBubble.SetActive(true);
            messageEinsteinText.text = "Welcome " + username;
            StartCoroutine(LoadMainSceneAfterDelay(2));
        }, error => { Debug.LogError(error.GenerateErrorReport()); });
    }


    IEnumerator LoadMainSceneAfterDelay(float delay)
    {
        // Počkaj určitý počet sekúnd
        yield return new WaitForSeconds(delay);

        messageEinsteinBubble.SetActive(false);

        // Potom načítaj hlavnú scénu
        SceneManager.LoadScene("Main");
    }

    public void ResetPasswordButton()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInput.text,
            TitleId = "9AEA7"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        messageGandhiText.text = "toto hovno robi";
    }

    void Login()
    {
        var request = new LoginWithCustomIDRequest 
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("Sicko dobre");
    }

    void OnError(PlayFabError error)
    {
        messageStalinBubble.SetActive(true);
        controlPanel.SetActive(true);

        loadingImage.SetActive(false);

        messageStalinText.text = error.ErrorMessage;
        Debug.Log("Nahovno daco");
        Debug.Log(error.GenerateErrorReport());
    }

}
