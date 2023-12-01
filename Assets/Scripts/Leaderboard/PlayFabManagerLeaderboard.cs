using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManagerLeaderboard : MonoBehaviour
{

    public static PlayFabManagerLeaderboard Instance { get; private set; }

    // public GameObject loadingImage;
    // public GameObject errorImage;

    public event Action<List<PlayerLeaderboardEntry>> OnLeaderboardLoaded;
    public string loggedInPlayerId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Login();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Login()
    {
        //        loadingImage.SetActive(true);
        string username = PlayerPrefs.GetString("username");
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");

        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnSuccess, OnError);
        //        loadingImage.SetActive(false);
    }

    void OnSuccess(LoginResult result)
    {
        Debug.Log("PlayFabManager = Sicko dobre");
        //      loadingImage.SetActive(false);
        loggedInPlayerId = result.PlayFabId;
        GetLeaderboard();
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("PlayFabManager = Daco nahovno");
        //      errorImage.SetActive(true);
        Debug.Log(error.GenerateErrorReport());
    }

    public void SendLeaderboard(int score)
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "RoyalRumble",
                    Value = score
                }
            }
        };
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Rekordy poslane");
    }

    public void GetLeaderboard()
    {
        //      loadingImage.SetActive(true);
        var request = new GetLeaderboardRequest
        {
            StatisticName = "RoyalRumble",
            StartPosition = 0,
            MaxResultsCount = 100,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        };
        PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult result)
    {
        foreach (var item in result.Leaderboard)
        {
            string username = string.IsNullOrEmpty(item.DisplayName) ? "Noname" : item.DisplayName;
            Debug.Log(item.Position + " " + username + " " + item.StatValue);
            OnLeaderboardLoaded?.Invoke(result.Leaderboard);
        }
    }
}
