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

            // Pokús sa o prihlásenie, ak užívateľ nie je prihlásený
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

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            var request = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password
            };

            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginError);
        }
        else
        {
            Debug.LogError("PlayFabManager: Prihlasovacie údaje nie sú dostupné!");
        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFabManager: Prihlásenie úspešné");
        // Tu môžete pridať akékoľvek ďalšie kroky po úspešnom prihlásení.
    }

    private void OnLoginError(PlayFabError error)
    {
        Debug.LogError("PlayFabManager: Chyba pri prihlásení: " + error.GenerateErrorReport());
    }

    public void SendLeaderboard(int score)
    {
        if (!PlayFabManagerLogin.IsLoggedIn)
        {
            Debug.Log("SendLeaderboard ==> Hráč nie je prihlásený, pokúšam sa o prihlásenie.");
            TryLoginAndSendLeaderboard(score);
            return;
        }

        UpdateLeaderboard(score);
    }

    private void TryLoginAndSendLeaderboard(int score)
    {
        string email = PlayerPrefs.GetString("email");
        string password = PlayerPrefs.GetString("password");

        if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
        {
            var request = new LoginWithEmailAddressRequest
            {
                Email = email,
                Password = password
            };

            PlayFabClientAPI.LoginWithEmailAddress(request,
                result =>
                {
                    Debug.Log("SendLeaderboard ==> Prihlásenie úspešné, aktualizujem leaderboard.");
                    UpdateLeaderboard(score);
                },
                error =>
                {
                    Debug.LogError("SendLeaderboard ==> Chyba pri prihlásení: " + error.GenerateErrorReport());
                });
        }
        else
        {
            Debug.LogError("SendLeaderboard ==> Prihlasovacie údaje nie sú dostupné!");
        }
    }

    private void UpdateLeaderboard(int score)
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

    private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("SendLeaderboard ==> Rekordy úspešne poslané.");
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("SendLeaderboard ==> Chyba: " + error.GenerateErrorReport());
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
