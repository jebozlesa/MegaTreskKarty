using UnityEngine;
using UnityEngine.SceneManagement;

public static class PlayerSessionLogout
{
    public static void LogoutAndLoadLogin(string loginSceneName = "Login")
    {
        ClearLocalSession();
        SceneManager.LoadScene(loginSceneName);
    }

    public static void ClearLocalSession()
    {
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.DeleteKey("email");
        PlayerPrefs.DeleteKey("password");
        PlayerPrefs.DeleteKey("LoggedInPlayerId");
        PlayerPrefs.DeleteKey("RoomCode");
        PlayerPrefs.DeleteKey("IsWaitingForOpponent");
        PlayerPrefs.Save();

        PlayFabManagerLogin.ClearRuntimeLoginState();

        if (PlayFabManagerLogin.Instance != null)
        {
            Object.Destroy(PlayFabManagerLogin.Instance.gameObject);
        }
    }
}
