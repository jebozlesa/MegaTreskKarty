using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Tutorial / Onboarding")]
    public TutorialService tutorialService;
    public ServerFunctionsManager serverFunctionsManager;
    public string mainSceneName = "Main";
    public string marketplaceSceneName = "Marketplace";
    public string librarySceneName = "Cards";

    public static PlayFabManagerLogin Instance { get; private set; }

    public static bool IsLoggedIn { get; private set; } = false;
    public string LoggedInPlayerId { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            IsLoggedIn = false;
            LoggedInPlayerId = string.Empty;
        }
    }

    public static void ClearRuntimeLoginState()
    {
        IsLoggedIn = false;

        if (Instance != null)
        {
            Instance.LoggedInPlayerId = string.Empty;
        }
    }

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
            var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
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
        AudioManager.Instance.PlayButtonClickSound();
        messageGandhiBubble.SetActive(false);
        messageStalinBubble.SetActive(false);
        controlPanel.SetActive(false);
        loadingImage.SetActive(true);

        // Kontrola, či sú všetky polia vyplnené
        if (string.IsNullOrEmpty(emailInput.text))
        {
            messageGandhiText.text = "Email is required!";
            messageGandhiBubble.SetActive(true);
            loadingImage.SetActive(false);
            controlPanel.SetActive(true);
            return;
        }

        if (string.IsNullOrEmpty(usernameInput.text))
        {
            messageGandhiText.text = "Username is required!";
            messageGandhiBubble.SetActive(true);
            loadingImage.SetActive(false);
            controlPanel.SetActive(true);
            return;
        }

        if (string.IsNullOrEmpty(passwordInput.text))
        {
            messageGandhiText.text = "Password is required!";
            messageGandhiBubble.SetActive(true);
            loadingImage.SetActive(false);
            controlPanel.SetActive(true);
            return;
        }

        if (passwordInput.text.Length < 6)
        {
            messageGandhiText.text = "Password too short!";
            messageGandhiBubble.SetActive(true);
            loadingImage.SetActive(false);
            controlPanel.SetActive(true);
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Email = emailInput.text,
            Username = usernameInput.text,
            Password = passwordInput.text,
            RequireBothUsernameAndEmail = false,
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
        PlayerPrefs.SetString("LoggedInPlayerId", result.PlayFabId);
        PlayerPrefs.Save();

        IsLoggedIn = true;

        LoggedInPlayerId = result.PlayFabId;

        // Nastavenie DisplayName na užívateľské meno
        UpdateUserTitleDisplayName(usernameInput.text);

        StartCoroutine(LoadTutorialRouteAfterDelay(2));
    }

    void UpdateUserTitleDisplayName(string displayName)
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = displayName };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdated, OnError);
    }

    void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("DisplayName updated successfully");
    }

    public void LoginButton()
    {
        AudioManager.Instance.PlayButtonClickSound();
        messageGandhiBubble.SetActive(false);
        messageStalinBubble.SetActive(false);
        controlPanel.SetActive(false);

        loadingImage.SetActive(true);

        var request = new LoginWithEmailAddressRequest
        {
            Email = emailInput.text,
            Password = passwordInput.text,
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        if (!string.IsNullOrEmpty(emailInput.text) && !string.IsNullOrEmpty(passwordInput.text))
        {
            PlayerPrefs.SetString("email", emailInput.text);
            PlayerPrefs.SetString("password", passwordInput.text);
        }

        PlayerPrefs.SetString("LoggedInPlayerId", result.PlayFabId);
        PlayerPrefs.Save();
        IsLoggedIn = true;
        LoggedInPlayerId = result.PlayFabId;

        // Get the username
        PlayFabClientAPI.GetAccountInfo(
            new GetAccountInfoRequest { PlayFabId = result.PlayFabId },
            resultAccountInfo =>
            {
                string username = resultAccountInfo.AccountInfo.Username;
                PlayerPrefs.SetString("username", username);

                loadingImage.SetActive(false);
                Debug.Log("Sicko dobre");
                messageEinsteinBubble.SetActive(true);
                messageEinsteinText.text = "Welcome " + username;
                StartCoroutine(LoadTutorialRouteAfterDelay(2));
            },
            error =>
            {
                Debug.LogError(error.GenerateErrorReport());
            }
        );
    }

    private void SaveEmailAndPasswordToPlayerPrefs(string email, string password)
    {
        PlayerPrefs.SetString("email", email);
        PlayerPrefs.SetString("password", password);
    }

    IEnumerator LoadMainSceneAfterDelay(float delay)
    {
        // Počkaj určitý počet sekúnd
        yield return new WaitForSeconds(delay);

        messageEinsteinBubble.SetActive(false);

        // Potom načítaj hlavnú scénu
        SceneManager.LoadScene("Main");
    }

    IEnumerator LoadMarketplaceSceneAfterDelay(float delay)
    {
        // Počkaj určitý počet sekúnd
        yield return new WaitForSeconds(delay);

        messageEinsteinBubble.SetActive(false);

        // Potom načítaj hlavnú scénu
        SceneManager.LoadScene("Marketplace");
    }

    IEnumerator LoadTutorialRouteAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        messageEinsteinBubble.SetActive(false);
        loadingImage.SetActive(true);

        TutorialService service = EnsureTutorialService();
        Task<TutorialStateResponse> stateTask = service.GetTutorialStateAsync(LoggedInPlayerId);
        yield return new WaitUntil(() => stateTask.IsCompleted);

        loadingImage.SetActive(false);

        TutorialStateResponse state = stateTask.Result;
        if (state == null || !state.success)
        {
            messageStalinBubble.SetActive(true);
            controlPanel.SetActive(true);
            messageStalinText.text = "Could not load tutorial state.";
            Debug.LogError(
                $"[PlayFabManagerLogin] Tutorial route failed: stage={state?.stage}, error={state?.error}"
            );
            yield break;
        }

        string sceneName = ResolveSceneForTutorialRoute(state.recommendedRoute);
        Debug.LogWarning(
            $"[PlayFabManagerLogin] Tutorial route resolved: player={LoggedInPlayerId}, route={state.recommendedRoute}, scene={sceneName}, "
                + $"safe={state.gates?.safeCardDeckState}, needsFirstPack={state.gates?.needsFirstPack}, "
                + $"needsLibrarySwap={state.gates?.needsLibraryDeckSwap}"
        );
        SceneManager.LoadScene(sceneName);
    }

    private TutorialService EnsureTutorialService()
    {
        if (tutorialService == null)
        {
            tutorialService = GetComponent<TutorialService>();
        }

        if (serverFunctionsManager == null)
        {
            serverFunctionsManager = GetComponent<ServerFunctionsManager>();
        }

        if (serverFunctionsManager == null)
        {
            serverFunctionsManager = gameObject.AddComponent<ServerFunctionsManager>();
        }

        if (tutorialService == null)
        {
            tutorialService = gameObject.AddComponent<TutorialService>();
        }

        if (tutorialService.serverFunctionsManager == null)
        {
            tutorialService.serverFunctionsManager = serverFunctionsManager;
        }

        return tutorialService;
    }

    private string ResolveSceneForTutorialRoute(string route)
    {
        if (string.Equals(route, "Marketplace", StringComparison.OrdinalIgnoreCase))
        {
            return marketplaceSceneName;
        }

        if (string.Equals(route, "Cards", StringComparison.OrdinalIgnoreCase))
        {
            return librarySceneName;
        }

        return mainSceneName;
    }

    public void ResetPasswordButton()
    {
        AudioManager.Instance.PlayButtonClickSound();
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = emailInput.text,
            TitleId = "9AEA7",
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        messageGandhiText.text = "toto hovno robi";
    }

    // void Login()
    // {
    //     var request = new LoginWithCustomIDRequest
    //     {
    //         CustomId = SystemInfo.deviceUniqueIdentifier,
    //         CreateAccount = true
    //     };
    //     PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
    // }

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
        IsLoggedIn = false;
        Debug.Log("Nahovno daco");
        Debug.Log(error.GenerateErrorReport());
    }
}
