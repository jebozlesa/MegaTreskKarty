using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManager : MonoBehaviour
{
 //   public GameObject loadingImage;
 //   public GameObject errorImage;

    public event Action<List<PlayerLeaderboardEntry>> OnLeaderboardLoaded;

    // Start is called before the first frame update
    void Awake()
    {
        Login();
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
        Debug.Log("Sicko dobre");
  //      loadingImage.SetActive(false);
        GetLeaderboard();
    }

    void OnError(PlayFabError error)
    {
        Debug.Log("Daco nahovno");
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


}
