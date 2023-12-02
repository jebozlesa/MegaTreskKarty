using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManagerLeaderboard : MonoBehaviour
{
    public static PlayFabManagerLeaderboard Instance { get; private set; }

    public event Action<List<PlayerLeaderboardEntry>> OnLeaderboardLoaded;
    public string loggedInPlayerId;

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

        if (PlayFabManagerLogin.Instance != null)
        {
            Debug.Log("PlayFabManagerLeaderboard: Logged in player ID: " + PlayFabManagerLogin.Instance.LoggedInPlayerId);
            GetLeaderboard();
        }
        else
        {
            Debug.LogError("PlayFabManagerLeaderboard: PlayFabManagerLogin instance is null.");
        }
    }

    public void SendLeaderboard(int score)
    {
        if (string.IsNullOrEmpty(loggedInPlayerId))
        {
            Debug.LogError("Player is not logged in.");
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
        Debug.Log("Leaderboard updated successfully.");
    }

    public void GetLeaderboard()
    {
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
        OnLeaderboardLoaded?.Invoke(result.Leaderboard);
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("Error with PlayFab leaderboard: " + error.GenerateErrorReport());
    }
}
