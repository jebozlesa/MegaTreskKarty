using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance { get; private set; }

    public int maxRetryAttempts = 3;
    public float retryDelaySeconds = 2f;

    public event Action<List<PlayerLeaderboardEntry>> OnLeaderboardLoaded;

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

    public void SendLeaderboard(int score)
    {
        if (!PlayFabManagerLogin.IsLoggedIn)
        {
            Debug.LogError("Hráč nie je prihlásený.");
            return;
        }

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

    void OnError(PlayFabError error)
    {
        Debug.LogError("Chyba: " + error.GenerateErrorReport());
    }

    public void GetLeaderboard()
    {
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
        OnLeaderboardLoaded?.Invoke(result.Leaderboard);
    }

    public void GetMyBestScore(string statisticName, Action<int> onScoreReceived, int attempt = 0)
    {
        var request = new GetPlayerStatisticsRequest
        {
            StatisticNames = new List<string> { statisticName }
        };

        PlayFabClientAPI.GetPlayerStatistics(request, result =>
        {
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
        }, error =>
        {
            if (attempt < maxRetryAttempts)
            {
                Debug.LogWarning($"Attempt {attempt + 1} failed, retrying in {retryDelaySeconds} seconds...");
                StartCoroutine(RetryAfterDelay(statisticName, onScoreReceived, attempt));
            }
            else
            {
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
