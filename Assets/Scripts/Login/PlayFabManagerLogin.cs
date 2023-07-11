using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.UI;

public class PlayFabManagerLogin : MonoBehaviour
{
    public TMP_Text messageText;
    public TMP_InputField usernameInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    // Start is called before the first frame update
    void Awake()
    {
       // Login();
    }

    public void RegisterButton()
    {
        if (passwordInput.text.Length < 6)
        {
            messageText.text = "Password too short!";
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
        messageText.text = "Registered and logged in";
    }

    public void LoginButton()
    {
        var request = new LoginWithEmailAddressRequest 
        {
            Email = emailInput.text,
            Password = passwordInput.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Sicko dobre");
        messageText.text = "OK";
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
        messageText.text = "toto hovno robi";
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
        messageText.text = error.ErrorMessage;
        Debug.Log("Nahovno daco");
        Debug.Log(error.GenerateErrorReport());
    }


}
