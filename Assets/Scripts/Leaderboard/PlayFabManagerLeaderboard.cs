using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManagerLeaderboard : MonoBehaviour
{
    public event Action<List<PlayerLeaderboardEntry>> OnLeaderboardLoaded;
    public string loggedInPlayerId;

    public void SendLeaderboard(int score)
    {
        loggedInPlayerId = ResolveLoggedInPlayerId();
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
        Debug.Log("[PlayFabManagerLeaderboard] Requesting RoyalRumble leaderboard.");
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
        List<PlayerLeaderboardEntry> leaderboard =
            result?.Leaderboard ?? new List<PlayerLeaderboardEntry>();
        Debug.Log(
            $"[PlayFabManagerLeaderboard] RoyalRumble leaderboard loaded: records={leaderboard.Count}."
        );
        OnLeaderboardLoaded?.Invoke(leaderboard);
    }

    void OnError(PlayFabError error)
    {
        Debug.LogError("Error with PlayFab leaderboard: " + error.GenerateErrorReport());
    }

    private static string ResolveLoggedInPlayerId()
    {
        return PlayFabManagerLogin.Instance != null
            ? PlayFabManagerLogin.Instance.LoggedInPlayerId
            : string.Empty;
    }
}
