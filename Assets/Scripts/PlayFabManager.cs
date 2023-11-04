using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance { get; private set; }

    // public GameObject loadingImage;
    // public GameObject errorImage;

    public int maxRetryAttempts = 3;
    public float retryDelaySeconds = 2f;

    public event Action<List<PlayerLeaderboardEntry>> OnLeaderboardLoaded;

    // private void Awake()
    // {
    //     if (Instance == null)
    //     {
    //         Instance = this;
    //         DontDestroyOnLoad(gameObject);
    //         Login();
    //     }
    //     else
    //     {
    //         Destroy(gameObject);
    //     }
    // }

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
            MaxResultsCount = 10,
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

    public void GetMyBestScore(string statisticName, Action<int> onScoreReceived, int attempt = 0)
    {
        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { statisticName }
        };

        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
            // Success
            int bestScore = 0;
            foreach (var eachStat in result.Statistics)
            {
                if (eachStat.StatisticName == statisticName)
                {
                    bestScore = eachStat.Value;
                    break;
                }
            }
            onScoreReceived?.Invoke(bestScore);
            Debug.Log("Your best score in " + statisticName + " is " + bestScore);

        }, error =>
        {
            // If there's an error and we have retries left, retry after a delay
            if (attempt < maxRetryAttempts)
            {
                Debug.LogWarning($"Attempt {attempt + 1} failed, retrying in {retryDelaySeconds} seconds...");
                StartCoroutine(RetryAfterDelay(statisticName, onScoreReceived, attempt));
            }
            else
            {
                // If no more retries are left, invoke the callback with a default value (e.g., 0)
                onScoreReceived?.Invoke(0);
                Debug.LogError("Error retrieving player stats after retries: " + error.GenerateErrorReport());
            }
        });
    }

    private IEnumerator RetryAfterDelay(string statisticName, Action<int> onScoreReceived, int attempt)
    {
        yield return new WaitForSeconds(retryDelaySeconds);
        GetMyBestScore(statisticName, onScoreReceived, attempt + 1);
    }


}
