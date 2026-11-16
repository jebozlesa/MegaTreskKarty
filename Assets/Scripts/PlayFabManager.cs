using System;
using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance { get; private set; }

    public int maxRetryAttempts = 3;
    public float retryDelaySeconds = 2f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (!PlayFabManagerLogin.IsLoggedIn)
            {
                TryLoginAgain();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void TryLoginAgain()
    {
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("PlayFabManager: Login credentials are not available.");
            return;
        }

        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginError);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFabManager: Login succeeded.");
    }

    private void OnLoginError(PlayFabError error)
    {
        Debug.LogError("PlayFabManager: Login failed: " + error.GenerateErrorReport());
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
            foreach (StatisticValue statistic in result.Statistics)
            {
                if (statistic.StatisticName == statisticName)
                {
                    bestScore = statistic.Value;
                    break;
                }
            }
            onScoreReceived?.Invoke(bestScore);
        }, error =>
        {
            if (attempt < maxRetryAttempts)
            {
                Debug.LogWarning(
                    $"Attempt {attempt + 1} failed, retrying in {retryDelaySeconds} seconds..."
                );
                StartCoroutine(RetryAfterDelay(statisticName, onScoreReceived, attempt));
            }
            else
            {
                onScoreReceived?.Invoke(0);
                Debug.LogError(
                    "Error retrieving player stats after retries: " + error.GenerateErrorReport()
                );
            }
        });
    }

    private IEnumerator RetryAfterDelay(
        string statisticName,
        Action<int> onScoreReceived,
        int attempt
    )
    {
        yield return new WaitForSeconds(retryDelaySeconds);
        GetMyBestScore(statisticName, onScoreReceived, attempt + 1);
    }
}
