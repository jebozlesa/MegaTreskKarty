using UnityEngine;

public static class GameAudioSettings
{
    public const string SoundMutedKey = "isSoundMuted";
    public const string MusicMutedKey = "isMusicMuted";

    public static bool IsSoundEnabled => PlayerPrefs.GetInt(SoundMutedKey, 0) == 0;
    public static bool IsMusicEnabled => PlayerPrefs.GetInt(MusicMutedKey, 0) == 0;

    public static void SetSoundEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(SoundMutedKey, enabled ? 0 : 1);
        PlayerPrefs.Save();
        ApplyMasterAudioState();
        RefreshMusic();
    }

    public static void SetMusicEnabled(bool enabled)
    {
        PlayerPrefs.SetInt(MusicMutedKey, enabled ? 0 : 1);
        PlayerPrefs.Save();
        RefreshMusic();
    }

    public static void ApplyStoredSettings()
    {
        ApplyMasterAudioState();
        RefreshMusic();
    }

    public static void ApplyMasterAudioState()
    {
        AudioListener.volume = IsSoundEnabled ? 1f : 0f;
    }

    public static bool ShouldPlaySoundEffect()
    {
        return IsSoundEnabled;
    }

    public static bool ShouldPlayMusicInScene(string sceneName)
    {
        if (!IsSoundEnabled || !IsMusicEnabled)
        {
            return false;
        }

        return sceneName == "Main"
            || sceneName == "Marketplace"
            || sceneName == "Settings";
    }

    private static void RefreshMusic()
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.RefreshMusicState();
        }
    }
}
