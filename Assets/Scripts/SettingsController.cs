using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsController : MonoBehaviour
{
    [Header("Labels")]
    [SerializeField] private TMP_Text soundLabel;
    [SerializeField] private TMP_Text musicLabel;

    [Header("Logout")]
    [SerializeField] private GameObject logoutConfirmationPanel;
    [SerializeField] private string loginSceneName = "Login";

    [Header("Navigation")]
    [SerializeField] private string avatarSceneName = "AvatarSelection";
    [SerializeField] private GameObject avatarEntryObject;

    [Header("External Links")]
    [SerializeField] private string termsUrl = "https://play.google.com/store";
    [SerializeField] private string privacyUrl = "https://play.google.com/store";

    private void Awake()
    {
        GameAudioSettings.ApplyStoredSettings();
        HideLogoutConfirmation();

        if (avatarEntryObject != null)
        {
            avatarEntryObject.SetActive(false);
        }

        RefreshLabels();
    }

    public void ToggleSound()
    {
        GameAudioSettings.SetSoundEnabled(!GameAudioSettings.IsSoundEnabled);
        RefreshLabels();
    }

    public void ToggleMusic()
    {
        GameAudioSettings.SetMusicEnabled(!GameAudioSettings.IsMusicEnabled);
        RefreshLabels();
    }

    public void ShowLogoutConfirmation()
    {
        if (logoutConfirmationPanel != null)
        {
            logoutConfirmationPanel.SetActive(true);
        }
    }

    public void HideLogoutConfirmation()
    {
        if (logoutConfirmationPanel != null)
        {
            logoutConfirmationPanel.SetActive(false);
        }
    }

    public void ConfirmLogout()
    {
        PlayerSessionLogout.LogoutAndLoadLogin(loginSceneName);
    }

    public void OpenTerms()
    {
        OpenUrl(termsUrl);
    }

    public void OpenPrivacy()
    {
        OpenUrl(privacyUrl);
    }

    public void OpenAvatarSelection()
    {
        if (!string.IsNullOrWhiteSpace(avatarSceneName))
        {
            SceneManager.LoadScene(avatarSceneName);
        }
    }

    private void RefreshLabels()
    {
        if (soundLabel != null)
        {
            soundLabel.text = GameAudioSettings.IsSoundEnabled ? "SOUND ON" : "SOUND OFF";
        }

        if (musicLabel != null)
        {
            musicLabel.text = GameAudioSettings.IsMusicEnabled ? "MUSIC ON" : "MUSIC OFF";
        }
    }

    private static void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("[SettingsController] External URL is empty.");
            return;
        }

        Application.OpenURL(url);
    }
}
